using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI winLoseText;
    [SerializeField] private Health playerHealth;
    [SerializeField] private Health enemyHealth;
    
    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    
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

        //  Freeze player animation/state to one consistent pose
        if (playerHealth != null)
        {
            var pm = playerHealth.GetComponentInChildren<PlayerMovement>();
            if (pm != null) pm.enabled = false;

            var pp = playerHealth.GetComponentInChildren<PlayerPunch>();
            if (pp != null) pp.enabled = false;

            var anim = playerHealth.GetComponentInChildren<Animator>();
            if (anim != null)
            {
                anim.Rebind();                // resets to default state
                anim.Update(0f);
                anim.SetBool(IsWalkingHash, false);
            }
        }

        if (GameManager.Instance == null) return;
        GameManager.Instance.EndGame();
    }


}