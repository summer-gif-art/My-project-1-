using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject hud;
    [SerializeField] private GameObject mainMenu;

    [Header("Optional References")]
    [SerializeField] private GameObject player;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        // Simple singleton for a single-scene project.
        // (No DontDestroyOnLoad needed since we don't change scenes.)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        ResumeGame(); // Start playing immediately
    }

    public void EndGame()
    {
        // Pause the game (UI still works, since it's not timeScale-dependent)
        Time.timeScale = 0f;

        // Keep HUD active so the win/lose message can be shown
        if (hud != null) hud.SetActive(true);

        Debug.Log("EndGame() called -> Game stopped");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;

        if (mainMenu != null) mainMenu.SetActive(false);
        if (hud != null) hud.SetActive(true);
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;

        if (mainMenu != null) mainMenu.SetActive(true);
        if (hud != null) hud.SetActive(false);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}