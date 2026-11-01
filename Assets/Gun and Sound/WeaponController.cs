using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    [Header("Idle Bobbing")]
    public float bobSpeed = 2f;
    public float bobAmountX = 0.02f;
    public float bobAmountY = 0.015f;

    [Header("Shooting / Recoil")]
    public float recoilAngle = 15f;
    public float recoilSpeed = 20f;
    public float returnSpeed = 8f;

    [Header("Muzzle Flash (Fully Static)")]
    public GameObject myMuzzleFlash;
    public float flashDuration = 0.05f;

    [Header("Dynamic Glow (3D Glow)")]
    public Light muzzleLight;            // Drag your Point Light here
    public float maxLightIntensity = 5f; // Initial brightness intensity
    public float lightCoolDownSpeed = 15f; // How fast the glow cools down and disappears

    [Header("Bullet / Tracer")]
    public GameObject bulletTracerPrefab; // Create a small 3D capsule/sphere and drag it here
    public Transform muzzlePoint;         // The point where the bullet starts (tip of the barrel)
    public float bulletSpeed = 100f;      // How fast the bullet flies forward
    public float bulletLifeTime = 0.2f;   // How long the bullet is visible before being destroyed

    [Header("Fire Rate")]
    public float fireRate = 0.5f;

    [Header("Audio / Sound")]
    public AudioClip gunFireSound;
    [Range(0f, 1f)] public float volume = 0.8f;
    [Tooltip("Sound delay in seconds. Set to 0 for instant playback.")]
    public float soundDelay = 0f;

    private AudioSource audioSource;
    private ParticleSystem flashParticles;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
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
            flashParticles = myMuzzleFlash.GetComponent<ParticleSystem>();
            if (flashParticles == null)
            {
                flashParticles = myMuzzleFlash.GetComponentInChildren<ParticleSystem>();
            }
            myMuzzleFlash.SetActive(false);
        }

        // Forces the light to be completely turned off when the game starts
        if (muzzleLight != null)
        {
            muzzleLight.intensity = 0f;
        }
    }

    void Update()
    {
        // 1. SHOOTING (Left Click)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + fireRate;
            currentRecoil = recoilAngle;

            // PLAY SOUND
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

            // ACTIVATE MUZZLE FLASH
            if (myMuzzleFlash != null)
            {
                myMuzzleFlash.SetActive(true);

                if (flashParticles != null)
                {
                    flashParticles.Clear();
                    flashParticles.Play();
                }

                flashTimer = Time.time + flashDuration;
            }

            // TURN ON GLOW (Point Light triggers instantly)
            if (muzzleLight != null)
            {
                muzzleLight.intensity = maxLightIntensity;
            }

            // SPAWN BULLET (Tracer)
            SpawnBulletTracer();
        }

        // Automatic deactivation of the 2D flash
        if (myMuzzleFlash != null && myMuzzleFlash.activeSelf && Time.time >= flashTimer)
        {
            myMuzzleFlash.SetActive(false);
        }

        // COOLING GLOW: Smoothly fades out the Point Light to 0 over time
        if (muzzleLight != null && muzzleLight.intensity > 0f)
        {
            muzzleLight.intensity = Mathf.MoveTowards(muzzleLight.intensity, 0f, Time.deltaTime * lightCoolDownSpeed);
        }

        // Smoothly return recoil
        currentRecoil = Mathf.Lerp(currentRecoil, 0f, Time.deltaTime * returnSpeed);

        // 2. IDLE BOBBING
        timer += Time.deltaTime * bobSpeed;
        float bobX = Mathf.Sin(timer) * bobAmountX;
        float bobY = Mathf.Sin(timer * 2f) * bobAmountY;

        Vector3 targetPosition = originalPosition + new Vector3(bobX, bobY, 0f);
        Quaternion targetRotation = originalRotation * Quaternion.Euler(currentRecoil, 0f, 0f);

        transform.localPosition = targetPosition;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * recoilSpeed);
    }

    void SpawnBulletTracer()
    {
        if (bulletTracerPrefab != null && muzzlePoint != null)
        {
            // Spawns the bullet at the muzzle
            GameObject tracer = Instantiate(bulletTracerPrefab, muzzlePoint.position, muzzlePoint.rotation);

            // Gives the bullet forward velocity (in the muzzle's forward direction)
            Rigidbody rb = tracer.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = muzzlePoint.forward * bulletSpeed;
            }
            else
            {
                // If there is no Rigidbody on the bullet, we add it safely because ConstantForce requires it
                rb = tracer.AddComponent<Rigidbody>();
                rb.useGravity = false; // Prevent the bullet from falling down

                tracer.AddComponent<ConstantForce>().force = muzzlePoint.forward * bulletSpeed * 10f;
            }

            // Automatically destroy the bullet after a fraction of a second so it doesn't fly forever
            Destroy(tracer, bulletLifeTime);
        }
    }

    void PlayShotSound()
    {
        audioSource.PlayOneShot(gunFireSound, volume);
    }
}
