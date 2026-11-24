using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoostCardUI : MonoBehaviour
{
    public MetaProgress.UpgradeType type;

    public TMP_Text titleText;
    public TMP_Text statusText;

    [Header("Free boost by watching an ad")]
    public Button adButton;
    public TMP_Text adButtonText;

    [Header("Or buy with coins (set Coin Cost to 0 to disable)")]
    public int coinCost = 100;
    public Button coinButton;          // optional
    public TMP_Text coinButtonText;    // optional

    private float refreshTimer;

    void Awake()
    {
        adButton.onClick.AddListener(OnAdPressed);
        if (coinButton != null) coinButton.onClick.AddListener(OnCoinPressed);
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

    // Re-check twice a second so the button wakes up when an ad finishes loading.
    void Update()
    {
        refreshTimer -= Time.unscaledDeltaTime;
        if (refreshTimer <= 0f)
        {
            refreshTimer = 0.5f;
            Refresh();
        }
    }

    string Description()
    {
        if (type == MetaProgress.UpgradeType.Speed)
            return "+" + Mathf.RoundToInt(MetaProgress.SpeedBoostBonus * 100f) + "% speed for your next run";
        return "+" + Mathf.RoundToInt(MetaProgress.CashBoostBonus * 100f) + "% cash income for your next run";
    }

    void Refresh()
    {
        bool active = MetaProgress.IsBoostActive(type);

        if (active)
        {
            statusText.text = "ACTIVE for your next run";
            adButton.interactable = false;
            adButtonText.text = "Active";
            if (coinButton != null) coinButton.gameObject.SetActive(false);
            return;
        }

        statusText.text = Description();

        bool adReady = AdManager.Instance != null &&
                       AdManager.Instance.IsRewardedReady &&
                       !AdManager.Instance.IsAdPlaying;
        adButton.interactable = adReady;
        adButtonText.text = adReady ? "Free: watch ad" : "Ad not ready";

        if (coinButton != null)
        {
            bool coinsEnabled = coinCost > 0;
            coinButton.gameObject.SetActive(coinsEnabled);
            if (coinsEnabled)
            {
                coinButton.interactable = MetaProgress.Coins >= coinCost;
                if (coinButtonText != null) coinButtonText.text = coinCost + " coins";
            }
        }
    }

    void OnAdPressed()
    {
        if (AdManager.Instance == null) return;

        AdManager.Instance.ShowRewarded(rewarded =>
        {
            if (rewarded) MetaProgress.ActivateBoost(type);
            Refresh();
        });
    }

    void OnCoinPressed()
    {
        MetaProgress.TryBuyBoost(type, coinCost);
    }
}