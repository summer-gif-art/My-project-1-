using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    [SerializeField]
    private TMP_Text ScoreText;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateScore(0);
    }
    
    public void UpdateScore(int score)
    {
        ScoreText.text = $"Score: {score}";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
