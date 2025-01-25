using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameManager gameManager;
    
    void Start()
    {
        gameManager.ChangeState(GameState.MainMenu);
    }

    void Update()
    {
        
    }

    public void StartGameButton()
    {
        gameManager.ChangeState(GameState.Playing);
    }

    public void ExitGameButton()
    {
        Application.Quit();
    }
}
