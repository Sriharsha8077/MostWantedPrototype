using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;

    [Tooltip("Drag the JoystickZone object here. Leave empty for a keyboard-only build.")]
    public TouchJoystick touchJoystick;   // TOUCH (no touch controls yet? delete this line and the 'touchJoystick' line in FixedUpdate)

    public bool IsInvulnerable { get; private set; }

    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Color baseColor = Color.white;
    private float invulnerableTimer;

  
   void Awake()
{
    rb = GetComponent<Rigidbody2D>();
    moveSpeed *= MetaProgress.TotalSpeedMultiplier;   // NEW: speed upgrade + any one-run boost
    sprite = GetComponent<SpriteRenderer>();          // (keep whatever else your Awake already has)
    if (sprite != null) baseColor = sprite.color;
}

    public void GrantInvulnerability(float seconds)
    {
        invulnerableTimer = Mathf.Max(invulnerableTimer, seconds);
        IsInvulnerable = true;
    }

    void Update()
    {
        if (!IsInvulnerable) return;
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        invulnerableTimer -= Time.deltaTime;

        // Blink so the player can see they're protected.
        if (sprite != null)
        {
            bool dim = Mathf.FloorToInt(Time.time * 10f) % 2 == 0;
            sprite.color = new Color(baseColor.r, baseColor.g, baseColor.b, dim ? 0.35f : baseColor.a);
        }

        if (invulnerableTimer <= 0f)
        {
            IsInvulnerable = false;
            if (sprite != null) sprite.color = baseColor;
        }
    }

    void FixedUpdate()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        Vector2 dir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (touchJoystick != null) dir += touchJoystick.Direction;   // TOUCH

        if (dir.magnitude > 1f) dir.Normalize();
        rb.linearVelocity = dir * moveSpeed;
    }

    void OnTriggerEnter2D(Collider2D other) { TryGetCaught(other); }
    void OnTriggerStay2D(Collider2D other) { TryGetCaught(other); }

    void TryGetCaught(Collider2D other)
    {
        if (IsInvulnerable) return;
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        if (other.CompareTag("Police"))
            GameManager.Instance.EndRun("Caught by police");
    }
}