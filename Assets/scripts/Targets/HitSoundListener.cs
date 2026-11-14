
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class HitSoundListener : MonoBehaviour
{
    [Header("Hit Sound")]
    public AudioClip hitSound;

    [Range(0f, 1f)]
    public float volume = 0.4f;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void OnEnable()
    {
        // Listen for successful hits
        ScoreHUD.OnHitRegistered += PlayHitSound;
    }

    void OnDisable()
    {
        // Stop listening when disabled
        ScoreHUD.OnHitRegistered -= PlayHitSound;
    }

    void PlayHitSound(int points)
    {
        // Play sound when the player scores
        if (points > 0 && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound, volume);
        }
    }
}
