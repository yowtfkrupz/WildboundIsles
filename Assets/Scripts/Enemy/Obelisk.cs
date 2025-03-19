using UnityEngine;
using TMPro;

public class Obelisk : MonoBehaviour
{
    public Material glowingRunesMaterial;
    public Material glowingBottomMaterial;
    public GameObject[] bosses;
    public Transform spawnPosition;
    public GameObject runes;
    public GameObject obeliskBottom;
    private bool bossSpawned = false;

    public TextMeshProUGUI textMeshProUGUI;
    public Canvas canvas;
    public float textDistanceFromObelisk = 2f;
    public float maxTextAlphaDistance = 8f;
    public float minTextAlpha = 0f;
    public float maxTextAlpha = 1f;

    private void Start()
    {
        if (textMeshProUGUI != null)
        {
            textMeshProUGUI.text = "Press E to summon The Lich";
        }
    }

    private void Update()
    {
        if (textMeshProUGUI != null && !bossSpawned)
        {
            UpdateTextPositionAndAlpha();
        }

        if (Vector3.Distance(transform.position, Camera.main.transform.position) < 3f && !bossSpawned && Input.GetKeyDown(KeyCode.E))
        {
            SpawnBoss();
        }
    }

    private void UpdateTextPositionAndAlpha()
    {
        Vector3 directionToPlayer = Camera.main.transform.position - transform.position;
        directionToPlayer.y = 0;

        Vector3 textPosition = transform.position + directionToPlayer.normalized * textDistanceFromObelisk;
        textMeshProUGUI.transform.position = textPosition;

        textMeshProUGUI.transform.LookAt(Camera.main.transform.position);
        textMeshProUGUI.transform.rotation = Quaternion.LookRotation(textMeshProUGUI.transform.position - Camera.main.transform.position);

        float distanceToPlayer = Vector3.Distance(transform.position, Camera.main.transform.position);
        float alphaValue = Mathf.Clamp01(1 - (distanceToPlayer / maxTextAlphaDistance));

        Color currentColor = textMeshProUGUI.color;
        currentColor.a = Mathf.Lerp(minTextAlpha, maxTextAlpha, alphaValue);
        textMeshProUGUI.color = currentColor;
    }

    private void SpawnBoss()
    {
        if (runes != null)
        {
            var renderer = runes.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = glowingRunesMaterial;
            }
        }

        if (obeliskBottom != null)
        {
            var bottomRenderer = obeliskBottom.GetComponent<Renderer>();
            if (bottomRenderer != null)
            {
                bottomRenderer.material = glowingBottomMaterial;
            }
        }

        int randomIndex = Random.Range(0, bosses.Length);

        Vector3 spawnLocation = spawnPosition != null ? spawnPosition.position : transform.position + transform.forward * 5f;
        GameObject boss = Instantiate(bosses[randomIndex], spawnLocation, Quaternion.identity);

        bossSpawned = true;

        if (textMeshProUGUI != null)
        {
            textMeshProUGUI.gameObject.SetActive(false);
        }
        var musicManager = FindObjectOfType<MusicManager>();
        if (musicManager != null)
        {
            Debug.Log("MusicManager nalezen, pokus o pøepnutí hudby.");
            musicManager.ActivateBossMusic(boss);
        }
        else
        {
            Debug.LogError("MusicManager nenalezen ve scénì.");
        }
    }
}
