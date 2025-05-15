using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Movement Settings")]
    [Tooltip("Enemy base movement speed")]
    public float speed = 3f;
    [Tooltip("Maximum escape distance from player")]
    public float maxEscapeDistance = 15f;
    [Tooltip("Minimum safe distance from player")]
    public float minSafeDistance = 8f;
    [Tooltip("How often to recalculate escape path (seconds)")]
    public float pathUpdateInterval = 0.5f;

    [Header("Attack Settings")]
    [Tooltip("Distance at which enemy can attack")]
    public float attackRange = 2f;
    [Tooltip("Damage dealt to player when vulnerable")]
    public float normalAttackDamage = 20f;
    [Tooltip("Damage dealt to player when in hunter mode")]
    public float hunterModeAttackDamage = 10f;
    [Tooltip("Time between attacks")]
    public float attackCooldown = 1.5f;
    [Tooltip("Duration of the attack animation")]
    public float attackAnimationDuration = 0.5f;

    private bool isVulnerable;
    private NavMeshAgent agent;
    private Animator animator;
    private float nextPathUpdate;
    private Vector3 escapePoint;
    private float nextAttackTime;
    private bool isAttacking;
    private PlayerHealth playerHealth;

    void Awake()
    {
        // Cache components
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                playerHealth = playerObj.GetComponent<PlayerHealth>();
            }
        }
        else
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }

        // Configure NavMeshAgent
        if (agent != null)
        {
            agent.speed = speed;
            agent.stoppingDistance = attackRange * 0.8f;
        }

        // Log setup status
        if (playerHealth == null)
        {
            Debug.LogError($"PlayerHealth component not found for enemy {gameObject.name}!");
        }
        else
        {
            Debug.Log($"Enemy {gameObject.name} initialized with player reference");
        }
    }

    void Update()
    {
        if (!enabled || player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check if we can attack
        if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime && !isAttacking)
        {
            StartAttack();
        }

        if (!isAttacking) // Only move if not attacking
        {
            if (isVulnerable)
            {
                // Update escape path at intervals
                if (Time.time >= nextPathUpdate)
                {
                    CalculateEscapePath();
                    nextPathUpdate = Time.time + pathUpdateInterval;
                }
            }
            else
            {
                // Chase player when not vulnerable
                agent.SetDestination(player.position);
            }
        }

        // Update animation
        if (animator)
        {
            animator.SetBool("Walk", agent.velocity.magnitude > 0.1f);
        }
    }

    void StartAttack()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        // Stop movement during attack
        agent.isStopped = true;

        // Trigger attack animation
        if (animator)
        {
            animator.SetTrigger("Attack");
        }

        // Look at player
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        // Perform the actual attack
        PerformAttack();

        // Reset after animation
        Invoke(nameof(EndAttack), attackAnimationDuration);
    }

    void EndAttack()
    {
        isAttacking = false;
        agent.isStopped = false;
    }

    void PerformAttack()
    {
        if (playerHealth != null)
        {
            float damage = isVulnerable ? hunterModeAttackDamage : normalAttackDamage;
            Debug.Log($"Enemy {gameObject.name} attacking player for {damage} damage");
            playerHealth.TakeDamage(damage);
        }
        else
        {
            Debug.LogWarning($"Enemy {gameObject.name} tried to attack but playerHealth is null!");
        }
    }

    void CalculateEscapePath()
    {
        if (player == null) return;

        // Get direction away from player
        Vector3 directionFromPlayer = transform.position - player.position;

        // Try to find a valid escape point
        for (int i = 0; i < 8; i++) // Try 8 different directions
        {
            // Calculate potential escape point
            Vector3 potentialEscapePoint = transform.position + Quaternion.Euler(0, i * 45, 0) * directionFromPlayer.normalized * maxEscapeDistance;

            // Sample the nearest valid position on the NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(potentialEscapePoint, out hit, maxEscapeDistance, NavMesh.AllAreas))
            {
                // Check if this point increases distance from player
                float newDistanceToPlayer = Vector3.Distance(hit.position, player.position);
                float currentDistanceToPlayer = Vector3.Distance(transform.position, player.position);

                if (newDistanceToPlayer > currentDistanceToPlayer && newDistanceToPlayer > minSafeDistance)
                {
                    escapePoint = hit.position;
                    agent.SetDestination(escapePoint);
                    return;
                }
            }
        }

        // If no good escape point found, just move in opposite direction
        Vector3 fallbackPoint = transform.position + directionFromPlayer.normalized * maxEscapeDistance;
        NavMeshHit fallbackHit;
        if (NavMesh.SamplePosition(fallbackPoint, out fallbackHit, maxEscapeDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(fallbackHit.position);
        }
    }

    public void SetVulnerable(bool state)
    {
        isVulnerable = state;
        Debug.Log($"Enemy {gameObject.name} vulnerability set to: {state}");

        // Update agent speed and attack range based on state
        if (agent != null)
        {
            // Move faster when escaping
            agent.speed = state ? speed * 1.5f : speed;
            // Adjust stopping distance based on state
            agent.stoppingDistance = attackRange * (state ? 1.2f : 0.8f);
        }
    }

    // Optional: Visualize attack range in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}