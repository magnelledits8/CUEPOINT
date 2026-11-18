
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    [Header("Settings (Aimlabs Raw Input)")]
    [Tooltip("Mouse sensitivity controlled by the settings slider.")]
    public float sensitivity = 0.04f;

    public Transform playerBody;

    [Header("Sensitivity Range")]
    [SerializeField] private float minSensitivity = 0.004f;
    [SerializeField] private float maxSensitivity = 0.165f;

    private float xRotation = 0f;
    private float yRotation = 0f;

    private bool wasPaused = false;

    private void OnEnable()
    {
        // Listen for sensitivity changes from all settings menus.
        SliderScript.ValueChanged += UpdateSensitivity;

        // Load the saved slider value.
        UpdateSensitivity(SliderScript.CurrentValue);
    }

    private void OnDisable()
    {
        // Stop listening when this component is disabled.
        SliderScript.ValueChanged -= UpdateSensitivity;
    }

    private void Start()
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

    private void Update()
    {
        // Stop camera movement while paused.
        if (Time.timeScale == 0f)
        {
            wasPaused = true;
            return;
        }

        // Ignore the first mouse movement after unpausing.
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

        // Vertical camera rotation.
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -89f, 89f);

        // Horizontal camera rotation.
        yRotation += mouseX;

        transform.localRotation =
            Quaternion.Euler(xRotation, yRotation, 0f);
    }

    private void UpdateSensitivity(int sliderValue)
    {
        // Convert slider value from 0-100 to 0-1.
        float normalizedValue = Mathf.Clamp01(sliderValue / 100f);

        // Increase sensitivity more slowly at lower values.
        float adjustedValue = Mathf.Pow(normalizedValue, 1.65f);

        // Apply the sensitivity range.
        sensitivity = Mathf.Lerp(
            minSensitivity,
            maxSensitivity,
            adjustedValue
        );
    }
}
