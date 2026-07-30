using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public bool isMenuActive;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isMenuActive = !isMenuActive;

            if (isMenuActive)
            {
                CursorLockManager.UnlockCursor();
                pauseMenu.SetActive(false);
            }
            else
            {
                CursorLockManager.LockCursor();
                Cursor.visible = false;
                pauseMenu.SetActive(true);
            }
        }
    }
}