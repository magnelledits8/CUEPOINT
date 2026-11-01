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
            musicAudio.Play();
        }
    }

    IEnumerator WaitAndPlay(MatchStartSequence seq)
    {
        // Wait for loading duration
        yield return new WaitForSeconds(seq.loadWaitDuration);

        // Wait for any countdown delay animation
        if (seq.countdownDelay > 0f)
        {
            yield return new WaitForSeconds(seq.countdownDelay);
        }

        // Calculate the total duration of the animation dynamically based on real seconds
        float animDuration = 0f;
        if (seq.countdownFrames != null && seq.framesPerSecond > 0f)
        {
            animDuration = seq.countdownFrames.Length / seq.framesPerSecond;
        }

        // Ensure playback time matches perfectly in real-time independent of frames
        float finalWaitTime = animDuration - overlapSeconds;

        if (finalWaitTime > 0f)
        {
            yield return new WaitForSeconds(finalWaitTime);
        }

        if (musicAudio != null && !musicAudio.isPlaying)
        {
            musicAudio.Play();
        }
    }
}
