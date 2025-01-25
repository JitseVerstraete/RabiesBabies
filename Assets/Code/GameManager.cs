using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum GameState
{
    MainMenu,
    PauseMenu,
    Playing,
    EndGame
}

public class GameManager: MonoBehaviour
{
    private static GameManager _instance;
    private GameState _currentState;
    [SerializeField] private TMP_Text _timerText;
    
    public GameObject mainMenu;
    public GameObject pauseMenu;
    public GameObject gameOverMenu;
    public GameObject timerUI;
    
    public float gameTime { get; set; }

    public void Start()
    {
        _instance = this;
        _currentState = GameState.MainMenu;
        mainMenu.SetActive(true);
    }
    
    private void Update()
    {
        switch (_currentState)
        {
            case GameState.MainMenu:
                break;
            case GameState.PauseMenu:
                break;
            case GameState.Playing:
                gameTime += Time.deltaTime;
                var minutes = Mathf.FloorToInt(gameTime / 60);
                var seconds = Mathf.FloorToInt(gameTime % 60);
                
                _timerText.text = $"{minutes:00}:{seconds:00}";
                break;
            case GameState.EndGame:
                break;
        }
    }
    
    public void ChangeState(GameState newState)
    {
        if (_currentState == newState) return;

        mainMenu.SetActive(newState == GameState.MainMenu);
        timerUI.SetActive(newState == GameState.Playing);
        //pauseMenu.SetActive(newState == GameState.Playing);
        //gameOverMenu.SetActive(newState == GameState.EndGame);
        
        switch (newState)
        {
            case GameState.MainMenu:
                Debug.Log("Main Menu");
                break;
            case GameState.PauseMenu:
                Debug.Log("Pause Menu");
                break;
            case GameState.Playing:
                Debug.Log("Playing Game");
                gameTime = 0f;
                break;
            case GameState.EndGame:
                Debug.Log("End Game");
                break;
        }
        _currentState = newState;
    }
    
}
