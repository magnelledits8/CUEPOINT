using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    [Header("Idle Bobbing")]
    public float bobSpeed = 2f;
    public float bobAmountX = 0.02f;
    public float bobAmountY = 0.015f;

    [Header("Skytte / Rekyl")]
    public float recoilAngle = 15f;
    public float recoilSpeed = 20f;
    public float returnSpeed = 8f;

    [Header("Muzzle Flash")]
    public GameObject myMuzzleFlash;
    public float flashDuration = 0.08f; // Höjd till 0.08 som standard för ökad stabilitet
    public float minFlashScale = 0.8f;
    public float maxFlashScale = 1.3f;

    [Header("Eldhastighet (Fire Rate)")]
    public float fireRate = 0.5f;

    [Header("Ljud / Audio")]
    public AudioClip gunFireSound;
    [Range(0f, 1f)] public float volume = 0.8f;
    [Tooltip("Fördröjning för ljudet i sekunder. Sätt till 0 för direkt uppspelning.")]
    public float soundDelay = 0f;

    private AudioSource audioSource;
    private ParticleSystem flashParticles; // Sparar partikelsystemet om det finns ett
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalFlashScale;
    private float currentRecoil = 0f;
    private float timer = 0f;

    private float nextTimeToFire = 0f;
    private float flashTimer = 0f;

    void Start()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.bypassEffects = true;
        audioSource.bypassListenerEffects = true;

        if (myMuzzleFlash != null)
        {
            originalFlashScale = myMuzzleFlash.transform.localScale;

            // Kolla om det finns ett partikelsystem på flamm-objektet eller dess barn
            flashParticles = myMuzzleFlash.GetComponent<ParticleSystem>();
            if (flashParticles == null)
            {
                flashParticles = myMuzzleFlash.GetComponentInChildren<ParticleSystem>();
            }

            myMuzzleFlash.SetActive(false);
        }
    }

    void Update()
    {
        // 1. SKYTTE (Vänsterklick)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + fireRate;
            currentRecoil = recoilAngle;

            // SPELA UPP LJUD
            if (audioSource != null && gunFireSound != null)
            {
                if (soundDelay > 0f)
                {
                    Invoke("PlayShotSound", soundDelay);
                }
                else
                {
                    PlayShotSound();
                }
            }

            // AKTIVERA REALISTISK MUZZLE FLASH
            if (myMuzzleFlash != null)
            {
                myMuzzleFlash.SetActive(true);

                // Slumpmässig rotation och storlek
                myMuzzleFlash.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
                float randomScaleModifier = Random.Range(minFlashScale, maxFlashScale);
                myMuzzleFlash.transform.localScale = originalFlashScale * randomScaleModifier;

                // Tvinga partikelsystemet att rensas och spelas upp på nytt exakt NU
                if (flashParticles != null)
                {
                    flashParticles.Clear();
                    flashParticles.Play();
                }

                flashTimer = Time.time + flashDuration;
            }
        }

        // Automatisk avstängning av Muzzle Flash
        if (myMuzzleFlash != null && myMuzzleFlash.activeSelf && Time.time >= flashTimer)
        {
            myMuzzleFlash.SetActive(false);
        }

        // Återpressa rekylen mjukt
        currentRecoil = Mathf.Lerp(currentRecoil, 0f, Time.deltaTime * returnSpeed);

        // 2. IDLE GUNG
        timer += Time.deltaTime * bobSpeed;
        float bobX = Mathf.Sin(timer) * bobAmountX;
        float bobY = Mathf.Sin(timer * 2f) * bobAmountY;

        Vector3 targetPosition = originalPosition + new Vector3(bobX, bobY, 0f);
        Quaternion targetRotation = originalRotation * Quaternion.Euler(currentRecoil, 0f, 0f);

        transform.localPosition = targetPosition;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * recoilSpeed);
    }

    void PlayShotSound()
    {
        audioSource.PlayOneShot(gunFireSound, volume);
    }
}
