
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PoliceUnit))]
public class PoliceAnimationController : MonoBehaviour
{
    private Animator animator;
    private PoliceUnit policeUnit;

    private Vector3 previousPosition;
    private int lastDirection = 0;

    private static readonly int DirectionHash =
        Animator.StringToHash("Direction");

    private static readonly int IsStunnedHash =
        Animator.StringToHash("IsStunned");

    private const float MovementThreshold = 0.000001f;

    void Awake()
    {
        animator = GetComponent<Animator>();
        policeUnit = GetComponent<PoliceUnit>();
    }

    void OnEnable()
    {
        // Reset position tracking when reused from the pool
        previousPosition = transform.position;
        lastDirection = 0;

        // Reset Animator to its default state
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);

            animator.SetInteger(DirectionHash, lastDirection);
            animator.SetBool(IsStunnedHash, false);
        }
    }

    void LateUpdate()
    {
        if (animator == null || policeUnit == null)
            return;

        // Freeze animation outside active gameplay
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            animator.speed = 0f;
            previousPosition = transform.position;
            return;
        }

        animator.speed = 1f;

        // Detect stun state
        bool stunned = policeUnit.disabledTimer > 0f;
        animator.SetBool(IsStunnedHash, stunned);

        // Calculate actual movement this frame
        Vector3 movement = transform.position - previousPosition;
        previousPosition = transform.position;

        // Keep last direction while stunned
        if (stunned)
            return;

        // Update direction only when actually moving
        if (movement.sqrMagnitude > MovementThreshold)
        {
            if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
            {
                lastDirection = movement.x < 0f ? 2 : 3;
            }
            else
            {
                lastDirection = movement.y < 0f ? 0 : 1;
            }
        }

        animator.SetInteger(DirectionHash, lastDirection);
    }
}
