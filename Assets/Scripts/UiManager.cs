using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public Transform playerTransform;
    public TMP_Text timerText;
    public TMP_Text moneyText;
    public TMP_Text wantedLevelText;
    public TMP_Text killsText;
    public Button[] abilityButtons;
    public TMP_Text[] abilityCostTexts;
    public Image[] abilityCooldownOverlays;

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        float t = GameManager.Instance.SurvivalTime;
        timerText.text = $"{Mathf.FloorToInt(t / 60f):00}:{Mathf.FloorToInt(t % 60f):00}";
        moneyText.text = $"${Mathf.FloorToInt(MoneyManager.Instance.Money)}";
        wantedLevelText.text = $"WANTED LV.{DifficultyManager.Instance.WantedLevel}";
        killsText.text = $"Police down: {GameManager.Instance.PoliceKilledThisRun}";

        for (int i = 0; i < AbilityManager.Instance.abilities.Count && i < abilityButtons.Length; i++)
        {
            var ability = AbilityManager.Instance.abilities[i];
            bool unlocked = DifficultyManager.Instance.WantedLevel >= ability.requiredWantedLevel;

            if (!unlocked)
            {
                abilityCostTexts[i].text = $"Lv.{ability.requiredWantedLevel}";
                abilityButtons[i].interactable = false;
                abilityCooldownOverlays[i].fillAmount = 1f;
                continue;
            }

            abilityCostTexts[i].text = $"${ability.cost}";
            abilityButtons[i].interactable =
                MoneyManager.Instance.Money >= ability.cost && ability.cooldownTimer <= 0;
            abilityCooldownOverlays[i].fillAmount =
                ability.cooldown > 0 ? Mathf.Clamp01(ability.cooldownTimer / ability.cooldown) : 0f;

            KeyCode key = KeyCode.Alpha1 + i;
            if (Input.GetKeyDown(key) && abilityButtons[i].interactable)
                OnAbilityButtonPressed(i);
        }
    }

    public void OnAbilityButtonPressed(int index)
    {
        AbilityManager.Instance.TryActivate(index, playerTransform.position);
    }
}