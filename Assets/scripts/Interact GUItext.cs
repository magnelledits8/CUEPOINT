using UnityEngine;

public class InteractTextGUI : MonoBehaviour
{
    [Header("Anpassa din text här:")]
    public string textToShow = "Press E to interact"; // Texten som visas på skärmen
    public Color textColor = Color.red;              // Textens grundfärg
    public Font textFont;                            // Egen font (valfritt)
    public int fontSize = 24;                        // Textens storlek

    [Header("Position på skärmen (Justera i Inspector):")]
    [Tooltip("0 är i mitten. Minus flyttar vänster, plus flyttar höger.")]
    public float offsetX = 0f;
    [Tooltip("0 är i mitten. Minus flyttar uppåt, plus flyttar nedåt.")]
    public float offsetY = 100f;

    [Header("Fade-inställningar:")]
    [Tooltip("Hur snabbt texten ska tona in (högre tal = snabbare fade).")]
    public float fadeSpeed = 3f;

    private bool playerIsClose = false;
    private float currentAlpha = 0f; // Håller koll på nuvarande synlighet (0 = osynlig, 1 = helt synlig)

    // Känner av om spelaren går in i knappens Trigger-zon
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsClose = true;
        }
    }

    // Känner av när spelaren lämnar zonen
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsClose = false;
        }
    }

    void Update()
    {
        // Om spelaren är nära, tona in texten mot 1 (100% synlig)
        if (playerIsClose)
        {
            currentAlpha = Mathf.MoveTowards(currentAlpha, 1f, fadeSpeed * Time.deltaTime);
        }
        // Om spelaren går iväg, tona ut texten mot 0 (0% synlig)
        else
        {
            currentAlpha = Mathf.MoveTowards(currentAlpha, 0f, fadeSpeed * Time.deltaTime);
        }
    }

    // OnGUI ritar ut texten på skärmen
    void OnGUI()
    {
        // Om texten är helt osynlig behöver vi inte rita ut den alls
        if (currentAlpha <= 0f) return;

        // Skapa en stil för vår text
        GUIStyle myStyle = new GUIStyle();

        // Skapa en kopia av färgen och applicera vår nuvarande fade-synlighet (alfa)
        Color finalColor = textColor;
        finalColor.a = currentAlpha;

        // Applicera dina inställningar från Inspector till stilen
        myStyle.normal.textColor = finalColor;
        myStyle.fontSize = fontSize;
        myStyle.alignment = TextAnchor.MiddleCenter; // Centrerar texten i sin textruta

        if (textFont != null)
        {
            myStyle.font = textFont;
        }

        // Räkna ut mitten av skärmen och lägg till dina Offset-värden från Inspector
        float screenX = (Screen.width / 2) + offsetX;
        float screenY = (Screen.height / 2) + offsetY;

        // Rita ut texten på skärmen (en ruta som är 400 pixlar bred och 50 pixlar hög)
        GUI.Label(new Rect(screenX - 200, screenY, 400, 50), textToShow, myStyle);
    }
}
