using UnityEngine;

/// <summary>
/// One manager for the level. It sends the planned music DSP start time to all
/// TargetCue markers and ticks them. It does not start music itself, so it can
/// work with an existing music/timing script.
/// </summary>
public class TargetCueManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private TargetCue[] cues;
    [SerializeField] private bool findCuesInChildren = true;

    private bool timelineRunning;

    private void Awake()
    {
        if ((cues == null || cues.Length == 0) && findCuesInChildren)
            cues = GetComponentsInChildren<TargetCue>(true);
    }

    /// <summary>
    /// Call this with the exact DSP time at which your music is scheduled to start.
    /// Example: audioSource.PlayScheduled(startDsp); BeginTimeline(startDsp);
    /// </summary>
    public void BeginTimeline(double musicStartDspTime)
    {
        if (playerCamera == null)
        {
            Debug.LogError("TargetCueManager: Assign the player's camera transform.", this);
            return;
        }

        if (cues == null || cues.Length == 0)
        {
            Debug.LogError("TargetCueManager: No TargetCue components were found or assigned.", this);
            return;
        }

        for (int i = 0; i < cues.Length; i++)
        {
            if (cues[i] != null)
                cues[i].Configure(playerCamera, musicStartDspTime);
        }

        timelineRunning = true;
    }

    /// <summary>Convenient test method: starts the cue timeline one second from now.</summary>
    public void BeginTimelineAfterOneSecond()
    {
        BeginTimeline(AudioSettings.dspTime + 1.0);
    }

    private void Update()
    {
        if (!timelineRunning)
            return;

        double now = AudioSettings.dspTime;
        for (int i = 0; i < cues.Length; i++)
        {
            if (cues[i] != null)
                cues[i].Tick(now);
        }
    }
}
