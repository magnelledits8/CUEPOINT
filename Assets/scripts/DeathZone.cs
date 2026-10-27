using UnityEngine;

public class DeathZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        CapsuleMovement playerScript = other.GetComponent<CapsuleMovement>();

        if (playerScript != null)
        {
            // Kör omedelbar respawn
            playerScript.Respawn();
        }
    }
}
