using System.Collections;
using UnityEngine;

public class PlayerPunch : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;        
    [SerializeField] private Collider2D punchHitbox;   // Trigger collider for punch
    [SerializeField] private Health playerHealth;

    [Header("Attack Settings")]
    [SerializeField] private int damage = 50;          
    [SerializeField] private float attackDuration = 0.2f; 
    [SerializeField] private string punchTriggerName = "Punch";

    private bool _isAttacking;

    private void Start()
    {
        // Get the Health component from the player (parent) if not assigned in the Inspector
        if (playerHealth == null)
            playerHealth = GetComponentInParent<Health>();

        if (punchHitbox == null)
            punchHitbox = GetComponent<Collider2D>();
        
        // Hitbox must start disabled
        if (punchHitbox != null)
            punchHitbox.enabled = false;
    }

    private void Update()
    {
        // Attack input is disabled upon player death.
        
        if (playerHealth != null && playerHealth.IsDead)
            return;
        
        if (Input.GetMouseButtonDown(0) && !_isAttacking)
        {
            StartCoroutine(PunchRoutine());
        }
    }

    private IEnumerator PunchRoutine()
    {
        if (playerHealth != null && playerHealth.IsDead)
            yield break;

        _isAttacking = true;

        // Play animation
        if (animator != null)
            animator.SetTrigger(punchTriggerName);

        // Enable hitbox
        punchHitbox.enabled = true;

        // Active hitbox for duration
        yield return new WaitForSeconds(attackDuration);

        // Disable hitbox
        punchHitbox.enabled = false;

        _isAttacking = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only damage when punch is active
        if (punchHitbox == null || !punchHitbox.enabled)
            return;

        // Try to get Health on the object OR its parent
        Health enemyHealth = other.GetComponentInParent<Health>();

        if (enemyHealth != null)
        {
            Debug.Log("Hit enemy! Damage: " + damage);
            enemyHealth.TakeDamage(damage);
        }
    }
}