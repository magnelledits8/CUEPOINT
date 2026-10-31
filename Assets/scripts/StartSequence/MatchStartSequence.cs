using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MatchStartSequence : MonoBehaviour
{
    [Header("UI Element")]
    public Image blackScreen;
    public Image countdownDisplay;

    [Header("Animation & Ljud")]
    [SerializeField] public Sprite[] countdownFrames;
    public AudioSource countdownAudio;
    public float framesPerSecond = 60f;

    [Header("Bakgrundsmusik")]
    public AudioSource backgroundMusicAudio;
    [Tooltip("Tid i sekunder baserat på när animationen STARTAR. Negativt värde = överlappning innan animationen är slut.")]
    public float musicDelayRelativeAndAnimation = 2.0f;

    [Header("Kontrollpanelen (Tid i sekunder)")]
    public float loadWaitDuration = 1.1f;
    public float fadeDuration = 3.1f;

    [Header("Finjustering (Tid i sekunder)")]
    public float countdownDelay = -0.42f;
    public float audioDelay = -0.42f;

    void Start()
    {
        if (blackScreen != null)
        {
            Color c = blackScreen.color;
            c.a = 1f;
            blackScreen.color = c;
            blackScreen.gameObject.SetActive(true);
        }

        if (countdownDisplay != null)
        {
            countdownDisplay.gameObject.SetActive(true);
            Color c = countdownDisplay.color;
            c.a = 0f;
            countdownDisplay.color = c;
        }

        if (countdownAudio != null && framesPerSecond > 0f)
        {
            float targetPitch = framesPerSecond / 60f;
            countdownAudio.pitch = targetPitch;
        }

        StartCoroutine(MasterSequence());
    }

    IEnumerator MasterSequence()
    {
        yield return new WaitForSeconds(loadWaitDuration);

        StartCoroutine(FadeBlackScreen());
        StartCoroutine(PlayCountdownAnimation());
        StartCoroutine(PlayAudioWithDelay());
        StartCoroutine(PlayMusicWithDelay());
    }

    IEnumerator FadeBlackScreen()
    {
        float elapsedTime = 0f;
        Color blackColor = blackScreen.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            blackColor.a = Mathf.Clamp01(1f - (elapsedTime / fadeDuration));
            blackScreen.color = blackColor;
            yield return null;
        }
        blackScreen.gameObject.SetActive(false);
    }

    IEnumerator PlayAudioWithDelay()
    {
        if (audioDelay > 0f)
        {
            yield return new WaitForSeconds(audioDelay);
        }

        if (countdownAudio != null)
        {
            countdownAudio.Play();
        }
    }

    IEnumerator PlayMusicWithDelay()
    {
        if (countdownDelay > 0f)
        {
            yield return new WaitForSeconds(countdownDelay);
        }

        float actualWaitTime = musicDelayRelativeAndAnimation;

        if (actualWaitTime > 0f)
        {
            yield return new WaitForSeconds(actualWaitTime);
        }

        if (backgroundMusicAudio != null)
        {
            backgroundMusicAudio.Play();
        }
    }

    IEnumerator PlayCountdownAnimation()
    {
        if (countdownDelay > 0f)
        {
            yield return new WaitForSeconds(countdownDelay);
        }

        if (countdownDisplay != null)
        {
            Color c = countdownDisplay.color;
            c.a = 1f;
            countdownDisplay.color = c;
        }

        if (countdownDisplay != null && countdownFrames != null && countdownFrames.Length > 0)
        {
            float timePerFrame = 1f / framesPerSecond;

            int startFrame = 0;
            if (countdownDelay < 0f)
            {
                int framesToSkip = Mathf.RoundToInt(Mathf.Abs(countdownDelay) * framesPerSecond);
                startFrame = Mathf.Min(framesToSkip, countdownFrames.Length - 1);
            }

            // SYNKKORREKTION: Letar nu efter det nya klassnamnet PlayerRunController!
            PlayerRunController laneMove = Object.FindFirstObjectByType<PlayerRunController>();
            if (laneMove != null)
            {
                laneMove.StartIntroSequence();
            }

            for (int i = startFrame; i < countdownFrames.Length; i++)
            {
                countdownDisplay.sprite = countdownFrames[i];
                yield return new WaitForSeconds(timePerFrame);
            }

            countdownDisplay.gameObject.SetActive(false);
        }
    }
}
