using UnityEngine;
using System;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance;

    [Header("Wanted level time thresholds (seconds)")]
    public float level2Time = 120f;
    public float level3Time = 300f;
    public float level4Time = 600f;
    public float level5Time = 1200f;

    public int WantedLevel { get; private set; } = 1;
    public bool PredictionEnabled { get; private set; } = false;

    public event Action<int> OnWantedLevelChanged;

    void Awake() { Instance = this; }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        float t = GameManager.Instance.SurvivalTime;
        int newLevel;
        if (t < level2Time) newLevel = 1;
        else if (t < level3Time) newLevel = 2;
        else if (t < level4Time) newLevel = 3;
        else if (t < level5Time) newLevel = 4;
        else newLevel = 5;

        PredictionEnabled = newLevel >= 3;

        if (newLevel != WantedLevel)
        {
            WantedLevel = newLevel;
            OnWantedLevelChanged?.Invoke(WantedLevel);
        }
    }

    public float GetSpawnInterval()
    {
        switch (WantedLevel)
        {
            case 1: return 2.5f;
            case 2: return 1.8f;
            case 3: return 1.1f;
            case 4: return 0.7f;
            default: return 0.4f;
        }
    }

    public int GetSpawnCountPerWave()
    {
        switch (WantedLevel)
        {
            case 1: return 1;
            case 2: return 2;
            case 3: return 3;
            case 4: return 5;
            default: return 7;
        }
    }
}