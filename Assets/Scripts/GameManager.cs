using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private static bool hasShownIntroThisSession = false;
    public static bool HasShownIntroThisSession => hasShownIntroThisSession;

    public enum GameState { WaitingToStart, Playing, GameOver }
    public GameState CurrentState { get; private set; } = GameState.WaitingToStart;

    public float SurvivalTime { get; private set; } = 0f;
    public int MaxPoliceAliveThisRun { get; private set; } = 0;
    public int AbilitiesUsedThisRun { get; private set; } = 0;
    public int PoliceKilledThisRun { get; private set; } = 0;
    public string CauseOfDeath { get; private set; } = "";

    public event Action OnRunStart;
    public event Action<string> OnRunEnd;

    void Awake() { Instance = this; }

    void Start()
    {
        // If we've already shown the intro once this session (i.e. this is a
        // restart-triggered scene reload, not the very first load), skip straight
        // to gameplay. IntroPanelUI handles showing the panel on first load.
        if (hasShownIntroThisSession)
            StartRun();
    }

    public void BeginFirstRun()
    {
        hasShownIntroThisSession = true;
        StartRun();
    }

    void Update()
    {
        if (CurrentState == GameState.Playing)
            SurvivalTime += Time.deltaTime;
    }

    public void StartRun()
    {
        CurrentState = GameState.Playing;
        SurvivalTime = 0f;
        MaxPoliceAliveThisRun = 0;
        AbilitiesUsedThisRun = 0;
        PoliceKilledThisRun = 0;
        CauseOfDeath = "";
        Time.timeScale = 1f;
        OnRunStart?.Invoke();
    }

    public void ReportPoliceCount(int currentCount)
    {
        if (currentCount > MaxPoliceAliveThisRun)
            MaxPoliceAliveThisRun = currentCount;
    }

    public void ReportAbilityUsed() { AbilitiesUsedThisRun++; }
    public void ReportPoliceKilled() { PoliceKilledThisRun++; }

    public void EndRun(string cause)
    {
        if (CurrentState == GameState.GameOver) return;
        CurrentState = GameState.GameOver;
        CauseOfDeath = cause;
        OnRunEnd?.Invoke(cause);
    }

    public void RestartRun()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}