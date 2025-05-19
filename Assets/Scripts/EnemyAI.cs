using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Movement Settings")]
    [Tooltip("Enemy base movement speed")]
    public float speed = 3f;
    [Tooltip("Distance within which the enemy detects the player")]
    public float detectionRadius = 20f; // Added detection radius
    [Tooltip("Maximum escape distance from player")]
    public float maxEscapeDistance = 15f;
    [Tooltip("Minimum safe distance from player when fleeing")]
    public float minFleeSafeDistance = 8f; // Renamed for clarity with fleeing
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

    private bool isVulnerable; // This is set by GameManager to indicate Hunter Mode
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
        if (player == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh) 
        {
            if (animator) animator.SetBool("Walk", false);
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > detectionRadius)
        {
            // Player is outside detection radius, enemy should be idle
            if (agent.hasPath)
            {
                agent.ResetPath(); // Stop current movement
            }
            if (isAttacking) // If was attacking, stop attack sequence
            {
                EndAttackClean();
            }
            if (animator) animator.SetBool("Walk", false);
            return; // Do nothing else if player is too far
        }

        // Player is within detection radius
        if (isVulnerable) // Hunter mode active (enemy is vulnerable)
        {
            // Flee from player
            if (isAttacking) // If was attacking, stop attack sequence to flee
            {
                EndAttackClean();
            }
            if (Time.time >= nextPathUpdate)
            {
                FleeFromPlayer();
                nextPathUpdate = Time.time + pathUpdateInterval;
            }
        }
        else
        {
            // Normal mode: Follow player and try to attack
            agent.SetDestination(player.position);
            agent.isStopped = false; // Ensure agent can move

            if (!isAttacking && Time.time >= nextAttackTime)
            {
                if (distanceToPlayer <= attackRange)
                {
                    StartAttack();
                }
            }
        }

        // Update animation based on agent's velocity and if it has a path
        if (animator)
        {
            animator.SetBool("Walk", agent.velocity.magnitude > 0.1f && agent.hasPath);
        }
    }

    public void SetVulnerable(bool vulnerable)
    {
        isVulnerable = vulnerable;
        // Speed adjustment is handled by flee/chase logic or can be general
        // If hunter mode starts (vulnerable = true), current chase/attack should be interrupted by Update logic
        // If hunter mode ends (vulnerable = false), Update logic will resume chase if player in range
        if (agent != null && agent.isOnNavMesh)
        {
             // Example: make enemy slower when vulnerable, faster when not, handled by flee/chase logic primarily
            // agent.speed = vulnerable ? speed * 0.7f : speed; // This might conflict with flee/chase specific speeds
        }
        // If becoming vulnerable while attacking, the attack should be interrupted.
        if (vulnerable && isAttacking)
        {
            EndAttackClean(); // Stop the attack to prioritize fleeing
        }
    }

    void StartAttack()
    {
        if (isAttacking) return;

        isAttacking = true;
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }

        // Perform the actual attack
        PerformAttack();

        // Reset after animation
        Invoke(nameof(EndAttack), attackAnimationDuration);
    }

    void EndAttack()
    {
        isAttacking = false;
        if (agent != null && agent.isOnNavMesh && !isVulnerable) // Only resume agent if not fleeing
        {
            agent.isStopped = false;
        }
        nextAttackTime = Time.time + attackCooldown;
    }

    // New method to cleanly end attack without setting next attack time, for interruptions
    void EndAttackClean()
    {
        isAttacking = false;
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false; // Allow agent to move (either flee or resume chase if mode changes)
        }
        // Do not set nextAttackTime here, as the attack was interrupted
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

    // Renamed from CalculateEscapePath to FleeFromPlayer for clarity
    void FleeFromPlayer()
    {
        if (player == null || !agent.isOnNavMesh) return;

        agent.speed = speed * 1.2f; // Flee slightly faster
        agent.stoppingDistance = 0.5f; // Allow getting closer to flee point

        // Get direction away from player
        Vector3 directionFromPlayer = (transform.position - player.position).normalized;

        // Try to find a valid escape point further away from the player
        // And also try to maintain a minimum safe distance if possible
        Vector3 potentialEscapePoint = transform.position + directionFromPlayer * maxEscapeDistance;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(potentialEscapePoint, out hit, maxEscapeDistance, NavMesh.AllAreas))
        {
            // Check if this point is further than minSafeDistance or at least further than current pos
            float distToPlayerFromHit = Vector3.Distance(hit.position, player.position);
            float currentDistToPlayer = Vector3.Distance(transform.position, player.position);

            if (distToPlayerFromHit > currentDistToPlayer && distToPlayerFromHit > minFleeSafeDistance)
            {
                agent.SetDestination(hit.position);
                agent.isStopped = false;
            }
            else
            {
                // If we can't find a good point far away, try to find one just away from player
                Vector3 awayPoint = transform.position + directionFromPlayer * 5f; // Move 5 units away
                if (NavMesh.SamplePosition(awayPoint, out hit, 5f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                    agent.isStopped = false;
                }
                else if (agent.hasPath) // If no good flee point, at least stop moving towards player
                {
                    agent.ResetPath();
                }
            }
        }
        else if (agent.hasPath) // If sampling fails, stop current path
        {
            agent.ResetPath();
        }
    }

    // Optional: Visualize attack range and detection radius in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.blue;
        if (agent != null && agent.hasPath)
        {
            Gizmos.DrawLine(transform.position, agent.destination);
        }
    }
}