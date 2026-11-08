
using UnityEngine;

public class TargetCue : MonoBehaviour
{
    [Header("Target Prefab & Placement")]
    [SerializeField] private SpiralTarget3DController targetPrefab;

    [Tooltip("Where the target begins moving.")]
    [SerializeField] private Transform spawnPoint;

    [Tooltip("Where the target stops and becomes ready.")]
    [SerializeField] private Transform hitPoint;

    [Header("Music Time (Seconds from song start)")]

    [Tooltip("The song time when the target is ready to shoot.")]
    [SerializeField, Min(0f)]
    private float readyTimeSeconds = 8.5f;

    [SerializeField, Min(0f)]
    private float movementDuration = 0.5f;

    [SerializeField, Min(0f)]
    private float chargeDuration = 0.9f;

    [SerializeField, Min(0f)]
    private float fadeInDuration = 0.3f;

    [Header("Just-In-Time Activation")]

    [Tooltip("Maximum distance from player before target activation.")]
    [SerializeField, Min(0.1f)]
    private float activationDistance = 35f;

    [Tooltip("Seconds before movement starts that the target can be created.")]
    [SerializeField, Min(0f)]
    private float prewarmSeconds = 0.5f;

    // NEW: The HUD can read the target's scheduled hit time.
    public float ReadyTimeSeconds
    {
        get { return readyTimeSeconds; }
    }

    private Transform playerCamera;
    private double musicStartDspTime;
    private bool timelineConfigured;
    private bool processed;

    public void Configure(
        Transform cameraTransform,
        double songStartDspTime)
    {
        playerCamera = cameraTransform;
        musicStartDspTime = songStartDspTime;

        timelineConfigured = true;
        processed = false;
    }

    // Called automatically by TargetCueManager.
    public void Tick(double currentDspTime)
    {
        if (!timelineConfigured || processed)
            return;

        if (targetPrefab == null ||
            spawnPoint == null ||
            hitPoint == null ||
            playerCamera == null)
        {
            return;
        }

        // Exact DSP time when target should be ready.
        double readyDspTime =
            musicStartDspTime + readyTimeSeconds;

        // Calculate when movement must begin.
        double movementStartDspTime =
            readyDspTime - movementDuration - chargeDuration;

        // Skip targets that have already missed their activation time.
        if (currentDspTime >= readyDspTime)
        {
            processed = true;

            Debug.LogWarning(
                name + ": Target was not activated in time. " +
                "Increase Activation Distance or check music time.",
                this
            );

            return;
        }

        // Wait until the target is close to its movement time.
        if (currentDspTime <
            movementStartDspTime - prewarmSeconds)
        {
            return;
        }

        // Check the player's distance to the target.
        float distance = Vector3.Distance(
            playerCamera.position,
            hitPoint.position
        );

        if (distance > activationDistance)
            return;

        // Create the target at its Spawn Point.
        SpiralTarget3DController target = Instantiate(
            targetPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        // Make sure the new target is active.
        if (!target.gameObject.activeSelf)
        {
            target.gameObject.SetActive(true);
        }

        // Start movement and animation with music timing.
        target.BeginScheduledSequence(
            spawnPoint.position,
            spawnPoint.rotation,
            hitPoint.position,
            hitPoint.rotation,
            movementStartDspTime,
            movementDuration,
            chargeDuration,
            fadeInDuration
        );

        processed = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (spawnPoint == null || hitPoint == null)
            return;

        // Spawn position.
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(
            spawnPoint.position,
            0.12f
        );

        // Hit position.
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(
            hitPoint.position,
            0.12f
        );

        // Movement path.
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            spawnPoint.position,
            hitPoint.position
        );

        // Spawn rotation.
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(
            spawnPoint.position,
            spawnPoint.forward * 0.5f
        );

        // Hit rotation.
        Gizmos.color = Color.green;
        Gizmos.DrawRay(
            hitPoint.position,
            hitPoint.forward * 0.5f
        );
    }
}
