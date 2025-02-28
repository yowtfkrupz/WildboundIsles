using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [Header("Objects")]
    public GameObject[] natureObjects;  // Přírodní objekty (stromy, rudy)
    public GameObject[] otherObjects;   // Ostatní objekty (obelisky, bedny)
    public float natureObjectChance = 0.8f;  // Pravděpodobnost generování přírodních objektů
    public float otherObjectChance = 0.05f;  // Pravděpodobnost generování jiných objektů

    [Header("Player")]
    [SerializeField] private Transform playerTransform;  // Transform hráče, který bude spawnovat objekty v jeho okolí

    [Header("Stone")]
    [SerializeField] private GameObject stonePrefab;  // Prefab kamene, který chceme spawnovat
    [SerializeField] private float stoneSpawnRadius = 10f;  // Radius pro spawn kamene kolem hráče

    [Header("Map Generation")]
    [SerializeField] private GameObject meshObject;  // Objekt obsahující mesh pro generování
    private GameObject resourcesContainer;

    public TerrainData terrainData;
    public NoiseData noiseData;
    public TextureData textureData;
    public Material terrainMaterial;

    [Range(0, 6)]
    public int editorPreviewLOD;

    public Texture2D spawnTexture;
    public float spawnProbabilityThreshold = 0.3f; // Sníženo pro rozptýlené shluky
    public float clusteringFactor = 0.15f; // Pravděpodobnost, že objekt bude vytvořen blízko jiného

    public bool autoUpdate;
    private float[,] falloffMap;

    void Start()
    {
        DrawMapInEditor();
        GenerateObjects();
    }

    public int mapChunkSize
    {
        get { return terrainData.useFlatShading ? 95 : 239; }
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
        // Vytvoř nový kontejner pro objekty
        if (resourcesContainer != null)
        {
            DestroyImmediate(resourcesContainer);  // Zničení starého kontejneru
        }
        resourcesContainer = new GameObject("ResourcesContainer");

        // Generování objektů (přírodní a jiné objekty)
        GenerateObjectsOnTerrain();

        // Spawn kamene v okolí hráče
        SpawnStoneNearPlayer();

        // Přidání MeshCollider a NavMesh pro terén
        AddMeshColliderToTerrain();
    }

    // Generování objektů na základě terénu
    private void GenerateObjectsOnTerrain()
    {
        float minDistanceForNatureObjects = 5f;
        float minDistanceForOtherObjects = 15f;

        List<Vector3> spawnedPositions = new List<Vector3>();  // Seznam pozic pro již vygenerované objekty
        int natureObjectsSpawned = 0;
        int otherObjectsSpawned = 0;

        int maxObjects = 500;
        int maxAttempts = 5000;

        for (int attempt = 0; attempt < maxAttempts && (natureObjectsSpawned + otherObjectsSpawned) < maxObjects; attempt++)
        {
            // Náhodně vyber vrchol
            MeshFilter meshFilter = meshObject.GetComponent<MeshFilter>();
            Mesh mesh = meshFilter.sharedMesh;
            int randomIndex = Random.Range(0, mesh.vertices.Length);
            Vector3 worldPosition = meshObject.transform.TransformPoint(mesh.vertices[randomIndex]);

            // Normalizuj výšku podle rozsahu
            float normalizedHeight = Mathf.InverseLerp(0f, 100f, worldPosition.y);

            if (normalizedHeight > 0.2f)  // Pokud je výška vhodná
            {
                GameObject objectToSpawn = null;

                if (otherObjectsSpawned < maxObjects && Random.Range(0f, 1f) < otherObjectChance)
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
                    // Generování náhodné rotace kolem osy Y
                    float randomYRotation = Random.Range(0f, 360f);  // Náhodná rotace
                    Quaternion randomRotation = Quaternion.Euler(0f, randomYRotation, 0f);  // Rotace pouze na ose Y

                    GameObject newObj = Instantiate(objectToSpawn, worldPosition, randomRotation);
                    newObj.transform.parent = resourcesContainer.transform;
                    spawnedPositions.Add(worldPosition);  // Přidáme pozici do seznamu
                }
            }
        }

        Debug.Log($"Generováno {natureObjectsSpawned} přírodních objektů a {otherObjectsSpawned} jiných objektů.");
    }

    // Kontrola, zda je pozice dostatečně vzdálená od ostatních
    private bool IsPositionFarEnough(Vector3 position, List<Vector3> existingPositions, float minDistance)
    {
        foreach (Vector3 existingPosition in existingPositions)
        {
            if (Vector3.Distance(position, existingPosition) < minDistance)
            {
                return false;  // Příliš blízko jiného objektu
            }
        }
        return true;  // Pozice je dostatečně vzdálená
    }

    // Spawn kamene poblíž hráče
    private void SpawnStoneNearPlayer()
    {
        if (stonePrefab != null && playerTransform != null)
        {
            // Náhodná pozice kolem hráče (v okruhu kolem hráče)
            Vector3 randomPosition = playerTransform.position + new Vector3(Random.Range(-stoneSpawnRadius, stoneSpawnRadius), 0f, Random.Range(-stoneSpawnRadius, stoneSpawnRadius));

            // Zajištění, že kámen spawnuje na zemi (správná výška)
            randomPosition.y = GetTerrainHeightAtPosition(randomPosition);  // Nastaví správnou výšku podle terénu

            // Spawnuje kámen
            GameObject stone = Instantiate(stonePrefab, randomPosition, Quaternion.identity);
            stone.transform.parent = resourcesContainer.transform;  // Nastavení rodiče
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
            return hit.point.y;  // Vrátí výšku povrchu, na který narazil raycast
        }
        return position.y;  // Pokud žádný povrch nebyl zasažen, vrátí původní výšku
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
