
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class SliderScript : MonoBehaviour
{
    [Header("Slider UI")]
    [SerializeField] private Slider _slider;
    [SerializeField] private TextMeshProUGUI _sliderText;

    [Header("Other Script")]
    [SerializeField] private UnityEvent<float> onSettingChanged;

    private const string SaveKey = "CUEPOINT_SLIDER_VALUE";
    private const int DefaultValue = 40;

    private static int currentValue;
    private static bool isLoaded = false;

    // Other scripts can listen for value changes.
    public static event Action<int> ValueChanged;

    // Other scripts can read the saved value.
    public static int CurrentValue
    {
        get
        {
            LoadValue();
            return currentValue;
        }
    }

    private static void LoadValue()
    {
        if (isLoaded)
            return;

        currentValue = PlayerPrefs.GetInt(
            SaveKey,
            DefaultValue
        );

        currentValue = Mathf.Clamp(currentValue, 0, 100);
        isLoaded = true;
    }

    private void Awake()
    {
        LoadValue();

        if (_slider == null || _sliderText == null)
        {
            Debug.LogError(
                "SliderScript: Assign Slider and TMP Text!"
            );
            return;
        }

        // Configure slider.
        _slider.minValue = 0;
        _slider.maxValue = 100;
        _slider.wholeNumbers = true;

        // Configure text.
        _sliderText.enableAutoSizing = false;
        _sliderText.alignment = TextAlignmentOptions.Right;
    }

    private void OnEnable()
    {
        if (_slider == null || _sliderText == null)
            return;

        _slider.onValueChanged.AddListener(OnSliderChanged);

        // Listen for changes from other sliders.
        ValueChanged += OnGlobalValueChanged;

        UpdateUI(CurrentValue);
    }

    private void Start()
    {
        // Apply saved setting to the connected script.
        onSettingChanged?.Invoke(CurrentValue);
    }

    private void OnSliderChanged(float value)
    {
        int newValue = Mathf.Clamp(
            Mathf.RoundToInt(value),
            0,
            100
        );

        if (newValue == CurrentValue)
            return;

        // Update shared value.
        currentValue = newValue;

        // Remember the setting.
        PlayerPrefs.SetInt(SaveKey, currentValue);

        // Notify every slider and listening script.
        ValueChanged?.Invoke(currentValue);
    }

    private void OnGlobalValueChanged(int value)
    {
        UpdateUI(value);

        // Send new value to the connected script.
        onSettingChanged?.Invoke(value);
    }

    private void UpdateUI(int value)
    {
        _slider.SetValueWithoutNotify(value);

        _sliderText.text = $"{value} / 100";
    }

    private void OnDisable()
    {
        if (_slider != null)
        {
            _slider.onValueChanged.RemoveListener(
                OnSliderChanged
            );
        }

        ValueChanged -= OnGlobalValueChanged;

        // Save when leaving the menu.
        PlayerPrefs.Save();
    }
}
