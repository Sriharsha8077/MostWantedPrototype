using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeCardUI : MonoBehaviour
{
    public MetaProgress.UpgradeType type;
    [Tooltip("Shown in the effect line, e.g. 'Speed' or 'Cash income'.")]
    public string effectLabel = "Speed";

    public TMP_Text titleText;
    public TMP_Text levelText;
    public TMP_Text effectText;
    public TMP_Text costText;
    public Button buyButton;

    [Tooltip("Optional: 5 small images that light up as levels are bought.")]
    public Image[] levelPips;
    public Color pipOn = new Color(1f, 0.8f, 0.2f, 1f);
    public Color pipOff = new Color(1f, 1f, 1f, 0.25f);

    void Awake()
    {
        buyButton.onClick.AddListener(OnBuyPressed);
    }

    void OnEnable()
    {
        MetaProgress.OnChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        MetaProgress.OnChanged -= Refresh;
    }

    void OnBuyPressed()
    {
        MetaProgress.TryBuyUpgrade(type);   // fires OnChanged, which refreshes the card
    }

    void Refresh()
    {
        int level = MetaProgress.GetLevel(type);
        int nowPercent = Mathf.RoundToInt(MetaProgress.GetBonus(type, level) * 100f);

        if (levelText != null) levelText.text = "Level " + level + " / " + MetaProgress.MaxLevel;

        if (MetaProgress.IsMaxed(type))
        {
            effectText.text = effectLabel + " +" + nowPercent + "%  (MAX)";
            costText.text = "MAXED";
            buyButton.interactable = false;
        }
        else
        {
            int nextPercent = Mathf.RoundToInt(MetaProgress.GetBonus(type, level + 1) * 100f);
            int cost = MetaProgress.GetNextCost(type);

            effectText.text = effectLabel + " +" + nowPercent + "%  ->  +" + nextPercent + "%";
            costText.text = cost + " coins";
            buyButton.interactable = MetaProgress.Coins >= cost;
        }

        if (levelPips != null)
        {
            for (int i = 0; i < levelPips.Length; i++)
                if (levelPips[i] != null) levelPips[i].color = i < level ? pipOn : pipOff;
        }
    }
}