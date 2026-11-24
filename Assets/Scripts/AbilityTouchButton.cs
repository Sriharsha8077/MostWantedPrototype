using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class AbilityTouchButton : MonoBehaviour
{
    // Every active touch button, so UIManager can update them all.
    public static readonly List<AbilityTouchButton> All = new List<AbilityTouchButton>();

    [Tooltip("0 = first row in AbilityManager, 7 = eighth row.")]
    public int abilityIndex;

    public TMP_Text costText;
    public TMP_Text nameText;          // short label until you have icons
    public Image cooldownOverlay;      // Filled / Radial 360 image

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnPressed);
    }

    void OnEnable() { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    void OnPressed()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.OnAbilityButtonPressed(abilityIndex);
    }

    // Called every frame by UIManager.
    public void Refresh(AbilityDefinition ability, bool unlocked, bool usable)
    {
        if (nameText != null) nameText.text = ability.abilityName;

        if (costText != null)
            costText.text = unlocked ? $"${ability.cost}" : $"Lv.{ability.requiredWantedLevel}";

        button.interactable = unlocked && usable;

        if (cooldownOverlay != null)
        {
            if (!unlocked) cooldownOverlay.fillAmount = 1f;
            else cooldownOverlay.fillAmount =
                ability.cooldown > 0f ? Mathf.Clamp01(ability.cooldownTimer / ability.cooldown) : 0f;
        }
    }
}