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

    // Animator parameter hash
    private static readonly int IsWalkingBoolHash = Animator.StringToHash("IsWalking");

    private bool _frozenOnDeath;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null)
            Debug.LogWarning("PlayerMovement: Missing Rigidbody2D on Player.");

        if (playerHealth == null)
            playerHealth = GetComponent<Health>();

        _animator = GetComponent<Animator>();
        if (_animator == null)
            Debug.LogWarning("PlayerMovement: Missing Animator on Player.");

        _originalScale = transform.localScale;
    }

    private void Update()
    {
        // If dead → stop movement/physics once and exit
        if (playerHealth != null && playerHealth.IsDead)
        {
            _direction = Vector3.zero;

            if (!_frozenOnDeath && _rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.angularVelocity = 0f;
                _rb.constraints = RigidbodyConstraints2D.FreezeAll;
                _frozenOnDeath = true;
            }

            // Optional: stop walk animation
            if (_animator != null)
                _animator.SetBool(IsWalkingBoolHash, false);

            return;
        }

        // Read movement input
        float x = Input.GetAxisRaw("Horizontal");
        _direction = new Vector3(x, 0f, 0f).normalized;
        
        // Update walking animation parameter
        if (_animator != null)
            _animator.SetBool(IsWalkingBoolHash, x != 0f);

        // Flip sprite based on horizontal direction (only if moving horizontally)
        if (x != 0f)
        {
            float scaleX = x < 0 ? -_originalScale.x : _originalScale.x;
            transform.localScale = new Vector3(scaleX, _originalScale.y, 1f);
        }
    }

    private void FixedUpdate()
    {
        if (_rb == null) return;
        
        if (playerHealth != null && playerHealth.IsDead) return;

        Vector2 newPosition = _rb.position + (Vector2)_direction * (speed * Time.fixedDeltaTime);
        _rb.MovePosition(newPosition);
    }

    public void AddScore(int amount)
    {
        _score += amount;

        if (GameManager.Instance != null && GameManager.Instance.hud != null)
        {
             HUD hud = GameManager.Instance.hud.GetComponent<HUD>();
            if (hud != null)
                hud.UpdateScore(_score);
        }
    }
}
