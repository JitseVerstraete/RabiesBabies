using System.Collections;
using Code;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum GameState
{
    MainMenu,
    PauseMenu,
    Playing,
    Ending,
    EndGame
}

public class GameManager: MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager instance { get { return _instance; } }
    private GameState _currentState;
    [SerializeField] private TMP_Text _timerText;
    [SerializeField] private TMP_Text _endText;
    
    [SerializeField] private float _gameEndDuration = 5f;
    public GameObject mainMenu;
    public GameObject pauseMenu;
    public GameObject gameOverMenu;
    public GameObject timerUI;
    public GameObject faceCamsUI;

    private bool _gameEndConditionReached = false;
    
    public BabySpawner babySpawner;
    public SoundManager soundManager;
    
    
    public float gameTime { get; set; }

    public void Start()
    {
        _instance = this;
        _currentState = GameState.MainMenu;
        mainMenu.SetActive(true);
    }

    private void Awake()
    {
        // Singleton setup
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ChangeState(GameState.PauseMenu);
        }
        
        switch (_currentState)
        {
            case GameState.MainMenu:
                Reset();
                break;
            case GameState.PauseMenu:
                break;
            case GameState.Playing:
                gameTime += Time.deltaTime;
                var minutes = Mathf.FloorToInt(gameTime / 60);
                var seconds = Mathf.FloorToInt(gameTime % 60);

                if (_gameEndConditionReached)
                {
                    ChangeState(GameState.Ending);
                }
                
                _timerText.text = $"{minutes:00}:{seconds:00}";
                break;
            case GameState.EndGame:
                break;
        }
    }

    private void Reset()
    {
        _gameEndConditionReached = false;
    }

    public void SetGameEndCondition()
    {
        _gameEndConditionReached = true;
    }
    
    public void ChangeState(GameState newState)
    {
        if (_currentState == newState) return;

        mainMenu.SetActive(newState == GameState.MainMenu);
        timerUI.SetActive(newState == GameState.Playing);
        faceCamsUI.SetActive(newState == GameState.Playing);
        pauseMenu.SetActive(newState == GameState.PauseMenu);
        gameOverMenu.SetActive(newState == GameState.EndGame);
        
        soundManager.ResumeSound("backgroundMusic");
        
        switch (newState)
        {
            case GameState.MainMenu:
                Debug.Log("Main Menu");
                gameTime = 0f;
                soundManager.ResetSpeed("backgroundMusic");
                babySpawner.ResetSpawner();
                FindFirstObjectByType<SecurityScreens>().Reset();
                DestroyAllObjectsSafely(GameObject.FindGameObjectsWithTag("FightCloud"));
                break;
            case GameState.PauseMenu:
                Debug.Log("Pause Menu");
                babySpawner.SetCanSpawn(false);
                soundManager.PauseSound("backgroundMusic");
                break;
            case GameState.Playing:
                Debug.Log("Playing Game");
                babySpawner.SetCanSpawn(true);
                break;
            case GameState.Ending:
                Debug.Log("Ending Game");
                StartCoroutine(WaitThenEndGame());
                break;
            case GameState.EndGame:
                Debug.Log("End Game");
                var minutes = Mathf.FloorToInt(gameTime / 60);
                var seconds = Mathf.FloorToInt(gameTime % 60);
                _endText.text = $"{minutes:00}:{seconds:00} without an accident!";
                break;
        }
        _currentState = newState;
    }
    
    private void DestroyAllObjectsSafely(GameObject[] objects)
    {
        for (int i = objects.Length - 1; i >= 0; i--)
        {
            if (objects[i] != null)
            {
                Destroy(objects[i]); // Mark the object for destruction
                objects[i] = null;   // Clear the reference to avoid access violations
            }
        }
    }


    private IEnumerator WaitThenEndGame()
    {
        yield return new WaitForSeconds(_gameEndDuration);
        ChangeState(GameState.EndGame);
    }
    
}
