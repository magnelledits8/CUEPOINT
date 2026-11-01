using UnityEngine;
using UnityEngine.SceneManagement; // Required to reload the scene

public class DeathZone : MonoBehaviour
{
    // This function runs automatically when something touches our invisible trigger box
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that fell down is actually the player
        // (Make sure your player has the "Player" tag at the top of the Inspector!)
        if (other.CompareTag("Player"))
        {
            RestartLevel();
        }
    }

    private void RestartLevel()
    {
        // Gets the name of the current active scene
        string currentSceneName = SceneManager.GetActiveScene().name;

        // Reloads the entire scene from second zero (restarts gameplay and music)
        SceneManager.LoadScene(currentSceneName);
    }
}
