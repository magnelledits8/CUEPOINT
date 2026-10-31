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

    [Header("Muzzle Flash (Helt Statisk)")]
    public GameObject myMuzzleFlash;
    public float flashDuration = 0.05f; 

    [Header("Dynamisk Ljusglöd (3D Glow)")]
    public Light muzzleLight;            // Dra in ditt Point Light här
    public float maxLightIntensity = 5f; // Hur starkt det lyser i början
    public float lightCoolDownSpeed = 15f; // Hur snabbt glöden svalnar och försvinner

    [Header("Kula / Spårljus (Tracer)")]
    public GameObject bulletTracerPrefab; // Skapa en liten 3D-kapsel/sfär och dra in här
    public Transform muzzlePoint;         // Punkten där kulan startar (längst fram på pipan)
    public float bulletSpeed = 100f;      // Hur snabbt kulan flyger framåt
    public float bulletLifeTime = 0.2f;   // Hur länge man hinner se kulan innan den tas bort

    [Header("Eldhastighet (Fire Rate)")]
    public float fireRate = 0.5f;

    [Header("Ljud / Audio")]
    public AudioClip gunFireSound;
    [Range(0f, 1f)] public float volume = 0.8f;
    [Tooltip("Fördröjning för ljudet i sekunder. Sätt till 0 för direkt uppspelning.")]
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

        // Tvingar ljuset att vara helt avstängt/släckt när spelet startar
        if (muzzleLight != null)
        {
            muzzleLight.intensity = 0f;
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

            // AKTIVERA MUZZLE FLASH
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

            // TÄND GLÖDEN (Point Light slår till direkt)
            if (muzzleLight != null)
            {
                muzzleLight.intensity = maxLightIntensity;
            }

            // SKJUT UT KULAN (Tracer)
            SpawnBulletTracer();
        }

        // Automatisk avstängning av 2D-flamman
        if (myMuzzleFlash != null && myMuzzleFlash.activeSelf && Time.time >= flashTimer)
        {
            myMuzzleFlash.SetActive(false);
        }

        // GLÖD SOM SVALNAR: Tonar ner Point Light mjukt till 0 över tid
        if (muzzleLight != null && muzzleLight.intensity > 0f)
        {
            muzzleLight.intensity = Mathf.MoveTowards(muzzleLight.intensity, 0f, Time.deltaTime * lightCoolDownSpeed);
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

    void SpawnBulletTracer()
    {
        if (bulletTracerPrefab != null && muzzlePoint != null)
        {
            // Skapar kulan vid pipan
            GameObject tracer = Instantiate(bulletTracerPrefab, muzzlePoint.position, muzzlePoint.rotation);
            
            // Ger kulan fart framåt (i pipans riktning)
            Rigidbody rb = tracer.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = muzzlePoint.forward * bulletSpeed;
            }
            else
            {
                // Om du inte har en Rigidbody på kulan så flyttar vi den via kod istället
                Destroy(tracer.AddComponent<Rigidbody>()); // Säkerhetsåtgärd
                tracer.AddComponent<ConstantForce>().force = muzzlePoint.forward * bulletSpeed * 10f;
            }

            // Förstör kulan automatiskt efter en bråkdel av en sekund så den inte flyger för evigt
            Destroy(tracer, bulletLifeTime);
        }
    }

    void PlayShotSound()
    {
        audioSource.PlayOneShot(gunFireSound, volume);
    }
}
