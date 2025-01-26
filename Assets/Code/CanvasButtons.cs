using UnityEngine;
using UnityEngine.SceneManagement;

public class CanvasButtons : MonoBehaviour
{
    public GameObject credits;
    public GameObject mainMenu;
    
    public void StartGameButton()
    {
        GameManager.Instance.ChangeState(GameState.Playing);
    }

    public void ResumeGameButton()
    {
        GameManager.Instance.UnPauseGame();
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

    public void CreditsButton()
    {
        credits.SetActive(true);
        mainMenu.SetActive(false);
    }

    public void BackToMainMenuButton()
    {
        mainMenu.SetActive(true);
        credits.SetActive(false);
    }
}
