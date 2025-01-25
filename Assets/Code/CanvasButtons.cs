using UnityEngine;

public class CanvasButtons : MonoBehaviour
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

    public void GoToMainMenuButton()
    {
        gameManager.ChangeState(GameState.MainMenu);
    }
}
