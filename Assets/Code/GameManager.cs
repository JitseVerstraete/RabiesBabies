using System;
using UnityEngine;

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
    public float gameTime { get; set; }

    public void Awake()
    {
        _instance = this;
        _currentState = GameState.MainMenu;
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
                break;
            case GameState.EndGame:
                break;
        }
    }
    
    public void ChangeState(GameState newState)
    {
        if (_currentState == newState) return;

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
