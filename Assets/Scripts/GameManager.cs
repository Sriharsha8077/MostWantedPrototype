using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

// Called by the menu's Play button, so the old "press any key" intro doesn't show after the menu.
public static void MarkIntroShown() { hasShownIntroThisSession = true; }

    // The cause string the nuke uses. A nuke is never revivable.
    public const string NukeCause = "Nuclear Detonation";

    private static bool hasShownIntroThisSession = false;                       // INTRO
    public static bool HasShownIntroThisSession => hasShownIntroThisSession;    // INTRO

    public enum GameState { WaitingToStart, Playing, ContinueOffer, GameOver }
    public GameState CurrentState { get; private set; } = GameState.WaitingToStart;   // INTRO (no intro screen? use GameState.Playing)

    public float SurvivalTime { get; private set; } = 0f;
    public int MaxPoliceAliveThisRun { get; private set; } = 0;
    public int AbilitiesUsedThisRun { get; private set; } = 0;
    public int PoliceKilledThisRun { get; private set; } = 0;
    public int RevivesUsedThisRun { get; private set; } = 0;
    public string CauseOfDeath { get; private set; } = "";

    public event Action OnRunStart;
    public event Action<string> OnRunEnd;        // fires once, at the real end of the run
    public event Action OnContinueOffered;       // player was caught and a revive is on offer
    public event Action OnRunResumed;            // player revived and play continues

    private string pendingCause = "";

    void Awake() { Instance = this; }

    void Start()
    {
        // INTRO: if the intro was already shown this session (scene reload on Restart), skip straight in.
        // No intro screen? Replace this whole method with:  void Start() { StartRun(); }
        if (hasShownIntroThisSession)
            StartRun();
    }

    public void BeginFirstRun()                                                  // INTRO
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
        RevivesUsedThisRun = 0;
        CauseOfDeath = "";
        pendingCause = "";
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
        if (CurrentState != GameState.Playing) return;   // already offering or over

        pendingCause = cause;

        if (ReviveManager.Instance != null && ReviveManager.Instance.ShouldOfferRevive(cause))
        {
            CurrentState = GameState.ContinueOffer;      // everything freezes (all scripts check for Playing)
            OnContinueOffered?.Invoke();
            return;
        }

        FinalizeRun(cause);
    }

    void FinalizeRun(string cause)
    {
        CurrentState = GameState.GameOver;
        CauseOfDeath = cause;
        OnRunEnd?.Invoke(cause);
    }

    // Called by ReviveManager after the ad was watched.
    public void ResumeAfterRevive()
    {
        if (CurrentState != GameState.ContinueOffer) return;
        RevivesUsedThisRun++;
        CurrentState = GameState.Playing;
        OnRunResumed?.Invoke();
    }

    // Called by the panel when the player says no, runs out of time, or the ad doesn't pay out.
    public void DeclineRevive()
    {
        if (CurrentState != GameState.ContinueOffer) return;
        FinalizeRun(pendingCause);
    }

    public void RestartRun()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}