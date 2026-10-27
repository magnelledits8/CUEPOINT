using UnityEngine;
// Vi måste lägga till denna rad för att kunna använda det nya Input-systemet
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("Skjutinställningar")]
    public float range = 100f;          // Hur långt skottet når
    public Camera fpsCam;               // Referens till din huvudkamera

    [Header("Sikte (Crosshair) Inställningar")]
    public bool showCrosshair = true;
    public Color crosshairColor = Color.green;
    public int crosshairSize = 10;
    public int crosshairThickness = 2;

    [Header("Effekter (Valfritt)")]
    public ParticleSystem muzzleFlash;

    void Update()
    {
        // Ritar en röd linje i Scene-vyn så du ser var kameran siktar
        if (fpsCam != null)
        {
            Debug.DrawRay(fpsCam.transform.position, fpsCam.transform.forward * range, Color.red);
        }

        // DET NYA INPUT-SYSTEMET: Kollar om vänster musknapp trycks ner denna frame
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

        // Skickar en stråle från kamerans mittpunkt, rakt framåt
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Debug.Log("Träffade i 3D-rymden: " + hit.transform.name);

            // Letar efter ditt korrekta målskript på objektet vi träffade (eller dess förälder)
            SpiralTarget3DController target = hit.transform.GetComponentInParent<SpiralTarget3DController>();

            if (target != null)
            {
                // Kallar på målets egna inbyggda funktion för att ta skada/explodera
                bool hitSuccess = target.TryShoot();

                if (hitSuccess)
                {
                    Debug.Log("Snyggt träffat! Målet exploderar.");
                }
                else
                {
                    Debug.Log("Du träffade målet, men det är inte redo ännu (måste ladda klart animationen till Frame 13).");
                }
            }
        }
    }

    // Ritar ut det gröna siktet i mitten av skärmen
    void OnGUI()
    {
        if (!showCrosshair || fpsCam == null) return;

        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        Texture2D texture = Texture2D.whiteTexture;
        GUI.color = crosshairColor;

        // Horisontellt streck
        GUI.DrawTexture(new Rect(screenCenter.x - crosshairSize, screenCenter.y - (crosshairThickness / 2f), crosshairSize * 2, crosshairThickness), texture);

        // Vertikalt streck
        GUI.DrawTexture(new Rect(screenCenter.x - (crosshairThickness / 2f), screenCenter.y - crosshairSize, crosshairThickness, crosshairSize * 2), texture);
    }
}
