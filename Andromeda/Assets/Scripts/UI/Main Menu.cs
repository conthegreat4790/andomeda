using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public bool gameStarted;
    public GameObject player;
    public GameObject mainMenu;

    void Update()
    {

    }

    public void PlayButton()
    {
        gameStarted = true;
        player.SetActive(true);
        mainMenu.SetActive(false);
        CursorLockManager.LockCursor();
    }

    public void QuitButton()
    {
        Application.Quit();
    }
}