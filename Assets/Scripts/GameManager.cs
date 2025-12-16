using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject HUD;
    public GameObject Player;
    
    [SerializeField]
    private GameObject MainMenu;
    public static GameManager Instance { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        DontDestroyOnLoad(gameObject);

        PauseGame();

    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        MainMenu.SetActive(false);
        HUD.SetActive(true);
    }
    
    public void PauseGame()
    {
        Time.timeScale = 0;
        MainMenu.SetActive(true);
        HUD.SetActive(false);
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
