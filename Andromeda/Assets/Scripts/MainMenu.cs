using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public bool isMenuActive;
    public Image mainMenuBackground;

    void Start()
    {
        isMenuActive = false;
        pauseMenu.SetActive(false);
        if (mainMenuBackground != null)
        {
            mainMenuBackground.enabled = false;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isMenuActive = !isMenuActive;

            if (isMenuActive)
            {
                CursorLockManager.UnlockCursor();
                pauseMenu.SetActive(false);
                if (mainMenuBackground != null)
                {
                    mainMenuBackground.enabled = true;
                }
            }
            else
            {
                CursorLockManager.LockCursor();
                Cursor.visible = false;
                pauseMenu.SetActive(true);
                if (mainMenuBackground != null)
                {
                    mainMenuBackground.enabled = false;
                }
            }
        }
    }
}