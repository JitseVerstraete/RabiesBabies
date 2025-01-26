using UnityEngine;
using UnityEngine.SceneManagement;

public class CanvasButtons : MonoBehaviour
{
    public GameObject credits;
    public GameObject mainMenu;
    
    public void StartGameButton()
    {
        if (GameManager.Instance.playedTutorial)
            GameManager.Instance.ChangeState(GameState.Playing);
        else
        {
            GameManager.Instance.ChangeState(GameState.Tutorial);
            GameManager.Instance.playedTutorial = true;            
        }
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
