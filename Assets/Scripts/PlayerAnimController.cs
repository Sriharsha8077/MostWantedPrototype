
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;

    private int lastDirection = 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // Stop animation when the game isn't running
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            animator.SetBool("IsMoving", false);
            return;
        }

        Vector2 velocity = rb.linearVelocity;

        bool isMoving = velocity.magnitude > 0.1f;

        if (isMoving)
        {
            // Calculate movement angle
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;

            // Convert angle to 0-360 degrees
            if (angle < 0)
                angle += 360f;

            // Divide movement into 8 directions
            int directionIndex = Mathf.RoundToInt(angle / 45f) % 8;

            // Map angle to Animator Direction values
            int[] directionMap = { 6, 5, 4, 3, 2, 1, 0, 7 };

            lastDirection = directionMap[directionIndex];
        }

        animator.SetBool("IsMoving", isMoving);
        animator.SetInteger("Direction", lastDirection);
    }
}
