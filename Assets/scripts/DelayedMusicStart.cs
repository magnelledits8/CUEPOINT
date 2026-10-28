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
        // Stänger av Play On Awake så att låten inte tjuvstartar innan timern kört
        musicAudio.playOnAwake = false;
        musicAudio.Stop();
    }

    void Start()
    {
        // Hittar automatiskt MatchStartSequence på banan
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

        // Vänta på eventuell delay på animationen
        if (seq.countdownDelay > 0f)
        {
            yield return new WaitForSeconds(seq.countdownDelay);
        }

        // Räkna ut animationens totala längd dynamiskt
        float animDuration = 0f;
        if (seq.countdownFrames != null && seq.framesPerSecond > 0f)
        {
            animDuration = seq.countdownFrames.Length / seq.framesPerSecond;
        }

        // Räkna ut när musiken ska starta minus din överlappning
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
