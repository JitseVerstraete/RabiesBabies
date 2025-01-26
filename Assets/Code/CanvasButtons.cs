using UnityEngine;

public class CanvasButtons : MonoBehaviour
{
    public void StartGameButton()
    {
        GameManager.Instance.ChangeState(GameState.Playing);
    }

    public void ExitGameButton()
    {
        Application.Quit();
    }

    public void GoToMainMenuButton()
    {
        GameManager.Instance.ChangeState(GameState.MainMenu);
    }
}
