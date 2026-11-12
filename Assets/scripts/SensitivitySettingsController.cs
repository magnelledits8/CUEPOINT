
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
        // Settings should be hidden when the scene starts.
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    private void Start()
    {
        // Load previously saved sensitivity.
        float savedSensitivity = PlayerPrefs.GetFloat(
            SensitivityKey,
            DefaultSensitivity
        );

        savedSensitivity = Mathf.Clamp01(savedSensitivity);

        // Find the gameplay camera if no reference was assigned.
        if (mouseLook == null)
        {
            mouseLook = FindFirstObjectByType<MouseLook>();
        }

        // Apply sensitivity in the current scene.
        if (mouseLook != null)
        {
            mouseLook.sensitivity = savedSensitivity;
        }

        // Configure the slider.
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
        // Convert 40/100 into sensitivity 0.4.
        float newSensitivity =
            Mathf.Clamp01(sliderValue / 100f);

        // Save the value for both scenes.
        PlayerPrefs.SetFloat(
            SensitivityKey,
            newSensitivity
        );

        UpdateSensitivityText(sliderValue);

        if (mouseLook == null)
        {
            mouseLook = FindFirstObjectByType<MouseLook>();
        }

        // Apply immediately if the player camera exists.
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
        // In Level1, let the existing PauseMenuController
        // handle switching between Pause and Settings.
        if (pauseMenuController != null)
        {
            pauseMenuController.OpenSettings();
            return;
        }

        // In Main Menu, hide the menu buttons.
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
        // Save when closing.
        PlayerPrefs.Save();

        // Return to the pause menu in Level1.
        if (pauseMenuController != null)
        {
            pauseMenuController.BackFromSettings();
            return;
        }

        // Return to the main menu.
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
