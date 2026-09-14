using UnityEngine;
using TMPro;

public class RunSummaryUI : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text survivalTimeText, moneyEarnedText, maxPoliceText, killsCountText,
                     abilitiesUsedText, causeOfDeathText, bestRecordText, adaptiveMessageText;

    void Start()
    {
        panel.SetActive(false);
        GameManager.Instance.OnRunEnd += HandleRunEnd;
    }

    void HandleRunEnd(string cause)
    {
        AdaptivePlayerProfile.Instance.CommitRunToPersistentBias();

        float survived = GameManager.Instance.SurvivalTime;
        float best = PlayerPrefs.GetFloat("best_survival_time", 0f);
        bool isNewBest = survived > best;
        if (isNewBest) { PlayerPrefs.SetFloat("best_survival_time", survived); PlayerPrefs.Save(); best = survived; }

        survivalTimeText.text = $"Survived: {FormatTime(survived)}";
        moneyEarnedText.text = $"Money earned: ${Mathf.FloorToInt(MoneyManager.Instance.Money)}";
        maxPoliceText.text = $"Most police at once: {GameManager.Instance.MaxPoliceAliveThisRun}";
        killsCountText.text = $"Police taken down: {GameManager.Instance.PoliceKilledThisRun}";
        abilitiesUsedText.text = $"Abilities used: {GameManager.Instance.AbilitiesUsedThisRun}";
        causeOfDeathText.text = $"Cause: {cause}";
        bestRecordText.text = isNewBest ? "New best survival time!" : $"Best: {FormatTime(best)}";

        int dominant = AdaptivePlayerProfile.Instance.GetDominantSector();
        adaptiveMessageText.text =
            $"You leaned {AdaptivePlayerProfile.Instance.GetSectorLabel(dominant)} when things got tight. " +
            "They'll be watching that side next time.";

        panel.SetActive(true);
    }

    string FormatTime(float t) => $"{Mathf.FloorToInt(t / 60f):00}:{Mathf.FloorToInt(t % 60f):00}";

    public void OnRestartPressed() => GameManager.Instance.RestartRun();
}