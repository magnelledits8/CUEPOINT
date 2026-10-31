using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float fallThreshold = -10f;

    // SYNKKORREKTION: Ändrat från PlayerLaneMovement till PlayerRunController!
    private PlayerRunController laneMove;
    private PlayerJumpAndCrouch jumpCrouch;

    void Awake()
    {
        // Ändrat till PlayerRunController här med!
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

        Debug.Log("Spelaren startade om!");

        if (laneMove != null)
        {
            laneMove.hasStartedRunning = true;
        }
    }
}
