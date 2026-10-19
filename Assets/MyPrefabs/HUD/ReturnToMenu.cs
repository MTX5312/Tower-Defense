using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToMenu : MonoBehaviour
{
    [SerializeField] private string menuSceneName = "MainMenu";

    public void VolverAlMenu()
    {
        if (string.IsNullOrWhiteSpace(menuSceneName))
        {
            Debug.LogError("No se asignó el nombre de la escena del menú.");
            return;
        }

        Time.timeScale = 1f;

        if (!Application.CanStreamedLevelBeLoaded(menuSceneName))
        {
            Debug.LogError(
                "No se puede cargar la escena: " + menuSceneName +
                ". Revisá Build Profiles y el nombre de la escena."
            );
            return;
        }

        SceneManager.LoadScene(menuSceneName);
    }
}