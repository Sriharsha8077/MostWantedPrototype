using UnityEngine;

public class PoliceUnit : MonoBehaviour
{
    [HideInInspector] public Transform playerTransform;
    [HideInInspector] public Rigidbody2D playerRb;

    public float baseSpeed = 3f;
    private float predictionTime = 0.5f;
    private float disabledTimer = 0f;
    private Vector2 surroundOffset;

    public void Disable(float duration) { disabledTimer = duration; }

    void OnEnable()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float radius = Random.Range(0.4f, 1.2f);
        surroundOffset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        if (playerTransform == null) return;

        if (disabledTimer > 0)
        {
            disabledTimer -= Time.deltaTime;
            return;
        }

        Vector2 targetPos = (Vector2)playerTransform.position + surroundOffset;

        if (DifficultyManager.Instance.PredictionEnabled)
        {
            Vector2 predictedOffset = playerRb.linearVelocity * predictionTime;
            targetPos = (Vector2)playerTransform.position + predictedOffset + surroundOffset;
        }

        Vector2 direction = (targetPos - (Vector2)transform.position).normalized;
        direction = (direction + GetSeparationVector() * 1.2f).normalized;

        transform.position += (Vector3)(direction * baseSpeed * Time.deltaTime);
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