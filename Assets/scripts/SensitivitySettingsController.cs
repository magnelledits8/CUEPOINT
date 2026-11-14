
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SensitivitySettingsController : MonoBehaviour
{
    [Header("Settings UI")]
    public GameObject settingsPanel;
    public Slider sensitivitySlider;
    public TMP_Text sensitivityText;

    [Header("Main Menu (Optional)")]
    public CanvasGroup mainMenuButtons;

    [Header("Pause Menu (Optional)")]
    public PauseMenuController pauseMenuController;

    [Header("Player (Optional)")]
    public MouseLook mouseLook;

    private const string SensitivityKey =
        "CUEPOINT_MOUSE_SENSITIVITY";

    private const float DefaultSensitivity = 0.4f;

    private void Awake()
    {
        // Hide settings when starting
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    private void Start()
    {
        // Load the saved sensitivity
        float savedSensitivity = PlayerPrefs.GetFloat(
            SensitivityKey,
            DefaultSensitivity
        );

        savedSensitivity = Mathf.Clamp01(savedSensitivity);

        // Find the player camera if needed
        if (mouseLook == null)
        {
            mouseLook = FindFirstObjectByType<MouseLook>();
        }

        // Set the mouse sensitivity
        if (mouseLook != null)
        {
            mouseLook.sensitivity = savedSensitivity;
        }

        // Set up the sensitivity slider
        if (sensitivitySlider != null)
        {
            sensitivitySlider.minValue = 0f;
            sensitivitySlider.maxValue = 100f;
            sensitivitySlider.wholeNumbers = true;

            float sliderValue =
                Mathf.Round(savedSensitivity * 100f);

            sensitivitySlider.SetValueWithoutNotify(
                sliderValue
            );

            UpdateSensitivityText(sliderValue);

            sensitivitySlider.onValueChanged.AddListener(
                ChangeSensitivity
            );
        }
        else
        {
            Debug.LogWarning(
                "Sensitivity slider is not assigned!",
                this
            );
        }
    }

    public void ChangeSensitivity(float sliderValue)
    {
        // Change the slider value into sensitivity
        float newSensitivity =
            Mathf.Clamp01(sliderValue / 100f);

        // Save the sensitivity for both scenes
        PlayerPrefs.SetFloat(
            SensitivityKey,
            newSensitivity
        );

        UpdateSensitivityText(sliderValue);

        if (mouseLook == null)
        {
            mouseLook = FindFirstObjectByType<MouseLook>();
        }

        // Update the mouse sensitivity
        if (mouseLook != null)
        {
            mouseLook.sensitivity = newSensitivity;
        }
    }

    private void UpdateSensitivityText(float value)
    {
        if (sensitivityText != null)
        {
            sensitivityText.text =
                Mathf.RoundToInt(value) + " / 100";
        }
    }

    public void OpenSettings()
    {
        // Use the pause menu settings
        // when playing the level
        if (pauseMenuController != null)
        {
            pauseMenuController.OpenSettings();
            return;
        }

        // Hide the main menu buttons
        if (mainMenuButtons != null)
        {
            mainMenuButtons.alpha = 0f;
            mainMenuButtons.interactable = false;
            mainMenuButtons.blocksRaycasts = false;
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void CloseSettings()
    {
        // Save the settings when closing
        PlayerPrefs.Save();

        // Go back to the pause menu
        if (pauseMenuController != null)
        {
            pauseMenuController.BackFromSettings();
            return;
        }

        // Go back to the main menu
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (mainMenuButtons != null)
        {
            mainMenuButtons.alpha = 1f;
            mainMenuButtons.interactable = true;
            mainMenuButtons.blocksRaycasts = true;
        }
    }

    private void OnDestroy()
    {
        if (sensitivitySlider != null)
        {
            sensitivitySlider.onValueChanged.RemoveListener(
                ChangeSensitivity
            );
        }

        PlayerPrefs.Save();
    }
}
