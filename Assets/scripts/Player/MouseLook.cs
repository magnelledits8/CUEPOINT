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

    void Start()
    {
        // Lock the cursor to the center of the screen and make it invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerBody == null && transform.parent != null)
        {
            playerBody = transform.parent;
        }
    }

    void Update()
    {
        // Reads mouse delta directly from the new Input System (FPS-independent Raw Input)
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = mouseDelta.x * sensitivity;
        float mouseY = mouseDelta.y * sensitivity;

        // 1. STRICT VERTICAL CLAMP (Aimlabs style): Never flips upside down
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -89f, 89f);

        // 2. Horizontal rotation (Look left/right)
        // Applied directly to the camera's transform instead of the body to prevent running sideways!
        yRotation += mouseX;

        // 3. Apply the full rotation stably directly to the camera's transform
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }
}
