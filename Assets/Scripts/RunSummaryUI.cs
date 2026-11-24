using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class RunSummaryUI : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text survivalTimeText, moneyEarnedText, maxPoliceText, killsCountText,
                     abilitiesUsedText, causeOfDeathText, bestRecordText, adaptiveMessageText;

    [Header("Coins (the menu currency)")]
    public TMP_Text coinsEarnedText;
    public Button doubleCoinsButton;
    public TMP_Text doubleCoinsButtonText;
    public Button menuButton;
    public string menuSceneName = "MenuScene";

    private float refreshTimer;

    void Start()
    {
        panel.SetActive(false);

        if (doubleCoinsButton != null) doubleCoinsButton.onClick.AddListener(OnDoubleCoinsPressed);
        if (menuButton != null) menuButton.onClick.AddListener(OnMenuPressed);

        GameManager.Instance.OnRunEnd += HandleRunEnd;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnRunEnd -= HandleRunEnd;
    }

    void Update()
    {
        // Re-check whether an ad is ready, in case it finished loading after the run ended.
        if (!panel.activeSelf || MetaProgress.LastRunDoubled) return;
        if (AdManager.Instance != null && AdManager.Instance.IsAdPlaying) return;

        refreshTimer -= Time.unscaledDeltaTime;
        if (refreshTimer <= 0f)
        {
            refreshTimer = 0.5f;
            RefreshDoubleButton(MetaProgress.LastRunCoins);
        }
    }

    void HandleRunEnd(string cause)
    {
        AdaptivePlayerProfile.Instance.CommitRunToPersistentBias();

        float survived = GameManager.Instance.SurvivalTime;
        float best = PlayerPrefs.GetFloat("best_survival_time", 0f);
        bool isNewBest = survived > best;
        if (isNewBest) { PlayerPrefs.SetFloat("best_survival_time", survived); PlayerPrefs.Save(); best = survived; }

        if (isNewBest && AudioManager.Instance != null)
            AudioManager.Instance.PlayNewBest();

        // Coins: one per second survived, paid once, when the run really ends.
        int coins = MetaProgress.AwardRunCoins(survived);
        MetaProgress.ClearRunBoosts();   // boosts only last for one run

        survivalTimeText.text = $"Survived: {FormatTime(survived)}";
        moneyEarnedText.text = $"Cash earned: ${Mathf.FloorToInt(MoneyManager.Instance.Money)}";
        maxPoliceText.text = $"Most police at once: {GameManager.Instance.MaxPoliceAliveThisRun}";
        killsCountText.text = $"Police taken down: {GameManager.Instance.PoliceKilledThisRun}";
        abilitiesUsedText.text = $"Abilities used: {GameManager.Instance.AbilitiesUsedThisRun}";

        int revives = GameManager.Instance.RevivesUsedThisRun;
        causeOfDeathText.text = revives > 0
            ? $"Cause: {cause}  (continued {revives}x)"
            : $"Cause: {cause}";

        bestRecordText.text = isNewBest ? "New best survival time!" : $"Best: {FormatTime(best)}";

        int dominant = AdaptivePlayerProfile.Instance.GetDominantSector();
        adaptiveMessageText.text =
            $"You leaned {AdaptivePlayerProfile.Instance.GetSectorLabel(dominant)} when things got tight. " +
            "They'll be watching that side next time.";

        if (coinsEarnedText != null)
            coinsEarnedText.text = $"+{coins} coins   (total {MetaProgress.Coins})";
        RefreshDoubleButton(coins);

        panel.SetActive(true);
    }

    void RefreshDoubleButton(int coins)
    {
        if (doubleCoinsButton == null) return;

        bool canOffer = coins > 0 &&
                        AdManager.Instance != null &&
                        AdManager.Instance.IsRewardedReady;

        doubleCoinsButton.gameObject.SetActive(canOffer);
        doubleCoinsButton.interactable = canOffer;
        if (canOffer && doubleCoinsButtonText != null)
            doubleCoinsButtonText.text = $"Watch ad: double your coins (+{coins})";
    }

    void OnDoubleCoinsPressed()
    {
        if (AdManager.Instance == null) return;
        doubleCoinsButton.interactable = false;

        AdManager.Instance.ShowRewarded(rewarded =>
        {
            if (rewarded)
            {
                MetaProgress.DoubleLastRunCoins();
                if (coinsEarnedText != null)
                    coinsEarnedText.text = $"+{MetaProgress.LastRunCoins * 2} coins  (doubled!)   (total {MetaProgress.Coins})";
                doubleCoinsButton.gameObject.SetActive(false);
            }
            else
            {
                doubleCoinsButton.interactable = true;   // ad closed early: allow another try
            }
        });
    }

    void OnMenuPressed()
    {
        SceneManager.LoadScene(menuSceneName);
    }

    string FormatTime(float t) => $"{Mathf.FloorToInt(t / 60f):00}:{Mathf.FloorToInt(t % 60f):00}";

    public void OnRestartPressed() => GameManager.Instance.RestartRun();
}