using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class TerrainChunk : MonoBehaviour
{
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;
    private TerrainChunkSettings settings;

    private Vector2 chunkCoord;
    private int chunkSize;
    private Bounds bounds;

    public void Initialize(Vector2 coord, int size, Material material, TerrainChunkSettings chunkSettings)
    {
        chunkCoord = coord;
        chunkSize = size;
        settings = chunkSettings;

        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();

        meshRenderer.material = material;

        Vector3 position = new Vector3(coord.x * size, 0, coord.y * size);
        transform.position = position;
        bounds = new Bounds(position + new Vector3(size / 2f, 0, size / 2f), new Vector3(size, settings.heightMultiplier * 2, size));

        GenerateChunkMesh(1); // Default high detail
    }

    public void UpdateChunkVisibility(Vector3 playerPosition, float maxViewDistance, bool isBaseChunk)
    {
        // Base chunk at (0,0) stays permanently loaded
        if (isBaseChunk)
        {
            gameObject.SetActive(true);
            return;
        }

        float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(playerPosition));
        bool isVisible = viewerDstFromNearestEdge <= maxViewDistance;

        gameObject.SetActive(isVisible);
    }

    public void GenerateChunkMesh(int lodStep)
    {
        Mesh mesh = new Mesh();
        mesh.name = $"Chunk_{chunkCoord.x}_{chunkCoord.y}";

        int numVerticesAxis = (chunkSize / lodStep) + 1;
        float actualCellSize = (float)chunkSize / (numVerticesAxis - 1);

        if (numVerticesAxis * numVerticesAxis > 65000)
        {
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }

        Vector3[] vertices = new Vector3[numVerticesAxis * numVerticesAxis];
        Vector2[] uvs = new Vector2[numVerticesAxis * numVerticesAxis];
        Vector4[] tangents = new Vector4[numVerticesAxis * numVerticesAxis];
        int[] triangles = new int[(numVerticesAxis - 1) * (numVerticesAxis - 1) * 6];

        float safeScale = Mathf.Max(settings.scale, 0.0001f);
        float safeUvScale = Mathf.Max(settings.uvScale, 0.0001f);

        System.Random prng = new System.Random(settings.seed);
        Vector2[] octaveOffsets = new Vector2[settings.octaves];
        for (int i = 0; i < settings.octaves; i++)
        {
            octaveOffsets[i] = new Vector2(prng.Next(-100000, 100000) + settings.offset.x, prng.Next(-100000, 100000) + settings.offset.y);
        }

        Vector2 elevOffset = new Vector2(prng.Next(-100000, 100000), prng.Next(-100000, 100000)) + settings.offset;
        Vector2 flatOffset = new Vector2(prng.Next(-100000, 100000), prng.Next(-100000, 100000)) + settings.offset;
        Vector2 mtnOffset = new Vector2(prng.Next(-100000, 100000), prng.Next(-100000, 100000)) + settings.offset;

        // 1. Calculate Vertices using Absolute World Position
        for (int z = 0; z < numVerticesAxis; z++)
        {
            for (int x = 0; x < numVerticesAxis; x++)
            {
                int i = z * numVerticesAxis + x;

                // Absolute World Position ensures seamless edges between chunks
                float worldX = transform.position.x + (x * actualCellSize);
                float worldZ = transform.position.z + (z * actualCellSize);

                // --- Base Noise ---
                float amplitude = 1;
                float frequency = 1;
                float baseNoise = 0;
                float maxPossibleHeight = 0;

                for (int o = 0; o < settings.octaves; o++)
                {
                    float sampleX = (worldX / safeScale) * frequency + octaveOffsets[o].x;
                    float sampleY = (worldZ / safeScale) * frequency + octaveOffsets[o].y;

                    baseNoise += Mathf.PerlinNoise(sampleX, sampleY) * amplitude;
                    maxPossibleHeight += amplitude;
                    amplitude *= settings.persistence;
                    frequency *= settings.lacunarity;
                }

                float normalizedBase = maxPossibleHeight > 0 ? Mathf.Clamp01(baseNoise / maxPossibleHeight) : 0f;

                // --- Flattening & Elevation ---
                float flattenMask = Mathf.Clamp01(Mathf.PerlinNoise((worldX * settings.flattenFrequency) + flatOffset.x, (worldZ * settings.flattenFrequency) + flatOffset.y));
                float flattenedBase = Mathf.Lerp(normalizedBase, 0f, flattenMask * settings.flattenAmount);

                float elevationMask = Mathf.Clamp01(Mathf.PerlinNoise((worldX * settings.elevationFrequency) + elevOffset.x, (worldZ * settings.elevationFrequency) + elevOffset.y));
                float finalNormalizedHeight = Mathf.Clamp01(flattenedBase + (elevationMask * settings.elevationStrength));

                float yHeight = settings.heightCurve != null ? settings.heightCurve.Evaluate(finalNormalizedHeight) * settings.heightMultiplier : finalNormalizedHeight * settings.heightMultiplier;

                // --- Mountain Generator ---
                if (settings.enableMountains)
                {
                    float mountainMask = Mathf.Clamp01(Mathf.PerlinNoise((worldX * settings.mountainFrequency) + mtnOffset.x, (worldZ * settings.mountainFrequency) + mtnOffset.y));
                    mountainMask = Mathf.Pow(mountainMask, 2.5f);
                    yHeight += mountainMask * settings.mountainHeight;
                }

                if (float.IsNaN(yHeight) || float.IsInfinity(yHeight)) yHeight = 0f;

                // Position relative to chunk root
                vertices[i] = new Vector3(x * actualCellSize, yHeight, z * actualCellSize);
                uvs[i] = new Vector2(worldX * safeUvScale, worldZ * safeUvScale);
                tangents[i] = new Vector4(1f, 0f, 0f, -1f);
            }
        }

        // 2. Build Triangles
        int tris = 0;
        for (int z = 0; z < numVerticesAxis - 1; z++)
        {
            for (int x = 0; x < numVerticesAxis - 1; x++)
            {
                int botLeft = z * numVerticesAxis + x;
                int botRight = botLeft + 1;
                int topLeft = (z + 1) * numVerticesAxis + x;
                int topRight = topLeft + 1;

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

        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    public void RebuildMesh(TerrainChunkSettings newSettings)
    {
        settings = newSettings;
        GenerateChunkMesh(1);
    }
}

[System.Serializable]
public class TerrainChunkSettings
{
    public int seed = 42;
    public float heightMultiplier = 25f;
    public float scale = 20f;
    public int octaves = 4;
    public float persistence = 0.5f;
    public float lacunarity = 2f;
    public float elevationFrequency = 0.005f;
    public float elevationStrength = 0.5f;
    public float flattenFrequency = 0.008f;
    public float flattenAmount = 0.7f;
    public bool enableMountains = true;
    public float mountainFrequency = 0.004f;
    public float mountainHeight = 40f;
    public float uvScale = 0.1f;
    public Vector2 offset = Vector2.zero;
    public AnimationCurve heightCurve = AnimationCurve.Linear(0, 0, 1, 1);
}