using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Escena del Nivel")]
    [SerializeField] private string levelSceneName = "Level1";

    public void StartGame()
    {
        if (string.IsNullOrWhiteSpace(levelSceneName))
        {
            Debug.LogError("MainMenuController: no se asignó el nombre de la escena del nivel.");
            return;
        }

        Time.timeScale = 1f;

        if (!Application.CanStreamedLevelBeLoaded(levelSceneName))
        {
            Debug.LogError(
                "MainMenuController: no se puede cargar la escena '" + levelSceneName +
                "'. Revisá Build Profiles / Build Settings y el nombre exacto."
            );
            return;
        }

        SceneManager.LoadScene(levelSceneName);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Debug.Log("MainMenuController: saliendo de la aplicación...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}