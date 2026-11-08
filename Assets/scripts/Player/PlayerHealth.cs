
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Respawn Settings")]
    public float fallThreshold = -10f;

    private bool isRestarting = false;

    void Update()
    {
        if (isRestarting)
            return;

        // Restart the entire level when pressing R
        if (Keyboard.current != null &&
            Keyboard.current.rKey.wasPressedThisFrame)
        {
            Respawn();
            return;
        }

        // Restart the entire level when falling
        if (transform.position.y < fallThreshold)
        {
            Respawn();
        }
    }

    public void Respawn()
    {
        if (isRestarting)
            return;

        isRestarting = true;

        Debug.Log("PLAYER HEALTH: Restarting entire scene.");

        string currentSceneName =
            SceneManager.GetActiveScene().name;

        SceneManager.LoadScene(currentSceneName);
    }
}
