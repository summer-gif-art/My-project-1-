using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 25f;
    [SerializeField] private Health playerHealth;

    [Header("Optional Collectable Reference")]
    [SerializeField] private GameObject diamond;

    // Movement & state
    private Vector3 _direction = Vector3.zero;
    private int _score;
    private Vector3 _originalScale;

    // Cached components
    private Rigidbody2D _rb;
    private Animator _animator;

    // Animator parameter hashes (faster than using strings each frame)
    private static readonly int AttackTriggerHash  = Animator.StringToHash("Attack");
    private static readonly int IsWalkingBoolHash  = Animator.StringToHash("IsWalking");
    private const string PunchStateName = "PlayerPunch";

    private void Start()
    {
        // Get required components
        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null)
        {
            Debug.LogWarning("PlayerMovement: Missing Rigidbody2D on Player.");
        }

        if (playerHealth == null)
        {
            playerHealth = GetComponent<Health>();
        }
        
        _animator = GetComponent<Animator>();
        if (_animator == null)
        {
            Debug.LogWarning("PlayerMovement: Missing Animator on Player.");
        }

        _originalScale = transform.localScale;
    }

    private void Update()
    {
        // Movement is disabled when the player is dead.
        if (playerHealth != null && playerHealth.IsDead)
            return; 

        // If we are still in the punch animation, ignore movement input for this frame
        if (_animator != null && !IsAnimationFinished(_animator, PunchStateName))
        {
            Debug.Log("Player is attacking - movement disabled this frame.");
            return;
        }
        
        // Disable all movement and physics when the player is dead.
        if (playerHealth != null && playerHealth.IsDead)
        {
            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;          // stop all movement
                _rb.angularVelocity = 0f;             // stop any rotation
                _rb.constraints = RigidbodyConstraints2D.FreezeAll;  // completely freeze the body
            }

            return; // do not process any input this frame
        }

        // Attack input (left mouse button)
        if (Input.GetMouseButtonDown(0) && _animator != null)
        {
            _animator.SetTrigger(AttackTriggerHash);
        }

        // Read movement input
        float y = Input.GetAxisRaw("Vertical");
        float x = Input.GetAxisRaw("Horizontal");
        _direction = new Vector3(x, y, 0f).normalized;

        bool isWalking = x != 0f || y != 0f;

        // Update walking animation parameter
        if (_animator != null)
        {
            _animator.SetBool(IsWalkingBoolHash, isWalking);
        }

        // Flip sprite based on horizontal direction
        if (isWalking)
        {
            float scaleX = x < 0 ? -_originalScale.x : _originalScale.x;
            transform.localScale = new Vector3(scaleX, _originalScale.y, 1f);
        }
    }

    private void FixedUpdate()
    {
        if (_rb == null) return;

        // Physics-based movement
        Vector2 newPosition = _rb.position + (Vector2)_direction * (speed * Time.fixedDeltaTime);
        _rb.MovePosition(newPosition);
    }

    /// <summary>
    /// Returns true when the given animation state has finished playing.
    /// </summary>
    private static bool IsAnimationFinished(Animator animator, string animationName)
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);

        // If we are not currently in this state, consider it "finished"
        if (!info.IsName(animationName))
            return true;

        // normalizedTime >= 1 means the animation completed at least once
        return info.normalizedTime >= 1f;
    }

    public void AddScore(int amount)
    {
        _score += amount;

        // Null checks to avoid errors if GameManager / HUD are not set
        if (GameManager.Instance != null && GameManager.Instance.HUD != null)
        {
            HUD hud = GameManager.Instance.HUD.GetComponent<HUD>();
            if (hud != null)
            {
                hud.UpdateScore(_score);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"On trigger enter = {other.name}");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log($"On trigger exit = {other.name}");
    }
}
