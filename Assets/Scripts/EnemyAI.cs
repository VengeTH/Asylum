using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Movement Settings")]
    [Tooltip("Enemy movement speed")]
    public float speed = 3f;
    [Tooltip("Distance to check for obstacles")]
    public float obstacleCheckDistance = 1.0f;
    [Tooltip("Radius of the obstacle check sphere")]
    public float obstacleCheckRadius = 0.2f;
    [Tooltip("Layers that the enemy considers as obstacles")]
    public LayerMask obstacleMask;

    private bool isVulnerable;
    private Rigidbody rb;
    private Animator animator;
    private Transform cachedTransform;
    private Vector3 currentDirection;

    void Awake()
    {
        // Cache components for better performance
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        cachedTransform = transform;

        // Validate required components
        if (!rb || !player)
        {
            enabled = false;
            return;
        }
    }

    void FixedUpdate()
    {
        if (!enabled) return;

        // Calculate direction to player
        currentDirection = (isVulnerable ?
            cachedTransform.position - player.position :
            player.position - cachedTransform.position).normalized;

        currentDirection.y = 0;

        // Check for obstacles
        bool blocked = Physics.SphereCast(
            cachedTransform.position,
            obstacleCheckRadius,
            currentDirection,
            out _,
            obstacleCheckDistance,
            obstacleMask
        );

        // Update movement and animation
        if (!blocked)
        {
            // Apply movement
            Vector3 moveVelocity = currentDirection * speed;
            rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);

            // Update rotation
            if (currentDirection != Vector3.zero)
            {
                cachedTransform.rotation = Quaternion.Slerp(
                    cachedTransform.rotation,
                    Quaternion.LookRotation(currentDirection),
                    10f * Time.deltaTime
                );
            }

            // Trigger animation
            if (animator)
            {
                animator.SetBool("Walk", true);
            }
        }
        else
        {
            // Stop horizontal movement when blocked
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);

            // Stop animation
            if (animator)
            {
                animator.SetBool("Walk", false);
            }
        }
    }

    public void SetVulnerable(bool state)
    {
        isVulnerable = state;
    }
}