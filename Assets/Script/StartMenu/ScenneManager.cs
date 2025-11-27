using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenneManager : MonoBehaviour
{
    public void ToMainMenu() => SceneManager.LoadScene("MainMenu");
    public void ToGame()     => SceneManager.LoadScene("Game");
    public void ToSettings() => SceneManager.LoadScene("Settings");

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}