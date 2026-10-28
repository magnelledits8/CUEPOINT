using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float fallThreshold = -10f;

    private PlayerLaneMovement laneMove;
    private PlayerJumpAndCrouch jumpCrouch;
    private PlayerCinematicIntro intro;

    void Awake()
    {
        laneMove = GetComponent<PlayerLaneMovement>();
        jumpCrouch = GetComponent<PlayerJumpAndCrouch>();
        intro = GetComponent<PlayerCinematicIntro>();
    }

    void Update()
    {
        // Kollar om spelaren faller eller om man trycker R för att starta om manuellt
        if (transform.position.y < fallThreshold || UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame)
        {
            Respawn();
        }
    }

    public void Respawn()
    {
        if (laneMove != null) laneMove.ResetMovement();
        if (jumpCrouch != null) jumpCrouch.ResetSlideAndJump();
        if (intro != null) intro.ResetCameraPos();

        Debug.Log("Spelaren startade om via det uppdelade systemet!");

        // Alternativ A: Starta om löpningen direkt (Skönast flyt så man slipper introt varje gång man dör)
        if (laneMove != null)
        {
            laneMove.hasStartedRunning = true;
        }

        // Alternativ B: Om du hellre vill köra igång introt/hoppet igen vid VARJE respawn, 
        // ta bort de tre raderna ovanför och bocka ur de två raderna nedanför istället:
        // if (intro != null) intro.TriggerStartSequence();
    }
}
