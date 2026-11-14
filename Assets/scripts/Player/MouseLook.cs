
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    [Header("Settings (Aimlabs Raw Input)")]
    [Tooltip("Adjust sensitivity here. Try a low value, e.g., between 0.1 and 0.8.")]
    public float sensitivity = 0.4f;
    public Transform playerBody;

    private float xRotation = 0f;
    private float yRotation = 0f;

    private bool wasPaused = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerBody == null && transform.parent != null)
        {
            playerBody = transform.parent;
        }

        Vector3 currentRotation = transform.localEulerAngles;

        xRotation = Mathf.DeltaAngle(0f, currentRotation.x);
        yRotation = currentRotation.y;
    }

    void Update()
    {
        // Stop camera movement when paused
        if (Time.timeScale == 0f)
        {
            wasPaused = true;
            return;
        }

        // Ignore mouse movement right after unpausing
        // This stops the camera from jumping
        if (wasPaused)
        {
            wasPaused = false;
            return;
        }

        if (Mouse.current == null)
        {
            return;
        }

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = mouseDelta.x * sensitivity;
        float mouseY = mouseDelta.y * sensitivity;

        // Move the camera up and down
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -89f, 89f);

        // Move the camera left and right
        yRotation += mouseX;

        // Rotate the camera without moving the player
        transform.localRotation =
            Quaternion.Euler(xRotation, yRotation, 0f);
    }
}
