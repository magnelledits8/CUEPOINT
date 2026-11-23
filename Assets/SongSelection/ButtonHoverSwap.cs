
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonHoverSwap : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Normal State")]
    public Image box1;
    public Image play1;

    [Header("Hover State")]
    public Image box2;
    public Image play2;

    [Header("Button Text")]
    public TMP_Text buttonText;

    [Header("Animation")]
    [Range(0.05f, 2f)]
    public float fadeDuration = 0.25f;

    [Header("Button")]
    public Button playTrackButton;

    private bool songSelected = false;
    private bool isHovering = false;

    private void Awake()
    {
        if (playTrackButton == null)
            playTrackButton = GetComponent<Button>();

        // Set the initial state immediately.
        SetAlpha(box1, 1f);
        SetAlpha(box2, 0f);
        SetAlpha(play1, 0f);
        SetAlpha(play2, 0f);
        SetTextAlpha(0f);

        UpdateButtonState();
    }

    private void Update()
    {
        float speed = Time.unscaledDeltaTime /
            Mathf.Max(0.01f, fadeDuration);

        bool showHover = songSelected && isHovering;

        // Background images.
        Fade(box1, showHover ? 0f : 1f, speed);
        Fade(box2, showHover ? 1f : 0f, speed);

        // Play icons.
        Fade(play1,
            songSelected && !showHover ? 1f : 0f,
            speed);

        Fade(play2,
            showHover ? 1f : 0f,
            speed);

        // Button text.
        if (buttonText != null)
        {
            Color color = buttonText.color;

            color.a = Mathf.MoveTowards(
                color.a,
                songSelected ? 1f : 0f,
                speed
            );

            buttonText.color = color;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }

    // Call when a song is selected.
    public void SelectSong()
    {
        songSelected = true;
        UpdateButtonState();
    }

    // Call when the selection is cleared.
    public void ClearSelection()
    {
        songSelected = false;
        isHovering = false;
        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        if (playTrackButton != null)
            playTrackButton.interactable = songSelected;
    }

    private void Fade(
        Image image,
        float targetAlpha,
        float speed)
    {
        if (image == null)
            return;

        Color color = image.color;

        color.a = Mathf.MoveTowards(
            color.a,
            targetAlpha,
            speed
        );

        image.color = color;
    }

    private void SetAlpha(Image image, float alpha)
    {
        if (image == null)
            return;

        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

    private void SetTextAlpha(float alpha)
    {
        if (buttonText == null)
            return;

        Color color = buttonText.color;
        color.a = alpha;
        buttonText.color = color;
    }
}
