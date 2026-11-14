
using UnityEngine;

public class TargetCueManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private TargetCue[] cues;
    [SerializeField] private bool findCuesInChildren = true;

    private bool timelineRunning;
    private double musicStartDspTime;

    // Let the HUD check if the timeline is running
    public bool TimelineRunning
    {
        get { return timelineRunning; }
    }

    // Give the HUD access to all target cues
    public TargetCue[] Cues
    {
        get { return cues; }
    }

    // Let the HUD use the music start time
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

    // Start the target timeline with the music
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

        // Save the music start time
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

    // Test the timeline with a one second delay
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
