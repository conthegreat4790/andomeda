using UnityEngine;

public class CursorLockManager : MonoBehaviour
{
    public GameObject pointer;

    void Start()
    {
        LockCursor();
    }

    void Update()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            pointer.transform.position = new Vector2(Screen.width / 2f, Screen.height / 2f);
        }
        else
        {
            pointer.transform.position = Input.mousePosition;
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
