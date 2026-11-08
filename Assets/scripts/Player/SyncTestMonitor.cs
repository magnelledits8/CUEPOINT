
using UnityEngine;

public class SyncTestMonitor : MonoBehaviour
{
    [Header("References")]
    public AudioSource musicSource;
    public PlayerRunController player;

    [Header("FPS Test")]
    public int targetFPS = 100;

    [Header("Display")]
    public bool showDebugInfo = true;

    private int originalFPS;
    private int originalVSync;

    private float actualFPS;
    private float fpsTimer;
    private int frameCounter;

    private float musicTime;
    private float previousMusicTime = -1f;
    private float previousZ;

    private int nextCheckpoint = 0;

    private readonly float[] checkpoints =
    {
        5f, 10f, 15f, 20f, 30f
    };

    void Start()
    {
        // Save original settings.
        originalFPS = Application.targetFrameRate;
        originalVSync = QualitySettings.vSyncCount;

        // Disable VSync during testing.
        QualitySettings.vSyncCount = 0;

        // Apply requested FPS limit.
        Application.targetFrameRate = targetFPS;

        if (musicSource == null || player == null)
        {
            Debug.LogError(
                "SyncTestMonitor: Assign Music Source and Player!"
            );
        }

        Debug.Log(
            "SYNC TEST STARTED | Target FPS: " + targetFPS
        );
    }

    void Update()
    {
        // Measure actual FPS.
        fpsTimer += Time.unscaledDeltaTime;
        frameCounter++;

        if (fpsTimer >= 0.5f)
        {
            actualFPS = frameCounter / fpsTimer;

            fpsTimer = 0f;
            frameCounter = 0;
        }

        if (musicSource == null ||
            musicSource.clip == null ||
            player == null)
        {
            return;
        }

        if (!musicSource.isPlaying)
        {
            previousMusicTime = -1f;
            return;
        }

        // Read the music position in samples.
        musicTime = (float)musicSource.timeSamples /
                    musicSource.clip.frequency;

        float currentZ = player.transform.position.z;

        // Reset checkpoints if the song restarts.
        if (previousMusicTime >= 0f &&
            musicTime < previousMusicTime - 0.1f)
        {
            nextCheckpoint = 0;
            previousMusicTime = -1f;
        }

        // Record player position at specific music times.
        while (nextCheckpoint < checkpoints.Length &&
               musicTime >= checkpoints[nextCheckpoint])
        {
            float checkpoint = checkpoints[nextCheckpoint];

            float measuredZ = currentZ;

            // Estimate the position at the exact checkpoint
            // between the previous and current rendered frames.
            if (previousMusicTime >= 0f &&
                musicTime > previousMusicTime)
            {
                float t = Mathf.Clamp01(
                    (checkpoint - previousMusicTime) /
                    (musicTime - previousMusicTime)
                );

                measuredZ = Mathf.Lerp(
                    previousZ,
                    currentZ,
                    t
                );
            }

            Debug.Log(
                "CUEPOINT SYNC TEST | " +
                "FPS Limit: " + targetFPS +
                " | Actual FPS: " + actualFPS.ToString("F1") +
                " | Music: " + checkpoint.ToString("F2") + "s" +
                " | Player Z: " + measuredZ.ToString("F3") +
                " | Speed: " + player.currentSpeed.ToString("F2")
            );

            nextCheckpoint++;
        }

        previousMusicTime = musicTime;
        previousZ = currentZ;
    }

    void OnGUI()
    {
        if (!showDebugInfo)
            return;

        float x = 10f;
        float y = Screen.height - 125f;

        GUI.Box(
            new Rect(x, y, 310f, 115f),
            "CUEPOINT - SYNC TEST"
        );

        GUI.Label(
            new Rect(x + 10, y + 28, 290, 20),
            "FPS: " + actualFPS.ToString("F1") +
            " / Limit: " + targetFPS
        );

        GUI.Label(
            new Rect(x + 10, y + 49, 290, 20),
            "Music Time: " + musicTime.ToString("F3") + " s"
        );

        if (player != null)
        {
            GUI.Label(
                new Rect(x + 10, y + 70, 290, 20),
                "Player Z: " +
                player.transform.position.z.ToString("F3")
            );

            GUI.Label(
                new Rect(x + 10, y + 91, 290, 20),
                "Speed: " + player.currentSpeed.ToString("F2")
            );
        }
    }

    void OnDestroy()
    {
        // Restore the original frame rate settings.
        QualitySettings.vSyncCount = originalVSync;
        Application.targetFrameRate = originalFPS;
    }
}
