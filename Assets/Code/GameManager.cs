using System.Collections;
using Code;
using TMPro;
using UnityEngine;

public enum GameState
{
    Start,
    MainMenu,
    PauseMenu,
    Tutorial,
    Playing,
    Ending,
    EndGame
}

public class GameManager: MonoBehaviour
{
    public static GameManager Instance;
    private GameState _currentState;
    [SerializeField] private TMP_Text _timerText;
    [SerializeField] private TMP_Text _endText;
    [SerializeField] private GameObject _intro;
    [SerializeField] private GameObject _outro;
    [SerializeField] private TMP_Text _endDaysText;
    [SerializeField] private TMP_Text _scoreText;
    
    [SerializeField] private float _gameEndDuration = 5f;
    public GameObject mainMenu;
    public GameObject pauseMenu;
    public GameObject gameOverMenu;
    public GameObject timerUI;
    public GameObject faceCamsUI;
    public GameObject tutorialUI;

    public bool playedTutorial { get; set; }
    private bool _gameEndConditionReached = false;
    private Vector3 _origCamPosition;
    private Quaternion _origCamRotation;
    
    public BabySpawner babySpawner;
    
    public float gameTime { get; set; }

    public void Start()
    {
        playedTutorial = false;
        
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        ChangeState(GameState.MainMenu);
        mainMenu.SetActive(true);
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_currentState == GameState.Playing)
                ChangeState(GameState.PauseMenu);
            else if (_currentState == GameState.PauseMenu)
            {
                UnPauseGame();
            }
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
        tutorialUI.SetActive(newState == GameState.Tutorial);
        
        SoundManager.Instance.ResumeSound("backgroundMusic");
        
        switch (newState)
        {
            case GameState.MainMenu:
                Debug.Log("Main Menu");
                gameTime = 0f;
                Time.timeScale = 1f;
                SoundManager.Instance.ResetSpeed("backgroundMusic");
                SoundManager.Instance.StopAllSounds();
                SoundManager.Instance.PlaySound("backgroundMusic");
                babySpawner.ResetSpawner();
                FindFirstObjectByType<SecurityScreens>()?.Reset();
                DestroyAllObjectsSafely(GameObject.FindGameObjectsWithTag("FightCloud"));
                
                // set up intro baby
                _intro.SetActive(true);
                _intro.GetComponentInChildren<Animator>().SetTrigger("sit");
                _origCamPosition = Camera.main.transform.position;
                _origCamRotation = Camera.main.transform.rotation;
                Camera.main.transform.position = _intro.transform.GetChild(1).position;
                Camera.main.transform.rotation = _intro.transform.GetChild(1).rotation;
                break;
            case GameState.PauseMenu:
                Debug.Log("Pause Menu");
                babySpawner.SetCanSpawn(false);
                SoundManager.Instance.PauseSound("backgroundMusic");
                break;
            case GameState.Tutorial:
                Debug.Log("Tutorial");
                break;
            case GameState.Playing:
                Debug.Log("Playing Game");
                StartCoroutine(SickIntroAnimation());
                break;
            case GameState.Ending:
                Debug.Log("Ending Game");
                StartCoroutine(WaitThenEndGame());
                break;
            case GameState.EndGame:
                Debug.Log("End Game");
                SoundManager.Instance.StopAllSounds();
                SoundManager.Instance.PlaySound("backgroundMusic");
                var minutes = Mathf.FloorToInt(gameTime / 60);
                var seconds = Mathf.FloorToInt(gameTime % 60);
                StartCoroutine(SickOutroAnimation($"You survived baby hell {minutes:00}:{seconds:00} without an accident!"));
                break;
        }
        _currentState = newState;
    }

    public void UnPauseGame()
    {
        _currentState = GameState.Playing;
        babySpawner.SetCanSpawn(true);
        pauseMenu.SetActive(false);
        SoundManager.Instance.ResumeSound("backgroundMusic");
    }

    private IEnumerator SickIntroAnimation()
    {
        // RABID!!
        GameObject[] particleRenderers = GameObject.FindGameObjectsWithTag("IntroParticles");
        foreach (GameObject particleRenderer in particleRenderers)
        {
            particleRenderer.GetComponent<ParticleSystem>().Play();
        }
        
        float t = 0;
        while (t<1)
        {
            t += Time.deltaTime;
            _intro.GetComponentInChildren<SkinnedMeshRenderer>().SetBlendShapeWeight(0, t * 100);
            _intro.GetComponentInChildren<SkinnedMeshRenderer>().materials[3].SetColor("_Emission", Color.Lerp(Color.white, Color.red, t));
            yield return null;
        }

        SoundManager.Instance.PlaySound("baby_growl_short");
        
        yield return new WaitForSeconds(1f);
        FindFirstObjectByType<SecurityScreens>(FindObjectsInactive.Include).gameObject.SetActive(true);

        t = 0;
        while (t<2)
        {
            t += Time.deltaTime;
            if (t <= 1)
            {
                Camera.main.transform.position = Vector3.Lerp(_intro.transform.GetChild(1).position, _intro.transform.GetChild(3).position, t);
                Camera.main.transform.rotation = Quaternion.Slerp(_intro.transform.GetChild(1).rotation, _intro.transform.GetChild(3).rotation, t);               
            }
            else
            {
                Camera.main.transform.position = Vector3.Lerp(_intro.transform.GetChild(3).position, _origCamPosition, t-1);
                Camera.main.transform.rotation = Quaternion.Slerp(_intro.transform.GetChild(3).rotation, _origCamRotation, t-1);
            }
            yield return null;
        }
        
        _intro.SetActive(false);
        
        // spawn fix first baby
        babySpawner.SetCanSpawn(true);
        babySpawner.SpawnDirect(_intro.transform.GetChild(2));
    }
    
    private IEnumerator SickOutroAnimation(string result)
    {
        FindFirstObjectByType<SecurityScreens>().gameObject.SetActive(false);

        float t = 0;
        while (t<2)
        {
            t += Time.deltaTime;
            Camera.main.transform.position = Vector3.Lerp(_origCamPosition, _outro.transform.GetChild(4).position, t/2);
            Camera.main.transform.rotation = Quaternion.Slerp(_origCamRotation, _outro.transform.GetChild(4).rotation, t/2);
            yield return null;
        }

        yield return new WaitForSeconds(1f);
        _endDaysText.text = "0 DAY(S)";
        yield return new WaitForSeconds(1f);
        _scoreText.text = result;
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
