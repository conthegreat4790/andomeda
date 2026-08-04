using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ChunkManager : MonoBehaviour
{
    [Header("Core Setup")]
    [SerializeField] private Transform player;
    [SerializeField] private int chunkSize = 64;
    [SerializeField] private float maxViewDistance = 250f;

    [Header("Base Chunk Setup")]
    [SerializeField] private bool previewBaseChunkInEditor = true;
    [SerializeField] private bool keepBaseChunkLoadedAtRuntime = true;
    [SerializeField] private Vector2 baseChunkCoord = Vector2.zero;

    [Header("Terrain Presets (Private)")]
    [SerializeField] private TerrainChunkSettings terrainPreset1 = new TerrainChunkSettings();
    [SerializeField] private TerrainChunkSettings terrainPreset2 = new TerrainChunkSettings();
    [SerializeField] private TerrainChunkSettings terrainPreset3 = new TerrainChunkSettings();

    [Header("Active Selection")]
    [Tooltip("Select which preset to build: 1, 2, or 3")]
    [Range(1, 3)]
    [SerializeField] private int activeTerrainType = 1;

    private bool isInitialized = false;

    private Dictionary<Vector2, TerrainChunk> chunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    private List<TerrainChunk> visibleChunks = new List<TerrainChunk>();
    private Vector2 currentChunkCoord;

    public TerrainChunkSettings GetActiveSettings()
    {
        switch (activeTerrainType)
        {
            case 1: return terrainPreset1;
            case 2: return terrainPreset2;
            case 3: return terrainPreset3;
            default: return terrainPreset1;
        }
    }

    private void OnValidate()
    {
        if (!Application.isPlaying && previewBaseChunkInEditor)
        {
            UnityEditor.EditorApplication.delayCall += UpdateEditorBaseChunk;
        }
    }

    private void Start()
    {
        if (Application.isPlaying)
        {
            ClearAllChunks();
            isInitialized = false;
        }
    }

    private void Update()
    {
        if (!Application.isPlaying) return;
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

    public void StartGeneratingTerrain()
    {
        TerrainChunkSettings activeSettings = GetActiveSettings();
        if (activeSettings.terrainMaterial == null)
        {
            Debug.LogError($"ChunkManager: Terrain Material is missing on Preset {activeTerrainType}!");
            return;
        }

        ClearAllChunks();

        if (player != null)
        {
            int currentChunkX = Mathf.FloorToInt(player.position.x / chunkSize);
            int currentChunkZ = Mathf.FloorToInt(player.position.z / chunkSize);
            currentChunkCoord = new Vector2(currentChunkX, currentChunkZ);
        }
        else
        {
            currentChunkCoord = baseChunkCoord;
        }

        isInitialized = true;
        UpdateVisibleChunks();
    }

    public void SetActiveTerrainType(int typeIndex)
    {
        activeTerrainType = Mathf.Clamp(typeIndex, 1, 3);
        if (isInitialized)
        {
            StartGeneratingTerrain();
        }
    }

    public void StopGeneratingTerrain()
    {
        isInitialized = false;
        ClearAllChunks();
    }

    // --- EDITOR PREVIEW LOGIC ---
    private void UpdateEditorBaseChunk()
    {
        if (this == null || Application.isPlaying) return;

        TerrainChunkSettings activeSettings = GetActiveSettings();
        if (activeSettings.terrainMaterial == null) return;

        TerrainChunk baseChunk = GetComponentInChildren<TerrainChunk>();

        if (baseChunk == null)
        {
            GameObject chunkObj = new GameObject($"Chunk_{baseChunkCoord.x}_{baseChunkCoord.y} (Editor Preview)");
            chunkObj.transform.parent = transform;

            baseChunk = chunkObj.AddComponent<TerrainChunk>();
            baseChunk.Initialize(baseChunkCoord, chunkSize, activeSettings);
        }
        else
        {
            baseChunk.RebuildMesh(activeSettings);
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
        TerrainChunkSettings activeSettings = GetActiveSettings();
        if (activeSettings.terrainMaterial == null) return;

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
                    newChunk.Initialize(viewedChunkCoord, chunkSize, activeSettings);

                    chunkDictionary.Add(viewedChunkCoord, newChunk);
                    visibleChunks.Add(newChunk);

                    bool isBase = keepBaseChunkLoadedAtRuntime && (viewedChunkCoord == baseChunkCoord);
                    newChunk.UpdateChunkVisibility(targetPos, maxViewDistance, isBase);
                }
            }
        }
    }
}