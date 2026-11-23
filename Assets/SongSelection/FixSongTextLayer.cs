
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FixSongTextLayer : MonoBehaviour
{
    [Header("Song Backgrounds")]
    public Image boxL1;
    public Image boxL2;

    [Header("Album Cover")]
    public Image imageSong;

    [Header("Sorting Order")]
    public int boxSortingOrder = 4900;
    public int coverSortingOrder = 4950;
    public int textSortingOrder = 5000;

    private void Awake()
    {
        // Force song backgrounds to render in front.
        SetupImage(boxL1, boxSortingOrder);
        SetupImage(boxL2, boxSortingOrder);

        // Force album cover to render above backgrounds.
        SetupImage(imageSong, coverSortingOrder);
    }

    private void Start()
    {
        // Find all TextMeshPro components.
        TMP_Text[] texts =
            GetComponentsInChildren<TMP_Text>(true);

        foreach (TMP_Text text in texts)
        {
            if (text is TextMeshProUGUI uiText)
            {
                ForceTextToFront(uiText);
            }
            else if (text is TextMeshPro worldText)
            {
                ConvertToUI(worldText);
            }
        }

        Debug.Log("Song images and text rendering fixed.");
    }

    private void SetupImage(Image image, int order)
    {
        if (image == null)
            return;

        Canvas canvas = image.GetComponent<Canvas>();

        if (canvas == null)
        {
            canvas = image.gameObject.AddComponent<Canvas>();
        }

        canvas.overrideSorting = true;
        canvas.sortingOrder = order;

        // Prevent decorative images from blocking clicks.
        image.raycastTarget = false;
    }

    private void ForceTextToFront(TextMeshProUGUI text)
    {
        Canvas canvas = text.GetComponent<Canvas>();

        if (canvas == null)
        {
            canvas = text.gameObject.AddComponent<Canvas>();
        }

        canvas.overrideSorting = true;
        canvas.sortingOrder = textSortingOrder;

        text.raycastTarget = false;
    }

    private void ConvertToUI(TextMeshPro original)
    {
        GameObject newObject = new GameObject(
            original.gameObject.name + "_UI",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );

        newObject.transform.SetParent(
            original.transform.parent,
            false
        );

        RectTransform rect =
            newObject.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        rect.localPosition = original.transform.localPosition;
        rect.localRotation = original.transform.localRotation;
        rect.localScale = Vector3.one;

        rect.sizeDelta = new Vector2(600f, 90f);

        TextMeshProUGUI newText =
            newObject.GetComponent<TextMeshProUGUI>();

        newText.font = original.font;
        newText.text = original.text;
        newText.fontSize = original.fontSize;
        newText.fontStyle = original.fontStyle;
        newText.alignment = original.alignment;
        newText.color = original.color;

        ForceTextToFront(newText);

        MeshRenderer renderer =
            original.GetComponent<MeshRenderer>();

        if (renderer != null)
        {
            renderer.enabled = false;
        }
    }
}
