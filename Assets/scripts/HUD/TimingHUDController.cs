
using UnityEngine;
using UnityEngine.UI;

public class TimingHUDController : MonoBehaviour
{
    [Header("References")]
    public TargetCueManager targetCueManager;
    public Image timingImage;

    [Header("Timing Animation - 5 Images")]
    public Sprite Timing_01;
    public Sprite Timing_02;
    public Sprite Timing_03;
    public Sprite Timing_04;
    public Sprite Timing_05;

    [Header("Timing Settings (Seconds)")]
    [Tooltip("How many seconds before the target's Ready Time the animation starts.")]
    [Min(0.1f)]
    public float chargeLeadSeconds = 1.2f;

    [Tooltip("How long the final timing image stays visible.")]
    [Min(0.01f)]
    public float perfectFlashSeconds = 0.08f;

    private Sprite currentSprite;

    private void Start()
    {
        if (targetCueManager == null)
        {
            targetCueManager =
                Object.FindFirstObjectByType<TargetCueManager>();
        }

        SetImage(Timing_01);
    }

    private void Update()
    {
        if (targetCueManager == null ||
            !targetCueManager.TimelineRunning)
        {
            SetImage(Timing_01);
            return;
        }

        TargetCue[] cues = targetCueManager.Cues;

        if (cues == null || cues.Length == 0)
        {
            SetImage(Timing_01);
            return;
        }

        // Use the same music timing as the targets
        double songTime =
            AudioSettings.dspTime -
            targetCueManager.MusicStartDspTime;

        double timeUntilNextTarget = double.PositiveInfinity;
        bool showPerfect = false;

        foreach (TargetCue cue in cues)
        {
            if (cue == null)
                continue;

            double readyTime = cue.ReadyTimeSeconds;
            double timeUntilReady = readyTime - songTime;

            // Show the last image when the target is ready
            if (timeUntilReady <= 0.0 &&
                timeUntilReady > -perfectFlashSeconds)
            {
                showPerfect = true;
            }

            if (timeUntilReady > 0.0 &&
                timeUntilReady < timeUntilNextTarget)
            {
                timeUntilNextTarget = timeUntilReady;
            }
        }

        // Show the perfect timing flash
        if (showPerfect)
        {
            SetImage(Timing_05);
            return;
        }

        // Show the first image when no target is close
        if (timeUntilNextTarget > chargeLeadSeconds)
        {
            SetImage(Timing_01);
            return;
        }

        // Change the images as the target gets closer to ready time
        float progress = Mathf.Clamp01(
            1f - (float)(timeUntilNextTarget / chargeLeadSeconds)
        );

        if (progress < 0.33f)
        {
            SetImage(Timing_02);
        }
        else if (progress < 0.66f)
        {
            SetImage(Timing_03);
        }
        else
        {
            SetImage(Timing_04);
        }
    }

    private void SetImage(Sprite newSprite)
    {
        if (timingImage == null || newSprite == null)
            return;

        if (currentSprite == newSprite)
            return;

        timingImage.sprite = newSprite;
        currentSprite = newSprite;
    }
}
