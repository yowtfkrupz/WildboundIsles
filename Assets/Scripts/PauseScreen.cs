using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseScreen : MonoBehaviour
{
    public GameObject pauseMenuUI; // Pøetáhni ruènì do inspektoru!
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

        // Zajistí, že menu je na zaèátku neaktivní
        pauseMenuUI.SetActive(false);

        // Pøiøazení funkcí tlaèítek
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitButtonClicked);
        if (menuButton != null) menuButton.onClick.AddListener(OnMenuButtonClicked);

        // Najdeme hráèe
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
        pauseMenuUI.SetActive(true); // Aktivace menu
        Time.timeScale = 0f; // Zastavení èasu

        // Zakáže pohyb hráèe
        if (player != null) player.enabled = false;

        // Zobrazí kurzor
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
        pauseMenuUI.SetActive(false); // Deaktivace menu
        Time.timeScale = 1f; // Obnovení èasu

        // Povolení pohybu hráèe
        if (player != null) player.enabled = true;

        // Skryje kurzor
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
