using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DelayedMusicStart : MonoBehaviour
{
    [Header("Overlap Settings")]
    [Tooltip("How many seconds BEFORE the countdown is fully completed should the song early start?")]
    public float overlapSeconds = 0.7f;

    private AudioSource musicAudio;

    void Awake()
    {
        musicAudio = GetComponent<AudioSource>();
        musicAudio.playOnAwake = false;
        musicAudio.Stop();
    }

    void Start()
    {
        MatchStartSequence sequence = Object.FindFirstObjectByType<MatchStartSequence>();

        if (sequence != null)
        {
            StartCoroutine(WaitAndPlay(sequence));
        }
        else
        {
            // If testing without a start sequence, schedule 0.5 seconds ahead for perfect sync
            double startDsp = AudioSettings.dspTime + 0.5;
            musicAudio.PlayScheduled(startDsp);

            TargetCueManager manager = Object.FindFirstObjectByType<TargetCueManager>();
            if (manager != null) manager.BeginTimeline(startDsp);
        }
    }

    IEnumerator WaitAndPlay(MatchStartSequence seq)
    {
        yield return new WaitForSeconds(seq.loadWaitDuration);

        if (seq.countdownDelay > 0f)
        {
            yield return new WaitForSeconds(seq.countdownDelay);
        }

        float animDuration = 0f;
        if (seq.countdownFrames != null && seq.framesPerSecond > 0f)
        {
            animDuration = seq.countdownFrames.Length / seq.framesPerSecond;
        }

        float finalWaitTime = animDuration - overlapSeconds;

        if (finalWaitTime > 0f)
        {
            yield return new WaitForSeconds(finalWaitTime);
        }

        if (musicAudio != null && !musicAudio.isPlaying)
        {
            // Schedule the start 0.1 seconds into the future to give Unity time to buffer the audio stream
            double scheduledStartTime = AudioSettings.dspTime + 0.1;
            musicAudio.PlayScheduled(scheduledStartTime);

            // Send the exact audio start time timestamp to the TargetCueManager
            TargetCueManager manager = Object.FindFirstObjectByType<TargetCueManager>();
            if (manager != null)
            {
                manager.BeginTimeline(scheduledStartTime);
            }
        }
    }
}
