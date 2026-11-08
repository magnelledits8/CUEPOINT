
using UnityEngine;

/// <summary>
/// Places and schedules a target in the level.
/// The target is instantiated shortly before its music time.
/// </summary>
public class TargetCue : MonoBehaviour
{
    [Header("Target Prefab & Placement")]
    [SerializeField] private SpiralTarget3DController targetPrefab;

    [Tooltip("Where the target starts moving.")]
    [SerializeField] private Transform spawnPoint;

    [Tooltip("Where the target stops and becomes ready.")]
    [SerializeField] private Transform hitPoint;

    [Header("Music Time (Seconds from song start)")]
    [SerializeField, Min(0f)] private float readyTimeSeconds = 8.5f;
    [SerializeField, Min(0f)] private float movementDuration = 0.5f;
    [SerializeField, Min(0f)] private float chargeDuration = 0.9f;
    [SerializeField, Min(0f)] private float fadeInDuration = 0.3f;

    [Header("Just-In-Time Activation")]
    [SerializeField, Min(0.1f)] private float activationDistance = 35f;
    [SerializeField, Min(0f)] private float prewarmSeconds = 0.5f;

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

    // Called by TargetCueManager every frame.
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

        double readyDspTime =
            musicStartDspTime + readyTimeSeconds;

        double movementStartDspTime =
            readyDspTime - movementDuration - chargeDuration;

        // Do not create targets that are already too late.
        if (currentDspTime >= readyDspTime)
        {
            processed = true;

            Debug.LogWarning(
                name + ": Target was not activated in time. " +
                "Increase Activation Distance or check its music time.",
                this
            );

            return;
        }

        // Wait until the target is close to its start time.
        if (currentDspTime <
            movementStartDspTime - prewarmSeconds)
        {
            return;
        }

        // Check the player's distance from the target.
        float distance = Vector3.Distance(
            playerCamera.position,
            hitPoint.position
        );

        if (distance > activationDistance)
            return;

        // Create the target prefab.
        SpiralTarget3DController target = Instantiate(
            targetPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        // FIX: An inactive prefab creates an inactive clone.
        // Activate the clone before starting its coroutine.
        if (!target.gameObject.activeSelf)
        {
            target.gameObject.SetActive(true);
        }

        // Start movement and animation on the music timeline.
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

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(spawnPoint.position, 0.12f);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(hitPoint.position, 0.12f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            spawnPoint.position,
            hitPoint.position
        );

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(
            spawnPoint.position,
            spawnPoint.forward * 0.5f
        );

        Gizmos.color = Color.green;
        Gizmos.DrawRay(
            hitPoint.position,
            hitPoint.forward * 0.5f
        );
    }
}
