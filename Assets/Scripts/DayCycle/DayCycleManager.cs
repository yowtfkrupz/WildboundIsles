using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DayCycleManager : MonoBehaviour
{
    [SerializeField] private Light DirectionalLight;
    [SerializeField] private DayCyclePreset Preset;

    [SerializeField, Range(0, 24)] private float TimeOfDay;
    [SerializeField, Range(0.1f, 5f)] private float speedMultiplier;

    [SerializeField] private float maxIntensity = 1.5f;
    private float baseIntensity = 0f;

    [SerializeField] private float maxShadowStrength = 1f;
    [SerializeField] private float minShadowStrength = 0.2f;

    private float dawn = 6f;
    private float dusk = 18f;
    private float noon = 12f;

    [Header("Enemy Spawn Settings")]
    [SerializeField] private GameObject[] enemies;
    [SerializeField] private int maxEnemies = 10;
    [SerializeField] private float spawnRadius = 50f;
    [SerializeField] private Transform mapCenter;

    private List<GameObject> spawnedEnemies = new List<GameObject>();

    public TextMeshProUGUI timeText;

    private void Start()
    {
        baseIntensity = maxIntensity / 2f;
        TimeOfDay = 12f;
    }

    private void Update()
    {
        UpdateTime();
        UpdateLighting(TimeOfDay / 24f);
        SpawnEnemiesAtNight();
    }

    private void UpdateTime()
    {
        // Pøevod èasu na hodiny a minuty
        int hours = Mathf.FloorToInt(TimeOfDay);
        int minutes = Mathf.FloorToInt((TimeOfDay - hours) * 60);
        timeText.text = $"{hours:00}:{minutes:00}";

        if (Application.isPlaying)
        {
            TimeOfDay += Time.deltaTime * speedMultiplier;
            TimeOfDay %= 24;
        }

        AdjustLightIntensity();
    }

    private void AdjustLightIntensity()
    {
        if (TimeOfDay >= dawn && TimeOfDay <= noon)
        {
            DirectionalLight.intensity = Mathf.Lerp(baseIntensity, maxIntensity, (TimeOfDay - dawn) / (noon - dawn));
            DirectionalLight.shadowStrength = Mathf.Lerp(minShadowStrength, maxShadowStrength, (TimeOfDay - dawn) / (noon - dawn));
        }
        else if (TimeOfDay > noon && TimeOfDay <= dusk)
        {
            DirectionalLight.intensity = Mathf.Lerp(maxIntensity, baseIntensity, (TimeOfDay - noon) / (dusk - noon));
            DirectionalLight.shadowStrength = Mathf.Lerp(maxShadowStrength, minShadowStrength, (TimeOfDay - noon) / (dusk - noon));
        }
        else
        {
            DirectionalLight.intensity = baseIntensity / 2;
            DirectionalLight.shadowStrength = minShadowStrength;
        }
    }

    private void SpawnEnemiesAtNight()
    {
        if (enemies == null || enemies.Length == 0 || mapCenter == null)
            return;

        if (spawnedEnemies.Count >= maxEnemies || TimeOfDay >= dawn && TimeOfDay <= dusk)
            return;

        GameObject enemyToSpawn = enemies[Random.Range(0, enemies.Length)];

        Vector3 randomPosition = mapCenter.position + new Vector3(
            Random.Range(-spawnRadius, spawnRadius),
            0,
            Random.Range(-spawnRadius, spawnRadius)
        );

        randomPosition.y = GetTerrainHeightAtPosition(randomPosition);

        GameObject spawnedEnemy = Instantiate(enemyToSpawn, randomPosition, Quaternion.identity);
        spawnedEnemies.Add(spawnedEnemy);
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

    private void UpdateLighting(float timePercent)
    {
        RenderSettings.ambientLight = Preset.AmbientColor.Evaluate(timePercent);
        RenderSettings.fogColor = Preset.FogColor.Evaluate(timePercent);

        if (DirectionalLight != null)
        {
            DirectionalLight.color = Preset.DirectionalColor.Evaluate(timePercent);
            DirectionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, 170f, 0));
        }
    }

    private void OnValidate()
    {
        if (DirectionalLight != null)
            return;

        if (RenderSettings.sun != null)
        {
            DirectionalLight = RenderSettings.sun;
        }
        else
        {
            Light[] lights = GameObject.FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    DirectionalLight = light;
                    return;
                }
            }
        }
    }
}
