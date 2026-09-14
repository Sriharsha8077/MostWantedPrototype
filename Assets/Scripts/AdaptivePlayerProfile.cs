using UnityEngine;

public class AdaptivePlayerProfile : MonoBehaviour
{
    public static AdaptivePlayerProfile Instance;
    public Transform playerTransform; // assign in Inspector

    private float[] sectorTime = new float[8]; // this-run tracking
    public float[] PersistentBias { get; private set; } = new float[8]; // across sessions
    private const string PrefKey = "escape_bias_";

    static readonly string[] Labels =
        { "east", "northeast", "north", "northwest", "west", "southwest", "south", "southeast" };

    void Awake() { Instance = this; LoadBias(); }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        Rigidbody2D rb = playerTransform.GetComponent<Rigidbody2D>();
        if (rb.linearVelocity.magnitude < 0.1f) return;
        sectorTime[DirectionToSector(rb.linearVelocity)] += Time.deltaTime;
    }

    int DirectionToSector(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;
        return Mathf.FloorToInt(angle / 45f) % 8;
    }

    public int GetDominantSector()
    {
        int best = 0;
        for (int i = 1; i < 8; i++) if (sectorTime[i] > sectorTime[best]) best = i;
        return best;
    }

    public string GetSectorLabel(int sector) => Labels[sector];

    public void CommitRunToPersistentBias()
    {
        float total = 0f;
        foreach (float v in sectorTime) total += v;
        if (total < 5f) return; // ignore very short runs

        int dominant = GetDominantSector();
        for (int i = 0; i < 8; i++)
            PersistentBias[i] = i == dominant
                ? Mathf.Min(PersistentBias[i] + 1f, 6f)
                : Mathf.Max(PersistentBias[i] - 0.3f, 0f);
        SaveBias();
    }

    public int GetBiasedSectorForSpawning()
    {
        int best = 0;
        for (int i = 1; i < 8; i++) if (PersistentBias[i] > PersistentBias[best]) best = i;
        return PersistentBias[best] > 0.5f ? best : -1;
    }

    void SaveBias()
    {
        for (int i = 0; i < 8; i++) PlayerPrefs.SetFloat(PrefKey + i, PersistentBias[i]);
        PlayerPrefs.Save();
    }

    void LoadBias()
    {
        for (int i = 0; i < 8; i++) PersistentBias[i] = PlayerPrefs.GetFloat(PrefKey + i, 0f);
    }
}