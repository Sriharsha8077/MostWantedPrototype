using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DayNightCycle : MonoBehaviour
{
    [System.Serializable]
    public class Phase
    {
        public string name = "Midday";
        [Tooltip("The colour laid over the whole screen. Alpha is the strength (0 = no tint).")]
        public Color tint = new Color(1f, 1f, 1f, 0f);
    }

    [Header("Overlay")]
    [Tooltip("A full-screen Image on its own canvas (behind the HUD). Raycast Target must be OFF.")]
    public Image overlay;

    [Header("Timing")]
    [Tooltip("Seconds each phase lasts. 30 = a new phase every 30 seconds. Use 60 for a slower cycle.")]
    public float secondsPerPhase = 30f;
    [Tooltip("How long the fade into the next phase takes, at the end of each phase.")]
    public float blendSeconds = 8f;

    [Header("Phases (in order, then it loops). Delete 'Night' if you only want three.")]
    public Phase[] phases = new Phase[]
    {
        new Phase { name = "Morning", tint = new Color(1.00f, 0.82f, 0.62f, 0.20f) },
        new Phase { name = "Midday",  tint = new Color(1.00f, 1.00f, 1.00f, 0.00f) },
        new Phase { name = "Evening", tint = new Color(1.00f, 0.45f, 0.20f, 0.28f) },
        new Phase { name = "Night",   tint = new Color(0.06f, 0.09f, 0.30f, 0.45f) }
    };

    [Header("Optional label")]
    public TMP_Text phaseLabel;

    private int lastIndex = -1;
    private int lastDay = -1;

    void Update()
    {
        if (overlay == null || phases == null || phases.Length == 0 || secondsPerPhase <= 0f) return;

        // The run's survival timer is the clock, so the cycle restarts every run,
        // pauses while the continue panel is open, and carries on after a revive.
        float t = GameManager.Instance != null ? GameManager.Instance.SurvivalTime : Time.timeSinceLevelLoad;

        float cycleLength = phases.Length * secondsPerPhase;
        float inCycle = t % cycleLength;
        int index = Mathf.Min(phases.Length - 1, Mathf.FloorToInt(inCycle / secondsPerPhase));
        float inPhase = inCycle - index * secondsPerPhase;

        Color color = phases[index].tint;

        float blend = Mathf.Clamp(blendSeconds, 0f, secondsPerPhase);
        float fadeStart = secondsPerPhase - blend;
        if (blend > 0f && inPhase > fadeStart)
        {
            float k = Mathf.SmoothStep(0f, 1f, (inPhase - fadeStart) / blend);
            Color next = phases[(index + 1) % phases.Length].tint;
            color = Color.Lerp(phases[index].tint, next, k);
        }

        overlay.color = color;

        // Update the label only when the phase actually changes.
        int day = Mathf.FloorToInt(t / cycleLength) + 1;
        if (phaseLabel != null && (index != lastIndex || day != lastDay))
        {
            phaseLabel.text = day > 1 ? "Day " + day + "  -  " + phases[index].name : phases[index].name;
            lastIndex = index;
            lastDay = day;
        }
    }
}