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
    private bool canAttack;
    private float distanceToPlayer;
    private bool isStunned = false;
    private float stunEndTime = 0f;

    [Header("Audio Settings")]
    public AudioClip monsterGrowlSound;
    public float minGrowlInterval = 5f;
    public float maxGrowlInterval = 10f;
    private AudioSource audioSource;
    private float nextGrowlTime;

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
                Debug.Log($"Found player: {playerObj.name}, Health component: {(playerHealth != null ? "Found" : "NOT FOUND")}");
            }
            else
            {
                Debug.LogError("Player object not found with 'Player' tag!");
            }
        }
        else
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            Debug.Log($"Using assigned player: {player.name}, Health component: {(playerHealth != null ? "Found" : "NOT FOUND")}");
        }

        if (agent != null)
        {
            originalSpeed = speed;
            agent.speed = originalSpeed;
            agent.stoppingDistance = attackRange * 0.8f;
            Debug.Log($"Enemy initialized with speed: {speed}, attack range: {attackRange}");
        }
        else
        {
            Debug.LogError("NavMeshAgent component missing!");
        }

        if (playerHealth == null)
        {
            Debug.LogError($"PlayerHealth component not found for enemy {gameObject.name}!");
        }
        else
        {
            currentState = EnemyState.FollowingPlayer;
            Debug.Log($"Enemy {gameObject.name} initialized. Current state: {currentState}, Attack damage: {normalAttackDamage}");
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        SetNextGrowlTime();
    }

    // Called by GameManager when enough objectives are collected
    public void ActivateObjectiveBasedAggression()
    {
        Debug.Log($"ActivateObjectiveBasedAggression called - Current state: {currentState}");
        if (isAttacking) EndAttackClean(); // Interrupt any ongoing attack

        currentState = EnemyState.AggressivePursuit;
        if (agent != null) agent.speed = originalSpeed * aggressiveModeSpeedMultiplier;
        fleeEndTime = 0; // Ensure not fleeing if switching from fleeing state

        // Set player invincible when entering hunter mode
        if (playerHealth != null)
        {
            Debug.Log("Setting player invincible in hunter mode");
            playerHealth.SetInvincible(true);
            Debug.Log($"Player invincibility set in hunter mode - IsInvincible: {playerHealth.IsInvincible}");
        }
        else
        {
            Debug.LogError("PlayerHealth component not found when trying to set invincibility!");
        }

        Debug.Log($"Enemy {gameObject.name}: Aggressive Pursuit activated due to objectives. Current state: {currentState}");
    }

    // Called by OrbManager when an orb is collected
    public void FleeFromOrbAndDeactivateAggression()
    {
        if (isAttacking) EndAttackClean(); // Interrupt any ongoing attack

        if (agent != null) agent.speed = originalSpeed; // Reset to normal speed, even if was aggressive
        currentState = EnemyState.FleeingFromOrb;
        fleeEndTime = Time.time + fleeDuration;

        // Remove player invincibility when leaving hunter mode
        if (playerHealth != null)
        {
            playerHealth.SetInvincible(false);
            Debug.Log("Player invincibility removed after leaving hunter mode");
        }

        Debug.Log($"Enemy {gameObject.name}: Orb collected. Fleeing and deactivating any aggression. Current state: {currentState}");
    }

    void Update()
    {
        if (isStunned)
        {
            agent.isStopped = true;
            if (Time.time >= stunEndTime)
            {
                isStunned = false;
                agent.isStopped = false;
            }
            return;
        }

        if (player == null)
        {
            Debug.LogWarning("Player reference is null in EnemyAI Update");
            return;
        }

        // Calculate distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        // Debug.Log($"Distance to player: {distanceToPlayer:F2}, Attack range: {attackRange}, Can attack: {canAttack}");

        // Check if player is in attack range
        bool inAttackRange = distanceToPlayer <= attackRange;

        // --- ANIMATOR: MOVEMENT ---
        if (animator != null)
        {
            bool walking = currentState == EnemyState.FollowingPlayer && !isAttacking && agent.velocity.magnitude > 0.1f && agent.speed == originalSpeed;
            bool running = currentState == EnemyState.AggressivePursuit && !isAttacking && agent.velocity.magnitude > 0.1f && agent.speed > originalSpeed;
            animator.SetBool("IsWalking", walking);
            animator.SetBool("IsRunning", running);
        }

        // Handle movement and attack
        HandleGrowlSound();

        if (!isAttacking)
        {
            if (inAttackRange)
            {
                // Stop and face the player
                agent.isStopped = true;
                transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z));

                // Start attack if cooldown is ready
                if (canAttack)
                {
                    Debug.Log($"ATTACK TRIGGERED - Distance: {distanceToPlayer:F2}, Range: {attackRange}");
                    StartAttack(normalAttackDamage);
                }
            }
            else
            {
                // Move towards player
                agent.isStopped = false;
                agent.SetDestination(player.transform.position);
            }
        }

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
                /* Commenting out animator code
                if (animator) animator.SetBool("Walk", agent.velocity.magnitude > 0.1f && agent.hasPath);
                */
                return; // Prioritize fleeing
            }
            else
            {
                currentState = EnemyState.Idle; // Fleeing finished, transition to Idle
                agent.ResetPath();
                Debug.Log($"Enemy {gameObject.name}: Fleeing finished. Transitioning to Idle.");
            }
        }

        // Force attack if very close to player
        if (distanceToPlayer <= attackRange && !isAttacking && Time.time >= nextAttackTime)
        {
            Debug.Log($"ATTACK TRIGGERED - Distance: {distanceToPlayer:F2}, Range: {attackRange}");
            StartAttack(normalAttackDamage);
        }

        switch (currentState)
        {
            case EnemyState.Idle:
                if (agent.hasPath) agent.ResetPath();
                /* Commenting out animator code
                if (animator) animator.SetBool("Walk", false);
                */
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
    }

    void StartAttack(float damageAmount)
    {
        if (isAttacking)
        {
            Debug.Log("Already attacking, skipping attack");
            return;
        }

        Debug.Log($"Starting attack sequence - Damage: {damageAmount}, Distance: {Vector3.Distance(transform.position, player.position):F2}");
        isAttacking = true;
        if (agent != null && agent.isOnNavMesh) agent.isStopped = true;

        // --- ANIMATOR: RANDOM ATTACK ---
        if (animator != null)
        {
            if (Random.value < 0.5f)
                animator.SetTrigger("Attack1_Trigger");
            else
                animator.SetTrigger("Attack2_Trigger");
        }

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
        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealth is null in PerformAttack!");
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        Debug.Log($"Performing attack - Distance: {distanceToPlayer:F2}, Range: {attackRange}, Damage: {damageAmount}, Current state: {currentState}");

        // Remove this block so enemy can be damaged in hunter mode
        //if (currentState == EnemyState.AggressivePursuit)
        //{
        //    Debug.Log($"Attack blocked - Player in hunter mode (State: {currentState})");
        //    return;
        //}

        // Double check player invincibility
        if (playerHealth.IsInvincible)
        {
            Debug.Log($"Attack blocked - Player is invincible (IsInvincible: {playerHealth.IsInvincible})");
            return;
        }

        if (distanceToPlayer <= attackRange * 1.5f)
        {
            Debug.Log($"Dealing damage to player - Current health before: {playerHealth.currentHealth}, IsInvincible: {playerHealth.IsInvincible}");
            playerHealth.TakeDamage(damageAmount);
            Debug.Log($"Damage dealt - New health: {playerHealth.currentHealth}");
        }
        else
        {
            Debug.Log($"Player too far - Distance: {distanceToPlayer:F2}, Required: {attackRange}");
        }

        /* Commenting out animator code
        if (animator) animator.SetTrigger("Attack");
        */
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

    // --- ANIMATOR: GET HIT ---
    public void PlayGetHitAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("GetHit_Trigger");
            // Also trigger attack animation when taking damage
            if (Random.value < 0.5f)
                animator.SetTrigger("Attack1_Trigger");
            else
                animator.SetTrigger("Attack2_Trigger");
        }
    }

    // --- ANIMATOR: DEATH ---
    public void PlayDeathAnimation()
    {
        if (animator != null)
            animator.SetTrigger("Death_Trigger");
    }

    public void Stun(float duration)
    {
        isStunned = true;
        stunEndTime = Time.time + duration;
        agent.isStopped = true;
        canAttack = false;
        PlayGetHitAnimation();
        Debug.Log($"Enemy {gameObject.name} stunned for {duration} seconds.");
    }

    // Removed SetVulnerable as its logic is now handled by the state machine and UpdateStateOnOrbCollection
    /*
    public void SetVulnerable(bool vulnerable)
    {
        // ... old code ...
    }
    */

    void SetNextGrowlTime()
    {
        nextGrowlTime = Time.time + Random.Range(minGrowlInterval, maxGrowlInterval);
    }

    void HandleGrowlSound()
    {
        if (monsterGrowlSound != null && audioSource != null && Time.time >= nextGrowlTime)
        {
            if (currentState == EnemyState.FollowingPlayer || currentState == EnemyState.AggressivePursuit)
            {
                if (!isAttacking && agent.velocity.magnitude > 0.1f) // Only growl if moving/pursuing and not attacking
                {
                    audioSource.PlayOneShot(monsterGrowlSound);
                    SetNextGrowlTime();
                }
                else if (!isAttacking && agent.velocity.magnitude <= 0.1f && currentState == EnemyState.FollowingPlayer) // Idle growl when following but not moving (e.g. player is close but enemy is stuck or waiting)
                {
                    // Potentially a different, more subtle growl or lower chance here
                    if(Random.value < 0.3f) // 30% chance to growl if idle but aware
                    {
                        audioSource.PlayOneShot(monsterGrowlSound);
                        SetNextGrowlTime();
                    }
                }
            }
        }
    }
}