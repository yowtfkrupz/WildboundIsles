using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public TMP_InputField seedInputField;
    public Button playButton;
    public Button settingsButton;
    public Button quitButton;

    public NoiseData noiseData; // Pøidáno pro pøístup k NoiseData

    void Start()
    {
        playButton.onClick.AddListener(OnPlayButtonClicked);
        settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        quitButton.onClick.AddListener(OnQuitButtonClicked);
    }

    void OnPlayButtonClicked()
    {
        // Získání textu z TMP input field (seed)
        string seedText = seedInputField.text;
        int seed;

        // Pokusíme se pøevést vstupní text na èíslo
        if (int.TryParse(seedText, out seed))
        {
            // Pokud je seed platné èíslo, použijeme ho
        }
        else
        {
            // Pokud je seed prázdný nebo neplatný, vygeneruj náhodný seed
            seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        }

        // Ulož seed do NoiseData
        noiseData.seed = seed;

        // Ulož seed do PlayerPrefs (pokud ho chceš uchovat pro další bìh hry)
        PlayerPrefs.SetInt("WorldSeed", seed);

        // Naèti herní scénu (zmìò "Scene" na název své scény)
        SceneManager.LoadScene("Scene");
    }

    void OnSettingsButtonClicked()
    {
        Debug.Log("Otevøeno nastavení (zatím bez funkce)");
    }

    void OnQuitButtonClicked()
    {
        Debug.Log("Hra je ukonèena");
        Application.Quit();
    }
}
