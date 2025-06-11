using UnityEngine;

public class PlayerOrcController : MonoBehaviour
{
    private Animator animator;
    public float currentHealth = 100f; // Player's current health
    public float maxHealth = 100f;     // Player's maximum health
    private bool isDead = false;       // Flag to check if player is dead
    public SwordAttack swordAttack;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("PlayerOrcController: Animator component not found on this GameObject.");
            enabled = false; // Disable script if no animator
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (animator == null) return;

        // If player is dead, do nothing else
        if (isDead)
        {
            // You might want to ensure other animation states are forced off here
            // depending on your Animator setup, e.g.,
            // animator.SetBool("IsWalking_Bool", false);
            return;
        }

        // --- DEATH CHECK ---
        // This is a simple health check. In a real game, health would be managed by other systems.
        // To test, you can temporarily set currentHealth to 0 in the Inspector or via another script.
        if (currentHealth <= 0)
        {
            isDead = true;
            animator.SetTrigger("Death_Trigger"); // Ensure "Death_Trigger" matches your Animator parameter (should trigger 'gethit')
            Debug.Log("Health depleted - Triggering Death_Trigger");
            // Optional: Disable player controls or other components here
            // enabled = false; // Disables this script if you want to stop all updates
            return; // Stop further input processing this frame
        }

        // --- MOVEMENT ---
        // Use CharacterController's velocity to determine if the player is moving
        CharacterController cc = GetComponent<CharacterController>();
        bool isMoving = false;
        bool isGrounded = false;
        if (cc != null)
        {
            Vector3 horizontalVelocity = cc.velocity;
            horizontalVelocity.y = 0f;
            isMoving = horizontalVelocity.magnitude > 0.1f;
            isGrounded = cc.isGrounded;
        }
        else
        {
            // fallback: WASD input (for non-CharacterController setups)
            isMoving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
            // fallback: always grounded
            isGrounded = true;
        }
        animator.SetBool("IsWalking_Bool", isMoving);
        animator.SetBool("IsGrounded", isGrounded);

        // --- JUMP ---
        // Only trigger jump animation when leaving the ground
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            animator.SetTrigger("Jump_Trigger");
            Debug.Log("Spacebar pressed - Triggering Jump_Trigger");
        }

        // --- ATTACK ---
        // Only allow attack in hunter mode
        if (Input.GetMouseButtonDown(0)) // 0 corresponds to the left mouse button
        {
            if (GameManager.instance != null && GameManager.instance.isTransformed)
            {
                animator.SetTrigger("Attack1_Trigger");
                if (swordAttack != null)
                    swordAttack.TriggerAttack();
                Debug.Log("Left mouse button pressed - Triggering Attack1_Trigger (Hunter Mode)");
            }
            else
            {
                Debug.Log("Attack ignored: Not in Hunter Mode");
            }
        }

        // --- TRIGGERS (ONE-SHOT ACTIONS) --- (Example: Ability1 - Temporarily commented out)
        /*
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            animator.SetTrigger("Ability1_Trigger"); // Ensure "Ability1_Trigger" matches your Animator parameter
            Debug.Log("Keypad 2 pressed - Triggering Ability1_Trigger");
        }
        */

        // --- PARAMETERS FOR SUSTAINED STATES OR MODES (INT, FLOAT) --- (Temporarily commented out)
        /*
        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            animator.SetInteger("ActionID_Int", 1);
            Debug.Log("Keypad 3 pressed - Setting ActionID_Int to 1");
        }

        if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            animator.SetFloat("Speed_Float", 0.5f);
            Debug.Log("Keypad 4 pressed - Setting Speed_Float to 0.5f");
        }
        */

        // General Note: If animations are not behaving as expected (e.g., getting stuck,
        // not transitioning back to Idle, or attack/jump not playing correctly),
        // the MOST LIKELY place to investigate is your Animator Controller
        // ("OrcDestroyer2 Animator Controller" or similar).
        // Key things to check in the Animator Controller:
        // 1. Parameter Names: Ensure they EXACTLY match the strings used in this script
        //    (e.g., "IsWalking_Bool", "Jump_Trigger", "Attack1_Trigger", "Death_Trigger").
        //    Case sensitivity matters!
        // 2. Transitions:
        //    - From Idle to Walk: Conditioned on "IsWalking_Bool" == true.
        //    - From Walk to Idle: Conditioned on "IsWalking_Bool" == false.
        //    - From Any State (or Idle/Walk) to Jump: Conditioned on "Jump_Trigger".
        //      - The Jump state should then transition back (e.g., to Idle or Walk) usually after it finishes (use "Has Exit Time" on the Jump state).
        //    - From Any State (or Idle/Walk) to Attack1: Conditioned on "Attack1_Trigger".
        //      - The Attack1 state should also transition back after finishing.
        //    - From Any State to Death (sequence starting with 'gethit'): Conditioned on "Death_Trigger".
        //      - The Death state is often a final state with no automatic exit.
        // 3. "Has Exit Time" on Animation States:
        //    - For states like Attack or Jump that should play once and then return:
        //      CHECK "Has Exit Time" on the animation state itself.
        //      The transition LEAVING this state (e.g., Jump -> Idle) should typically have NO conditions and rely on the exit time.
        //    - For states like Idle or Walk:
        //      UNCHECK "Has Exit Time" on the animation state itself. Transitions are driven by parameter changes (e.g., IsWalking_Bool).
        // 4. Layers: For Idle, Walk, Jump, Attack, Death, a single Base Layer is usually sufficient.
        //    Separate layers are for more advanced scenarios (e.g., upper body attack while lower body runs).
        //    Focus on getting the Base Layer correct first.
        // 5. Default State: Ensure your "Idle" animation is set as the Default State in the Animator Controller layer.
    }

    // Play the get hit animation from other scripts (e.g., when taking damage)
    public void PlayGetHitAnimation()
    {
        if (animator != null && !isDead)
        {
            animator.SetTrigger("GetHit_Trigger");
            Debug.Log("GetHit_Trigger activated");
        }
    }

    // --- GET HIT (EXAMPLE USAGE) ---
    // To play the get hit animation, call PlayGetHitAnimation() from another script when the player takes damage.
    // Example:
    //   GetComponent<PlayerOrcController>().PlayGetHitAnimation();
}