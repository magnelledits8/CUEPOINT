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
        // Check if the object entering the slope is the player
        if (other.CompareTag("Player"))
        {
            Rigidbody playerRb = other.GetComponent<Rigidbody>();

            if (playerRb != null)
            {
                // We apply a direct downward velocity or force to lock the player to the slope.
                // This counters the forward air-time momentum from the running script.
                Vector3 currentVelocity = playerRb.linearVelocity;

                // Push the player firmly downward along the Y axis
                playerRb.linearVelocity = new Vector3(currentVelocity.x, -downForce, currentVelocity.z);
            }
        }
    }
}
