using UnityEngine;

public class EnemyAttackRange : MonoBehaviour
{
    // Indicates whether the player is currently inside the enemy attack range
    public bool PlayerInRange { get; private set; }

    // Cached reference to the player's Health component
    public Health PlayerHealth { get; private set; }

    // Called when another collider enters this trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Try to get the Health component from the entering object or its parent
        // (Player colliders are often on child objects)
        Health h = other.GetComponentInParent<Health>();

        // Make sure:
        // 1. A Health component exists
        // 2. The object belongs to the Player (PlayerMovement is a reliable identifier)
        if (h != null && other.GetComponentInParent<PlayerMovement>() != null)
        {
            PlayerInRange = true;
            PlayerHealth = h;

            Debug.Log("Player ENTER range");
        }
    }

    // Called when another collider exits this trigger
    private void OnTriggerExit2D(Collider2D other)
    {
        // Again, try to get the Health component from the exiting object
        Health h = other.GetComponentInParent<Health>();

        // Only reset if the exiting object is the same player we tracked
        if (h != null && h == PlayerHealth)
        {
            PlayerInRange = false;
            PlayerHealth = null;

            Debug.Log("Player EXIT range");
        }
    }
}