using UnityEngine;

public class TouchControls : MonoBehaviour
{
    // Static so it survives the scene reload on Restart.
    public static bool Active { get; private set; }

    [Header("Shown ONLY when touch controls are on")]
    public GameObject[] showWhenTouch;

    [Header("Hidden when touch controls are on (desktop-only UI)")]
    public GameObject[] hideWhenTouch;

    [Header("Testing")]
    [Tooltip("Tick to see the touch UI on desktop / in the Editor. UNTICK before you publish.")]
    public bool forceTouchUI = false;

    void Awake()
    {
        Apply(forceTouchUI || Application.isMobilePlatform || Active);
    }

    void Update()
    {
        // A real touch switches the touch UI on (covers mobile browsers and touchscreen laptops).
        if (!Active && Input.touchCount > 0) Apply(true);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // Press T in the Editor (or a Development Build) to flip between touch and desktop UI.
        if (Input.GetKeyDown(KeyCode.T)) Apply(!Active);
#endif
    }

    void Apply(bool on)
    {
        Active = on;
        foreach (GameObject go in showWhenTouch) { if (go != null) go.SetActive(on); }
        foreach (GameObject go in hideWhenTouch) { if (go != null) go.SetActive(!on); }
    }
}