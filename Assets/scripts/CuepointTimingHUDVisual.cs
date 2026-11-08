
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CuepointTimingHUDVisual : MonoBehaviour
{
    [Header("References")]
    public TargetCueManager targetCueManager;
    public Image timingImage;

    [Header("Timing Animation - 9 Images")]
    public Sprite Timing_01;
    public Sprite Timing_02;
    public Sprite Timing_03;
    public Sprite Timing_04;
    public Sprite Timing_05;
    public Sprite Timing_06;
    public Sprite Timing_07;
    public Sprite Timing_08;
    public Sprite Timing_09;

    [Header("Animation Speed (Seconds)")]
    [Tooltip("Slow animation before the final fast movement.")]
    [Min(0.05f)]
    public float earlyWarningSeconds = 0.75f;

    [Tooltip("Duration of the fast final movement before Perfect.")]
    [Min(0.05f)]
    public float chargeLeadSeconds = 0.20f;

    [Header("Visual Effects")]
    [Min(0f)]
    public float fadeInSeconds = 0.08f;

    [Min(0f)]
    public float perfectHoldSeconds = 0.05f;

    [Min(0.01f)]
    public float fadeOutSeconds = 0.10f;

    [Range(0f, 1f)]
    public float overlayOpacity = 0.85f;

    private Sprite[] chargeFrames;

    private class Pulse
    {
        public TargetCue cue;
        public Image firstImage;
        public Image secondImage;
        public bool usedThisFrame;
    }

    private readonly List<Pulse> pulses = new List<Pulse>();

    private void Start()
    {
        if (targetCueManager == null)
        {
            targetCueManager =
                Object.FindFirstObjectByType<TargetCueManager>();
        }

        if (timingImage == null)
        {
            timingImage = GetComponentInChildren<Image>(true);
        }

        chargeFrames = new Sprite[]
        {
            Timing_02,
            Timing_03,
            Timing_04,
            Timing_05,
            Timing_06,
            Timing_07,
            Timing_08
        };

        if (timingImage != null)
        {
            timingImage.sprite = Timing_01;
            timingImage.color = Color.white;
            timingImage.raycastTarget = false;
        }
        else
        {
            Debug.LogError(
                "TimingHUD: TimingImage is missing!",
                this
            );
        }
    }

    private void Update()
    {
        if (timingImage == null)
            return;

        if (targetCueManager == null ||
            !targetCueManager.TimelineRunning)
        {
            HideAllPulses();
            return;
        }

        TargetCue[] cues = targetCueManager.Cues;

        if (cues == null || cues.Length == 0)
        {
            HideAllPulses();
            return;
        }

        // Same DSP clock as the targets and music.
        double songTime =
            AudioSettings.dspTime -
            targetCueManager.MusicStartDspTime;

        foreach (Pulse pulse in pulses)
        {
            pulse.usedThisFrame = false;
        }

        double earlyDuration =
            Mathf.Max(0.05f, earlyWarningSeconds);

        double finalDuration =
            Mathf.Max(0.05f, chargeLeadSeconds);

        double totalDuration =
            earlyDuration + finalDuration;

        double endDuration =
            perfectHoldSeconds + fadeOutSeconds;

        // Each target gets its own timing pulse.
        foreach (TargetCue cue in cues)
        {
            if (cue == null)
                continue;

            double readyTime = cue.ReadyTimeSeconds;

            double animationStart =
                readyTime - totalDuration;

            double animationEnd =
                readyTime + endDuration;

            if (songTime < animationStart ||
                songTime >= animationEnd)
            {
                continue;
            }

            Pulse pulse = GetPulse(cue);

            pulse.usedThisFrame = true;

            UpdatePulse(
                pulse,
                songTime,
                animationStart,
                readyTime,
                earlyDuration,
                finalDuration
            );
        }

        // Hide overlays that have completed.
        foreach (Pulse pulse in pulses)
        {
            if (!pulse.usedThisFrame)
            {
                pulse.cue = null;
                pulse.firstImage.enabled = false;
                pulse.secondImage.enabled = false;
            }
        }
    }

    private Pulse GetPulse(TargetCue cue)
    {
        // Reuse the visual for an existing target.
        foreach (Pulse pulse in pulses)
        {
            if (pulse.cue == cue)
                return pulse;
        }

        // Reuse an available visual.
        foreach (Pulse pulse in pulses)
        {
            if (pulse.cue == null)
            {
                pulse.cue = cue;
                return pulse;
            }
        }

        // Create two images for smooth crossfading.
        Pulse newPulse = new Pulse();

        newPulse.cue = cue;

        newPulse.firstImage =
            CreateOverlayImage("Timing Pulse A");

        newPulse.secondImage =
            CreateOverlayImage("Timing Pulse B");

        pulses.Add(newPulse);

        return newPulse;
    }

    private Image CreateOverlayImage(string objectName)
    {
        GameObject obj = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        obj.transform.SetParent(
            timingImage.transform,
            false
        );

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;

        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        rect.localScale = Vector3.one;

        Image image = obj.GetComponent<Image>();

        image.raycastTarget = false;
        image.enabled = false;
        image.color = Color.clear;

        return image;
    }

    private void UpdatePulse(
        Pulse pulse,
        double songTime,
        double animationStart,
        double readyTime,
        double earlyDuration,
        double finalDuration)
    {
        double finalStartTime =
            readyTime - finalDuration;

        float alpha = overlayOpacity;

        // STAGE 1: SLOW BUILD-UP
        if (songTime < finalStartTime)
        {
            float progress = Mathf.Clamp01(
                (float)(
                    (songTime - animationStart) /
                    earlyDuration
                )
            );

            float fade = 1f;

            if (fadeInSeconds > 0f)
            {
                fade = Mathf.Clamp01(
                    (float)(
                        (songTime - animationStart) /
                        fadeInSeconds
                    )
                );
            }

            // Images 02 -> 03 -> 04.
            float framePosition = progress * 2f;

            SetBlend(
                pulse,
                framePosition,
                alpha * fade
            );

            return;
        }

        // STAGE 2: FAST FINAL MOVEMENT
        if (songTime < readyTime)
        {
            float progress = Mathf.Clamp01(
                (float)(
                    (songTime - finalStartTime) /
                    finalDuration
                )
            );

            // Images 04 -> 05 -> 06 -> 07 -> 08.
            float framePosition =
                2f + progress * 4f;

            SetBlend(
                pulse,
                framePosition,
                alpha
            );

            return;
        }

        // STAGE 3: PERFECT
        float afterReady =
            (float)(songTime - readyTime);

        float perfectAlpha = alpha;

        // Keep Perfect briefly visible.
        if (afterReady > perfectHoldSeconds)
        {
            float fadeProgress = Mathf.Clamp01(
                (afterReady - perfectHoldSeconds) /
                Mathf.Max(0.01f, fadeOutSeconds)
            );

            perfectAlpha *= 1f - fadeProgress;
        }

        SetOverlay(
            pulse.firstImage,
            Timing_09,
            perfectAlpha
        );

        SetOverlay(
            pulse.secondImage,
            null,
            0f
        );
    }

    // Smoothly blend between neighboring images.
    private void SetBlend(
        Pulse pulse,
        float framePosition,
        float alpha)
    {
        int firstIndex = Mathf.Clamp(
            Mathf.FloorToInt(framePosition),
            0,
            chargeFrames.Length - 1
        );

        int secondIndex = Mathf.Min(
            firstIndex + 1,
            chargeFrames.Length - 1
        );

        float blend =
            framePosition - firstIndex;

        blend = Mathf.SmoothStep(
            0f,
            1f,
            blend
        );

        SetOverlay(
            pulse.firstImage,
            chargeFrames[firstIndex],
            (1f - blend) * alpha
        );

        SetOverlay(
            pulse.secondImage,
            chargeFrames[secondIndex],
            blend * alpha
        );
    }

    private void SetOverlay(
        Image image,
        Sprite sprite,
        float alpha)
    {
        if (image == null)
            return;

        if (sprite == null || alpha <= 0.001f)
        {
            image.enabled = false;
            return;
        }

        image.enabled = true;
        image.sprite = sprite;

        image.color = new Color(
            1f,
            1f,
            1f,
            Mathf.Clamp01(alpha)
        );
    }

    private void HideAllPulses()
    {
        foreach (Pulse pulse in pulses)
        {
            pulse.cue = null;
            pulse.usedThisFrame = false;

            pulse.firstImage.enabled = false;
            pulse.secondImage.enabled = false;
        }
    }
}
