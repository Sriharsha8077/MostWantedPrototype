using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;   // NEW: touch buttons call back through this

    public Transform playerTransform;
    public TMP_Text timerText;
    public TMP_Text moneyText;
    public TMP_Text wantedLevelText;
    public TMP_Text killsText;
    public Button[] abilityButtons;
    public TMP_Text[] abilityCostTexts;
    public Image[] abilityCooldownOverlays;

    void Awake() { Instance = this; }   // NEW

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        float t = GameManager.Instance.SurvivalTime;
        int wanted = DifficultyManager.Instance.WantedLevel;
        var abilities = AbilityManager.Instance.abilities;

        timerText.text = $"{Mathf.FloorToInt(t / 60f):00}:{Mathf.FloorToInt(t % 60f):00}";
        moneyText.text = $"${Mathf.FloorToInt(MoneyManager.Instance.Money)}";
        wantedLevelText.text = $"WANTED LV.{wanted}";
        killsText.text = $"Police down: {GameManager.Instance.PoliceKilledThisRun}";

        // ---------- desktop ability bar (unchanged behaviour, keys 1-8 + mouse) ----------
        for (int i = 0; i < abilities.Count && i < abilityButtons.Length; i++)
        {
            var ability = abilities[i];
            bool unlocked = wanted >= ability.requiredWantedLevel;
            bool keyPressed = Input.GetKeyDown(KeyCode.Alpha1 + i);

            if (!unlocked)
            {
                abilityCostTexts[i].text = $"Lv.{ability.requiredWantedLevel}";
                abilityButtons[i].interactable = false;
                abilityCooldownOverlays[i].fillAmount = 1f;
                if (keyPressed) PlayDenied();
                continue;
            }

            abilityCostTexts[i].text = $"${ability.cost}";
            abilityButtons[i].interactable =
                MoneyManager.Instance.Money >= ability.cost && ability.cooldownTimer <= 0;
            abilityCooldownOverlays[i].fillAmount =
                ability.cooldown > 0 ? Mathf.Clamp01(ability.cooldownTimer / ability.cooldown) : 0f;

            if (keyPressed)
            {
                if (abilityButtons[i].interactable) OnAbilityButtonPressed(i);
                else PlayDenied();
            }
        }

        // ---------- NEW: touch ability buttons ----------
        for (int i = 0; i < AbilityTouchButton.All.Count; i++)
        {
            AbilityTouchButton tb = AbilityTouchButton.All[i];
            if (tb.abilityIndex < 0 || tb.abilityIndex >= abilities.Count) continue;

            AbilityDefinition a = abilities[tb.abilityIndex];
            bool unlocked = wanted >= a.requiredWantedLevel;
            bool usable = MoneyManager.Instance.Money >= a.cost && a.cooldownTimer <= 0f;
            tb.Refresh(a, unlocked, usable);
        }
    }

    public void OnAbilityButtonPressed(int index)
    {
        AbilityManager.Instance.TryActivate(index, playerTransform.position);
    }

    void PlayDenied()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUIDenied();   // AUDIO
    }
}