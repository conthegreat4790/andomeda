using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ChunkManager : MonoBehaviour
{
    [Header("Core Setup")]
    public Transform player;
    public Material terrainMaterial;
    public int chunkSize = 64; // Size of each chunk in Unity world units
    public float maxViewDistance = 250f;

    [Header("Base Chunk Setup")]
    [Tooltip("If true, chunk (0,0) will auto-generate and update live in the Editor so you can preview noise settings.")]
    public bool previewBaseChunkInEditor = true;
    public bool keepBaseChunkLoadedAtRuntime = true;
    public Vector2 baseChunkCoord = Vector2.zero;

    [Header("Terrain Generation Dials")]
    public TerrainChunkSettings terrainSettings;

    private Dictionary<Vector2, TerrainChunk> chunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    private List<TerrainChunk> visibleChunks = new List<TerrainChunk>();
    private Vector2 currentChunkCoord;

    private void OnValidate()
    {
        // Live preview ONLY in Edit mode
        if (!Application.isPlaying && previewBaseChunkInEditor)
        {
            // Delay call to prevent Unity Editor execution loops
            UnityEditor.EditorApplication.delayCall += UpdateEditorBaseChunk;
        }
    }

    private void Start()
    {
        if (Application.isPlaying)
        {
            // Clean out any editor-preview objects before starting runtime generation
            ClearAllChunks();
            UpdateVisibleChunks();
        }
    }

    private void Update()
    {
        // Only run full world streaming during active Play Mode
        if (!Application.isPlaying) return;

        if (player == null) return;

        int currentChunkX = Mathf.FloorToInt(player.position.x / chunkSize);
        int currentChunkZ = Mathf.FloorToInt(player.position.z / chunkSize);

        Vector2 newChunkCoord = new Vector2(currentChunkX, currentChunkZ);

        if (newChunkCoord != currentChunkCoord)
        {
            currentChunkCoord = newChunkCoord;
            UpdateVisibleChunks();
        }
    }

    // --- EDITOR PREVIEW LOGIC ---
    private void UpdateEditorBaseChunk()
    {
        if (this == null || terrainMaterial == null || Application.isPlaying) return;

        // Ensure we only have the base chunk present in the scene hierarchy while editing
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
                DestroyImmediate(child);
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