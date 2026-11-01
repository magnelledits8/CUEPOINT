using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float fallThreshold = -10f;

    // SYNC CORRECTION: Changed from PlayerLaneMovement to PlayerRunController!
    private PlayerRunController laneMove;
    private PlayerJumpAndCrouch jumpCrouch;

    void Awake()
    {
        // Changed to PlayerRunController here as well!
        laneMove = GetComponent<PlayerRunController>();
        jumpCrouch = GetComponent<PlayerJumpAndCrouch>();
    }

    void Update()
    {
        if (transform.position.y < fallThreshold || UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame)
        {
            Respawn();
        }
    }

    public void Respawn()
    {
        if (laneMove != null) laneMove.ResetMovement();
        if (jumpCrouch != null) jumpCrouch.ResetSlideAndJump();

        Debug.Log("Player restarted!");

        if (laneMove != null)
        {
            laneMove.hasStartedRunning = true;
        }
    }
}
