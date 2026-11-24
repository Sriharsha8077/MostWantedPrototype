using System;
using System.Collections;
using UnityEngine;
#if ADMOB_ENABLED
using GoogleMobileAds.Api;
#endif

public class AdManager : MonoBehaviour
{
    public static AdManager Instance;

    public enum SimulatedResult { Reward, ClosedEarly, NotAvailable }

    [Header("Ad unit IDs. These are Google's public TEST IDs. Swap in your own before release.")]
    public string androidRewardedId = "ca-app-pub-3940256099942544/5224354917";
    public string iosRewardedId = "ca-app-pub-3940256099942544/1712485313";

    [Header("Editor testing (no effect on a phone)")]
    public bool simulateInEditor = true;
    public SimulatedResult simulatedResult = SimulatedResult.Reward;
    public float simulatedAdSeconds = 2f;

    public bool IsAdPlaying { get; private set; }

    private Action<bool> pendingCallback;

#if ADMOB_ENABLED && (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
    private RewardedAd rewardedAd;
    private bool rewardEarned;
    private int loadFailures;
#endif

    void Awake()
    {
        // Survive scene reloads; destroy any duplicate created by a reload.
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (Instance != this) return;

#if ADMOB_ENABLED && (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        MobileAds.RaiseAdEventsOnUnityMainThread = true;   // so ad callbacks can safely touch Unity objects
        MobileAds.Initialize(initStatus => LoadRewarded());
#endif
    }

    // True when an ad can be shown right now.
    public bool IsRewardedReady
    {
        get
        {
#if UNITY_EDITOR
            return simulateInEditor && simulatedResult != SimulatedResult.NotAvailable;
#elif ADMOB_ENABLED && (UNITY_ANDROID || UNITY_IOS)
            return rewardedAd != null && rewardedAd.CanShowAd();
#else
            return false;
#endif
        }
    }

    // onFinished(true) = the player earned the reward. onFinished(false) = no reward.
    // The callback is always invoked exactly once.
    public void ShowRewarded(Action<bool> onFinished)
    {
        if (IsAdPlaying) return;

        if (!IsRewardedReady)
        {
            if (onFinished != null) onFinished(false);
            return;
        }

        IsAdPlaying = true;
        pendingCallback = onFinished;
        AudioListener.pause = true;   // silence the game while the ad plays

#if UNITY_EDITOR
        StartCoroutine(SimulateAd());
#elif ADMOB_ENABLED && (UNITY_ANDROID || UNITY_IOS)
        rewardEarned = false;
        rewardedAd.Show(reward => { rewardEarned = true; });
#else
        FinishAd(false);
#endif
    }

    void FinishAd(bool rewarded)
    {
        if (!IsAdPlaying) return;
        IsAdPlaying = false;
        AudioListener.pause = false;

        Action<bool> callback = pendingCallback;
        pendingCallback = null;
        if (callback != null) callback(rewarded);
    }

#if UNITY_EDITOR
    IEnumerator SimulateAd()
    {
        Debug.Log("[AdManager] Simulated rewarded ad playing...");
        yield return new WaitForSecondsRealtime(simulatedAdSeconds);
        FinishAd(simulatedResult == SimulatedResult.Reward);
    }
#endif

#if ADMOB_ENABLED && (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        void LoadRewarded()
    {
        if (rewardedAd != null) { rewardedAd.Destroy(); rewardedAd = null; }

#if UNITY_ANDROID
        // Development builds always use Google's test ad unit, so live ads can never be clicked by accident.
        string adUnitId = Debug.isDebugBuild ? "ca-app-pub-3940256099942544/5224354917" : androidRewardedId;
#else
        string adUnitId = Debug.isDebugBuild ? "ca-app-pub-3940256099942544/1712485313" : iosRewardedId;
#endif

        RewardedAd.Load(adUnitId, new AdRequest(), (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                loadFailures++;
                Debug.LogWarning("[AdManager] Rewarded ad failed to load: " + error);
                StartCoroutine(RetryLoad());
                return;
            }

            Debug.Log("[AdManager] Rewarded ad loaded and ready.");
            loadFailures = 0;
            rewardedAd = ad;

            ad.OnAdFullScreenContentClosed += () => StartCoroutine(FinishAfterClose());
            ad.OnAdFullScreenContentFailed += (AdError adError) =>
            {
                Debug.LogWarning("[AdManager] Rewarded ad failed to show: " + adError);
                FinishAd(false);
                LoadRewarded();
            };
        });
    }

    IEnumerator RetryLoad()
    {
        yield return new WaitForSecondsRealtime(Mathf.Min(120f, 15f * loadFailures));
        LoadRewarded();
    }

    // Short wait so the "reward earned" callback is always processed before we judge the result.
    IEnumerator FinishAfterClose()
    {
        yield return new WaitForSecondsRealtime(0.25f);
        FinishAd(rewardEarned);
        LoadRewarded();   // an ad can only be shown once per load, so load the next one
    }
#endif
}