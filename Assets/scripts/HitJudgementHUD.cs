
using System.Collections;
using TMPro;
using UnityEngine;

public class HitJudgementHUD : MonoBehaviour
{
    public static HitJudgementHUD Instance { get; private set; }

    [Header("Text Reference")]
    public TextMeshProUGUI judgementText;

    [Header("Animation Timing")]
    public float fadeInDuration = 0.05f;
    public float holdDuration = 0.25f;
    public float fadeOutDuration = 0.20f;

    [Header("Scale Animation")]
    public float popScale = 1.15f;

    [Header("Colors")]
    public Color earlyColor = new Color(0.3f, 0.8f, 1f);
    public Color perfectColor = new Color(1f, 0.65f, 0.15f);
    public Color lateColor = new Color(0.7f, 0.85f, 1f);
    public Color missColor = new Color(1f, 0.25f, 0.25f);

    [Header("Testing")]
    [Tooltip("Shows PERFECT when the scene starts. Disable after testing.")]
    public bool testPerfectOnStart = false;

    public float testDisplaySeconds = 2f;

    private Coroutine currentRoutine;
    private Vector3 originalScale;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate HitJudgementHUD found.", this);
            Destroy(this);
            return;
        }

        Instance = this;

        if (judgementText == null)
        {
            judgementText = GetComponent<TextMeshProUGUI>();
        }

        originalScale = transform.localScale;

        if (judgementText != null)
        {
            judgementText.alpha = 0f;
        }
        else
        {
            Debug.LogError(
                "HitJudgementHUD: Missing TextMeshPro reference!",
                this
            );
        }
    }

    private void Start()
    {
        if (testPerfectOnStart)
        {
            ShowJudgement(
                "PERFECT",
                perfectColor,
                testDisplaySeconds
            );
        }
    }

    public void ShowEarly()
    {
        ShowJudgement("EARLY", earlyColor, holdDuration);
    }

    public void ShowPerfect()
    {
        ShowJudgement("PERFECT", perfectColor, holdDuration);
    }

    public void ShowLate()
    {
        ShowJudgement("LATE", lateColor, holdDuration);
    }

    public void ShowMiss()
    {
        ShowJudgement("MISS", missColor, holdDuration);
    }

    public void ShowJudgement(string message, Color color)
    {
        ShowJudgement(message, color, holdDuration);
    }

    private void ShowJudgement(
        string message,
        Color color,
        float displayDuration)
    {
        if (judgementText == null)
            return;

        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(
            AnimateJudgement(message, color, displayDuration)
        );
    }

    private IEnumerator AnimateJudgement(
        string message,
        Color color,
        float displayDuration)
    {
        judgementText.text = message;
        judgementText.color = color;
        judgementText.alpha = 0f;

        transform.localScale = originalScale * popScale;

        // Fade in the hit result
        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = fadeInDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / fadeInDuration);

            judgementText.alpha = progress;

            transform.localScale = Vector3.Lerp(
                originalScale * popScale,
                originalScale,
                progress
            );

            yield return null;
        }

        judgementText.alpha = 1f;
        transform.localScale = originalScale;

        // Keep the result on screen for a short time
        if (displayDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(
                displayDuration
            );
        }

        // Fade out the hit result
        elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = fadeOutDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / fadeOutDuration);

            judgementText.alpha = 1f - progress;

            yield return null;
        }

        judgementText.alpha = 0f;
        currentRoutine = null;
    }

    [ContextMenu("TEST - Show Perfect")]
    private void TestPerfect()
    {
        ShowPerfect();
    }

    [ContextMenu("TEST - Show Early")]
    private void TestEarly()
    {
        ShowEarly();
    }

    [ContextMenu("TEST - Show Late")]
    private void TestLate()
    {
        ShowLate();
    }

    [ContextMenu("TEST - Show Miss")]
    private void TestMiss()
    {
        ShowMiss();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
