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

    [Header("Roaming Settings")]
    [Tooltip("Distance at which the enemy stops following the player and starts roaming.")]
    public float roamingDistanceThreshold = 10f;
    [Tooltip("The range within which to pick a random roam point from the current position.")]
    public float roamPointRange = 20f;
    [Tooltip("How often to pick a new roam destination (seconds) if stuck or idle.")]
    public float roamNewDestinationInterval = 5f;

    [Header("Door Interaction")]
    [Tooltip("Time in seconds the enemy waits at a closed door.")]
    public float doorWaitTime = 10f;

    private bool isVulnerable;
    private bool isRoaming = false;
    private float nextRoamDestinationTime;
    private Vector3 currentRoamTarget;
    private bool isWaitingForDoor = false;
    private Coroutine doorWaitCoroutine;
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

        if (isWaitingForDoor) // If waiting for a door, do nothing else related to movement
        {
            if (agent.velocity.magnitude > 0.1f && agent.hasPath) agent.ResetPath(); // Stop if somehow moving
            if (animator) animator.SetBool("Walk", false);
            return;
        }

        // Check if we can attack
        if (!isRoaming && distanceToPlayer <= attackRange && Time.time >= nextAttackTime && !isAttacking)
        {
            StartAttack();
        }

        if (!isAttacking) // Only move if not attacking
        {
            if (isVulnerable)
            {
                isRoaming = false; // Not roaming when vulnerable and escaping
                // agent.speed is set in SetVulnerable
                if (Time.time >= nextPathUpdate)
                {
                    CalculateEscapePath();
                    nextPathUpdate = Time.time + pathUpdateInterval;
                }
            }
            else // Not vulnerable
            {
                if (distanceToPlayer >= roamingDistanceThreshold)
                {
                    if (!isRoaming) // Transition to roaming
                    {
                        isRoaming = true;
                        nextRoamDestinationTime = Time.time; // Pick a new destination immediately
                    }
                    agent.speed = speed * 0.7f; // Set roaming speed
                    Roam();
                }
                else // Chase player
                {
                    isRoaming = false;
                    agent.speed = speed; // Set default chase speed
                    agent.SetDestination(player.position);
                    CheckForDoors();
                }
            }
        }

        // Update animation
        if (animator)
        {
            animator.SetBool("Walk", agent.velocity.magnitude > 0.1f && !isAttacking && !isWaitingForDoor);
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
        Debug.Log($"Enemy {gameObject.name} EndAttack called at {Time.time}"); // Added for debugging
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
            agent.speed = state ? speed * 5f : speed;
            // Adjust stopping distance based on state
            agent.stoppingDistance = attackRange * (state ? 1.2f : 0.8f);
        }
    }

    void Roam()
    {
        // If we have a path and are moving towards it, and it's not time for a new one yet
        if (agent.hasPath && agent.remainingDistance > agent.stoppingDistance && Time.time < nextRoamDestinationTime)
        {
            CheckForDoors(); // Check for doors while roaming
            return;
        }

        // Time to find a new random destination or current path is invalid/completed
        Vector3 randomDirection = Random.insideUnitSphere * roamPointRange;
        randomDirection += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, roamPointRange, NavMesh.AllAreas))
        {
            currentRoamTarget = hit.position;
            agent.SetDestination(currentRoamTarget);
            // Set next roam time with some variability
            nextRoamDestinationTime = Time.time + roamNewDestinationInterval + Random.Range(-roamNewDestinationInterval * 0.2f, roamNewDestinationInterval * 0.2f);
            Debug.Log($"Enemy {gameObject.name} new roam target: {currentRoamTarget} (next update in {nextRoamDestinationTime - Time.time:F1}s)");
            CheckForDoors(); // Check for doors after setting new roam path
        }
        else
        {
            // If failed to find a point, try again sooner
            nextRoamDestinationTime = Time.time + 1f; // Try again in 1 second
            Debug.LogWarning($"Enemy {gameObject.name} failed to find a roam point near {randomDirection}.");
        }
    }

    void CheckForDoors()
    {
        if (isWaitingForDoor || !agent.hasPath || agent.velocity.sqrMagnitude < 0.01f) return; // Not moving or already waiting

        RaycastHit hitInfo;
        // Raycast slightly in front and in the direction of movement
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f; // Agent's center height
        Vector3 direction = agent.velocity.normalized;
        float rayDistance = agent.stoppingDistance + 1.0f; // Check a bit beyond stopping distance

        // Debug.DrawRay(rayOrigin, direction * rayDistance, Color.yellow, 0.1f);

        if (Physics.Raycast(rayOrigin, direction, out hitInfo, rayDistance))
        {
            if (hitInfo.collider.CompareTag("Door"))
            {
                // A more robust check would be if the agent's path is partially blocked by this door
                // For simplicity, if we are close to a door and nearly stopped, assume it's blocking.
                if (Vector3.Distance(transform.position, hitInfo.point) < agent.stoppingDistance + 0.5f && agent.velocity.magnitude < 0.2f)
                {
                    Debug.Log($"Enemy {gameObject.name} detected door: {hitInfo.collider.name} while moving towards {agent.destination}. Path status: {agent.pathStatus}");
                    StartWaitingForDoor(hitInfo.collider.gameObject);
                }
            }
        }
    }

    void StartWaitingForDoor(GameObject doorObject)
    {
        if (isWaitingForDoor) return; // Already waiting

        isWaitingForDoor = true;
        agent.isStopped = true; 
        // agent.ResetPath(); // Clearing path might not be desired if we want to resume it
        Debug.Log($"Enemy {gameObject.name} is waiting for door {doorObject.name} to open for {doorWaitTime} seconds.");
        if (animator) animator.SetBool("Walk", false); // Stop walk animation

        if (doorWaitCoroutine != null)
        {
            StopCoroutine(doorWaitCoroutine);
        }
        doorWaitCoroutine = StartCoroutine(WaitForDoorSequence(doorObject));
    }

    System.Collections.IEnumerator WaitForDoorSequence(GameObject doorObject)
    {
        yield return new WaitForSeconds(doorWaitTime);

        Debug.Log($"Enemy {gameObject.name} finished waiting for door {doorObject.name}. Assuming it's open now.");
        isWaitingForDoor = false;
        agent.isStopped = false; // Allow agent to move again
        // The enemy will re-evaluate its state (roam/chase) in the next Update.
        // If it was chasing, it will try to set destination to player again.
        // If it was roaming, Roam() will be called and might pick a new point or continue.
        // To prevent immediate re-triggering if the door didn't "actually" open,
        // a more complex system (e.g. ignoring this door for a bit) would be needed.
        doorWaitCoroutine = null;
        nextRoamDestinationTime = Time.time; // Force re-evaluation of roam path if roaming
    }

    // Optional: Visualize attack range in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}