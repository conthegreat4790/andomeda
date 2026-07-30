using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class CustomTerrainGenerator : MonoBehaviour
{
    [Header("General Settings")]
    public bool autoUpdate = true;
    public int seed = 42;

    [Header("Terrain Dimensions")]
    public int width = 256;
    public int length = 256;
    public int height = 100; // Increased default height for bigger elevation features

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
    [Tooltip("Controls how spread out high-elevation mountain zones are.")]
    [Range(0.0001f, 0.05f)]
    public float elevationFrequency = 0.005f;

    [Tooltip("How much extra height is added in high-elevation areas.")]
    [Range(0f, 1f)]
    public float elevationStrength = 0.5f;

    [Header("Flattener Control")]
    [Tooltip("Controls how spread out the flat areas are.")]
    [Range(0.0001f, 0.05f)]
    public float flattenFrequency = 0.008f;

    [Tooltip("Higher values force more regions to become flat plains/valleys.")]
    [Range(0f, 1f)]
    public float flattenAmount = 0.7f;

    [Header("Offset & Curve")]
    public Vector2 offset = Vector2.zero;
    public AnimationCurve heightCurve = AnimationCurve.Linear(0, 0, 1, 1);

    private Terrain terrain;

    private void OnValidate()
    {
        if (autoUpdate)
        {
            GenerateTerrain();
        }
    }

    [ContextMenu("Generate Terrain")]
    public void GenerateTerrain()
    {
        terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrainData(terrain.terrainData);
    }

    private TerrainData GenerateTerrainData(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, height, length);
        terrainData.SetHeights(0, 0, GenerateHeights());
        return terrainData;
    }

    private float[,] GenerateHeights()
    {
        float[,] heights = new float[width, length];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        // Separate offsets for the macro features so they don't line up directly
        Vector2 elevationOffset = new Vector2(prng.Next(-100000, 100000), prng.Next(-100000, 100000)) + offset;
        Vector2 flattenOffset = new Vector2(prng.Next(-100000, 100000), prng.Next(-100000, 100000)) + offset;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < length; y++)
            {
                // 1. Calculate Base Multi-Octave Noise
                float amplitude = 1;
                float frequency = 1;
                float baseNoise = 0;
                float maxPossibleHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x / scale) * frequency + octaveOffsets[i].x;
                    float sampleY = (y / scale) * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                    baseNoise += perlinValue * amplitude;

                    maxPossibleHeight += amplitude;
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                float normalizedBase = Mathf.Clamp01(baseNoise / maxPossibleHeight);

                // 2. Calculate Flatness Mask
                float flattenSampleX = (x * flattenFrequency) + flattenOffset.x;
                float flattenSampleY = (y * flattenFrequency) + flattenOffset.y;
                float flattenMask = Mathf.PerlinNoise(flattenSampleX, flattenSampleY); // 0.0 to 1.0

                // Blend base noise toward flat based on flattenAmount
                float flattenedBase = Mathf.Lerp(normalizedBase, 0f, flattenMask * flattenAmount);

                // 3. Calculate Regional Elevation Boost
                float elevSampleX = (x * elevationFrequency) + elevationOffset.x;
                float elevSampleY = (y * elevationFrequency) + elevationOffset.y;
                float elevationMask = Mathf.PerlinNoise(elevSampleX, elevSampleY); // 0.0 to 1.0

                // Combine base shape with regional elevation boost
                float finalHeight = flattenedBase + (elevationMask * elevationStrength);

                // Clamp and apply curve
                finalHeight = Mathf.Clamp01(finalHeight);
                heights[x, y] = heightCurve.Evaluate(finalHeight);
            }
        }

        return heights;
    }
}