using UnityEngine;

public class Collectable : MonoBehaviour
{
    [SerializeField] private int scoreValue = 10;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(Tags.Player))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.AddScore(scoreValue);
                Debug.Log($"Collectible picked up by {other.name}");
                Destroy(gameObject);
            }
        }
    }
}