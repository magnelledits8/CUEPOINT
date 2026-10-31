using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("Skjutinställningar")]
    public float range = 100f;
    public Camera fpsCam;

    [Header("Sikte (Crosshair) Inställningar")]
    public bool showCrosshair = true;
    public Color crosshairColor = Color.green;
    public int crosshairSize = 10;
    public int crosshairThickness = 2;

    [Header("Effekter (Valfritt)")]
    public ParticleSystem muzzleFlash;

    void Update()
    {
        if (fpsCam != null)
        {
            Debug.DrawRay(fpsCam.transform.position, fpsCam.transform.forward * range, Color.red);
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        if (fpsCam == null)
        {
            Debug.LogError("Du måste dra in din kamera i fältet 'Fps Cam' på PlayerShooting-skriptet!");
            return;
        }

        RaycastHit hit;

        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Debug.Log("Träffade i 3D-rymden: " + hit.transform.name);

            // OPTIMERING: Sök direkt efter målet på det träffade objektets collider för bättre FPS-stabilitet
            SpiralTarget3DController target = hit.transform.GetComponent<SpiralTarget3DController>();

            if (target != null)
            {
                bool hitSuccess = target.TryShoot();

                if (hitSuccess)
                {
                    Debug.Log("Snyggt träffat! Målet exploderar.");
                }
                else
                {
                    Debug.Log("Du träffade målet, men det är inte redo ännu.");
                }
            }
        }
    }

    void OnGUI()
    {
        if (!showCrosshair || fpsCam == null) return;

        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        Texture2D texture = Texture2D.whiteTexture;
        GUI.color = crosshairColor;

        GUI.DrawTexture(new Rect(screenCenter.x - crosshairSize, screenCenter.y - (crosshairThickness / 2f), crosshairSize * 2, crosshairThickness), texture);
        GUI.DrawTexture(new Rect(screenCenter.x - (crosshairThickness / 2f), screenCenter.y - crosshairSize, crosshairThickness, crosshairSize * 2), texture);
    }
}
