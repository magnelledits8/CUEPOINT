
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CuepointTimeRuler : MonoBehaviour
{
    [Header("Player Position Calibration")]
    [Tooltip("A known time in the gameplay song.")]
    public float referenceMusicTime = 10f;

    [Tooltip("Player Z position at the reference music time.")]
    public float playerZAtReferenceTime = 324.93f;

    [Tooltip("Player forward speed after the intro.")]
    public float playerSpeed = 35f;

    [Header("Target Placement")]
    [Tooltip("How far ahead of the player the target should be at Ready Time.")]
    public float shootingDistance = 15f;

    public float markerHeight = 1.5f;
    public float corridorHalfWidth = 5f;
    public float labelPositionX = 7f;

    [Header("Music Timeline")]
    public float firstMusicSecond = 5f;
    public float lastMusicSecond = 120f;

    [Tooltip("Distance in seconds between ruler markers.")]
    [Min(0.25f)]
    public float secondsBetweenMarkers = 5f;

    [Header("Visual Settings")]
    public bool showTargetPlacement = true;
    public bool showPlayerPosition = false;

    private void OnDrawGizmos()
    {
        if (playerSpeed <= 0f ||
            secondsBetweenMarkers <= 0f)
        {
            return;
        }

        for (
            float songTime = firstMusicSecond;
            songTime <= lastMusicSecond + 0.001f;
            songTime += secondsBetweenMarkers
        )
        {
            // Estimated player position at this song time.
            float playerZ =
                playerZAtReferenceTime +
                (songTime - referenceMusicTime) *
                playerSpeed;

            // Suggested position for a target's Hit Point.
            float targetZ = playerZ + shootingDistance;

            if (showTargetPlacement)
            {
                Gizmos.color = new Color(
                    1f, 0.65f, 0.2f, 0.85f
                );

                Vector3 left = new Vector3(
                    -corridorHalfWidth,
                    markerHeight,
                    targetZ
                );

                Vector3 right = new Vector3(
                    corridorHalfWidth,
                    markerHeight,
                    targetZ
                );

                Gizmos.DrawLine(left, right);

                Gizmos.DrawSphere(
                    new Vector3(0f, markerHeight, targetZ),
                    0.25f
                );

#if UNITY_EDITOR
                Handles.color = Color.white;

                Handles.Label(
                    new Vector3(
                        labelPositionX,
                        markerHeight,
                        targetZ
                    ),
                    "READY " + songTime.ToString("F1") +
                    "s  |  Hit Z: " + targetZ.ToString("F1")
                );
#endif
            }

            if (showPlayerPosition)
            {
                Gizmos.color = Color.cyan;

                Gizmos.DrawLine(
                    new Vector3(
                        -corridorHalfWidth,
                        markerHeight,
                        playerZ
                    ),
                    new Vector3(
                        corridorHalfWidth,
                        markerHeight,
                        playerZ
                    )
                );
            }
        }
    }
}
