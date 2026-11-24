using UnityEngine;

public class ReviveManager : MonoBehaviour
{
    public static ReviveManager Instance;

    [Header("Rules")]
    public bool reviveEnabled = true;
    public int maxRevivesPerRun = 5;
    [Tooltip("Only offer a continue from this Wanted Level up.")]
    public int minWantedLevelToOffer = 2;
    [Tooltip("0 = offer again immediately. Try 5-8 if the next ad isn't always ready in time.")]
    public float minSecondsBetweenRevives = 0f;

    [Header("What a revive does")]
    public PlayerController player;
    [Tooltip("Police this close are removed.")]
    public float clearRadius = 6f;
    [Tooltip("Police between clearRadius and this are frozen briefly.")]
    public float stunRadius = 12f;
    public float stunSeconds = 2f;
    public float invulnerableSeconds = 3f;
    [Tooltip("Extra radius per Wanted Level above 1, so a revive still works when the horde is huge.")]
    public float extraRadiusPerWantedLevel = 1f;

    private float lastReviveAt = -999f;

    void Awake() { Instance = this; }

    // How many continues the player still has, counting the one currently on offer.
    public int RevivesLeft
    {
        get { return Mathf.Max(0, maxRevivesPerRun - GameManager.Instance.RevivesUsedThisRun); }
    }

    public bool ShouldOfferRevive(string cause)
    {
        if (!reviveEnabled) return false;
        if (cause == GameManager.NukeCause) return false;   // nuke = the final ending
        if (GameManager.Instance.RevivesUsedThisRun >= maxRevivesPerRun) return false;
        if (DifficultyManager.Instance.WantedLevel < minWantedLevelToOffer) return false;

        if (GameManager.Instance.RevivesUsedThisRun > 0 &&
            GameManager.Instance.SurvivalTime - lastReviveAt < minSecondsBetweenRevives) return false;

        if (AdManager.Instance == null || !AdManager.Instance.IsRewardedReady) return false;
        return true;
    }

    public void DoRevive()
    {
        int wanted = DifficultyManager.Instance.WantedLevel;
        float bonus = extraRadiusPerWantedLevel * (wanted - 1);
        float clear = clearRadius + bonus;
        float stun = stunRadius + bonus;

        Vector2 center = player.transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, stun);

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Police")) continue;

            float distance = Vector2.Distance(center, hit.transform.position);
            if (distance <= clear)
            {
                PolicePool.Instance.Return(hit.gameObject);
            }
            else
            {
                PoliceUnit unit = hit.GetComponent<PoliceUnit>();
                if (unit != null) unit.Disable(stunSeconds);
            }
        }

        lastReviveAt = GameManager.Instance.SurvivalTime;
        player.GrantInvulnerability(invulnerableSeconds);
        GameManager.Instance.ResumeAfterRevive();
    }
}