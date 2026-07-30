using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public Camera mainMenuCamera;
    public Camera mainCamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainMenuCamera.enabled = true;
        mainCamera.enabled = false;
        Cursor.visible = true;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool isMenuActive = mainMenuCamera.enabled;
            mainMenuCamera.enabled = !isMenuActive;
            mainCamera.enabled = isMenuActive;
            Cursor.visible = !isMenuActive;
        }
    }
}
