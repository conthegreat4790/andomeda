using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject escapeMenu;
    public GameObject pausePanel;
    public GameObject optionsPanel;
    public bool isMenuActive;
    public string currentMenu;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isMenuActive)
        {
            escapeMenu.SetActive(true);
            isMenuActive = true;

            CursorLockManager.UnlockCursor();
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && isMenuActive)
        {
            if (currentMenu == "options")
            {
                currentMenu = "top layer pause";
            }

            if (currentMenu == "top layer pause")
            {
                escapeMenu.SetActive(false);
                isMenuActive = false;

                CursorLockManager.LockCursor();
            }
        }

        if (isMenuActive)
        {
            if (currentMenu == "top layer pause")
            {
                escapeMenu.SetActive(true);
                pausePanel.SetActive(true);
                optionsPanel.SetActive(false);
            }
            else if (currentMenu == "options")
            {
                escapeMenu.SetActive(true);
                optionsPanel.SetActive(true);
                pausePanel.SetActive(false);
            }
        }
    }

    public void ClickMenuButton(string input)
    {
        currentMenu = input;
    }
}