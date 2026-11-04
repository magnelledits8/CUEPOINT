
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class IntroController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public CanvasGroup buttonsCanvasGroup;

    public float fadeDelay = 3f;
    public float fadeDuration = 1.5f;

    private RenderTexture videoTexture;

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Dölj knapparna från början
        buttonsCanvasGroup.alpha = 0;
        buttonsCanvasGroup.interactable = false;
        buttonsCanvasGroup.blocksRaycasts = false;

        // Visa videon i Canvas istället för framför kameran
        RawImage videoImage = GetComponent<RawImage>();

        if (videoImage == null)
            videoImage = gameObject.AddComponent<RawImage>();

        videoImage.raycastTarget = false;

        videoTexture = new RenderTexture(Screen.width, Screen.height, 0);
        videoTexture.Create();

        videoPlayer.Stop();
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = videoTexture;

        videoImage.texture = videoTexture;

        // Starta videon
        videoPlayer.Play();

        // Starta knapparnas fade-in
        StartCoroutine(FadeInButtons());
    }

    IEnumerator FadeInButtons()
    {
        yield return new WaitForSeconds(fadeDelay);

        float timer = 0;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            buttonsCanvasGroup.alpha = Mathf.Clamp01(timer / fadeDuration);
            yield return null;
        }

        buttonsCanvasGroup.alpha = 1;
        buttonsCanvasGroup.interactable = true;
        buttonsCanvasGroup.blocksRaycasts = true;
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.targetTexture = null;

        if (videoTexture != null)
        {
            videoTexture.Release();
            Destroy(videoTexture);
        }
    }
}
