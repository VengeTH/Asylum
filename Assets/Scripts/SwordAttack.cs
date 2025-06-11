using UnityEngine;
using System.Collections;

public class SwordAttack : MonoBehaviour
{
    private Collider swordCollider;
    public float attackColliderDuration = 0.3f; // Duration collider stays enabled during attack
    private Coroutine attackCoroutine;

    void Start()
    {
        swordCollider = GetComponent<Collider>();
        if (swordCollider == null)
        {
            Debug.LogError("SwordAttack: No Collider found on sword GameObject!");
            enabled = false;
            return;
        }
        swordCollider.enabled = false; // Start disabled
    }

    // Call this method from your attack logic (e.g., when attack animation starts)
    public void TriggerAttack()
    {
        Debug.Log("TriggerAttack called, enabling sword collider.");
        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(EnableColliderForDuration());
    }

    private IEnumerator EnableColliderForDuration()
    {
        swordCollider.enabled = true;
        yield return new WaitForSeconds(attackColliderDuration);
        swordCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Sword collider triggered with: " + other.name);
        EnemyAI enemy = other.GetComponentInParent<EnemyAI>();
        if (enemy == null)
        {
            Debug.Log("No EnemyAI found in parent of: " + other.name + ". Parent: " + (other.transform.parent != null ? other.transform.parent.name : "null"));
        }
        if (enemy != null && GameManager.instance != null && GameManager.instance.isTransformed)
        {
            Debug.Log("Enemy hit and stunned: " + other.name);
            enemy.Stun(10f);
        }
    }
}
// Usage: In your attack logic (e.g., PlayerOrcController), call swordAttack.TriggerAttack() when the attack is performed.