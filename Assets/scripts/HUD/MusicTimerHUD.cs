using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MusicTimerHUD : MonoBehaviour
{
    [Header("Music playing in Level1")]
    [SerializeField] private AudioSource musicSource;

    [Header("Objects in Canvas")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Image remainingRing;
    [SerializeField] private Image innerLight;

    private bool musicHasPlayed;

    private void Update()
    {
        if (musicSource == null || musicSource.clip == null)
            return;

        AudioClip clip = musicSource.clip;
        int totalSamples = clip.samples;
        if (totalSamples <= 0)
            return;

        if (musicSource.isPlaying)
            musicHasPlayed = true;

        int currentSample = Mathf.Clamp(musicSource.timeSamples, 0, totalSamples);

        // Keep the timer at the end when the music stops
        if (musicHasPlayed && !musicSource.isPlaying && currentSample == 0)
            currentSample = totalSamples;

        float elapsed01 = Mathf.Clamp01((float)currentSample / totalSamples);
        float remaining01 = 1f - elapsed01;
        float remainingSeconds = clip.length * remaining01;

        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(remainingSeconds);
            timerText.text = $"{seconds / 60:00}:{seconds % 60:00}";
        }

        if (remainingRing != null)
            remainingRing.fillAmount = remaining01;

        if (innerLight != null)
            innerLight.fillAmount = elapsed01;
    }
}
