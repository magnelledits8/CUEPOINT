using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRunController : MonoBehaviour
{
    [Header("Laneinställningar")]
    public float laneDistance = 3f;
    public float laneChangeSpeed = 10f;
    public int currentLane = 0;

    [Header("Gång- & Springinställningar")]
    public float moveSpeed = 35f;
    public float sprintSpeed = 35f;
    public float currentSpeed;

    [Header("Positionsbaserad Intro-trigg (PROFFS)")]
    [Tooltip("Vid vilken exakt Z-position på banan ska introt ta slut och koden ta över?")]
    public float triggerGameplayAtZ = 20.16f; // Satt till ditt exakta värde från bilden!

    [Header("Mjuk acceleration EFTER introt")]
    [Tooltip("Vilken fart vill du att koden ska börja på i exakt den frame den tar över?")]
    public float animationEndSpeed = 22.2f; // Satt till ditt värde från bilden!
    [Tooltip("Hur många sekunder ska det ta för koden och tempot att nå maxfart (35)?")]
    public float rampDuration = 0.5f; // Optimerat värde för ett hypersnabbt och rappt flyt!

    private float accelerationTimer = 0f;
    private float targetPositionX;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private Animator animator;

    [HideInInspector] public bool hasStartedRunning = false;
    [HideInInspector] public bool isInStartSequence = false;
    [HideInInspector] public float targetPositionZ;
    [HideInInspector] public bool isRampingSpeed = false;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
        ResetMovement();
    }

    public void StartIntroSequence()
    {
        ResetMovement();
        hasStartedRunning = false; // Koden flyttar inte spelaren under introt
        isInStartSequence = true;
        isRampingSpeed = false;
        currentSpeed = 0f;

        if (animator != null)
        {
            animator.enabled = true;
            animator.Play("IntroSequence"); // Spelar din intro-animation (steg -> hopp -> landning)
        }
    }

    void Update()
    {
        // 1. INTRO-FAS: Kollar varje bildruta om din animation har flyttat spelaren till Z: 20.16
        if (isInStartSequence && !hasStartedRunning)
        {
            if (transform.position.z >= triggerGameplayAtZ)
            {
                SwitchToGameplay();
            }
            return; // Låt animationen sköta allt tills vi passerat mållinjen
        }

        if (!hasStartedRunning) return;

        float lerpFactor = 1f - Mathf.Exp(-laneChangeSpeed * Time.deltaTime);

        // --- ORDINARIE SPELKONTROLLER (A & D) ---
        // Aktiveras automatiskt här så fort introt är över!
        if (Keyboard.current.aKey.wasPressedThisFrame && currentLane > -1) currentLane--;
        if (Keyboard.current.dKey.wasPressedThisFrame && currentLane < 1) currentLane++;

        targetPositionX = spawnPosition.x + (currentLane * laneDistance);
        float maxSpeed = Keyboard.current.leftShiftKey.isPressed ? sprintSpeed : moveSpeed;

        // --- MJUK KOD-ACCELERATION (RAMP UP) ---
        if (isRampingSpeed)
        {
            accelerationTimer += Time.deltaTime;
            float progress = accelerationTimer / rampDuration;

            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress));
            currentSpeed = Mathf.Lerp(animationEndSpeed, maxSpeed, t);

            if (progress >= 1f)
            {
                isRampingSpeed = false; // Rampen klar! Spelaren är nu uppe i 35 i fart
            }
        }
        else
        {
            currentSpeed = maxSpeed;
        }

        // Flytta spelaren spikrakt framåt på Z-axeln via kod
        targetPositionZ += currentSpeed * Time.deltaTime;

        Vector3 newPos = transform.position;
        newPos.x = Mathf.Lerp(newPos.x, targetPositionX, lerpFactor);
        newPos.z = targetPositionZ;
        transform.position = newPos;
    }

    private void SwitchToGameplay()
    {
        isInStartSequence = false;
        hasStartedRunning = true;
        isRampingSpeed = true;

        // Lås kodens målbana till exakt den koordinat där din animation stannade
        targetPositionZ = transform.position.z;
        accelerationTimer = 0f;
        currentSpeed = animationEndSpeed;

        // CLEAN ÖVERGÅNG: Vi stänger av Animatorn helt här. 
        // Eftersom koden tar över i exakt samma koordinat och fortsätter framåt, blir det helt sömlöst!
        if (animator != null)
        {
            animator.enabled = false;
        }

        Debug.Log("Z-Position nådd! Skripten tar över kontrollen vid Z: " + targetPositionZ);
    }

    public void ResetMovement()
    {
        currentLane = 0;
        targetPositionX = spawnPosition.x;
        targetPositionZ = spawnPosition.z;
        transform.position = spawnPosition;
        transform.rotation = spawnRotation;
        currentSpeed = 0f;
        accelerationTimer = 0f;
        hasStartedRunning = false;
        isInStartSequence = false;
        isRampingSpeed = false;

        if (animator != null)
        {
            animator.enabled = true;
        }
    }
}
