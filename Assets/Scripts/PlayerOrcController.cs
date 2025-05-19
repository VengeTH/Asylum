using UnityEngine;

public class PlayerOrcController : MonoBehaviour
{
    private Animator animator;
    public float currentHealth = 100f; // Player's current health
    public float maxHealth = 100f;     // Player's maximum health
    private bool isDead = false;       // Flag to check if player is dead

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
            animator.SetTrigger("Death_Tri"); // Ensure "Death_Tri" matches your Animator parameter (should trigger 'gethit')
            Debug.Log("Health depleted - Triggering Death_Tri");
            // Optional: Disable player controls or other components here
            // enabled = false; // Disables this script if you want to stop all updates
            return; // Stop further input processing this frame
        }

        // --- MOVEMENT ---
        // WASD input controls walking.
        // Stopping (no WASD input) should lead back to Idle via Animator transitions.
        // Ensure "IsWalking" is a parameter in your Animator Controller.
        // Transition from Idle to Walk: IsWalking == true
        // Transition from Walk to Idle: IsWalking == false
        bool isMoving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
        animator.SetBool("IsWalking", isMoving);

        // --- JUMP ---
        // Spacebar triggers jump.
        // Ensure "Jump_Trig" is a Trigger parameter in your Animator Controller.
        // Transition from Any State (or Idle/Walk) to Jump: Condition on Jump_Trig.
        // The Jump animation state should ideally have "Has Exit Time" checked to automatically
        // transition back to Idle/Walk after the animation completes.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Consider adding a check here if the player is grounded before allowing a jump.
            animator.SetTrigger("Jump_Trig");
            Debug.Log("Spacebar pressed - Triggering Jump_Trig");
        }

        // --- ATTACK ---
        // Right mouse click triggers attack.
        // Ensure "Attack1_T" is a Trigger parameter in your Animator Controller.
        // Transition from Any State (or Idle/Walk) to Attack: Condition on Attack1_T.
        // The Attack animation state should also ideally have "Has Exit Time" checked.
        if (Input.GetMouseButtonDown(1)) // 1 corresponds to the right mouse button
        {
            // Consider adding checks here, e.g., if not already attacking, or if grounded.
            animator.SetTrigger("Attack1_T");
            Debug.Log("Right mouse button pressed - Triggering Attack1_T");
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
        //    (e.g., "IsWalking", "Jump_Trig", "Attack1_T", "Death_Tri").
        //    Case sensitivity matters!
        // 2. Transitions:
        //    - From Idle to Walk: Conditioned on "IsWalking" == true.
        //    - From Walk to Idle: Conditioned on "IsWalking" == false.
        //    - From Any State (or Idle/Walk) to Jump: Conditioned on "Jump_Trig".
        //      - The Jump state should then transition back (e.g., to Idle or Walk) usually after it finishes (use "Has Exit Time" on the Jump state).
        //    - From Any State (or Idle/Walk) to Attack1: Conditioned on "Attack1_T".
        //      - The Attack1 state should also transition back after finishing.
        //    - From Any State to Death (sequence starting with 'gethit'): Conditioned on "Death_Tri".
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
}