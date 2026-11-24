using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance;
    public float Money { get; private set; } = 0f;
    public float baseIncomePerSecond = 5f;

    private float cashMultiplier = 1f;

    void Awake()
    {
        Instance = this;
        // Read once per run: permanent cash upgrade + any one-run boost.
        cashMultiplier = MetaProgress.TotalCashMultiplier;
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        float rateMultiplier = 1f + (DifficultyManager.Instance.WantedLevel - 1) * 0.15f;
        Money += baseIncomePerSecond * rateMultiplier * cashMultiplier * Time.deltaTime;
    }

    public bool TrySpend(float amount)
    {
        if (Money < amount) return false;
        Money -= amount;
        return true;
    }
}