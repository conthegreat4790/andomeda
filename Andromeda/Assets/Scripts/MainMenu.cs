using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public bool isMenuActive;
    public Image mainMenuBackground;
    public AudioSource menuOpenCloseSound;
    public Button optionsButton;
    public Button exitButton;
    void Start()
    {
        isMenuActive = false;
        pauseMenu.SetActive(false);
        if (mainMenuBackground != null)
        {
            mainMenuBackground.enabled = false;
        }
        optionsButton.gameObject.SetActive(false);
        exitButton.gameObject.SetActive(false);
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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            menuOpenCloseSound.PlayOneShot(menuOpenCloseSound.clip);
        }
        optionsButton.gameObject.SetActive(isMenuActive);
        exitButton.gameObject.SetActive(isMenuActive);
    }
}