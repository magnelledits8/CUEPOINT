
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DelayedMusicStart : MonoBehaviour
{
    [Header("Overlap Settings")]
    [Tooltip("How many seconds before the complete countdown duration music should start.")]
    public float overlapSeconds = 0.7f;

    private AudioSource musicAudio;
    private bool musicScheduled;

    // Let MatchStartSequence access the music
    public AudioSource MusicSource
    {
        get { return musicAudio; }
    }

    private void Awake()
    {
        musicAudio = GetComponent<AudioSource>();

        musicAudio.playOnAwake = false;
        musicAudio.Stop();

        musicScheduled = false;
    }

    private void Start()
    {
        MatchStartSequence sequence =
            Object.FindFirstObjectByType<MatchStartSequence>();

        if (sequence != null)
        {
            StartCoroutine(WaitForCountdown(sequence));
        }
        else
        {
            StartCoroutine(StartWithoutCountdown());
        }
    }

    private IEnumerator WaitForCountdown(MatchStartSequence sequence)
    {
        // Wait until the countdown is ready
        while (sequence != null && !sequence.IsCountdownScheduled)
        {
            yield return null;
        }

        if (sequence == null)
            yield break;

        if (musicAudio == null || musicAudio.clip == null)
        {
            Debug.LogError(
                "DelayedMusicStart: No gameplay AudioClip assigned."
            );
            yield break;
        }

        // Calculate the full countdown time
        double fullDuration =
            sequence.countdownFrames.Length /
            (double)Mathf.Max(1f, sequence.framesPerSecond);

        // Keep the same music timing as before
        // Add the small delay before starting
        double musicDelay =
            fullDuration - overlapSeconds + 0.1;

        // Make sure the music does not start too early
        double startDsp =
            sequence.CountdownDspStartTime +
            musicDelay;

        if (startDsp < AudioSettings.dspTime + 0.2)
        {
            Debug.LogWarning(
                "DelayedMusicStart: Music start was too close. " +
                "Scheduling slightly later."
            );

            startDsp = AudioSettings.dspTime + 0.2;
        }

        ScheduleMusic(startDsp);
    }

    private IEnumerator StartWithoutCountdown()
    {
        if (musicAudio == null || musicAudio.clip == null)
            yield break;

        AudioClip clip = musicAudio.clip;

        if (clip.loadState == AudioDataLoadState.Unloaded)
        {
            clip.LoadAudioData();
        }

        while (clip.loadState == AudioDataLoadState.Loading)
        {
            yield return null;
        }

        if (clip.loadState != AudioDataLoadState.Loaded)
        {
            Debug.LogError(
                "DelayedMusicStart: Audio could not be loaded."
            );
            yield break;
        }

        ScheduleMusic(AudioSettings.dspTime + 0.5);
    }

    private void ScheduleMusic(double startDsp)
    {
        if (musicScheduled)
            return;

        if (musicAudio == null || musicAudio.clip == null)
            return;

        musicScheduled = true;

        musicAudio.Stop();
        musicAudio.playOnAwake = false;
        musicAudio.timeSamples = 0;

        // Start the music using the DSP clock
        musicAudio.PlayScheduled(startDsp);

        // Use the same start time for the targets
        TargetCueManager manager =
            Object.FindFirstObjectByType<TargetCueManager>();

        if (manager != null)
        {
            manager.BeginTimeline(startDsp);
        }
        else
        {
            Debug.LogWarning(
                "DelayedMusicStart: TargetCueManager not found."
            );
        }

        Debug.Log(
            "CUEPOINT: Music and targets scheduled at DSP " +
            startDsp.ToString("F3")
        );
    }
}
