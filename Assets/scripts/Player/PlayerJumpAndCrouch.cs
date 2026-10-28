using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJumpAndCrouch : MonoBehaviour
{
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

    private Rigidbody rb;
    private PlayerLaneMovement laneMove;
    private bool isGrounded;
    private bool isSliding = false;
    private float slideTimer = 0f;
    private float originalHeightY;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        laneMove = GetComponent<PlayerLaneMovement>();

        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.useGravity = false;
        }
        originalHeightY = transform.localScale.y;
    }

    void Update()
    {
        // Om spelet eller introt inte är igång rullar vi inte
        if (laneMove == null || !laneMove.hasStartedRunning) return;

        isGrounded = Physics.Raycast(transform.position, Vector3.down, (transform.localScale.y * 1.1f));

        // Hopp-buffert
        if (jumpBufferTimer > 0f) jumpBufferTimer -= Time.deltaTime;
        if (Keyboard.current.spaceKey.wasPressedThisFrame) jumpBufferTimer = jumpBufferTime;
        if (jumpBufferTimer > 0f && isGrounded) NormalJump();

        // Slide-logik
        if (Keyboard.current.leftCtrlKey.wasPressedThisFrame && isGrounded && !isSliding) StartSlide();

        if (isSliding) UpdateSlide();
    }

    void FixedUpdate()
    {
        if (rb != null && !isGrounded)
        {
            rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
        }
    }

    public void NormalJump()
    {
        jumpBufferTimer = 0f;
        float totalGravity = Mathf.Abs(Physics.gravity.y) + extraGravity;
        float jumpVelocity = Mathf.Sqrt(2f * jumpHeight * totalGravity);
        Vector3 newVelocity = new Vector3(rb.linearVelocity.x, jumpVelocity, rb.linearVelocity.z);

        if (Keyboard.current.wKey.isPressed) newVelocity.z += jumpForwardBoost;
        rb.linearVelocity = newVelocity;
    }

    void StartSlide()
    {
        isSliding = true;
        slideTimer = slideDuration;
        float targetHeight = originalHeightY * 0.5f;
        float heightDifference = (originalHeightY - targetHeight) / 2f;
        transform.position = new Vector3(transform.position.x, transform.position.y - heightDifference, transform.position.z);
        transform.localScale = new Vector3(transform.localScale.x, targetHeight, transform.localScale.z);
    }

    void UpdateSlide()
    {
        slideTimer -= Time.deltaTime;
        if (laneMove != null) laneMove.targetPositionZ += slideSpeed * Time.deltaTime;

        if (slideTimer <= 0f)
        {
            isSliding = false;
            float targetHeight = originalHeightY;
            float heightDifference = (targetHeight - transform.localScale.y) / 2f;
            transform.position = new Vector3(transform.position.x, transform.position.y + heightDifference, transform.position.z);
            transform.localScale = new Vector3(transform.localScale.x, originalHeightY, transform.localScale.z);
        }
    }

    public void ResetSlideAndJump()
    {
        isSliding = false;
        transform.localScale = new Vector3(transform.localScale.x, originalHeightY, transform.localScale.z);
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}