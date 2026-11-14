
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("ClickSound")]
    public AudioClip clickSound;

    // Load the selected scene
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Start the game
    public void PlayGame()
    {
        PlayClickSound();
        LoadScene("Level1");
    }

    // Open the main menu
    public void OpenMenu()
    {
        PlayClickSound();
        LoadScene("Menu");
    }

    // Close the game
    public void QuitGame()
    {
        PlayClickSound();
        Debug.Log("Game Exit...");
        Application.Quit();
    }

    // Play the button click sound
    private void PlayClickSound()
    {
        if (clickSound != null)
        {
            AudioSource.PlayClipAtPoint(
                clickSound,
                Camera.main != null ? Camera.main.transform.position : Vector3.zero
            );
        }
    }
}
