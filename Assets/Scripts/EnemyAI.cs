using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    private enum EnemyState { Idle, FollowingPlayer, FleeingFromOrb, AggressivePursuit }
    private EnemyState currentState = EnemyState.Idle;

    [Header("References")]
    public Transform player;

    [Header("Movement Settings")]
    [Tooltip("Enemy base movement speed")]
    public float speed = 3f;
    [Tooltip("Distance within which the enemy detects the player for normal following")]
    public float detectionRadius = 20f;
    [Tooltip("Maximum escape distance from player when fleeing an orb")]
    public float maxEscapeDistance = 15f;
    [Tooltip("Minimum safe distance from player when fleeing an orb")]
    public float minFleeSafeDistance = 8f; 
    [Tooltip("How often to recalculate escape path (seconds) when fleeing")]
    public float pathUpdateInterval = 0.5f;
    [Tooltip("Duration for fleeing after a normal orb pickup")]
    public float fleeDuration = 5f;
    [Tooltip("Speed multiplier when in aggressive pursuit mode")]
    public float aggressiveModeSpeedMultiplier = 1.5f;

    [Header("Attack Settings")]
    [Tooltip("Distance at which enemy can attack")]
    public float attackRange = 2f;
    [Tooltip("Damage dealt to player during normal follow")]
    public float normalAttackDamage = 10f; // Adjusted for clarity
    [Tooltip("Damage dealt to player when in aggressive pursuit mode")]
    public float aggressiveModeAttackDamage = 20f; // Renamed from hunterModeAttackDamage
    [Tooltip("Time between attacks")]
    public float attackCooldown = 1.5f;
    [Tooltip("Duration of the attack animation")]
    public float attackAnimationDuration = 0.5f;

    private NavMeshAgent agent;
    private Animator animator;
    private float nextPathUpdate;
    private Vector3 escapePoint;
    private float nextAttackTime;
    private bool isAttacking;
    private PlayerHealth playerHealth;
    private float fleeEndTime;
    private float originalSpeed; // To store original speed before aggressive mode

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

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

        if (agent != null)
        {
            originalSpeed = speed; // Store original speed
            agent.speed = originalSpeed;
            agent.stoppingDistance = attackRange * 0.8f;
        }

        if (playerHealth == null)
        {
            Debug.LogError($"PlayerHealth component not found for enemy {gameObject.name}!");
        }
        else
        {
            Debug.Log($"Enemy {gameObject.name} initialized. Current state: {currentState}");
        }
    }

    // Called by GameManager when enough objectives are collected
    public void ActivateObjectiveBasedAggression()
    {
        if (isAttacking) EndAttackClean(); // Interrupt any ongoing attack

        currentState = EnemyState.AggressivePursuit;
        if (agent != null) agent.speed = originalSpeed * aggressiveModeSpeedMultiplier;
        fleeEndTime = 0; // Ensure not fleeing if switching from fleeing state
        Debug.Log($"Enemy {gameObject.name}: Aggressive Pursuit activated due to objectives.");
    }

    // Called by OrbManager when an orb is collected
    public void FleeFromOrbAndDeactivateAggression()
    {
        if (isAttacking) EndAttackClean(); // Interrupt any ongoing attack

        if (agent != null) agent.speed = originalSpeed; // Reset to normal speed, even if was aggressive
        currentState = EnemyState.FleeingFromOrb;
        fleeEndTime = Time.time + fleeDuration;
        Debug.Log($"Enemy {gameObject.name}: Orb collected. Fleeing and deactivating any aggression. Current state: {currentState}");
    }

    void Update()
    {
        if (player == null || agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
        {
            if (animator) animator.SetBool("Walk", false);
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Handle FleeingFromOrb state first as it's temporary and overrides others (except new aggressive trigger)
        if (currentState == EnemyState.FleeingFromOrb)
        {
            if (Time.time < fleeEndTime)
            {
                if (Time.time >= nextPathUpdate) // Recalculate flee path periodically
                {
                    FleeFromPlayer();
                    nextPathUpdate = Time.time + pathUpdateInterval;
                }
                if (animator) animator.SetBool("Walk", agent.velocity.magnitude > 0.1f && agent.hasPath);
                return; // Prioritize fleeing
            }
            else
            {
                currentState = EnemyState.Idle; // Fleeing finished, transition to Idle
                agent.ResetPath();
                Debug.Log($"Enemy {gameObject.name}: Fleeing finished. Transitioning to Idle.");
            }
        }

        switch (currentState)
        {
            case EnemyState.Idle:
                if (agent.hasPath) agent.ResetPath();
                if (animator) animator.SetBool("Walk", false);
                if (distanceToPlayer <= detectionRadius)
                {
                    currentState = EnemyState.FollowingPlayer;
                    Debug.Log($"Enemy {gameObject.name}: Player detected. Transitioning to FollowingPlayer.");
                }
                break;

            case EnemyState.FollowingPlayer:
                if (distanceToPlayer > detectionRadius)
                {
                    currentState = EnemyState.Idle;
                    if (agent.hasPath) agent.ResetPath();
                    Debug.Log($"Enemy {gameObject.name}: Player lost. Transitioning to Idle.");
                    break;
                }
                agent.SetDestination(player.position);
                agent.isStopped = isAttacking;
                if (!isAttacking && Time.time >= nextAttackTime && distanceToPlayer <= attackRange)
                {
                    StartAttack(normalAttackDamage);
                }
                break;

            case EnemyState.AggressivePursuit:
                agent.SetDestination(player.position);
                agent.isStopped = isAttacking;
                // No detectionRadius check, always pursues
                if (!isAttacking && Time.time >= nextAttackTime && distanceToPlayer <= attackRange)
                {
                    StartAttack(aggressiveModeAttackDamage);
                }
                break;
        }

        if (animator && !isAttacking) // Don't override attack animation with walk
        {
            animator.SetBool("Walk", agent.velocity.magnitude > 0.1f && agent.hasPath);
        }
    }

    void StartAttack(float damageAmount)
    {
        if (isAttacking) return;

        isAttacking = true;
        if (agent != null && agent.isOnNavMesh) agent.isStopped = true;
        
        PerformAttack(damageAmount);
        Invoke(nameof(EndAttack), attackAnimationDuration);
    }

    void EndAttack()
    {
        isAttacking = false;
        if (agent != null && agent.isOnNavMesh) agent.isStopped = false; // Allow movement again
        nextAttackTime = Time.time + attackCooldown;
    }

    void EndAttackClean() // For interruptions
    {
        isAttacking = false;
        CancelInvoke(nameof(EndAttack)); // Stop scheduled EndAttack if interrupted
        if (agent != null && agent.isOnNavMesh) agent.isStopped = false;
    }

    void PerformAttack(float damageAmount)
    {
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageAmount);
            Debug.Log($"Enemy attacking player for {damageAmount} damage. Current state: {currentState}");
        }
        if (animator) animator.SetTrigger("Attack"); // Ensure you have an "Attack" trigger in your Animator
    }

    void FleeFromPlayer()
    {
        if (!agent.isOnNavMesh || player == null) return;

        Vector3 fleeDirection = (transform.position - player.position).normalized;
        // Ensure fleeDirection is not zero (e.g., if enemy is exactly on player position)
        if (fleeDirection == Vector3.zero) fleeDirection = Random.onUnitSphere; 
        fleeDirection.y = 0; // Keep flee direction horizontal if appropriate for your game

        Vector3 targetPosition = transform.position + fleeDirection * (minFleeSafeDistance + (maxEscapeDistance - minFleeSafeDistance) * 0.5f);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, maxEscapeDistance, NavMesh.AllAreas))
        {
            escapePoint = hit.position;
            agent.SetDestination(escapePoint);
            agent.isStopped = false;
        }
        else
        {
            // Fallback: try a random point further away if direct flee path fails
            for (int i = 0; i < 5; i++) // Try a few times
            {
                Vector3 randomDirection = Random.insideUnitSphere * maxEscapeDistance;
                randomDirection.y = 0; // Keep horizontal
                randomDirection += transform.position;
                if (NavMesh.SamplePosition(randomDirection, out hit, maxEscapeDistance, NavMesh.AllAreas))
                {
                    escapePoint = hit.position;
                    agent.SetDestination(escapePoint);
                    agent.isStopped = false;
                    return; // Found a point
                }
            }
            Debug.LogWarning($"Enemy {gameObject.name} could not find a valid flee point after multiple attempts.");
            if (agent.hasPath) agent.ResetPath(); // Stop if no valid flee point found
        }
    }

    // Removed SetVulnerable as its logic is now handled by the state machine and UpdateStateOnOrbCollection
    /*
    public void SetVulnerable(bool vulnerable)
    {
        // ... old code ...
    }
    */
}