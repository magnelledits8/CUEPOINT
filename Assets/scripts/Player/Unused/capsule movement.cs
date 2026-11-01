using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CapsuleMovement : MonoBehaviour
{
    [Header("Laneinställningar")]
    public float laneDistance = 3f;
    public float laneChangeSpeed = 10f;
    public int currentLane = 0;

    [Header("Ggång- & Springinställningar")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 7f;
    public float currentSpeed;

    [Header("Filmisk Startsekvens (Automatisk)")]
    [Tooltip("Should the player run automatically (like Subway Surfers)?")]
    public bool autoRun = true;
    [Tooltip("How long (in seconds) it takes for the speed to ramp up to max after the jump.")]
    public float speedRampDuration = 2.0f;
    [Tooltip("The delay in seconds after landing before the speed begins to ramp up.")]
    public float postJumpDelay = 0.2f;

    private bool hasStartedRunning = false;
    private bool isInStartSequence = false;
    private float speedRampTimer = 0f;

    [Header("Hopp- & Gravitationsinställningar")]
    public float jumpHeight = 3f;
    public float jumpForwardBoost = 2f;
    public float extraGravity = 15f;

    [Header("Hopp-Reaktivitet (Queueing)")]
    public float jumpBufferTime = 0.2f;
    private float jumpBufferTimer = 0f;

    [Header("Slideinställningar")]
    public float slideSpeed = 12f;
    public float slideDuration = 0.5f;

    [Header("Respawn / Startposition")]
    public float fallThreshold = -10f;

    private Rigidbody rb;
    private bool isGrounded;
    private bool isSliding = false;
    private float slideTimer = 0f;
    private Vector3 slideDirection;

    private float originalHeightY;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;

    private float targetPositionX;
    private float targetPositionZ;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.useGravity = false;
        }
        else
        {
            Debug.LogError("The player needs a Rigidbody component attached!");
        }

        originalHeightY = transform.localScale.y;
    }

    void Start()
    {
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;

        targetPositionX = transform.position.x;
        targetPositionZ = transform.position.z;

        // Start with a speed of 0 until the countdown triggers us
        currentSpeed = 0f;
        hasStartedRunning = false;
        isInStartSequence = false;
    }

    // This function is called from inside the MatchStartSequence script when 1 second remains!
    public void TriggerStartSequence()
    {
        if (!hasStartedRunning && !isInStartSequence)
        {
            StartCoroutine(CinematicStartRoutine());
        }
    }

    IEnumerator CinematicStartRoutine()
    {
        isInStartSequence = true;

        // 1. Take a small step forward
        float stepTimer = 0f;
        float stepDuration = 0.2f; // How long the small step lasts
        while (stepTimer < stepDuration)
        {
            stepTimer += Time.deltaTime;
            targetPositionZ += 1.5f * Time.deltaTime; // Slower speed for the small step
            yield return null;
        }

        // 2. Jump automatically
        isGrounded = Physics.Raycast(transform.position, Vector3.down, (transform.localScale.y * 1.1f));
        Jump();

        // Wait until the player has landed on the ground again
        yield return new Vector3(0, 0, 0); // Minor rule break for frame-waiting
        while (!isGrounded)
        {
            isGrounded = Physics.Raycast(transform.position, Vector3.down, (transform.localScale.y * 1.1f));
            yield return null;
        }

        // 3. Delay after landing
        yield return new WaitForSeconds(postJumpDelay);

        // 4. Start actual running and begin ramping up speed
        isInStartSequence = false;
        hasStartedRunning = true;
        speedRampTimer = 0f;
    }

    void Update()
    {
        if (transform.position.y < fallThreshold)
        {
            Respawn();
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            Respawn();
        }

        isGrounded = Physics.Raycast(transform.position, Vector3.down, (transform.localScale.y * 1.1f));

        // Block all controls (lane changes, jumps, slides) until the start sequence is fully finished
        if (!hasStartedRunning)
        {
            // Update positions during the cinematic start so the X-axis and Z-axis are still applied
            Vector3 startPos = transform.position;
            startPos.x = Mathf.Lerp(startPos.x, targetPositionX, laneChangeSpeed * Time.deltaTime);
            startPos.z = targetPositionZ;
            transform.position = startPos;
            return;
        }

        // --- LANE CONTROLS (A & D) ---
        if (Keyboard.current.aKey.wasPressedThisFrame && !isSliding)
        {
            if (currentLane > -1) currentLane--;
        }
        if (Keyboard.current.dKey.wasPressedThisFrame && !isSliding)
        {
            if (currentLane < 1) currentLane++;
        }

        targetPositionX = spawnPosition.x + (currentLane * laneDistance);

        // --- JUMP BUFFER ---
        if (jumpBufferTimer > 0f) jumpBufferTimer -= Time.deltaTime;
        if (Keyboard.current.spaceKey.wasPressedThisFrame) jumpBufferTimer = jumpBufferTime;
        if (jumpBufferTimer > 0f && isGrounded) Jump();

        // --- SLIDE ---
        if (Keyboard.current.leftCtrlKey.wasPressedThisFrame && isGrounded && !isSliding) StartSlide();

        if (isSliding)
        {
            UpdateSlide();
            return;
        }

        // --- SPEED RAMP & AUTORUN ---
        float maxSpeed = Keyboard.current.leftShiftKey.isPressed ? sprintSpeed : moveSpeed;

        if (speedRampTimer < speedRampDuration)
        {
            speedRampTimer += Time.deltaTime;
            // Ramped value from 0 to max speed
            currentSpeed = Mathf.Lerp(0f, maxSpeed, speedRampTimer / speedRampDuration);
        }
        else
        {
            currentSpeed = maxSpeed;
        }

        // The player now runs forward COMPLETELY AUTOMATICALLY on the global Z-axis
        targetPositionZ += currentSpeed * Time.deltaTime;

        // --- APPLY SMOOTH MOVEMENT ---
        Vector3 newPos = transform.position;
        newPos.x = Mathf.Lerp(newPos.x, targetPositionX, laneChangeSpeed * Time.deltaTime);
        newPos.z = targetPositionZ;
        transform.position = newPos;
    }

    void FixedUpdate()
    {
        if (rb != null && !isGrounded)
        {
            rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
        }
    }

    void Jump()
    {
        jumpBufferTimer = 0f;

        float totalGravity = Mathf.Abs(Physics.gravity.y) + extraGravity;
        float jumpVelocity = Mathf.Sqrt(2f * jumpHeight * totalGravity);

        Vector3 newVelocity = new Vector3(rb.linearVelocity.x, jumpVelocity, rb.linearVelocity.z);

        // If we are moving forward, add our boost
        if (hasStartedRunning || isInStartSequence)
        {
            newVelocity.z += jumpForwardBoost;
        }

        rb.linearVelocity = newVelocity;
    }

    void StartSlide()
    {
        isSliding = true;
        slideTimer = slideDuration;
        slideDirection = Vector3.forward;

        float targetHeight = originalHeightY * 0.5f;
        float heightDifference = (originalHeightY - targetHeight) / 2f;
        transform.position = new Vector3(transform.position.x, transform.position.y - heightDifference, transform.position.z);
        transform.localScale = new Vector3(transform.localScale.x, targetHeight, transform.localScale.z);
    }

    void UpdateSlide()
    {
        slideTimer -= Time.deltaTime;

        targetPositionZ += slideSpeed * Time.deltaTime;

        Vector3 newPos = transform.position;
        newPos.x = Mathf.Lerp(newPos.x, targetPositionX, laneChangeSpeed * Time.deltaTime);
        newPos.z = targetPositionZ;
        transform.position = newPos;

        if (slideTimer <= 0f)
        {
            isSliding = false;

            float targetHeight = originalHeightY;
            float heightDifference = (targetHeight - transform.localScale.y) / 2f;
            transform.position = new Vector3(transform.position.x, transform.position.y + heightDifference, transform.position.z);
            transform.localScale = new Vector3(transform.localScale.x, originalHeightY, transform.localScale.z);
        }
    }

    public void Respawn()
    {
        if (isSliding)
        {
            isSliding = false;
            transform.localScale = new Vector3(transform.localScale.x, originalHeightY, transform.localScale.z);
        }

        currentLane = 0;
        targetPositionX = spawnPosition.x;
        targetPositionZ = spawnPosition.z;

        transform.position = spawnPosition;
        transform.rotation = spawnRotation;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Reset start values upon respawn
        currentSpeed = 0f;
        hasStartedRunning = false;
        isInStartSequence = false;

        // Restart the sequence (or trigger it based on your needs)
        TriggerStartSequence();
    }
}
