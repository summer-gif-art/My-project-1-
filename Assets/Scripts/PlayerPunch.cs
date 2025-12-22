using System.Collections;
using UnityEngine;

public class PlayerPunch : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Health playerHealth;

    [Header("Hitbox")]
    [SerializeField] private BoxCollider2D hitbox;
    [SerializeField] private float hitboxOffsetX = 0.6f;

    [Header("Attack Settings")]
    [SerializeField] private int damage = 50;
    [SerializeField] private float attackDuration = 0.2f;
    [SerializeField] private string punchTriggerName = "Punch";

    private bool _isAttacking;
    private bool _hitThisPunch;

    private void Start()
    {
        if (playerHealth == null)
            playerHealth = GetComponentInParent<Health>();

        if (animator == null)
            animator = GetComponentInParent<Animator>();

        if (hitbox == null)
            hitbox = GetComponent<BoxCollider2D>();

        if (hitbox == null)
        {
            Debug.LogError("PlayerPunch: Missing BoxCollider2D on PunchHitbox.");
            return;
        }

        hitbox.isTrigger = true;
        hitbox.enabled = false;
    }

    private void Update()
    {
        if (playerHealth != null && playerHealth.IsDead)
            return;

        if (Input.GetMouseButtonDown(0) && !_isAttacking)
            StartCoroutine(PunchRoutine());
    }

    private IEnumerator PunchRoutine()
    {
        if (playerHealth != null && playerHealth.IsDead)
            yield break;

        _isAttacking = true;
        _hitThisPunch = false;

// move hitbox to facing side
        float facing = Mathf.Sign(transform.root.localScale.x); // +1 right, -1 left
        Vector2 off = hitbox.offset;
        off.x = facing * Mathf.Abs(hitboxOffsetX);
        hitbox.offset = off;



        if (animator != null)
            animator.SetTrigger(punchTriggerName);

        hitbox.enabled = true;

        // immediate overlap check
        TryHitNow();

        yield return new WaitForSeconds(attackDuration);

        hitbox.enabled = false;
        _isAttacking = false;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!hitbox.enabled || !_isAttacking || _hitThisPunch)
            return;

        TryDamage(other);
    }

    private void TryHitNow()
    {
        if (_hitThisPunch) return;

        Collider2D[] hits = new Collider2D[8];
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;

        int count = hitbox.Overlap(filter, hits);
        for (int i = 0; i < count; i++)
        {
            if (hits[i] != null)
            {
                TryDamage(hits[i]);
                if (_hitThisPunch) return;
            }
        }
    }

    private void TryDamage(Collider2D other)
    {
        if (_hitThisPunch) return;

        Health enemyHealth = other.GetComponentInParent<Health>();
        if (enemyHealth != null && enemyHealth != playerHealth)
        {
            _hitThisPunch = true;
            Debug.Log("Hit enemy! Damage: " + damage);
            enemyHealth.TakeDamage(damage);
        }
    }
}
