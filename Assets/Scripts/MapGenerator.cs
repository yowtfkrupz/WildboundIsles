using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine.AI;

public class MapGenerator : MonoBehaviour
{
    [Header("Objects")]
    public GameObject[] natureObjects;
    public GameObject[] otherObjects;
    public GameObject portalObject;
    public float natureObjectChance = 0.8f;
    public float otherObjectChance = 0.05f;

    [Header("Player")]
    [SerializeField] private Transform playerTransform;

    [Header("Stone")]
    [SerializeField] private GameObject stonePrefab;
    [SerializeField] private float stoneSpawnRadius = 10f;

    [Header("Map Generation")]
    [SerializeField] private GameObject meshObject;
    private GameObject resourcesContainer;

    public TerrainData terrainData;
    public NoiseData noiseData;
    public TextureData textureData;
    public Material terrainMaterial;

    [Range(0, 6)]
    public int editorPreviewLOD;

    private NavMeshSurface navMeshSurface;

    public Texture2D spawnTexture;
    public float spawnProbabilityThreshold = 0.3f;
    public float clusteringFactor = 0.15f;

    public bool autoUpdate;
    private float[,] falloffMap;

    void Start()
    {
        DrawMapInEditor();
        GenerateObjects();
        textureData.ApplyToMaterial(terrainMaterial);

        UpdateMeshCollider();
        GenerateNavMesh();
    }

