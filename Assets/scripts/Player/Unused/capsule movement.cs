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
    [Tooltip("Ska spelaren springa automatiskt (som Subway Surfers)?")]
    public bool autoRun = true;
    [Tooltip("Hur lång tid (i sekunder) det tar för farten att rampa upp till max efter hoppet.")]
    public float speedRampDuration = 2.0f;
    [Tooltip("Hur lång fördröjning (delay) det är efter landningen innan farten börjar rampa upp.")]
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
            Debug.LogError("Spelaren behöver en Rigidbody-komponent fäst vid sig!");
        }

        originalHeightY = transform.localScale.y;
    }

    void Start()
    {
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
        
        targetPositionX = transform.position.x; 
        targetPositionZ = transform.position.z; 

        // Starta med hastighet 0 fram till att nedräkningen triggar igång oss
        currentSpeed = 0f;
        hasStartedRunning = false;
        isInStartSequence = false;
    }

    // Denna funktion kallas på inifrån MatchStartSequence-skriptet när det är 1 sek kvar!
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

        // 1. Ta ett litet steg framåt
        float stepTimer = 0f;
        float stepDuration = 0.2f; // Hur länge lilla steget varar
        while (stepTimer < stepDuration)
        {
            stepTimer += Time.deltaTime;
            targetPositionZ += 1.5f * Time.deltaTime; // Saktare fart för det lilla steget
            yield return null;
        }

        // 2. Hoppa automatiskt
        isGrounded = Physics.Raycast(transform.position, Vector3.down, (transform.localScale.y * 1.1f));
        Jump();

        // Vänta tills spelaren har landat på marken igen
        yield return new Vector3(0, 0, 0); // Litet regelbrott för frame-väntan
        while (!isGrounded)
        {
            isGrounded = Physics.Raycast(transform.position, Vector3.down, (transform.localScale.y * 1.1f));
            yield return null;
        }

        // 3. Delay efter landning
        yield return new WaitForSeconds(postJumpDelay);

        // 4. Starta den riktiga löpningen och börja rampa upp farten
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

        // Blockera alla kontroller (lanebyten, hopp, slides) tills startsekvensen är helt klar
        if (!hasStartedRunning)
        {
            // Uppdatera positioner under filmiska starten så att X-axeln och Z-axeln ändå appliceras
            Vector3 startPos = transform.position;
            startPos.x = Mathf.Lerp(startPos.x, targetPositionX, laneChangeSpeed * Time.deltaTime);
            startPos.z = targetPositionZ;
            transform.position = startPos;
            return;
        }

        // --- LANE-KONTROLLER (A & D) ---
        if (Keyboard.current.aKey.wasPressedThisFrame && !isSliding)
        {
            if (currentLane > -1) currentLane--;
        }
        if (Keyboard.current.dKey.wasPressedThisFrame && !isSliding)
        {
            if (currentLane < 1) currentLane++;
        }

        targetPositionX = spawnPosition.x + (currentLane * laneDistance);

        // --- HOPP-BUFFER ---
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

        // --- HASTIGHETS-RAMP & AUTORUN ---
        float maxSpeed = Keyboard.current.leftShiftKey.isPressed ? sprintSpeed : moveSpeed;

        if (speedRampTimer < speedRampDuration)
        {
            speedRampTimer += Time.deltaTime;
            // Rampat värde från 0 till maxfarten
            currentSpeed = Mathf.Lerp(0f, maxSpeed, speedRampTimer / speedRampDuration);
        }
        else
        {
            currentSpeed = maxSpeed;
        }

        // Spelaren springer nu framåt HELT AUTOMATISKT på den globala Z-axeln
        targetPositionZ += currentSpeed * Time.deltaTime;

        // --- APPLICERA MJUK RÖRELSE ---
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

        // Om vi rör oss framåt lägger vi till vår boost
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

        // Återställ startvärden vid respawn
        currentSpeed = 0f;
        hasStartedRunning = false;
        isInStartSequence = false;
        
        // Starta om sekvensen (eller trigga den baserat på dina behov)
        TriggerStartSequence();
    }
}
