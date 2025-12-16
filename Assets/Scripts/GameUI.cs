using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI winLoseText;
    [SerializeField] private Health playerHealth;
    [SerializeField] private Health enemyHealth;

    private bool _gameEnded;

    private void Start()
    {
        if (winLoseText != null)
            winLoseText.gameObject.SetActive(false);

        if (playerHealth != null)
            playerHealth.OnDeath += OnPlayerDeath;

        if (enemyHealth != null)
            enemyHealth.OnDeath += OnEnemyDeath;
    }

    private void OnPlayerDeath()
    {
        if (_gameEnded) return;
        ShowMessage("YOU LOSE");
    }

    private void OnEnemyDeath()
    {
        if (_gameEnded) return;
        ShowMessage("YOU WON");
    }

    private void ShowMessage(string msg)
    {
        _gameEnded = true;

        if (winLoseText == null) return;

        winLoseText.text = msg;
        winLoseText.gameObject.SetActive(true);
    }
}