    public int mapChunkSize
    {
        get { return terrainData.useFlatShading ? 95 : 239; }
    }
    private void UpdateMeshCollider()
    {

        MeshCollider existingCollider = meshObject.GetComponent<MeshCollider>();
        if (existingCollider != null)
        {
            Destroy(existingCollider);
        }

        MeshCollider newCollider = meshObject.AddComponent<MeshCollider>();
        newCollider.sharedMesh = meshObject.GetComponent<MeshFilter>().sharedMesh;

        Debug.Log("MeshCollider aktualizován.");
    }
    private void GenerateNavMesh()
    {
        navMeshSurface = meshObject.GetComponent<NavMeshSurface>();
        if (navMeshSurface == null)
        {
            navMeshSurface = meshObject.AddComponent<NavMeshSurface>();
        }

        navMeshSurface.collectObjects = CollectObjects.Children;
        navMeshSurface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
        navMeshSurface.BuildNavMesh();
        Debug.Log("NavMesh vygenerován.");
    }
    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData(Vector2.zero);
        MapDisplay display = FindObjectOfType<MapDisplay>();
        display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, editorPreviewLOD, terrainData.useFlatShading));
    }

    public MapData GenerateMapData(Vector2 centre)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, noiseData.seed, noiseData.noiseScale, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, centre + noiseData.offset, noiseData.normalizeMode);

        if (terrainData.useFalloff)
        {
            if (falloffMap == null)
                falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize + 2);

            for (int y = 0; y < mapChunkSize + 2; y++)
            {
                for (int x = 0; x < mapChunkSize + 2; x++)
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
            }
        }

        textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);
        return new MapData(noiseMap);
    }

    public void GenerateObjects()
    {
        if (resourcesContainer != null)
        {
            DestroyImmediate(resourcesContainer);
        }
        resourcesContainer = new GameObject("ResourcesContainer");


        GenerateObjectsOnTerrain();
        SpawnStoneNearPlayer();
        AddMeshColliderToTerrain();
    }
    private void GenerateObjectsOnTerrain()
    {
        float minDistanceForNatureObjects = 1f;
        float minDistanceForOtherObjects = 10f;
        float minDistanceForPortalObject = 20f;
        List<Vector3> spawnedPositions = new List<Vector3>();
        int natureObjectsSpawned = 0;
        int otherObjectsSpawned = 0;
        bool portalSpawned = false;

        int maxObjects = 1500;
        int maxAttempts = 5000;

        for (int attempt = 0; attempt < maxAttempts && (natureObjectsSpawned + otherObjectsSpawned) < maxObjects; attempt++)
        {
            MeshFilter meshFilter = meshObject.GetComponent<MeshFilter>();
            Mesh mesh = meshFilter.sharedMesh;
            int randomIndex = Random.Range(0, mesh.vertices.Length);
            Vector3 worldPosition = meshObject.transform.TransformPoint(mesh.vertices[randomIndex]);

            float normalizedHeight = Mathf.InverseLerp(0f, 100f, worldPosition.y);

            if (normalizedHeight > 0.1f)
            {
                GameObject objectToSpawn = null;

                if (!portalSpawned)
                {
                    if (IsPositionFarEnough(worldPosition, spawnedPositions, minDistanceForPortalObject))
                    {
                        objectToSpawn = portalObject;
                        portalSpawned = true;
                        Debug.Log($"Portál generován na: {worldPosition}");
                    }
                }
                else if (otherObjectsSpawned < maxObjects && Random.Range(0f, 1f) < otherObjectChance)
                {
                    if (IsPositionFarEnough(worldPosition, spawnedPositions, minDistanceForOtherObjects))
                    {
                        objectToSpawn = otherObjects[Random.Range(0, otherObjects.Length)];
                        otherObjectsSpawned++;
                    }
                }
                else if (natureObjectsSpawned < maxObjects)
                {
                    if (IsPositionFarEnough(worldPosition, spawnedPositions, minDistanceForNatureObjects))
                    {
                        objectToSpawn = natureObjects[Random.Range(0, natureObjects.Length)];
                        natureObjectsSpawned++;
                    }
                }

                if (objectToSpawn != null)
                {
                    float randomYRotation = Random.Range(0f, 360f);
                    Quaternion randomRotation = Quaternion.Euler(0f, randomYRotation, 0f);

                    GameObject newObj = Instantiate(objectToSpawn, worldPosition, randomRotation);
                    newObj.transform.parent = resourcesContainer.transform;
                    spawnedPositions.Add(worldPosition);
                }
            }
        }

        Debug.Log($"Generováno {natureObjectsSpawned} přírodních objektů a {otherObjectsSpawned} jiných objektů.");
    }
    private bool IsPositionFarEnough(Vector3 position, List<Vector3> existingPositions, float minDistance)
    {
        foreach (Vector3 existingPosition in existingPositions)
        {
            if (Vector3.Distance(position, existingPosition) < minDistance)
            {
                return false;
            }
        }
        return true;
    }
    private void SpawnStoneNearPlayer()
    {
        if (stonePrefab != null && playerTransform != null)
        {
            Vector3 randomPosition = playerTransform.position + new Vector3(Random.Range(-stoneSpawnRadius, stoneSpawnRadius), 0f, Random.Range(-stoneSpawnRadius, stoneSpawnRadius));

            randomPosition.y = GetTerrainHeightAtPosition(randomPosition);

            GameObject stone = Instantiate(stonePrefab, randomPosition, Quaternion.identity);
            stone.transform.parent = resourcesContainer.transform;
            Debug.Log($"Kámen spawnuje na pozici: {randomPosition}");
        }
        else
        {
            Debug.LogError("Prefab kamene nebo transform hráče není přiřazen v Inspectoru!");
        }
    }

    private void AddMeshColliderToTerrain()
    {
        MeshCollider meshCollider = meshObject.GetComponent<MeshCollider>();
        if (meshCollider == null)
        {
            meshCollider = meshObject.AddComponent<MeshCollider>();
        }
        meshCollider.sharedMesh = meshObject.GetComponent<MeshFilter>().sharedMesh;
        Debug.Log("MeshCollider přidán k terénu.");
    }

    private float GetTerrainHeightAtPosition(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up * 100f, Vector3.down, out hit, Mathf.Infinity))
        {
            return hit.point.y;
        }
        return position.y;
    }

    void OnValidate()
    {
        if (terrainData != null)
        {
            terrainData.OnValuesUpdated -= OnValuesUpdated;
            terrainData.OnValuesUpdated += OnValuesUpdated;
        }
        if (noiseData != null)
        {
            noiseData.OnValuesUpdated -= OnValuesUpdated;
            noiseData.OnValuesUpdated += OnValuesUpdated;
        }
        if (textureData != null)
        {
            textureData.OnValuesUpdated -= OnTextureValuesUpdated;
            textureData.OnValuesUpdated += OnTextureValuesUpdated;
        }
    }

    void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }

    void OnTextureValuesUpdated()
    {
        textureData.ApplyToMaterial(terrainMaterial);
    }
}

public struct MapData
{
    public readonly float[,] heightMap;
    public MapData(float[,] heightMap)
    {
        this.heightMap = heightMap;
    }
}
