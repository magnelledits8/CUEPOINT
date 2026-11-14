using UnityEngine;
using UnityEngine.SceneManagement; // Used to restart the level

public class DeathZone : MonoBehaviour
{
    // Check when something enters the death zone
    private void OnTriggerEnter(Collider other)
    {
        // Check if the player entered the zone
        // The player needs the Player tag
        if (other.CompareTag("Player"))
        {
            RestartLevel();
        }
    }

    private void RestartLevel()
    {
        // Get the current scene name
        string currentSceneName = SceneManager.GetActiveScene().name;

        // Restart the level and music
        SceneManager.LoadScene(currentSceneName);
    }
}
