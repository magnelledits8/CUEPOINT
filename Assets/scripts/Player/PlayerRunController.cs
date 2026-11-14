using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRunController : MonoBehaviour
{
    [Header("Lane Settings")]
    public float laneDistance = 3f;
    public float laneChangeSpeed = 10f;
    public int currentLane = 0;

    [Header("Walk & Sprint Settings")]
    public float moveSpeed = 35f;
    public float sprintSpeed = 35f;
    public float currentSpeed;

    [Header("Position-Based Intro Trigger (PRO)")]
    [Tooltip("At which exact Z-position on the track should the intro end and the script take over?")]
    public float triggerGameplayAtZ = 20.16f; // Position where the intro ends

    [Header("Smooth Acceleration AFTER Intro")]
    [Tooltip("What speed do you want the script to start at in the exact frame it takes over?")]
    public float animationEndSpeed = 22.2f; // Speed when the intro finishes
    [Tooltip("How many seconds should it take for the script and tempo to reach max speed (35)?")]
    public float rampDuration = 0.5f; // Time to reach full speed

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
        hasStartedRunning = false; // Let the animation control the player
        isInStartSequence = true;
        isRampingSpeed = false;
        currentSpeed = 0f;

        if (animator != null)
        {
            animator.enabled = true;
            animator.Play("IntroSequence"); // Play the intro animation
        }
    }

    void Update()
    {
        // Check if the player has reached the gameplay position
        if (isInStartSequence && !hasStartedRunning)
        {
            if (transform.position.z >= triggerGameplayAtZ)
            {
                SwitchToGameplay();
            }
            return; // Keep using the animation until the intro ends
        }

        if (!hasStartedRunning) return;

        float lerpFactor = 1f - Mathf.Exp(-laneChangeSpeed * Time.deltaTime);

        // Move between the three lanes with A and D
        // Enable controls after the intro
        if (Keyboard.current.aKey.wasPressedThisFrame && currentLane > -1) currentLane--;
        if (Keyboard.current.dKey.wasPressedThisFrame && currentLane < 1) currentLane++;

        targetPositionX = spawnPosition.x + (currentLane * laneDistance);
        float maxSpeed = Keyboard.current.leftShiftKey.isPressed ? sprintSpeed : moveSpeed;

        // Slowly increase the running speed
        if (isRampingSpeed)
        {
            accelerationTimer += Time.deltaTime;
            float progress = accelerationTimer / rampDuration;

            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress));
            currentSpeed = Mathf.Lerp(animationEndSpeed, maxSpeed, t);

            if (progress >= 1f)
            {
                isRampingSpeed = false; // Player has reached full speed
            }
        }
        else
        {
            currentSpeed = maxSpeed;
        }

        // Move the player forward on the Z axis
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

        // Continue moving from where the intro ended
        targetPositionZ = transform.position.z;
        accelerationTimer = 0f;
        currentSpeed = animationEndSpeed;

        // Turn off the Animator when gameplay starts
        // Let the script take over the movement
        if (animator != null)
        {
            animator.enabled = false;
        }

        Debug.Log("Z-Position reached! Script taking over control at Z: " + targetPositionZ);
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
