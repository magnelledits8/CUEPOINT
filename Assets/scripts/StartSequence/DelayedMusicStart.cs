using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DelayedMusicStart : MonoBehaviour
{
    [Header("Inställningar för överlappning")]
    [Tooltip("Hur många sekunder INNAN nedräkningen är helt klar ska låten tjuvstarta?")]
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
        // Vänta på laddningstiden
        yield return new WaitForSeconds(seq.loadWaitDuration);

        // Vänta på eventuell fördröjning på animationen
        if (seq.countdownDelay > 0f)
        {
            yield return new WaitForSeconds(seq.countdownDelay);
        }

        // Räkna ut animationens totala längd dynamiskt baserat på reella sekunder
        float animDuration = 0f;
        if (seq.countdownFrames != null && seq.framesPerSecond > 0f)
        {
            animDuration = seq.countdownFrames.Length / seq.framesPerSecond;
        }

        // Säkra att speltiden synkas perfekt i realtid oberoende av bildrutor
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
