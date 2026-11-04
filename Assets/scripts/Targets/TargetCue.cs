using UnityEngine;

/// <summary>
/// A lightweight cue marker in the level timeline. The actual target prefab is only
/// instantiated when the player is close enough and the cue is near its music time.
/// </summary>
public class TargetCue : MonoBehaviour
{
    [Header("Target Prefab & Placement")]
    [SerializeField] private SpiralTarget3DController targetPrefab;
    [Tooltip("Where the target begins its flight. The rotation of this object determines the starting angle.")]
    [SerializeField] private Transform spawnPoint;
    [Tooltip("Where the target stops and becomes ready. The rotation of this object determines the final angle.")]
    [SerializeField] private Transform hitPoint;

    [Header("Music Time (Seconds from song start)")]
    [Tooltip("The exact song time in seconds when the target should be stationary on the Hit Point and ready to be shot.")]
    [SerializeField, Min(0f)] private float readyTimeSeconds = 8.5f;
    [SerializeField, Min(0f)] private float movementDuration = 0.5f;
    [SerializeField, Min(0f)] private float chargeDuration = 0.9f;
    [SerializeField, Min(0f)] private float fadeInDuration = 0.3f;

    [Header("Just-In-Time Activation")]
    [Tooltip("The target prefab is not instantiated until the player camera is within this distance from the hitPoint.")]
    [SerializeField, Min(0.1f)] private float activationDistance = 35f;
    [Tooltip("How many seconds before the movement starts that the prefab is allowed to instantiate and pre-warm.")]
    [SerializeField, Min(0f)] private float prewarmSeconds = 0.5f;

    private Transform playerCamera;
    private double musicStartDspTime;
    private bool timelineConfigured;
    private bool processed;

    public void Configure(Transform cameraTransform, double songStartDspTime)
    {
        playerCamera = cameraTransform;
        musicStartDspTime = songStartDspTime;
        timelineConfigured = true;
        processed = false;
    }

    /// <summary>Called automatically by TargetCueManager every frame.</summary>
    public void Tick(double currentDspTime)
    {
        if (!timelineConfigured || processed)
            return;

        if (targetPrefab == null || spawnPoint == null || hitPoint == null || playerCamera == null)
            return;

        double readyDspTime = musicStartDspTime + readyTimeSeconds;
        double movementStartDspTime = readyDspTime - movementDuration - chargeDuration;

        // If the player missed the window to activate the target in time, skip it
        if (currentDspTime >= readyDspTime)
        {
            processed = true;
            Debug.LogWarning(name + ": Target was not activated in time. Increase Activation Distance or check its music time.", this);
            return;
        }

        // Wait until it is time to pre-warm the prefab
        if (currentDspTime < movementStartDspTime - prewarmSeconds)
            return;

        // Check the current distance to the player camera
        float distance = Vector3.Distance(playerCamera.position, hitPoint.position);
        if (distance > activationDistance)
            return;

        // Instantiate the target using the exact starting position and rotation of the spawnPoint
        SpiralTarget3DController target = Instantiate(
            targetPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        // Begin the sequence perfectly synchronized with the audio DSP clock
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
        Gizmos.DrawLine(spawnPoint.position, hitPoint.position);

        // Draws small arrows in the Scene view to visualize the rotation alignment
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(spawnPoint.position, spawnPoint.forward * 0.5f);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(hitPoint.position, hitPoint.forward * 0.5f);
    }
}
