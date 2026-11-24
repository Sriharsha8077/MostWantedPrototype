using UnityEngine;

public class PoliceUnit : MonoBehaviour
{
    [HideInInspector] public Transform playerTransform;
    [HideInInspector] public Rigidbody2D playerRb;

    public float baseSpeed = 3f;

    [Header("Status tints (so the player can see who is affected)")]
    public Color stunTint = new Color(1f, 0.92f, 0.2f);      // yellow
    public Color slowTint = new Color(0.4f, 0.9f, 0.5f);     // green
    public Color blindTint = new Color(0.75f, 0.75f, 0.75f); // grey

    private float predictionTime = 0.5f;
    private Vector2 surroundOffset;

    public float disabledTimer;       // stun: no movement
    private float slowTimer;
    private float slowMultiplier = 1f;
    private float blindTimer;          // smoke: heads for blindTarget instead of the player
    private Vector2 blindTarget;

    private SpriteRenderer sprite;
    private Color baseColor = Color.white;
    private bool tinted;

    void Awake()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite != null) baseColor = sprite.color;
    }

    // ---------- effects (called by AbilityManager and ReviveManager) ----------

    public void Disable(float duration)
    {
        disabledTimer = Mathf.Max(disabledTimer, duration);
    }

    public void Slow(float multiplier, float duration)
    {
        multiplier = Mathf.Clamp(multiplier, 0.05f, 1f);
        // If already slowed, keep the stronger slow and the longer duration.
        slowMultiplier = slowTimer > 0f ? Mathf.Min(slowMultiplier, multiplier) : multiplier;
        slowTimer = Mathf.Max(slowTimer, duration);
    }

    public void Blind(Vector2 lastKnownPosition, float duration)
    {
        blindTarget = lastKnownPosition;
        blindTimer = Mathf.Max(blindTimer, duration);
    }

    // ---------- pooling ----------

    void OnEnable()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float radius = Random.Range(0.4f, 1.2f);
        surroundOffset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

        // Pooled officers are re-used, so clear anything left over from their previous life.
        disabledTimer = 0f;
        slowTimer = 0f;
        slowMultiplier = 1f;
        blindTimer = 0f;
        tinted = false;
        if (sprite != null) sprite.color = baseColor;
    }

    // ---------- movement ----------

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        if (playerTransform == null) return;

        float dt = Time.deltaTime;

        bool stunned = disabledTimer > 0f;
        bool blinded = blindTimer > 0f;
        bool slowed = slowTimer > 0f;
        ApplyTint(stunned, blinded, slowed);

        if (stunned)
        {
            disabledTimer -= dt;
            return;
        }

        float speedMult = 1f;
        if (slowed)
        {
            slowTimer -= dt;
            speedMult = slowMultiplier;
        }

        Vector2 position = transform.position;
        Vector2 targetPos;

        if (blinded)
        {
            // Lost track of the player: walk to where they were, a little slower.
            blindTimer -= dt;
            targetPos = blindTarget;
            speedMult *= 0.7f;
        }
        else
        {
            targetPos = (Vector2)playerTransform.position + surroundOffset;

            if (DifficultyManager.Instance.PredictionEnabled)
            {
                Vector2 predictedOffset = playerRb.linearVelocity * predictionTime;
                targetPos = (Vector2)playerTransform.position + predictedOffset + surroundOffset;
            }
        }

        Vector2 toTarget = targetPos - position;

        // A blinded officer that has reached the old spot just stands there, confused.
        if (blinded && toTarget.sqrMagnitude < 0.09f) return;

        Vector2 direction = toTarget.normalized;
        direction = (direction + GetSeparationVector() * 1.2f).normalized;

        transform.position += (Vector3)(direction * baseSpeed * speedMult * dt);
    }

    void ApplyTint(bool stunned, bool blinded, bool slowed)
    {
        if (sprite == null) return;

        if (stunned) { sprite.color = stunTint; tinted = true; }
        else if (blinded) { sprite.color = blindTint; tinted = true; }
        else if (slowed) { sprite.color = slowTint; tinted = true; }
        else if (tinted) { sprite.color = baseColor; tinted = false; }
    }

    Vector2 GetSeparationVector()
    {
        Vector2 separation = Vector2.zero;
        Collider2D[] neighbors = Physics2D.OverlapCircleAll(transform.position, 0.7f);
        foreach (var col in neighbors)
        {
            if (col.gameObject == gameObject || !col.CompareTag("Police")) continue;
            Vector2 away = (Vector2)(transform.position - col.transform.position);
            if (away.magnitude > 0.01f) separation += away.normalized / away.magnitude;
            else separation += Random.insideUnitCircle;
        }
        return separation;
    }
}