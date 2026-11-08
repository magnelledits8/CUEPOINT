
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("Shooting Settings (Invisible Logic)")]
    public float range = 100f;
    public Camera fpsCam;

    [Header("Crosshair Settings")]
    public bool showCrosshair = true;
    public Color crosshairColor = Color.green;
    public int crosshairSize = 10;
    public int crosshairThickness = 2;

    void Update()
    {
        // Draws a red line in the Scene view to help you see your invisible raycast
        if (fpsCam != null)
        {
            Debug.DrawRay(fpsCam.transform.position, fpsCam.transform.forward * range, Color.red);
        }

        // Register shot exactly when clicking (The weapon's own script handles the fire rate separately)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            ShootRaycast();
        }
    }

    void ShootRaycast()
    {
        if (fpsCam == null)
        {
            Debug.LogError("You must assign your camera to the 'Fps Cam' field on the PlayerShooting script!");
            return;
        }

        // Tracks whether this shot successfully hit a ready target
        bool successfulShot = false;

        RaycastHit hit;

        // Fire the completely invisible laser beam straight ahead from the camera
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Debug.Log("Invisible hit in 3D space: " + hit.transform.name);

            // Search for the target script on the hit object or its parent
            SpiralTarget3DController target = hit.transform.GetComponentInParent<SpiralTarget3DController>();

            if (target != null)
            {
                successfulShot = target.TryShoot();

                if (successfulShot)
                {
                    Debug.Log("Nice shot! The target explodes.");
                }
                else
                {
                    Debug.Log("You hit the target, but it's not ready yet.");
                }
            }
            else
            {
                Debug.Log("Miss! You hit something that isn't a target.");
            }
        }
        else
        {
            Debug.Log("Miss! You didn't hit anything.");
        }

        // NEW: Reset combo if the shot was not successful
        if (!successfulShot)
        {
            if (ScoreHUD.Instance != null)
            {
                ScoreHUD.Instance.RegisterMiss();
            }
        }
    }

    void OnGUI()
    {
        if (!showCrosshair || fpsCam == null) return;

        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        Texture2D texture = Texture2D.whiteTexture;
        GUI.color = crosshairColor;

        // Draw the green 2D crosshair in the center of the screen
        GUI.DrawTexture(new Rect(screenCenter.x - crosshairSize, screenCenter.y - (crosshairThickness / 2f), crosshairSize * 2, crosshairThickness), texture);
        GUI.DrawTexture(new Rect(screenCenter.x - (crosshairThickness / 2f), screenCenter.y - crosshairSize, crosshairThickness, crosshairSize * 2), texture);
    }
}
