
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinScreenController : MonoBehaviour
{
    [Header("Scenes")]
    public string menuScene = "menu";

    // Remembers which level was completed.
    public static string lastCompletedLevel = "Level1";

    private void Start()
    {
        // Restore normal game time and audio.
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // Show cursor for the results menu.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // RETRY - Restart the completed level.
    public void Retry()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        SceneManager.LoadScene(lastCompletedLevel);
    }

    // MAIN MENU - Return to the menu scene.
    public void MainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(menuScene);
    }
}
