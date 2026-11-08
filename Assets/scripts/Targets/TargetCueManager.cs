
using UnityEngine;

public class TargetCueManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private TargetCue[] cues;
    [SerializeField] private bool findCuesInChildren = true;

    private bool timelineRunning;
    private double musicStartDspTime;

    // Allows the Timing HUD to check if the timeline is running.
    public bool TimelineRunning
    {
        get { return timelineRunning; }
    }

    // Allows the Timing HUD to read all target cues.
    public TargetCue[] Cues
    {
        get { return cues; }
    }

    // Allows the Timing HUD to use the same music start time.
    public double MusicStartDspTime
    {
        get { return musicStartDspTime; }
    }

    private void Awake()
    {
        if ((cues == null || cues.Length == 0) &&
            findCuesInChildren)
        {
            cues = GetComponentsInChildren<TargetCue>(true);
        }
    }

    // Called by DelayedMusicStart with the scheduled DSP music time.
    public void BeginTimeline(double startDspTime)
    {
        if (playerCamera == null)
        {
            Debug.LogError(
                "TargetCueManager: Assign the player's camera transform.",
                this
            );
            return;
        }

        if (cues == null || cues.Length == 0)
        {
            Debug.LogError(
                "TargetCueManager: No TargetCue components found.",
                this
            );
            return;
        }

        // Store the exact music start time for targets and HUD.
        musicStartDspTime = startDspTime;

        for (int i = 0; i < cues.Length; i++)
        {
            if (cues[i] != null)
            {
                cues[i].Configure(
                    playerCamera,
                    musicStartDspTime
                );
            }
        }

        timelineRunning = true;
    }

    // Test method.
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
            {
                cues[i].Tick(now);
            }
        }
    }
}
