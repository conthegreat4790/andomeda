using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ChunkManager : MonoBehaviour
{
    [Header("Core Setup")]
    public Transform player;
    public Material terrainMaterial;
    public int chunkSize = 64; 
    public float maxViewDistance = 250f;

    [Header("Base Chunk Setup")]
    public bool previewBaseChunkInEditor = true;
    public bool keepBaseChunkLoadedAtRuntime = true;
    public Vector2 baseChunkCoord = Vector2.zero;

    [Header("Terrain Generation Dials")]
    public TerrainChunkSettings terrainSettings;

    // Control Flag
    private bool isInitialized = false;

    private Dictionary<Vector2, TerrainChunk> chunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    private List<TerrainChunk> visibleChunks = new List<TerrainChunk>();
    private Vector2 currentChunkCoord;

    private void OnValidate()
    {
        // Live preview ONLY in Edit mode
        if (!Application.isPlaying && previewBaseChunkInEditor)
        {
            UnityEditor.EditorApplication.delayCall += UpdateEditorBaseChunk;
        }
    }

    private void Start()
    {
        if (Application.isPlaying)
        {
            // Clear editor preview chunks on play start
            ClearAllChunks();
            isInitialized = false; // Waiting for manual trigger call
        }
    }

    private void Update()
    {
        if (!Application.isPlaying) return;

        // Only stream chunks after StartGeneratingTerrain() has been called
        if (!isInitialized || player == null) return;

        int currentChunkX = Mathf.FloorToInt(player.position.x / chunkSize);
        int currentChunkZ = Mathf.FloorToInt(player.position.z / chunkSize);

        Vector2 newChunkCoord = new Vector2(currentChunkX, currentChunkZ);

        if (newChunkCoord != currentChunkCoord)
        {
            currentChunkCoord = newChunkCoord;
            UpdateVisibleChunks();
        }
    }

    /// <summary>
    /// Call this function from another script or event to begin terrain generation.
    /// It generates the base chunk first and starts streaming chunks around the player.
    /// </summary>
    public void StartGeneratingTerrain()
    {
        if (terrainMaterial == null)
        {
            Debug.LogError("ChunkManager: Terrain Material is missing!");
            return;
        }

        // Clean out any leftover preview chunks
        ClearAllChunks();

        // Calculate initial player chunk position
        if (player != null)
        {
            int currentChunkX = Mathf.FloorToInt(player.position.x / chunkSize);
            int currentChunkZ = Mathf.FloorToInt(player.position.z / chunkSize);
            currentChunkCoord = newChunkCoord(currentChunkX, currentChunkZ);
        }
        else
        {
            currentChunkCoord = baseChunkCoord;
        }

        // Enable runtime streaming
        isInitialized = true;

        // Immediately generate base chunk and visible surrounding chunks
        UpdateVisibleChunks();
    }

    /// <summary>
    /// Optional: Call this if you want to pause terrain streaming or reset terrain generation.
    /// </summary>
    public void StopGeneratingTerrain()
    {
        isInitialized = false;
        ClearAllChunks();
    }

    private Vector2 newChunkCoord(int x, int z)
    {
        return new Vector2(x, z);
    }

    // --- EDITOR PREVIEW LOGIC ---
    private void UpdateEditorBaseChunk()
    {
        if (this == null || terrainMaterial == null || Application.isPlaying) return;

        TerrainChunk baseChunk = GetComponentInChildren<TerrainChunk>();

        if (baseChunk == null)
        {
            GameObject chunkObj = new GameObject($"Chunk_{baseChunkCoord.x}_{baseChunkCoord.y} (Editor Preview)");
            chunkObj.transform.parent = transform;

            baseChunk = chunkObj.AddComponent<TerrainChunk>();
            baseChunk.Initialize(baseChunkCoord, chunkSize, terrainMaterial, terrainSettings);
        }
        else
        {
            baseChunk.RebuildMesh(terrainSettings);
        }
    }

    private void ClearAllChunks()
    {
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in transform)
        {
            children.Add(child.gameObject);
        }

        foreach (GameObject child in children)
        {
            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child, true);
            }
        }

        chunkDictionary.Clear();
        visibleChunks.Clear();
    }

    // --- RUNTIME GENERATION LOGIC ---
    private void UpdateVisibleChunks()
    {
        if (terrainMaterial == null) return;

        Vector3 targetPos = player != null ? player.position : Vector3.zero;
        int chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDistance / chunkSize);

        for (int i = visibleChunks.Count - 1; i >= 0; i--)
        {
            if (visibleChunks[i] == null) continue;

            bool isBase = keepBaseChunkLoadedAtRuntime && 
                          (visibleChunks[i].transform.position.x == baseChunkCoord.x * chunkSize) && 
                          (visibleChunks[i].transform.position.z == baseChunkCoord.y * chunkSize);

            visibleChunks[i].UpdateChunkVisibility(targetPos, maxViewDistance, isBase);
        }

        for (int zOffset = -chunksVisibleInViewDst; zOffset <= chunksVisibleInViewDst; zOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoord.x + xOffset, currentChunkCoord.y + zOffset);

                if (chunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    if (chunkDictionary[viewedChunkCoord] == null) continue;

                    bool isBase = keepBaseChunkLoadedAtRuntime && (viewedChunkCoord == baseChunkCoord);
                    chunkDictionary[viewedChunkCoord].UpdateChunkVisibility(targetPos, maxViewDistance, isBase);

                    if (chunkDictionary[viewedChunkCoord].gameObject.activeSelf && !visibleChunks.Contains(chunkDictionary[viewedChunkCoord]))
                    {
                        visibleChunks.Add(chunkDictionary[viewedChunkCoord]);
                    }
                }
                else
                {
                    GameObject chunkObj = new GameObject($"Chunk_{viewedChunkCoord.x}_{viewedChunkCoord.y}");
                    chunkObj.transform.parent = transform;

                    TerrainChunk newChunk = chunkObj.AddComponent<TerrainChunk>();
                    newChunk.Initialize(viewedChunkCoord, chunkSize, terrainMaterial, terrainSettings);

                    chunkDictionary.Add(viewedChunkCoord, newChunk);
                    visibleChunks.Add(newChunk);

                    bool isBase = keepBaseChunkLoadedAtRuntime && (viewedChunkCoord == baseChunkCoord);
                    newChunk.UpdateChunkVisibility(targetPos, maxViewDistance, isBase);
                }
            }
        }
    }
}