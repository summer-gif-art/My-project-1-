using System.Collections;
using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI winLoseText;
    [SerializeField] private Health playerHealth;
    [SerializeField] private Health enemyHealth;

    // Animator parameter hash for performance
    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");

    // Prevents multiple win/lose triggers
    private bool _gameEnded;

    private void Start()
    {
        // Hide the win/lose message at the beginning
        if (winLoseText != null)
            winLoseText.gameObject.SetActive(false);

        // Subscribe to death events
        if (playerHealth != null)
            playerHealth.OnDeath += OnPlayerDeath;

        if (enemyHealth != null)
            enemyHealth.OnDeath += OnEnemyDeath;
    }

    // Called when the player dies
    private void OnPlayerDeath()
    {
        if (_gameEnded) return;

        // Delay allows final animations to complete
        StartCoroutine(ShowMessageAfterDelay("YOU LOSE", 0.4f));
    }

    // Called when the enemy dies
    private void OnEnemyDeath()
    {
        if (_gameEnded) return;

        // Delay ensures the final punch animation is visible
        StartCoroutine(ShowMessageAfterDelay("YOU WON", 0.4f));
    }

    // Shows the result message after a short real-time delay
    private IEnumerator ShowMessageAfterDelay(string msg, float delaySeconds)
    {
        _gameEnded = true;

        // Use real-time delay so it works even if Time.timeScale is set to 0 later
        yield return new WaitForSecondsRealtime(delaySeconds);

        ShowMessageNow(msg);
    }

    // Displays the win/lose message and freezes player input
    private void ShowMessageNow(string msg)
    {
        if (winLoseText == null) return;

        // Update and display the UI text
        winLoseText.text = msg;
        winLoseText.gameObject.SetActive(true);

        // Disable player control scripts while keeping the current animation pose
        if (playerHealth != null)
        {
            var pm = playerHealth.GetComponentInChildren<PlayerMovement>();
            if (pm != null) pm.enabled = false;

            var pp = playerHealth.GetComponentInChildren<PlayerPunch>();
            if (pp != null) pp.enabled = false;

            var rb = playerHealth.GetComponentInChildren<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }
            
            var anim = playerHealth.GetComponentInChildren<Animator>();
            if (anim != null)
            {
                // Stop walking animation without resetting the animator state
                anim.SetBool(IsWalkingHash, false);
            }
        }

        // Freeze the game
        if (GameManager.Instance == null) return;
        GameManager.Instance.EndGame(0.4f);
    }
}
