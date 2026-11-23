
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SongSelectionController : MonoBehaviour
{
    [Header("Play Track Button")]
    public Button playTrackButton;
    public ButtonHoverSwap playTrackHover;

    private SongItemUI selectedSong;
    private SongItemUI[] songs;

    private void Awake()
    {
        // Find all songs inside SongSelectionMenu.
        songs = GetComponentsInChildren<SongItemUI>(true);

        foreach (SongItemUI song in songs)
        {
            song.SetSelected(false);
        }

        selectedSong = null;

        if (playTrackButton != null)
        {
            playTrackButton.onClick.AddListener(PlaySelectedSong);
            playTrackButton.interactable = false;
        }
    }

    private void Start()
    {
        if (playTrackHover != null)
            playTrackHover.ClearSelection();
    }

    private void OnDestroy()
    {
        if (playTrackButton != null)
        {
            playTrackButton.onClick.RemoveListener(
                PlaySelectedSong
            );
        }
    }

    public void SelectSong(SongItemUI song)
    {
        if (song == null)
            return;

        // Deselect the previous song.
        if (selectedSong != null)
            selectedSong.SetSelected(false);

        // Select the new song.
        selectedSong = song;
        selectedSong.SetSelected(true);

        // Show the PlayTrack icon and text.
        if (playTrackHover != null)
            playTrackHover.SelectSong();

        if (playTrackButton != null)
            playTrackButton.interactable = true;

        Debug.Log("Selected song: " + song.name);
    }

    public void PlaySelectedSong()
    {
        if (selectedSong == null)
            return;

        string sceneName = selectedSong.sceneName;

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("No scene assigned!");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}
