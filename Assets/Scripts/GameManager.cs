using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject hud;
    public GameObject player;
    [SerializeField] private GameObject mainMenu;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ResumeGame(); // start playing immediately

    }

    public void EndGame()
    {
        Time.timeScale = 0f;

        // Keep the HUD active to display the win/lose message
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
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}