using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameManager gameManager;

    public void StartGameButton()
    {
        gameManager.ChangeState(GameState.Playing);
    }

    public void ExitGameButton()
    {
        Application.Quit();
    }
}
