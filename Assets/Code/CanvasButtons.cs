using UnityEngine;
using UnityEngine.SceneManagement;

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
        SceneManager.LoadScene(0);
        // GameManager.Instance.ChangeState(GameState.MainMenu);
    }
}
