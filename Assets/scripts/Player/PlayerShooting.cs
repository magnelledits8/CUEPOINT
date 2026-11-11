
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

    [Header("Pause Protection")]
    [Tooltip("Delay before shooting is enabled after closing the pause menu.")]
    public float resumeShootDelay = 0.15f;

    private bool wasPaused = false;
    private bool waitForMouseRelease = false;
    private float resumeTime = 0f;

    void Update()
    {
        if (fpsCam != null)
        {
            Debug.DrawRay(
                fpsCam.transform.position,
                fpsCam.transform.forward * range,
                Color.red
            );
        }

        // Never shoot while the pause menu is open.
        if (Time.timeScale == 0f)
        {
            wasPaused = true;
            waitForMouseRelease = true;
            return;
        }

        // Detect when the game has just resumed.
        if (wasPaused)
        {
            wasPaused = false;
            resumeTime = Time.unscaledTime;
        }

        if (Mouse.current == null)
        {
            return;
        }

        // Ignore the Continue click until released.
        if (waitForMouseRelease)
        {
            if (Mouse.current.leftButton.isPressed)
            {
                return;
            }

            waitForMouseRelease = false;
        }

        // Small extra delay after resuming.
        if (Time.unscaledTime - resumeTime < resumeShootDelay)
        {
            return;
        }

        // Normal shooting input.
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            ShootRaycast();
        }
    }

    void ShootRaycast()
    {
        if (fpsCam == null)
        {
            Debug.LogError(
                "You must assign your camera to the 'Fps Cam' field on the PlayerShooting script!"
            );
            return;
        }

        bool successfulShot = false;

        RaycastHit hit;

        if (Physics.Raycast(
            fpsCam.transform.position,
            fpsCam.transform.forward,
            out hit,
            range))
        {
            Debug.Log(
                "Invisible hit in 3D space: " +
                hit.transform.name
            );

            SpiralTarget3DController target =
                hit.transform.GetComponentInParent<
                    SpiralTarget3DController>();

            if (target != null)
            {
                successfulShot = target.TryShoot();

                if (successfulShot)
                {
                    Debug.Log(
                        "Nice shot! The target explodes."
                    );
                }
                else
                {
                    Debug.Log(
                        "You hit the target, but it's not ready yet."
                    );
                }
            }
            else
            {
                Debug.Log(
                    "Miss! You hit something that isn't a target."
                );
            }
        }
        else
        {
            Debug.Log(
                "Miss! You didn't hit anything."
            );
        }

        // Preserve the existing miss/combo logic.
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
        if (!showCrosshair || fpsCam == null)
        {
            return;
        }

        Vector2 screenCenter = new Vector2(
            Screen.width / 2,
            Screen.height / 2
        );

        Texture2D texture = Texture2D.whiteTexture;

        GUI.color = crosshairColor;

        GUI.DrawTexture(
            new Rect(
                screenCenter.x - crosshairSize,
                screenCenter.y - (crosshairThickness / 2f),
                crosshairSize * 2,
                crosshairThickness
            ),
            texture
        );

        GUI.DrawTexture(
            new Rect(
                screenCenter.x - (crosshairThickness / 2f),
                screenCenter.y - crosshairSize,
                crosshairThickness,
                crosshairSize * 2
            ),
            texture
        );
    }
}
