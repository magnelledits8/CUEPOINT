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

        // Keep the player still until the countdown starts
        currentSpeed = 0f;
        hasStartedRunning = false;
        isInStartSequence = false;
    }

    // Start the player movement during the countdown
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

        // Take a small step forward
        float stepTimer = 0f;
        float stepDuration = 0.2f; // Time for the first step
        while (stepTimer < stepDuration)
        {
            stepTimer += Time.deltaTime;
            targetPositionZ += 1.5f * Time.deltaTime; // Move forward slowly
            yield return null;
        }

        // Make the player jump in intro
        isGrounded = Physics.Raycast(transform.position, Vector3.down, (transform.localScale.y * 1.1f));
        Jump();

        // Wait until the player lands
        yield return new Vector3(0, 0, 0); // Wait for the next frame
        while (!isGrounded)
        {
            isGrounded = Physics.Raycast(transform.position, Vector3.down, (transform.localScale.y * 1.1f));
            yield return null;
        }

        // Wait a little after landing
        yield return new WaitForSeconds(postJumpDelay);

        // Start running and slowly increase the speed
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

        // Disable player controls during the intro
        if (!hasStartedRunning)
        {
            // Update the player's position during the intro
            Vector3 startPos = transform.position;
            startPos.x = Mathf.Lerp(startPos.x, targetPositionX, laneChangeSpeed * Time.deltaTime);
            startPos.z = targetPositionZ;
            transform.position = startPos;
            return;
        }

        // Move between the three lanes with A and D
        if (Keyboard.current.aKey.wasPressedThisFrame && !isSliding)
        {
            if (currentLane > -1) currentLane--;
        }
        if (Keyboard.current.dKey.wasPressedThisFrame && !isSliding)
        {
            if (currentLane < 1) currentLane++;
        }

        targetPositionX = spawnPosition.x + (currentLane * laneDistance);

        // Save the jump input for a short time
        if (jumpBufferTimer > 0f) jumpBufferTimer -= Time.deltaTime;
        if (Keyboard.current.spaceKey.wasPressedThisFrame) jumpBufferTimer = jumpBufferTime;
        if (jumpBufferTimer > 0f && isGrounded) Jump();

        // Slide when pressing Left Ctrl
        if (Keyboard.current.leftCtrlKey.wasPressedThisFrame && isGrounded && !isSliding) StartSlide();

        if (isSliding)
        {
            UpdateSlide();
            return;
        }

        // Control the running speed
        float maxSpeed = Keyboard.current.leftShiftKey.isPressed ? sprintSpeed : moveSpeed;

        if (speedRampTimer < speedRampDuration)
        {
            speedRampTimer += Time.deltaTime;
            // Increase the speed from 0 to max speed
            currentSpeed = Mathf.Lerp(0f, maxSpeed, speedRampTimer / speedRampDuration);
        }
        else
        {
            currentSpeed = maxSpeed;
        }

        // Move the player forward automatically
        targetPositionZ += currentSpeed * Time.deltaTime;

        // Move smoothly between lanes
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

        // Add extra forward speed when jumping
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

        // Reset the player's speed and start settings
        currentSpeed = 0f;
        hasStartedRunning = false;
        isInStartSequence = false;

        // Start the intro sequence again
        TriggerStartSequence();
    }
}
