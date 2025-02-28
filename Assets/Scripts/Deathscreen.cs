using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Deathscreen : MonoBehaviour
{
    public Button quitButton;
    public Button menuButton;

    // Start is called before the first frame update
    void Start()
    {
        quitButton.onClick.AddListener(OnQuitButtonClicked);
        menuButton.onClick.AddListener(OnMenuButtonClicked);
    }

    void OnQuitButtonClicked()
    {
        Debug.Log("Hra je ukonèena");
        Application.Quit();
    }

    void OnMenuButtonClicked()
    {
        Debug.Log("Hráè odešel do menu!");
        SceneManager.LoadScene("Menu");
    }
}
