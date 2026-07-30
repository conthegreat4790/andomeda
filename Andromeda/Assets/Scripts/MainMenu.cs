using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public Camera mainMenuCamera;
    public Camera mainCamera;
    bool isMenuActive = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoSpawn()
    {
        if (FindObjectOfType<MainMenu>() != null) return;
        var go = new GameObject("MainMenuController");
        go.AddComponent<MainMenu>();
    }

    void Awake()
    {
        if (mainMenuCamera == null)
        {
            var go = GameObject.Find("MainMenuCamera");
            if (go != null) mainMenuCamera = go.GetComponent<Camera>();
        }
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        if (mainMenuCamera == null || mainCamera == null)
        {
            Debug.LogError("MainMenu: missing camera references; disabling.");
            enabled = false;
        }
    }

    void Start()
    {
        mainMenuCamera.enabled = true;
        mainCamera.enabled = false;
        Cursor.visible = true;
        isMenuActive = true;
    }

    void Update()
    {
        if (!enabled || mainMenuCamera == null || mainCamera == null) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isMenuActive = !isMenuActive;
            if (isMenuActive)
            {
                mainMenuCamera.enabled = true;
                mainCamera.enabled = false;
                Cursor.visible = true;
            }
            else
            {
                mainMenuCamera.enabled = false;
                mainCamera.enabled = true;
                Cursor.visible = false;
            }
        }
    }
}