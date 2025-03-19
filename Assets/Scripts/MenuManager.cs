using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public TMP_InputField seedInputField;
    public Button playButton;
    public Button quitButton;

    public NoiseData noiseData;

    void Start()
    {
        playButton.onClick.AddListener(OnPlayButtonClicked);
        quitButton.onClick.AddListener(OnQuitButtonClicked);
    }

    void OnPlayButtonClicked()
    {
        string seedText = seedInputField.text;
        int seed;

        if (int.TryParse(seedText, out seed))
        {
            // Pokud je seed platné èíslo, použijeme ho
        }
        else
        {
            seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        }
        noiseData.seed = seed;

        PlayerPrefs.SetInt("WorldSeed", seed);

        SceneManager.LoadScene("Scene");
    }
    void OnQuitButtonClicked()
    {
        Debug.Log("Hra je ukonèena");
        Application.Quit();
    }
}
