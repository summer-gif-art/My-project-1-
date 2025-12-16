using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;          
    [SerializeField] private Animator animator;         
    [SerializeField] private Health health;             
    [SerializeField] private SpriteRenderer spriteRenderer; 

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;      
    [SerializeField] private float stopDistance = 1.5f; 

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 2f; 
    [SerializeField] private float attackDelay = 0.3f;  
    [SerializeField] private int normalDamage = 30;     
    [SerializeField] private int strongDamage = 60;     

    [Header("Enemy Type")]
    [SerializeField] private bool isStrongEnemy;        
    [SerializeField] private Color strongTint = Color.green; 

    private Coroutine _attackRoutine;
    private bool _isDead;
    private bool _isHurt;   //  enemy cannot attack/move while hurt

    // Animator parameter hashes 
    private static readonly int PunchTriggerHash   = Animator.StringToHash("Punch");
    private static readonly int IsWalkingBoolHash  = Animator.StringToHash("IsWalking");
    private static readonly int DieTriggerHash     = Animator.StringToHash("Die");
    private static readonly int HurtTriggerHash    = Animator.StringToHash("Hurt"); // hurt animation trigger

    private void Awake()
    {
        // Auto-assign components if not set in Inspector
        if (health == null)
            health = GetComponent<Health>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // Randomly determine if this enemy is strong
        isStrongEnemy = Random.value > 0.5f;

        // Tint strong enemies visually
        if (isStrongEnemy && spriteRenderer != null)
            spriteRenderer.color = strongTint;

        // Subscribe to health events
        if (health != null)
        {
            health.OnDeath   += OnDeath;
            health.OnDamaged += OnDamaged;   // called whenever the enemy takes damage
        }
    }

    private void Update()
    {
        // Enemy cannot move/attack while dead or hurt
        if (_isDead || _isHurt || player == null)
            return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > stopDistance)
        {
            // Player is far → chase
            MoveTowardsPlayer();

            // If player moved away, stop any running attack coroutine
            if (_attackRoutine != null)
            {
                StopCoroutine(_attackRoutine);
                _attackRoutine = null;
            }
        }
        else
        {
            // Player is in attack range
            if (animator != null)
                animator.SetBool(IsWalkingBoolHash, false);

            // Start a NEW attack cycle only if not already attacking
            if (_attackRoutine == null)
            {
                _attackRoutine = StartCoroutine(AttackRoutine());
            }
        }
    }

    private void MoveTowardsPlayer()
    {
        if (player == null) return;

        // Move direction
        Vector3 direction = (player.position - transform.position).normalized;
        float step = moveSpeed * Time.deltaTime;
        transform.position += direction * step;

        // Play walking animation
        if (animator != null)
            animator.SetBool(IsWalkingBoolHash, true);

        // Flip sprite depending on movement direction
        if (spriteRenderer != null)
        {
            if (direction.x < 0)
                spriteRenderer.flipX = true;
            else if (direction.x > 0)
                spriteRenderer.flipX = false;
        }
    }

    // A single attack cycle (not a loop), allowing interruption when hurt
    private IEnumerator AttackRoutine()   // NEW – replaces AttackLoop
    {
        if (_isDead || player == null)
        {
            _attackRoutine = null;
            yield break;
        }

        // Play attack animation
        if (animator != null)
        {
            animator.SetTrigger(PunchTriggerHash);
            animator.SetBool(IsWalkingBoolHash, false);
        }

        // Wait until the actual hit moment in the punch animation
        yield return new WaitForSeconds(attackDelay);

        // Deal damage only if still valid (not hurt/dead)
        if (!_isDead && !_isHurt && player != null)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null && !playerHealth.IsDead)
            {
                float distance = Vector2.Distance(transform.position, player.position);
                if (distance <= stopDistance + 0.1f)
                {
                    int damage = isStrongEnemy ? strongDamage : normalDamage;
                    playerHealth.TakeDamage(damage);
                }
            }
        }

        // Cooldown before another attack is allowed
        yield return new WaitForSeconds(attackCooldown);

        // Allow starting a new attack
        _attackRoutine = null;
    }

    // Called every time the enemy takes damage
    private void OnDamaged()   // NEW
    {
        if (_isDead) return;

        _isHurt = true;   // Enemy becomes "stunned" briefly

        // Cancel any running attack (required by assignment)
        if (_attackRoutine != null)
        {
            StopCoroutine(_attackRoutine);
            _attackRoutine = null;
        }

        // Play hurt animation
        if (animator != null)
        {
            animator.ResetTrigger(PunchTriggerHash);
            animator.SetBool(IsWalkingBoolHash, false);
            animator.SetTrigger(HurtTriggerHash);
        }

        StartCoroutine(HurtRoutine());
    }

    // Duration of Hurt state
    private IEnumerator HurtRoutine()   
    {
        yield return new WaitForSeconds(0.3f); // Match your Hurt animation length
        _isHurt = false;
    }

    private void OnDeath()
    {
        _isDead = true;

        // Stop any attack activity
        if (_attackRoutine != null)
        {
            StopCoroutine(_attackRoutine);
            _attackRoutine = null;
        }

        // Play death animation
        if (animator != null)
        {
            animator.SetBool(IsWalkingBoolHash, false);
            animator.SetTrigger(DieTriggerHash);
        }

        // Disable collider so the corpse doesn't block the player
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

    }
}
