using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;

    void Awake() { rb = GetComponent<Rigidbody2D>(); }

    void FixedUpdate()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        float x = Input.GetAxisRaw("Horizontal"); // A/D and Left/Right arrow, mapped by default
        float y = Input.GetAxisRaw("Vertical");   // W/S and Up/Down arrow, mapped by default
        Vector2 dir = new Vector2(x, y);
        if (dir.magnitude > 1f) dir.Normalize(); // diagonal isn't faster than straight

        rb.linearVelocity = dir * moveSpeed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Police") && GameManager.Instance.CurrentState == GameManager.GameState.Playing)
            GameManager.Instance.EndRun("Caught by police");
    }
}