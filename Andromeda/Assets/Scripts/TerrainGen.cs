using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CustomMeshTerrain : MonoBehaviour
{
    [Header("General Settings")]
    public bool autoUpdate = true;
    public int seed = 42;

    [Header("Base Grid Settings")]
    [Range(2, 1500)]
    public int baseWidth = 30;  // Base grid width
    [Range(2, 1500)]
    public int baseLength = 30; // Base grid length
    public float cellSize = 1f;

    [Header("Subdivision & Displacement")]
    [Tooltip("Subdivides each quad twice (4x4 sub-quads per base quad).")]
    public bool applyDoubleSubdivision = true;
    [Tooltip("If checked, Y displacement is handled by your Shader Material instead of vertex position.")]
    public bool materialHandlesDisplacement = true;
    public float heightMultiplier = 25f;

    [Header("Base Perlin Noise Dials")]
    [Range(0.001f, 100f)]
    public float scale = 20f;

    [Range(1, 8)]
    public int octaves = 4;

    [Range(0f, 1f)]
    public float persistence = 0.5f;

    [Range(1f, 5f)]
    public float lacunarity = 2f;

    [Header("High Elevation Control")]
    [Range(0.0001f, 0.05f)]
    public float elevationFrequency = 0.005f;

    [Range(0f, 1f)]
    public float elevationStrength = 0.5f;

    [Header("Flattener Control")]
    [Range(0.0001f, 0.05f)]
    public float flattenFrequency = 0.008f;

    [Range(0f, 1f)]
    public float flattenAmount = 0.7f;

    [Header("Offset & Curve")]
    public Vector2 offset = Vector2.zero;
    public AnimationCurve heightCurve = AnimationCurve.Linear(0, 0, 1, 1);

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    private void OnValidate()
    {
        if (autoUpdate)
        {
            GenerateTerrainMesh();
        }
    }

    [ContextMenu("Generate Mesh")]
    public void GenerateTerrainMesh()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();

        Mesh mesh = CreateTerrainMesh();
        meshFilter.sharedMesh = mesh;

        if (meshCollider != null)
        {
            meshCollider.sharedMesh = mesh;
        }
    }

    private Mesh CreateTerrainMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Subdivided Displacement Mesh";

        // Calculate resolution based on subdivision
        int subdivisionFactor = applyDoubleSubdivision ? 4 : 1;
        int width = (baseWidth - 1) * subdivisionFactor + 1;
        int length = (baseLength - 1) * subdivisionFactor + 1;
        float adjustedCellSize = cellSize / subdivisionFactor;

        if (width * length > 65000)
        {
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }

        Vector3[] vertices = new Vector3[width * length];
        Vector2[] uvs = new Vector2[width * length];
        Vector4[] tangents = new Vector4[width * length];
        int[] triangles = new int[(width - 1) * (length - 1) * 6];

        // Seed offsets
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        Vector2 elevationOffset = new Vector2(prng.Next(-100000, 100000), prng.Next(-100000, 100000)) + offset;
        Vector2 flattenOffset = new Vector2(prng.Next(-100000, 100000), prng.Next(-100000, 100000)) + offset;

        // 1. Generate Vertices using World Coordinates (Fixes noise artifacts)
        for (int z = 0; z < length; z++)
        {
            for (int x = 0; x < width; x++)
            {
                int i = z * width + x;

                float worldX = x * adjustedCellSize;
                float worldZ = z * adjustedCellSize;

                float yHeight = 0f;

                if (!materialHandlesDisplacement)
                {
                    float amplitude = 1;
                    float frequency = 1;
                    float baseNoise = 0;
                    float maxPossibleHeight = 0;

                    // Base Noise sampled via World Space
                    for (int o = 0; o < octaves; o++)
                    {
                        float sampleX = (worldX / scale) * frequency + octaveOffsets[o].x;
                        float sampleY = (worldZ / scale) * frequency + octaveOffsets[o].y;

                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                        baseNoise += perlinValue * amplitude;

                        maxPossibleHeight += amplitude;
                        amplitude *= persistence;
                        frequency *= lacunarity;
                    }

                    float normalizedBase = Mathf.Clamp01(baseNoise / maxPossibleHeight);

                    // Flatten Mask sampled via World Space
                    float flattenSampleX = (worldX * flattenFrequency) + flattenOffset.x;
                    float flattenSampleY = (worldZ * flattenFrequency) + flattenOffset.y;
                    float flattenMask = Mathf.PerlinNoise(flattenSampleX, flattenSampleY);
                    float flattenedBase = Mathf.Lerp(normalizedBase, 0f, flattenMask * flattenAmount);

                    // Elevation Mask sampled via World Space
                    float elevSampleX = (worldX * elevationFrequency) + elevationOffset.x;
                    float elevSampleY = (worldZ * elevationFrequency) + elevationOffset.y;
                    float elevationMask = Mathf.PerlinNoise(elevSampleX, elevSampleY);

                    float finalNormalizedHeight = Mathf.Clamp01(flattenedBase + (elevationMask * elevationStrength));
                    yHeight = heightCurve.Evaluate(finalNormalizedHeight) * heightMultiplier;
                }

                vertices[i] = new Vector3(worldX, yHeight, worldZ);
                uvs[i] = new Vector2((float)x / (width - 1), (float)z / (length - 1));
                tangents[i] = new Vector4(1f, 0f, 0f, -1f);
            }
        }

        // 2. Build Alternating Triangles (Eliminates diagonal shading seams)
        int tris = 0;
        for (int z = 0; z < length - 1; z++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                int botLeft = z * width + x;
                int botRight = botLeft + 1;
                int topLeft = (z + 1) * width + x;
                int topRight = topLeft + 1;

                // Alternate diagonal split per checkerboard quad to prevent ridge lines
                if ((x + z) % 2 == 0)
                {
                    triangles[tris + 0] = botLeft;
                    triangles[tris + 1] = topLeft;
                    triangles[tris + 2] = botRight;

                    triangles[tris + 3] = botRight;
                    triangles[tris + 4] = topLeft;
                    triangles[tris + 5] = topRight;
                }
                else
                {
                    triangles[tris + 0] = botLeft;
                    triangles[tris + 1] = topLeft;
                    triangles[tris + 2] = topRight;

                    triangles[tris + 3] = botLeft;
                    triangles[tris + 4] = topRight;
                    triangles[tris + 5] = botRight;
                }

                tris += 6;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.tangents = tangents;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}