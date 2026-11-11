
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
    public Light muzzleLight;
    public float maxLightIntensity = 5f;
    public float lightCoolDownSpeed = 15f;

    [Header("Bullet / Tracer")]
    public GameObject bulletTracerPrefab;
    public Transform muzzlePoint;
    public float bulletSpeed = 100f;
    public float bulletLifeTime = 0.2f;

    [Header("Fire Rate")]
    public float fireRate = 0.5f;

    [Header("Audio / Sound")]
    public AudioClip gunFireSound;

    [Range(0f, 1f)]
    public float volume = 0.8f;

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
                flashParticles =
                    myMuzzleFlash.GetComponentInChildren<ParticleSystem>();
            }

            myMuzzleFlash.SetActive(false);
        }

        if (muzzleLight != null)
        {
            muzzleLight.intensity = 0f;
        }
    }

    void Update()
    {
        // Shooting is only allowed while the game is active
        // and the cursor is locked to the gameplay camera.
        bool canShoot =
            Time.timeScale > 0f &&
            Cursor.lockState == CursorLockMode.Locked;

        // SHOOTING
        if (canShoot &&
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame &&
            Time.time >= nextTimeToFire)
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

            // TURN ON GLOW
            if (muzzleLight != null)
            {
                muzzleLight.intensity = maxLightIntensity;
            }

            SpawnBulletTracer();
        }

        // Hide muzzle flash after its duration.
        if (myMuzzleFlash != null &&
            myMuzzleFlash.activeSelf &&
            Time.time >= flashTimer)
        {
            myMuzzleFlash.SetActive(false);
        }

        // Cool down muzzle light.
        if (muzzleLight != null && muzzleLight.intensity > 0f)
        {
            muzzleLight.intensity = Mathf.MoveTowards(
                muzzleLight.intensity,
                0f,
                Time.deltaTime * lightCoolDownSpeed
            );
        }

        // Return recoil.
        currentRecoil = Mathf.Lerp(
            currentRecoil,
            0f,
            Time.deltaTime * returnSpeed
        );

        // Idle bobbing.
        timer += Time.deltaTime * bobSpeed;

        float bobX = Mathf.Sin(timer) * bobAmountX;
        float bobY = Mathf.Sin(timer * 2f) * bobAmountY;

        Vector3 targetPosition =
            originalPosition + new Vector3(bobX, bobY, 0f);

        Quaternion targetRotation =
            originalRotation *
            Quaternion.Euler(currentRecoil, 0f, 0f);

        transform.localPosition = targetPosition;

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            Time.deltaTime * recoilSpeed
        );
    }

    void SpawnBulletTracer()
    {
        if (bulletTracerPrefab != null && muzzlePoint != null)
        {
            GameObject tracer = Instantiate(
                bulletTracerPrefab,
                muzzlePoint.position,
                muzzlePoint.rotation
            );

            Rigidbody rb = tracer.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.linearVelocity = muzzlePoint.forward * bulletSpeed;
            }
            else
            {
                rb = tracer.AddComponent<Rigidbody>();
                rb.useGravity = false;

                tracer.AddComponent<ConstantForce>().force =
                    muzzlePoint.forward * bulletSpeed * 10f;
            }

            Destroy(tracer, bulletLifeTime);
        }
    }

    void PlayShotSound()
    {
        if (audioSource != null && gunFireSound != null)
        {
            audioSource.PlayOneShot(gunFireSound, volume);
        }
    }
}
