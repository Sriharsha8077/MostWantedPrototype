using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ContinuePanelUI : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text titleText;
    public TMP_Text messageText;
    public TMP_Text countdownText;
    public Button watchAdButton;
    public Button noThanksButton;
    public float decisionSeconds = 6f;

    private float remaining;
    private bool offering;
    private bool waitingForAd;

    void Start()
    {
        panel.SetActive(false);
        watchAdButton.onClick.AddListener(OnWatchAdPressed);
        noThanksButton.onClick.AddListener(OnNoThanksPressed);
        GameManager.Instance.OnContinueOffered += ShowOffer;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnContinueOffered -= ShowOffer;
    }

    void ShowOffer()
    {
        offering = true;
        waitingForAd = false;
        remaining = decisionSeconds;

        titleText.text = "Caught!";
        messageText.text = "Watch a short ad to keep running?";
        watchAdButton.interactable = true;
        noThanksButton.interactable = true;
        panel.SetActive(true);
    }

    void Update()
    {
        if (!offering || waitingForAd) return;

        remaining -= Time.unscaledDeltaTime;
        if (countdownText.text != "") countdownText.text = Mathf.CeilToInt(Mathf.Max(0f, remaining)).ToString();

        if (remaining <= 0f) Decline();
    }

    void OnWatchAdPressed()
    {
        if (!offering || waitingForAd) return;

        waitingForAd = true;
        watchAdButton.interactable = false;
        noThanksButton.interactable = false;
        messageText.text = "Playing ad...";
        countdownText.text = "";

        AdManager.Instance.ShowRewarded(OnAdFinished);
    }

    void OnAdFinished(bool rewarded)
    {
        waitingForAd = false;
        if (!offering) return;

        if (rewarded)
        {
            Close();
            ReviveManager.Instance.DoRevive();
        }
        else
        {
            // Ad closed early or failed: no revive. Show a short message, then go to the summary.
            messageText.text = "No reward earned.";
            remaining = 1.5f;
        }
    }

    void OnNoThanksPressed() { Decline(); }

    void Decline()
    {
        Close();
        GameManager.Instance.DeclineRevive();
    }

    void Close()
    {
        offering = false;
        panel.SetActive(false);
    }
}