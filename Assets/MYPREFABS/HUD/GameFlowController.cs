using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowController : MonoBehaviour
{
    [Header("Referencias de Juego")]
    public CastleController castle;
    public EnemySpawner enemySpawner;

    [Header("Paneles de UI")]
    public GameObject pausePanel;
    public GameObject defeatPanel;
    public GameObject victoryPanel;

    [Header("Escenas")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;
    private bool defeatTriggered = false;
    private bool victoryTriggered = false;

    void Start()
    {
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (defeatPanel != null)
            defeatPanel.SetActive(false);

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        if (castle == null)
            castle = FindAnyObjectByType<CastleController>();

        if (enemySpawner == null)
            enemySpawner = FindAnyObjectByType<EnemySpawner>();
    }

    void Update()
    {
        if (defeatTriggered || victoryTriggered)
            return;

        if (castle != null && castle.IsDestroyed)
        {
            TriggerDefeat();
            return;
        }

        if (enemySpawner != null && enemySpawner.AllWavesFinished)
        {
            TriggerVictory();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (defeatTriggered || victoryTriggered)
            return;

        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        if (defeatTriggered || victoryTriggered || isPaused)
            return;

        isPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    public void TriggerDefeat()
    {
        if (defeatTriggered || victoryTriggered)
            return;

        defeatTriggered = true;
        isPaused = false;
        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        if (defeatPanel != null)
            defeatPanel.SetActive(true);
    }

    public void TriggerVictory()
    {
        if (victoryTriggered || defeatTriggered)
            return;

        victoryTriggered = true;
        isPaused = false;
        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (defeatPanel != null)
            defeatPanel.SetActive(false);

        if (victoryPanel != null)
            victoryPanel.SetActive(true);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;

        if (string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            Debug.LogError("GameFlowController: no se asignó el nombre de la escena del menú principal.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
        {
            Debug.LogError(
                "GameFlowController: no se puede cargar la escena del menú '" + mainMenuSceneName +
                "'. Revisá Build Profiles / Build Settings y el nombre exacto."
            );
            return;
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }
}