using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;             
    [SerializeField] private Animator animator;
    [SerializeField] private Health health;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private EnemyAttackRange attackRange;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackDelay = 0.3f;
    [SerializeField] private int normalDamage = 30;
    [SerializeField] private int strongDamage = 60;

    [Header("Hit Distance (X axis only)")]
    [SerializeField] private float hitDistance = 1.6f;

    [Header("Enemy Type")]
    [SerializeField] private bool isStrongEnemy;
    [SerializeField] private Color strongTint = Color.green;

    [Header("Chase On X Only (recommended for beat'em up)")]
    [SerializeField] private bool chaseOnXOnly = true;

    private Rigidbody2D _rb;
    private Coroutine _attackRoutine;

    private bool _isDead;
    private bool _isHurt;

    private static readonly int PunchTriggerHash = Animator.StringToHash("Punch");
    private static readonly int IsWalkingBoolHash = Animator.StringToHash("IsWalking");
    private static readonly int DieTriggerHash = Animator.StringToHash("Die");
    private static readonly int HurtTriggerHash = Animator.StringToHash("Hurt");

    private void Awake()
    {
        if (health == null) health = GetComponent<Health>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag(Tags.Player);
            if (p != null) player = p.transform;
        }
    }

    private void Start()
    {
        // Random enemy type
        isStrongEnemy = Random.value > 0.5f;
        if (isStrongEnemy && spriteRenderer != null)
            spriteRenderer.color = strongTint;

        // Subscribe to health events
        if (health != null)
        {
            health.OnDeath += OnDeath;
            health.OnDamaged += OnDamaged;
        }

        if (_rb != null)
            _rb.freezeRotation = true;

        if (attackRange == null)
            Debug.LogWarning("EnemyController: Missing EnemyAttackRange reference! Assign it in the Inspector.");

        // OPTIONAL: ground snap (only works if you have a Ground layer)
        // If you don't have a "Ground" layer, comment this block out.
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 20f, LayerMask.GetMask("Ground"));
        if (hit.collider != null)
        {
            Vector3 p = transform.position;
            p.y = hit.point.y;
            transform.position = p;
        }
    }

    private void FixedUpdate()
    {
        if (_isDead || _isHurt || player == null || _rb == null)
            return;

        bool inRange = attackRange != null && attackRange.PlayerInRange;
        float dx = Mathf.Abs(_rb.position.x - player.position.x);

        // Debug (optional)
        Debug.Log($"InRange={inRange} dx={dx:F3} hitDist={hitDistance}");

        // Not in trigger range -> chase
        if (!inRange)
        {
            MoveTowardsPlayer_Physics();
            StopAttackRoutineIfRunning();
            return;
        }

        // In trigger range but not close enough -> keep moving closer
        if (!IsCloseEnoughToHit())
        {
            MoveTowardsPlayer_Physics();
            StopAttackRoutineIfRunning();
            return;
        }

        // Close enough -> stop and attack
        if (animator != null)
            animator.SetBool(IsWalkingBoolHash, false);

        // Stop movement so the enemy doesn't keep sliding while attacking
        if (_rb != null)
            _rb.linearVelocity = Vector2.zero;

        if (_attackRoutine == null)
            _attackRoutine = StartCoroutine(AttackRoutine());
    }

    private void MoveTowardsPlayer_Physics()
    {
        Vector2 pos = _rb.position;
        Vector2 target = player.position;

        if (chaseOnXOnly)
            target.y = pos.y;

        Vector2 dir = target - pos;

        if (dir.sqrMagnitude < 0.0001f)
        {
            if (animator != null) animator.SetBool(IsWalkingBoolHash, false);
            return;
        }

        dir.Normalize();

        Vector2 newPos = pos + dir * (moveSpeed * Time.fixedDeltaTime);
        _rb.MovePosition(newPos);

        if (animator != null)
            animator.SetBool(IsWalkingBoolHash, true);

        if (spriteRenderer != null)
        {
            if (dir.x < 0) spriteRenderer.flipX = false;
            else if (dir.x > 0) spriteRenderer.flipX = true;
            Debug.Log($"dir.x={dir.x:F2} flipX={spriteRenderer.flipX} SR={spriteRenderer.name}");

        }
    }

    private bool IsCloseEnoughToHit()
    {
        if (_rb == null || player == null) return false;
        float dx = Mathf.Abs(_rb.position.x - player.position.x);
        return dx <= hitDistance;
    }

    private IEnumerator AttackRoutine()
    {
        Debug.Log("AttackRoutine START");

        if (_isDead || _isHurt)
        {
            _attackRoutine = null;
            yield break;
        }

        if (animator != null)
        {
            animator.SetBool(IsWalkingBoolHash, false);
            animator.SetTrigger(PunchTriggerHash);
        }

        yield return new WaitForSeconds(attackDelay);

        bool canDamage =
            !_isDead && !_isHurt &&
            attackRange != null && attackRange.PlayerInRange &&
            IsCloseEnoughToHit();

        Debug.Log($"Trying to damage. InRange={attackRange != null && attackRange.PlayerInRange}, Close={IsCloseEnoughToHit()}, PlayerHealthNull={(attackRange == null || attackRange.PlayerHealth == null)}");

        if (canDamage)
        {
            Health playerHealth = attackRange.PlayerHealth;

            if (playerHealth != null && !playerHealth.IsDead)
            {
                int dmg = isStrongEnemy ? strongDamage : normalDamage;
                playerHealth.TakeDamage(dmg);
                Debug.Log($"DAMAGE APPLIED: {dmg}");
            }
        }

        yield return new WaitForSeconds(attackCooldown);
        _attackRoutine = null;
    }

    private void StopAttackRoutineIfRunning()
    {
        if (_attackRoutine != null)
        {
            StopCoroutine(_attackRoutine);
            _attackRoutine = null;
        }
    }

    private void OnDamaged()
    {
        if (_isDead) return;

        _isHurt = true;
        StopAttackRoutineIfRunning();

        if (animator != null)
        {
            animator.ResetTrigger(PunchTriggerHash);
            animator.SetBool(IsWalkingBoolHash, false);
            animator.SetTrigger(HurtTriggerHash);
        }

        StartCoroutine(HurtRoutine());
    }

    private IEnumerator HurtRoutine()
    {
        yield return new WaitForSeconds(0.3f);
        _isHurt = false;
    }

    private void OnDeath()
    {
        if (_isDead) return;   // חשוב! למנוע קריאה כפולה
        _isDead = true;

        StopAttackRoutineIfRunning();

        if (animator != null)
        {
            animator.SetBool(IsWalkingBoolHash, false);
            animator.SetTrigger(DieTriggerHash);
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (_rb != null)
        {
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
            _rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        StartCoroutine(BlinkAndDisappear());
    }

    
    private IEnumerator BlinkAndDisappear()
    {
        if (spriteRenderer == null)
        {
            Debug.LogError("Blink: spriteRenderer is NULL");
            yield break;
        }

        Debug.Log("Blink START");

        // תן רגע לראות את ה-Die לפני ההבהוב
        float preDelay = 0.2f;
        float end = Time.realtimeSinceStartup + 1f;  // 1 שנייה הבהוב
        float interval = 0.1f;
        float nextToggle = Time.realtimeSinceStartup + preDelay;

        spriteRenderer.enabled = true;

        while (Time.realtimeSinceStartup < end)
        {
            if (Time.realtimeSinceStartup >= nextToggle)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
                nextToggle += interval;
            }
            yield return null; // מחכה פריים (לא תלוי timescale)
        }

        spriteRenderer.enabled = true;
        Debug.Log("Blink END -> Destroy");
        Destroy(gameObject);
    }
    
    
}
