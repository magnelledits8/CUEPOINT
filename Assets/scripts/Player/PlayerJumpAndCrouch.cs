using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJumpAndCrouch : MonoBehaviour
{
    [Header("Jump & Gravity Settings")]
    public float jumpHeight = 3f;
    public float jumpForwardBoost = 2f;
    public float extraGravity = 15f;

    [Header("Jump Responsiveness (Queueing)")]
    public float jumpBufferTime = 0.2f;
    private float jumpBufferTimer = 0f;

    [Header("Slide Settings (UPDATED)")]
    public float slideDuration = 0.5f;

    [Header("Smooth Camera Movement (NEW)")]
    [Tooltip("Drag your Main Camera here")]
    public Transform mainCameraTransform;
    [Tooltip("How much the camera should physically drop downward during a slide (in meters)")]
    public float cameraDropAmount = 1.2f;
    [Tooltip("How smoothly/fast the camera glides down and up. Higher value = faster, lower value = smoother")]
    public float slideLerpSpeed = 10f;

    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private PlayerRunController laneMove;
    private bool isGrounded;
    private bool isSliding = false;
    private float slideTimer = 0f;

    private float originalColliderHeight;
    private Vector3 originalColliderCenter;
    private Vector3 originalCameraLocalPosition;
    private Vector3 targetCameraLocalPosition; // Keeps track of where the camera SHOULD be heading

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        laneMove = GetComponent<PlayerRunController>();

        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.useGravity = false;
        }

        if (capsuleCollider != null)
        {
            originalColliderHeight = capsuleCollider.height;
            originalColliderCenter = capsuleCollider.center;
        }

        if (mainCameraTransform != null)
        {
            originalCameraLocalPosition = mainCameraTransform.localPosition;
            targetCameraLocalPosition = originalCameraLocalPosition; // Start at original position
        }
    }

    void Update()
    {
        if (laneMove == null || !laneMove.hasStartedRunning) return;

        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);

        if (jumpBufferTimer > 0f) jumpBufferTimer -= Time.deltaTime;
        if (Keyboard.current.spaceKey.wasPressedThisFrame) jumpBufferTimer = jumpBufferTime;
        if (jumpBufferTimer > 0f && isGrounded) NormalJump();

        if (Keyboard.current.leftCtrlKey.wasPressedThisFrame && isGrounded && !isSliding) StartSlide();

        if (isSliding) UpdateSlide();

        // SMOOTH MOVEMENT: Glides the camera smoothly toward the target (either down or up) every frame
        if (mainCameraTransform != null)
        {
            mainCameraTransform.localPosition = Vector3.Lerp(
                mainCameraTransform.localPosition,
                targetCameraLocalPosition,
                Time.deltaTime * slideLerpSpeed
            );
        }
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

        // Hitbox shrinks instantly to avoid obstacles immediately upon button press
        if (capsuleCollider != null)
        {
            float targetHeight = originalColliderHeight * 0.5f;
            float heightDifference = (originalColliderHeight - targetHeight) / 2f;

            capsuleCollider.height = targetHeight;
            capsuleCollider.center = new Vector3(originalColliderCenter.x, originalColliderCenter.y - heightDifference, originalColliderCenter.z);
        }

        // Set the camera target to be DOWN
        targetCameraLocalPosition = new Vector3(
            originalCameraLocalPosition.x,
            originalCameraLocalPosition.y - cameraDropAmount,
            originalCameraLocalPosition.z
        );
    }

    void UpdateSlide()
    {
        slideTimer -= Time.deltaTime;
        // slideSpeed has been removed from here so the player moves entirely via your other script!

        if (slideTimer <= 0f)
        {
            ResetSlideAndJump();
        }
    }

    public void ResetSlideAndJump()
    {
        isSliding = false;

        // Reset hitbox
        if (capsuleCollider != null)
        {
            capsuleCollider.height = originalColliderHeight;
            capsuleCollider.center = originalColliderCenter;
        }

        // Set the camera target to go UP again
        targetCameraLocalPosition = originalCameraLocalPosition;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
