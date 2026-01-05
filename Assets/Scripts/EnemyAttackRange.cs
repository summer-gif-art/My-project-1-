using UnityEngine;



public class EnemyAttackRange : MonoBehaviour
{
    public bool PlayerInRange { get; private set; }
    public Health PlayerHealth { get; private set; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Fast filter first
        if (!other.CompareTag(Tags.Player)) return;

        // Cache once on enter
        var health = other.GetComponentInParent<Health>();
        if (health == null) return;

        PlayerInRange = true;
        PlayerHealth = health;

        Debug.Log("Player ENTER range");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(Tags.Player)) return;

        // Reset only if this is the same player we tracked
        var health = other.GetComponentInParent<Health>();
        if (health != null && health == PlayerHealth)
        {
            PlayerInRange = false;
            PlayerHealth = null;

            Debug.Log("Player EXIT range");
        }
    }
}