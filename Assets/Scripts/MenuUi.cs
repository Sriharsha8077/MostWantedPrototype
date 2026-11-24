using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuUI : MonoBehaviour
{
    public TMP_Text coinsText;
    public TMP_Text bestTimeText;     // optional
    public Button playButton;
    public string gameSceneName = "SampleScene";

    void Awake()
    {
        playButton.onClick.AddListener(OnPlayPressed);
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

    void Refresh()
    {
        coinsText.text = MetaProgress.Coins.ToString();

        if (bestTimeText != null)
        {
            float best = PlayerPrefs.GetFloat("best_survival_time", 0f);
            bestTimeText.text = best > 0f
                ? "Best: " + Mathf.FloorToInt(best / 60f).ToString("00") + ":" + Mathf.FloorToInt(best % 60f).ToString("00")
                : "";
        }
    }

    void OnPlayPressed()
    {
        GameManager.MarkIntroShown();   // the menu replaces the old intro screen
        SceneManager.LoadScene(gameSceneName);
    }

#if UNITY_EDITOR
    // Editor-only test keys (these don't exist in a real build).
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C)) MetaProgress.AddCoins(500);   // C = +500 coins
        if (Input.GetKeyDown(KeyCode.R)) MetaProgress.ResetAll();      // R = wipe all progress
    }
#endif
}