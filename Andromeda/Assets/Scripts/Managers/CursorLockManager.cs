using UnityEngine;

public class CursorLockManager : MonoBehaviour
{
    public GameObject pointer;
    public bool cursorLocked;

    void Start()
    {
        UnlockCursor();
    }

    void Update()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            pointer.transform.position = new Vector2(Screen.width / 2f, Screen.height / 2f);
            cursorLocked = true;
        }
        else
        {
            pointer.transform.position = Input.mousePosition;
            cursorLocked = false;
        }
    }

    public static void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public static void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
    }
}
