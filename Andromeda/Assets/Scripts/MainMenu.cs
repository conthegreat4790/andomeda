using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [FormerlySerializedAs("pauseMenu")]
    public GameObject pausePanel;
    public GameObject optionsPanel;
    public bool isMenuActive;
    public AudioSource menuOpenCloseSound;
    public Button optionsButton;
    public Button exitButton;

    private void Awake()
    {
        if (optionsButton != null)
        {
            optionsButton.onClick.AddListener(OpenOptions);
        }
    }

    private void Start()
    {
        ShowPausePanel(false);
    }

    private void OnDestroy()
    {
        if (optionsButton != null)
        {
            optionsButton.onClick.RemoveListener(OpenOptions);
        }
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
        {
            return;
        }

        if (optionsPanel != null && optionsPanel.activeSelf)
        {
            ShowPausePanel();
        }
        else
        {
            TogglePausePanel();
        }

        PlayMenuSound();
    }

    public void OpenOptions()
    {
        if (!isMenuActive || optionsPanel == null)
        {
            return;
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        optionsPanel.SetActive(true);
        CursorLockManager.UnlockCursor();
    }

    public void ShowPausePanel(bool playSound = true)
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }

        isMenuActive = true;
        CursorLockManager.UnlockCursor();

        if (playSound)
        {
            PlayMenuSound();
        }
    }

    public void CloseMenus()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }

        isMenuActive = false;
        CursorLockManager.LockCursor();
    }

    public void TogglePausePanel()
    {
        if (isMenuActive)
        {
            CloseMenus();
        }
        else
        {
            ShowPausePanel();
        }
    }

    private void PlayMenuSound()
    {
        if (menuOpenCloseSound != null && menuOpenCloseSound.clip != null)
        {
            menuOpenCloseSound.PlayOneShot(menuOpenCloseSound.clip);
        }
    }
}