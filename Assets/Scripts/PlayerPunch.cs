using System.Collections;
using UnityEngine;

public class PlayerPunch : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Health playerHealth;

    [Header("Hitbox")]
    [SerializeField] private BoxCollider2D hitbox;
    [SerializeField] private float hitboxOffsetX = 0.3f; // keep your good distance

    [Header("Attack Settings")]
    [SerializeField] private int damage = 50;
    [SerializeField] private float attackDuration = 0.2f;

    private bool _isAttacking;
    private bool _hitThisPunch;

    // Reused buffer (no allocations per punch)
    private readonly Collider2D[] _hits = new Collider2D[8];
    private ContactFilter2D _filter;

    private static readonly int PunchHash = Animator.StringToHash("Punch");

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

        // Only detect solid colliders (enemy body), ignore triggers
        _filter = new ContactFilter2D { useTriggers = false };
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

        // IMPORTANT: PlayerPunch is on PunchHitbox (child),
        // so get facing from PLAYER (root), not from this transform.
        float facing = Mathf.Sign(transform.root.localScale.x); // +1 right, -1 left

        // Flip hitbox ONLY by offset sign (same distance/size as before)
        Vector2 off = hitbox.offset;
        off.x = facing * Mathf.Abs(hitboxOffsetX);
        hitbox.offset = off;

        if (animator != null)
            animator.SetTrigger(PunchHash);

        hitbox.enabled = true;

        // One overlap check during the punch window
        TryHitNow();

        yield return new WaitForSeconds(attackDuration);

        hitbox.enabled = false;
        _isAttacking = false;
    }

    private void TryHitNow()
    {
        if (_hitThisPunch) return;

        int count = hitbox.Overlap(_filter, _hits);
        for (int i = 0; i < count; i++)
        {
            Collider2D c = _hits[i];
            if (c == null) continue;

            TryDamage(c);
            if (_hitThisPunch) return;
        }
    }

    private void TryDamage(Collider2D other)
    {
        if (_hitThisPunch) return;
        if (other == null) return;
        if (other.isTrigger) return;

        Health enemyHealth = other.GetComponentInParent<Health>();
        if (enemyHealth != null && enemyHealth != playerHealth)
        {
            _hitThisPunch = true;
            enemyHealth.TakeDamage(damage);
        }
    }
}
