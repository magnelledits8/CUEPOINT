
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinScreenController : MonoBehaviour
{
    [Header("Scenes")]
    public string menuScene = "menu";

    // Save the last completed level
    public static string lastCompletedLevel = "Level1";

    private void Start()
    {
        // Reset the game time and sound
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // Show the mouse cursor in the win screen
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Restart the level
    public void Retry()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        SceneManager.LoadScene(lastCompletedLevel);
    }

    // Go back to the main menu
    public void MainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(menuScene);
    }
}
