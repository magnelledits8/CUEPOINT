using UnityEngine;

public class SlopeForceTrigger : MonoBehaviour
{
    [Header("Slope Settings")]
    [Tooltip("How strongly the floor should pull the player down to prevent bouncing.")]
    public float downForce = 25f;

    [Tooltip("The vertical position offset adjustment if needed.")]
    public float yOffset = 0f;

    private void OnTriggerStay(Collider other)
    {
        // Check if the player is on the slope
        if (other.CompareTag("Player"))
        {
            Rigidbody playerRb = other.GetComponent<Rigidbody>();

            if (playerRb != null)
            {
                // Keep the player moving down the slope
                // Stop the player from bouncing into the air
                Vector3 currentVelocity = playerRb.linearVelocity;

                // Push the player down on the Y axis
                playerRb.linearVelocity = new Vector3(currentVelocity.x, -downForce, currentVelocity.z);
            }
        }
    }
}
