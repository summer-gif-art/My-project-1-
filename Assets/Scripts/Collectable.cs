using UnityEngine;

public class Collectable : MonoBehaviour
{
    private const int score = 10;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.Player.GetComponent<PlayerMovement>().AddScore(score);

            Destroy(gameObject);
            Debug.Log($"$On trigger enter = {other.name}");
        }
    }
}
