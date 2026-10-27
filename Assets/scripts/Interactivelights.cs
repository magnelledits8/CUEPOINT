using UnityEngine;
using UnityEngine.InputSystem; // Krävs för att använda det nya Input-systemet (Keyboard.current)

public class StoneInteractable : MonoBehaviour
{
    [Header("Koppla komponenter här:")]
    public Light stoneLight; // Select stone
    public BarricadeController barricade; // Select barricade

    private bool playerIsClose = false; // Checks if player is close

    // Automatic start if player touch collider
    private void OnTriggerEnter(Collider other)
    {
        // Check if player tag
        if (other.CompareTag("Player"))
        {
            playerIsClose = true; // If the player is close
        }
    }

    // Starts when player leaves the collider
    private void OnTriggerExit(Collider other)
    {
        // Check if it a player tag
        if (other.CompareTag("Player"))
        {
            playerIsClose = false; // Player no longer close
        }
    }

    // Update every frame)
    void Update()
    {
        // If player press E
        if (playerIsClose && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ActivateStone(); // Activate or deactivate stone
        }
    }

    
    void ActivateStone()
    {
        // If we connected the light
        if (stoneLight != null)
        {
            // If light is on it turns off + opposite
            stoneLight.enabled = !stoneLight.enabled;

            // If stone being activated, sends signal to barricade
            if (stoneLight.enabled)
            {
                if (barricade != null)
                {
                    barricade.LightActivated(); // Sends signal to barricade when stone is activated
                }
            }
        }
    }
}
