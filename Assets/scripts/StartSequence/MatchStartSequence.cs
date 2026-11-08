
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MatchStartSequence : MonoBehaviour
{
    [Header("UI Elements")]
    public Image blackScreen;
    public Image countdownDisplay;

    [Header("Countdown Animation")]
    public Sprite[] countdownFrames;
    public AudioSource countdownAudio;

    [Tooltip("Animation frames per second, not the computer's FPS.")]
    public float framesPerSecond = 60f;

    [Header("Background Music - Legacy")]
    [Tooltip("Not used. DelayedMusicStart controls gameplay music.")]
    public AudioSource backgroundMusicAudio;

    [Tooltip("Old setting. Not used by this version.")]
    public float musicDelayRelativeAndAnimation = 2f;

    [Header("Control Panel (Seconds)")]
    public float loadWaitDuration = 1.1f;
    public float fadeDuration = 3.1f;

    [Header("Fine-Tuning (Seconds)")]
    [Tooltip("Negative = skip the first part of the animation.")]
    public float countdownDelay = -0.42f;

    [Tooltip("Positive = delay audio. Negative = start immediately.")]
    public float audioDelay = -0.42f;

    // Shared timing information for DelayedMusicStart.
    public bool IsCountdownScheduled { get; private set; }
    public double CountdownDspStartTime { get; private set; }

    private void Start()
    {
        IsCountdownScheduled = false;

        if (blackScreen != null)
        {
            Color color = blackScreen.color;
            color.a = 1f;
            blackScreen.color = color;
            blackScreen.gameObject.SetActive(true);
        }

        if (countdownDisplay != null)
        {
            countdownDisplay.gameObject.SetActive(true);

            Color color = countdownDisplay.color;
            color.a = 0f;
            countdownDisplay.color = color;
        }

        StartCoroutine(PrepareAndStart());
    }

    private IEnumerator PrepareAndStart()
    {
        DelayedMusicStart music =
            Object.FindFirstObjectByType<DelayedMusicStart>();

        if (music == null || music.MusicSource == null ||
            music.MusicSource.clip == null)
        {
            Debug.LogError(
                "MatchStartSequence: Gameplay music was not found."
            );
            yield break;
        }

        if (countdownFrames == null ||
            countdownFrames.Length == 0)
        {
            Debug.LogError(
                "MatchStartSequence: Countdown frames are missing."
            );
            yield break;
        }

        // Prepare music before scheduling the countdown.
        yield return StartCoroutine(
            PrepareAudio(music.MusicSource.clip)
        );

        if (music.MusicSource.clip.loadState !=
            AudioDataLoadState.Loaded)
        {
            Debug.LogError(
                "MatchStartSequence: Gameplay music failed to load."
            );
            yield break;
        }

        // Prepare countdown audio.
        if (countdownAudio != null &&
            countdownAudio.clip != null)
        {
            yield return StartCoroutine(
                PrepareAudio(countdownAudio.clip)
            );

            if (countdownAudio.clip.loadState !=
                AudioDataLoadState.Loaded)
            {
                Debug.LogError(
                    "MatchStartSequence: Countdown audio failed to load."
                );
                yield break;
            }
        }

        // Keep your existing extra loading wait.
        if (loadWaitDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(
                loadWaitDuration
            );
        }

        // Give Unity time to prepare scheduled playback.
        CountdownDspStartTime = AudioSettings.dspTime + 0.5;

        // DelayedMusicStart can now schedule music
        // relative to this exact countdown start.
        IsCountdownScheduled = true;

        StartCoroutine(FadeBlackScreen());
        StartCoroutine(PlayCountdownAnimation());

        ScheduleCountdownAudio();

        Debug.Log(
            "CUEPOINT: Countdown scheduled at DSP " +
            CountdownDspStartTime.ToString("F3")
        );
    }

    private IEnumerator PrepareAudio(AudioClip clip)
    {
        if (clip == null)
            yield break;

        if (clip.loadState == AudioDataLoadState.Unloaded)
        {
            clip.LoadAudioData();
        }

        float startTime = Time.realtimeSinceStartup;

        while (clip.loadState == AudioDataLoadState.Loading ||
               clip.loadState == AudioDataLoadState.Unloaded)
        {
            if (Time.realtimeSinceStartup - startTime > 20f)
            {
                Debug.LogError(
                    "Audio loading timed out: " + clip.name
                );
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator FadeBlackScreen()
    {
        if (blackScreen == null)
            yield break;

        while (AudioSettings.dspTime < CountdownDspStartTime)
            yield return null;

        if (fadeDuration <= 0f)
        {
            blackScreen.gameObject.SetActive(false);
            yield break;
        }

        Color color = blackScreen.color;

        while (true)
        {
            double elapsed =
                AudioSettings.dspTime - CountdownDspStartTime;

            float progress = Mathf.Clamp01(
                (float)(elapsed / fadeDuration)
            );

            color.a = 1f - progress;
            blackScreen.color = color;

            if (progress >= 1f)
                break;

            yield return null;
        }

        blackScreen.gameObject.SetActive(false);
    }

    private IEnumerator PlayCountdownAnimation()
    {
        while (AudioSettings.dspTime < CountdownDspStartTime)
            yield return null;

        float animationSpeed = Mathf.Max(1f, framesPerSecond);

        // Preserve your old negative countdown delay.
        int firstFrame = 0;

        if (countdownDelay < 0f)
        {
            firstFrame = Mathf.Clamp(
                Mathf.RoundToInt(
                    -countdownDelay * animationSpeed
                ),
                0,
                countdownFrames.Length - 1
            );
        }

        // A positive delay starts the animation later.
        double animationStart =
            CountdownDspStartTime + Mathf.Max(0f, countdownDelay);

        while (AudioSettings.dspTime < animationStart)
            yield return null;

        if (countdownDisplay != null)
        {
            countdownDisplay.gameObject.SetActive(true);

            Color color = countdownDisplay.color;
            color.a = 1f;
            countdownDisplay.color = color;
        }

        // Preserve the existing player intro trigger.
        PlayerRunController player =
            Object.FindFirstObjectByType<PlayerRunController>();

        if (player != null)
        {
            player.StartIntroSequence();
        }

        double visibleDuration =
            (countdownFrames.Length - firstFrame) /
            (double)animationSpeed;

        while (true)
        {
            double elapsed =
                AudioSettings.dspTime - animationStart;

            if (elapsed >= visibleDuration)
                break;

            int frameIndex = Mathf.Clamp(
                firstFrame +
                Mathf.FloorToInt(
                    (float)(elapsed * animationSpeed)
                ),
                firstFrame,
                countdownFrames.Length - 1
            );

            if (countdownDisplay != null)
            {
                countdownDisplay.sprite =
                    countdownFrames[frameIndex];
            }

            yield return null;
        }

        if (countdownDisplay != null)
        {
            countdownDisplay.gameObject.SetActive(false);
        }
    }

    private void ScheduleCountdownAudio()
    {
        if (countdownAudio == null ||
            countdownAudio.clip == null)
        {
            return;
        }

        countdownAudio.Stop();
        countdownAudio.playOnAwake = false;

        // Keep the original pitch adjustment.
        countdownAudio.pitch =
            Mathf.Max(1f, framesPerSecond) / 60f;

        // The old code treated negative delays as immediate.
        double audioStart =
            CountdownDspStartTime + Mathf.Max(0f, audioDelay);

        countdownAudio.PlayScheduled(audioStart);
    }
}
