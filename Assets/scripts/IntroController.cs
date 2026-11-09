
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class IntroController : MonoBehaviour
{
    [Header("Intro Video")]
    public VideoPlayer videoPlayer;

    [Header("Menu Buttons")]
    public CanvasGroup buttonsCanvasGroup;

    [Header("Button Fade")]
    public float fadeDelay = 3f;
    public float fadeDuration = 1.5f;

    [Header("Tutorial Popup")]
    public GameObject tutorialPopup;

    [Tooltip("Seconds after menu buttons become visible.")]
    public float tutorialDelay = 1f;

    [Header("Tutorial Animation")]
    public float tutorialFadeIn = 0.4f;
    public float tutorialFadeOut = 0.3f;

    private const string TutorialSeenKey =
        "CUEPOINT_TUTORIAL_SEEN";

    private RenderTexture videoTexture;
    private CanvasGroup tutorialGroup;
    private Coroutine tutorialRoutine;

    private bool tutorialClosing = false;

    private void Awake()
    {
        // Find the CanvasGroup on the tutorial parent.
        if (tutorialPopup != null)
        {
            tutorialGroup =
                tutorialPopup.GetComponent<CanvasGroup>();

            // Add one automatically if missing.
            if (tutorialGroup == null)
            {
                tutorialGroup =
                    tutorialPopup.AddComponent<CanvasGroup>();
            }

            // Hide the entire tutorial, including X.
            tutorialGroup.alpha = 0f;
            tutorialGroup.interactable = false;
            tutorialGroup.blocksRaycasts = false;

            tutorialPopup.SetActive(false);
        }
    }

    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (videoPlayer == null ||
            buttonsCanvasGroup == null)
        {
            Debug.LogError(
                "IntroController: VideoPlayer or " +
                "Buttons CanvasGroup is missing.",
                this
            );

            return;
        }

        // Hide menu buttons at start.
        buttonsCanvasGroup.alpha = 0f;
        buttonsCanvasGroup.interactable = false;
        buttonsCanvasGroup.blocksRaycasts = false;

        // Display video in the Canvas.
        RawImage videoImage = GetComponent<RawImage>();

        if (videoImage == null)
        {
            videoImage = gameObject.AddComponent<RawImage>();
        }

        videoImage.raycastTarget = false;

        videoTexture = new RenderTexture(
            Screen.width,
            Screen.height,
            0
        );

        videoTexture.Create();

        videoPlayer.Stop();
        videoPlayer.renderMode =
            VideoRenderMode.RenderTexture;

        videoPlayer.targetTexture = videoTexture;
        videoImage.texture = videoTexture;

        // Play intro.
        videoPlayer.Play();

        // Show menu, then tutorial if needed.
        StartCoroutine(FadeInButtons());
    }

    private IEnumerator FadeInButtons()
    {
        yield return new WaitForSeconds(fadeDelay);

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            buttonsCanvasGroup.alpha =
                Mathf.Clamp01(timer / fadeDuration);

            yield return null;
        }

        buttonsCanvasGroup.alpha = 1f;

        bool firstTime =
            PlayerPrefs.GetInt(TutorialSeenKey, 0) == 0;

        if (firstTime &&
            tutorialPopup != null &&
            tutorialGroup != null)
        {
            // Menu remains visible but cannot be clicked.
            buttonsCanvasGroup.interactable = false;
            buttonsCanvasGroup.blocksRaycasts = false;

            // Wait one second after the button fade.
            yield return new WaitForSecondsRealtime(
                Mathf.Max(0f, tutorialDelay)
            );

            tutorialRoutine = StartCoroutine(
                FadeInTutorial()
            );
        }
        else
        {
            UnlockMenu();
        }
    }

    private IEnumerator FadeInTutorial()
    {
        tutorialPopup.SetActive(true);

        tutorialGroup.alpha = 0f;
        tutorialGroup.interactable = false;
        tutorialGroup.blocksRaycasts = true;

        float timer = 0f;

        while (timer < tutorialFadeIn)
        {
            timer += Time.unscaledDeltaTime;

            tutorialGroup.alpha =
                tutorialFadeIn <= 0f
                ? 1f
                : Mathf.Clamp01(timer / tutorialFadeIn);

            yield return null;
        }

        tutorialGroup.alpha = 1f;
        tutorialGroup.interactable = true;
        tutorialGroup.blocksRaycasts = true;

        tutorialRoutine = null;
    }

    // Connect this function to your X button.
    public void CloseTutorial()
    {
        if (tutorialPopup == null ||
            tutorialGroup == null ||
            !tutorialPopup.activeInHierarchy ||
            tutorialClosing)
        {
            return;
        }

        tutorialClosing = true;

        if (tutorialRoutine != null)
        {
            StopCoroutine(tutorialRoutine);
        }

        tutorialRoutine = StartCoroutine(
            FadeOutTutorial()
        );
    }

    private IEnumerator FadeOutTutorial()
    {
        tutorialGroup.interactable = false;

        float startAlpha = tutorialGroup.alpha;
        float timer = 0f;

        while (timer < tutorialFadeOut)
        {
            timer += Time.unscaledDeltaTime;

            float progress =
                tutorialFadeOut <= 0f
                ? 1f
                : Mathf.Clamp01(
                    timer / tutorialFadeOut
                );

            tutorialGroup.alpha = Mathf.Lerp(
                startAlpha,
                0f,
                progress
            );

            yield return null;
        }

        tutorialGroup.alpha = 0f;
        tutorialGroup.blocksRaycasts = false;

        tutorialPopup.SetActive(false);

        // Remember that the tutorial was closed.
        PlayerPrefs.SetInt(TutorialSeenKey, 1);
        PlayerPrefs.Save();

        UnlockMenu();

        tutorialClosing = false;
        tutorialRoutine = null;
    }

    private void UnlockMenu()
    {
        if (buttonsCanvasGroup != null)
        {
            buttonsCanvasGroup.interactable = true;
            buttonsCanvasGroup.blocksRaycasts = true;
        }
    }

    // Use this in Inspector to test the tutorial again.
    [ContextMenu("Reset Tutorial For Testing")]
    public void ResetTutorialForTesting()
    {
        PlayerPrefs.DeleteKey(TutorialSeenKey);
        PlayerPrefs.Save();

        Debug.Log(
            "Tutorial reset. It will appear next time.",
            this
        );
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.targetTexture = null;
        }

        if (videoTexture != null)
        {
            videoTexture.Release();
            Destroy(videoTexture);
        }
    }
}
