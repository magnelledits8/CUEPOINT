
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

    // Let the HUD read when the target should be hit
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

    // Update the target timing from the manager
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

        // Get the exact time when the target should be ready
        double readyDspTime =
            musicStartDspTime + readyTimeSeconds;

        // Calculate when the target should start moving
        double movementStartDspTime =
            readyDspTime - movementDuration - chargeDuration;

        // Skip the target if its ready time has passed
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

        // Wait until it is almost time to spawn
        if (currentDspTime <
            movementStartDspTime - prewarmSeconds)
        {
            return;
        }

        // Check how far the player is from the target
        float distance = Vector3.Distance(
            playerCamera.position,
            hitPoint.position
        );

        if (distance > activationDistance)
            return;

        // Spawn the target at its starting position
        SpiralTarget3DController target = Instantiate(
            targetPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        // Make sure the target is active
        if (!target.gameObject.activeSelf)
        {
            target.gameObject.SetActive(true);
        }

        // Start the movement and animation with the music
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

        // Show the spawn position as gizmos
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(
            spawnPoint.position,
            0.12f
        );

        // Show the hit position as gizmos
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(
            hitPoint.position,
            0.12f
        );

        // Show the path between the two positions as gizmos
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            spawnPoint.position,
            hitPoint.position
        );

        // Show the spawn direction
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(
            spawnPoint.position,
            spawnPoint.forward * 0.5f
        );

        // Show the hit direction
        Gizmos.color = Color.green;
        Gizmos.DrawRay(
            hitPoint.position,
            hitPoint.forward * 0.5f
        );
    }
}
