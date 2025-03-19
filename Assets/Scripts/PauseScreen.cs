using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseScreen : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public Button quitButton;
    public Button menuButton;

    private bool isPaused = false;
    private Player player;

    void Start()
    {
        if (pauseMenuUI == null)
        {
            Debug.LogError("pauseMenuUI není pøiøazeno! Pøiøaï ruènì v inspektoru.");
            return;
        }

        pauseMenuUI.SetActive(false);

        if (quitButton != null) quitButton.onClick.AddListener(OnQuitButtonClicked);
        if (menuButton != null) menuButton.onClick.AddListener(OnMenuButtonClicked);

        player = FindObjectOfType<Player>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC stisknuto! isPaused: " + isPaused);
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    void PauseGame()
    {
        Debug.Log("PauseGame() called");

        if (pauseMenuUI == null)
        {
            Debug.LogError("pauseMenuUI je NULL! Zkontroluj inspektor.");
            return;
        }

        isPaused = true;
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;

        if (player != null) player.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void ResumeGame()
    {
        Debug.Log("ResumeGame() called");

        if (pauseMenuUI == null)
        {
            Debug.LogError("pauseMenuUI je NULL! Zkontroluj inspektor.");
            return;
        }

        isPaused = false;
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;

        if (player != null) player.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnQuitButtonClicked()
    {
        Debug.Log("Hra ukonèena.");
        Application.Quit();
    }

    void OnMenuButtonClicked()
    {
        Debug.Log("Pøechod do menu.");
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
}
