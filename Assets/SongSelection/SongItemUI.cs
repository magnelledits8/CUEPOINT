
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SongItemUI : MonoBehaviour
{
    [Header("Box Images")]
    public Image boxL1;
    public Image boxL2;

    [Header("Song Settings")]
    public string sceneName = "Level1";

    [Header("Animation")]
    [Range(0.05f, 2f)]
    public float fadeDuration = 0.25f;

    private Button songButton;
    private SongSelectionController controller;
    private bool isSelected;

    private void Awake()
    {
        songButton = GetComponent<Button>();

        controller =
            GetComponentInParent<SongSelectionController>();

        SetAlpha(boxL1, 1f);
        SetAlpha(boxL2, 0f);

        songButton.onClick.AddListener(SelectThisSong);
    }

    private void OnDestroy()
    {
        if (songButton != null)
            songButton.onClick.RemoveListener(SelectThisSong);
    }

    private void Update()
    {
        float step = Time.unscaledDeltaTime /
                     Mathf.Max(0.01f, fadeDuration);

        Fade(boxL1, isSelected ? 0f : 1f, step);
        Fade(boxL2, isSelected ? 1f : 0f, step);
    }

    private void SelectThisSong()
    {
        if (controller != null)
            controller.SelectSong(this);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
    }

    private void Fade(Image image, float target, float step)
    {
        if (image == null)
            return;

        Color color = image.color;

        color.a = Mathf.MoveTowards(
            color.a,
            target,
            step
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
}
