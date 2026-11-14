
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelFinishTrigger : MonoBehaviour
{
    [Header("Winning Screen Scene")]
    public string winSceneName = "WinningScreen";

    private bool hasFinished = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasFinished)
            return;

        // Check if the player reached the finish
        bool isPlayer = other.CompareTag("Player") ||
                        other.transform.root.CompareTag("Player");

        if (!isPlayer)
            return;

        FinishLevel();
    }

    public void FinishLevel()
    {
        if (hasFinished)
            return;

        hasFinished = true;

        // Save which level was completed
        WinScreenController.lastCompletedLevel =
            SceneManager.GetActiveScene().name;

        Debug.Log("CUEPOINT: Level completed!");

        // Reset the game time and sound
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // Load the winning screen
        SceneManager.LoadScene(winSceneName);
    }
}
