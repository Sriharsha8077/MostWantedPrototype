using UnityEngine;

public class AbilityPulse : MonoBehaviour
{
    public float duration = 0.45f;

    private SpriteRenderer sr;
    private float spriteSize = 1f;
    private float targetDiameter = 1f;
    private float elapsed;
    private Color color = Color.white;

    public void Init(float radius, Color tint)
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null) spriteSize = sr.sprite.bounds.size.x;

        targetDiameter = radius * 2f;
        color = tint;
        SetScale(0.1f);
        if (sr != null) sr.color = new Color(tint.r, tint.g, tint.b, 0.45f);
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        float eased = 1f - (1f - t) * (1f - t);   // fast at first, then eases out

        SetScale(Mathf.Lerp(0.1f, targetDiameter, eased));
        if (sr != null) sr.color = new Color(color.r, color.g, color.b, Mathf.Lerp(0.45f, 0f, t));

        if (t >= 1f) Destroy(gameObject);
    }

    void SetScale(float worldDiameter)
    {
        float s = worldDiameter / Mathf.Max(0.0001f, spriteSize);
        transform.localScale = new Vector3(s, s, 1f);
    }
}