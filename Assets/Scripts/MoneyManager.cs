using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance;
    public float Money { get; private set; } = 0f;
    public float baseIncomePerSecond = 5f;

    void Awake() { Instance = this; }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        float rateMultiplier = 1f + (DifficultyManager.Instance.WantedLevel - 1) * 0.15f;
        Money += baseIncomePerSecond * rateMultiplier * Time.deltaTime;
    }

    public bool TrySpend(float amount)
    {
        if (Money < amount) return false;
        Money -= amount;
        return true;
    }
}