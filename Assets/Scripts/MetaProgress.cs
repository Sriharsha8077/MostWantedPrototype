using System;
using UnityEngine;

public static class MetaProgress
{
    public enum UpgradeType { Speed, Cash }

    // ============================================================
    //  TUNING: edit these numbers to rebalance the whole economy.
    //  To change the level cap: change MaxLevel AND make every
    //  array below the right length (costs = MaxLevel entries,
    //  bonuses = MaxLevel + 1 entries, with 0 first).
    // ============================================================
    public const int MaxLevel = 5;
    public const float CoinsPerSecond = 1f;   // 100 seconds survived = 100 coins

    // Index = level (0..5). Level 0 = no upgrade.
    static readonly float[] SpeedBonusByLevel = { 0f, 0.04f, 0.08f, 0.12f, 0.16f, 0.20f };
    static readonly float[] CashBonusByLevel  = { 0f, 0.10f, 0.20f, 0.30f, 0.40f, 0.50f };

    // Index = current level (0..4). The value is the price of the NEXT level.
    static readonly int[] SpeedCosts = { 100, 200, 300, 400, 500 };
    static readonly int[] CashCosts  = { 150, 300, 450, 600, 750 };

    // One-run boosts.
    public const float SpeedBoostBonus = 0.10f;
    public const float CashBoostBonus  = 0.25f;

    // ============================================================

    const string CoinsKey = "meta_coins";
    const string SpeedKey = "meta_speed_level";
    const string CashKey  = "meta_cash_level";

    // Fires whenever coins, levels or boosts change. UI cards listen to this.
    public static event Action OnChanged;

    // One-run boosts live in memory only. They last until the next run ends.
    static bool speedBoostActive;
    static bool cashBoostActive;

    // The last finished run, for the summary screen.
    public static int LastRunCoins { get; private set; }
    public static bool LastRunDoubled { get; private set; }

    public static int Coins { get { return PlayerPrefs.GetInt(CoinsKey, 0); } }

    // ---------- permanent upgrades ----------

    public static int GetLevel(UpgradeType type)
    {
        string key = type == UpgradeType.Speed ? SpeedKey : CashKey;
        return Mathf.Clamp(PlayerPrefs.GetInt(key, 0), 0, MaxLevel);
    }

    public static bool IsMaxed(UpgradeType type) { return GetLevel(type) >= MaxLevel; }

    // Price of the next level, or -1 if already maxed.
    public static int GetNextCost(UpgradeType type)
    {
        int level = GetLevel(type);
        if (level >= MaxLevel) return -1;
        int[] costs = type == UpgradeType.Speed ? SpeedCosts : CashCosts;
        return costs[level];
    }

    public static float GetBonus(UpgradeType type, int level)
    {
        level = Mathf.Clamp(level, 0, MaxLevel);
        return type == UpgradeType.Speed ? SpeedBonusByLevel[level] : CashBonusByLevel[level];
    }

    public static bool TryBuyUpgrade(UpgradeType type)
    {
        int cost = GetNextCost(type);
        if (cost < 0 || Coins < cost) return false;

        PlayerPrefs.SetInt(CoinsKey, Coins - cost);
        string key = type == UpgradeType.Speed ? SpeedKey : CashKey;
        PlayerPrefs.SetInt(key, GetLevel(type) + 1);
        PlayerPrefs.Save();

        if (OnChanged != null) OnChanged();
        return true;
    }

    // ---------- multipliers used by the game ----------

    public static float TotalSpeedMultiplier
    {
        get
        {
            float bonus = GetBonus(UpgradeType.Speed, GetLevel(UpgradeType.Speed))
                          + (speedBoostActive ? SpeedBoostBonus : 0f);
            float cap = 1f + SpeedBonusByLevel[MaxLevel] + SpeedBoostBonus;   // hard ceiling
            return Mathf.Min(1f + bonus, cap);
        }
    }

    public static float TotalCashMultiplier
    {
        get
        {
            float bonus = GetBonus(UpgradeType.Cash, GetLevel(UpgradeType.Cash))
                          + (cashBoostActive ? CashBoostBonus : 0f);
            float cap = 1f + CashBonusByLevel[MaxLevel] + CashBoostBonus;     // hard ceiling
            return Mathf.Min(1f + bonus, cap);
        }
    }

    // ---------- one-run boosts ----------

    public static bool IsBoostActive(UpgradeType type)
    {
        return type == UpgradeType.Speed ? speedBoostActive : cashBoostActive;
    }

    public static void ActivateBoost(UpgradeType type)
    {
        if (type == UpgradeType.Speed) speedBoostActive = true;
        else cashBoostActive = true;
        if (OnChanged != null) OnChanged();
    }

    // Buy a boost with coins (the alternative to watching an ad).
    public static bool TryBuyBoost(UpgradeType type, int cost)
    {
        if (IsBoostActive(type) || cost <= 0 || Coins < cost) return false;

        PlayerPrefs.SetInt(CoinsKey, Coins - cost);
        PlayerPrefs.Save();
        ActivateBoost(type);
        return true;
    }

    // Called when a run ends so boosts only ever last for one run.
    public static void ClearRunBoosts()
    {
        speedBoostActive = false;
        cashBoostActive = false;
        if (OnChanged != null) OnChanged();
    }

    // ---------- coins ----------

    public static void AddCoins(int amount)
    {
        if (amount <= 0) return;
        PlayerPrefs.SetInt(CoinsKey, Coins + amount);
        PlayerPrefs.Save();
        if (OnChanged != null) OnChanged();
    }

    // Called once, when a run really ends. Returns the coins earned.
    public static int AwardRunCoins(float survivedSeconds)
    {
        int coins = Mathf.FloorToInt(survivedSeconds * CoinsPerSecond);
        LastRunCoins = coins;
        LastRunDoubled = false;
        AddCoins(coins);
        return coins;
    }

    // Rewarded-ad bonus: pays the same amount again, once per run. Returns the extra coins.
    public static int DoubleLastRunCoins()
    {
        if (LastRunDoubled || LastRunCoins <= 0) return 0;
        LastRunDoubled = true;
        AddCoins(LastRunCoins);
        return LastRunCoins;
    }

    // For testing: wipes coins, upgrades and boosts.
    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(CoinsKey);
        PlayerPrefs.DeleteKey(SpeedKey);
        PlayerPrefs.DeleteKey(CashKey);
        PlayerPrefs.Save();
        speedBoostActive = false;
        cashBoostActive = false;
        LastRunCoins = 0;
        LastRunDoubled = false;
        if (OnChanged != null) OnChanged();
    }
}