
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

        // Only the player can finish the level.
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

        // Remember the completed level for Retry.
        WinScreenController.lastCompletedLevel =
            SceneManager.GetActiveScene().name;

        Debug.Log("CUEPOINT: Level completed!");

        // Restore normal game time and audio.
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // Open WinningScreen.
        SceneManager.LoadScene(winSceneName);
    }
}
