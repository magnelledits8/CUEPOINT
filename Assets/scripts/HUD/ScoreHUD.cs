using TMPro;
using UnityEngine;

public class ScoreHUD : MonoBehaviour
{
    public static ScoreHUD Instance { get; private set; }

    [Header("Text in Canvas")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text comboText;

    [Header("Points")]
    [SerializeField, Min(0)] private int pointsPerHit = 300;

    private int score;
    private int combo;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Only one ScoreHUD should exist in this scene.", this);
            Destroy(this);
            return;
        }

        Instance = this;
        RefreshText();
    }

    public void RegisterHit()
    {
        score += pointsPerHit;
        combo++;
        RefreshText();
    }

    public void RegisterMiss()
    {
        combo = 0;
        RefreshText();
    }

    private void RefreshText()
    {
        if (scoreText != null)
            scoreText.text = score.ToString("D6");

        if (comboText != null)
            comboText.text = combo.ToString();
    }

    [ContextMenu("TEST - Add successful hit")]
    private void TestHit() => RegisterHit();

    [ContextMenu("TEST - Register miss")]
    private void TestMiss() => RegisterMiss();

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
