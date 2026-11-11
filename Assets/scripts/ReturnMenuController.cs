
using System.Collections;
using UnityEngine;

public class ReturnMenuController : MonoBehaviour
{
    [Header("Menu Buttons")]
    public CanvasGroup buttonsCanvasGroup;

    [Header("Fade Settings")]
    public float fadeDelay = 0.1f;
    public float fadeDuration = 0.4f;

    private void Start()
    {
        // Make sure the game is no longer paused.
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // Show and unlock the mouse cursor.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (buttonsCanvasGroup == null)
        {
            Debug.LogError(
                "ReturnMenuController: Buttons CanvasGroup is missing!",
                this
            );
            return;
        }

        // Start with invisible, non-clickable buttons.
        buttonsCanvasGroup.alpha = 0f;
        buttonsCanvasGroup.interactable = false;
        buttonsCanvasGroup.blocksRaycasts = false;

        StartCoroutine(FadeInButtons());
    }

    private IEnumerator FadeInButtons()
    {
        yield return new WaitForSecondsRealtime(
            Mathf.Max(0f, fadeDelay)
        );

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            buttonsCanvasGroup.alpha =
                fadeDuration <= 0f
                ? 1f
                : Mathf.Clamp01(timer / fadeDuration);

            yield return null;
        }

        buttonsCanvasGroup.alpha = 1f;
        buttonsCanvasGroup.interactable = true;
        buttonsCanvasGroup.blocksRaycasts = true;
    }
}
