using UnityEngine;
using UnityEngine.EventSystems;

public class TouchJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Visuals (children of this zone)")]
    public RectTransform joystickBase;
    public RectTransform knob;

    [Header("Feel")]
    [Tooltip("How far the knob can travel from the centre, in canvas units.")]
    public float radius = 100f;
    [Range(0f, 0.5f)] public float deadZone = 0.12f;
    [Tooltip("ON: the joystick appears wherever the thumb lands. OFF: it stays at its home spot.")]
    public bool floating = true;
    [Range(0f, 1f)] public float idleAlpha = 0.35f;
    [Range(0f, 1f)] public float activeAlpha = 0.85f;

    // PlayerController reads this. (0,0) when nobody is touching.
    public Vector2 Direction { get; private set; }

    private const int NoPointer = -999;
    private int activePointer = NoPointer;
    private RectTransform zone;
    private Vector2 homePosition;
    private CanvasGroup baseGroup;

    void Awake()
    {
        zone = (RectTransform)transform;
        homePosition = joystickBase.anchoredPosition;

        baseGroup = joystickBase.GetComponent<CanvasGroup>();
        if (baseGroup == null) baseGroup = joystickBase.gameObject.AddComponent<CanvasGroup>();
        baseGroup.blocksRaycasts = false;   // visuals must never steal touches from the zone
        baseGroup.alpha = idleAlpha;
    }

    void OnDisable()
    {
        Release();   // safety: hiding the touch UI mid-touch can't leave the player drifting
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (activePointer != NoPointer) return;   // already tracking one finger
        activePointer = eventData.pointerId;

        if (floating)
        {
            Vector3 world;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    zone, eventData.position, eventData.pressEventCamera, out world))
            {
                joystickBase.position = world;
            }
        }

        baseGroup.alpha = activeAlpha;
        UpdateStick(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointer) return;
        UpdateStick(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointer) return;
        Release();
    }

    void UpdateStick(PointerEventData eventData)
    {
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBase, eventData.position, eventData.pressEventCamera, out local);

        Vector2 clamped = Vector2.ClampMagnitude(local, radius);
        knob.anchoredPosition = clamped;

        Vector2 raw = clamped / radius;
        float mag = raw.magnitude;
        if (mag < deadZone) Direction = Vector2.zero;
        else Direction = raw.normalized * ((mag - deadZone) / (1f - deadZone));
    }

    void Release()
    {
        activePointer = NoPointer;
        Direction = Vector2.zero;
        if (knob != null) knob.anchoredPosition = Vector2.zero;
        if (joystickBase != null) joystickBase.anchoredPosition = homePosition;
        if (baseGroup != null) baseGroup.alpha = idleAlpha;
    }
}