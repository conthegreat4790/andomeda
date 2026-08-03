using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public bool isMenuActive;
    public AudioSource menuOpenCloseSound;

    void Start()
    {
        isMenuActive = false;
        pauseMenu.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isMenuActive = !isMenuActive;

            if (isMenuActive)
            {
                CursorLockManager.UnlockCursor();
                pauseMenu.SetActive(true);
            }
            else
            {
                CursorLockManager.LockCursor();
                pauseMenu.SetActive(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            menuOpenCloseSound.PlayOneShot(menuOpenCloseSound.clip);
        }
    }
}