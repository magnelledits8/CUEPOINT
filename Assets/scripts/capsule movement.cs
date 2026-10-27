using UnityEngine;
using UnityEngine.InputSystem;

public class CapsuleMovement : MonoBehaviour
{
    [Header("Gång- & Springinställningar")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 7f;
    public float currentSpeed;

    [Header("Hopp- & Gravitationsinställningar")]
    [Tooltip("Hur högt karaktären hoppar.")]
    public float jumpHeight = 3f;
    [Tooltip("Hur mycket extra fart framåt spelaren får vid hopp (hopplängd).")]
    public float jumpForwardBoost = 2f;
    [Tooltip("Extra gravitation för att spelaren inte ska sväva för länge. Högre värde = snabbare fall.")]
    public float extraGravity = 15f;

    [Header("Slideinställningar")]
    [Tooltip("Hur snabbt spelaren glider.")]
    public float slideSpeed = 12f;
    [Tooltip("Hur många sekunder sliden varar (längd).")]
    public float slideDuration = 0.5f;

    [Header("Respawn / Startposition")]
    [Tooltip("Om spelaren faller under denna Y-höjd i världen, spawnar den om automatiskt.")]
    public float fallThreshold = -10f;

    private Rigidbody rb;
    private bool isGrounded;
    private bool isSliding = false;
    private float slideTimer = 0f;
    private Vector3 slideDirection;

    private float originalHeightY;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;

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

        if (Keyboard.current.leftCtrlKey.wasPressedThisFrame && isGrounded && !isSliding)
        {
            StartSlide();
        }

        if (isSliding)
        {
            UpdateSlide();
            return;
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            Jump();
        }

        if (Keyboard.current.leftShiftKey.isPressed)
        {
            currentSpeed = sprintSpeed;
        }
        else
        {
            currentSpeed = moveSpeed;
        }

        float step = currentSpeed * Time.deltaTime;

        if (Keyboard.current.wKey.isPressed) { transform.Translate(0, 0, step); }
        if (Keyboard.current.sKey.isPressed) { transform.Translate(0, 0, -step); }
        if (Keyboard.current.aKey.isPressed) { transform.Translate(-step, 0, 0); }
        if (Keyboard.current.dKey.isPressed) { transform.Translate(step, 0, 0); }
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
        float totalGravity = Mathf.Abs(Physics.gravity.y) + extraGravity;
        float jumpVelocity = Mathf.Sqrt(2f * jumpHeight * totalGravity);

        Vector3 newVelocity = new Vector3(rb.linearVelocity.x, jumpVelocity, rb.linearVelocity.z);

        if (Keyboard.current.wKey.isPressed)
        {
            newVelocity += transform.forward * jumpForwardBoost;
        }

        rb.linearVelocity = newVelocity;
    }

    void StartSlide()
    {
        isSliding = true;
        slideTimer = slideDuration;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f))
        {
            slideDirection = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;
        }
        else
        {
            slideDirection = transform.forward;
        }

        float targetHeight = originalHeightY * 0.5f;
        float heightDifference = (originalHeightY - targetHeight) / 2f;
        transform.position = new Vector3(transform.position.x, transform.position.y - heightDifference, transform.position.z);

        transform.localScale = new Vector3(transform.localScale.x, targetHeight, transform.localScale.z);
    }

    void UpdateSlide()
    {
        slideTimer -= Time.deltaTime;
        transform.Translate(slideDirection * slideSpeed * Time.deltaTime, Space.World);

        if (slideTimer <= 0f)
        {
            isSliding = false;

            float targetHeight = originalHeightY;
            float heightDifference = (targetHeight - transform.localScale.y) / 2f;
            transform.position = new Vector3(transform.position.x, transform.position.y + heightDifference, transform.position.z);

            transform.localScale = new Vector3(transform.localScale.x, originalHeightY, transform.localScale.z);
        }
    }

    // Denna funktion kallas på från DeathZone
    public void Respawn()
    {
        if (isSliding)
        {
            isSliding = false;
            transform.localScale = new Vector3(transform.localScale.x, originalHeightY, transform.localScale.z);
        }

        transform.position = spawnPosition;
        transform.rotation = spawnRotation;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log("Spelaren startade om via Respawn!");
    }
}
