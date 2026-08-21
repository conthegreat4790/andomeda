using System.Collections;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RaycastManager : MonoBehaviour
{
    public Camera cam;
    public ShipUI shipUI;
    public GameObject interactText;
    public Animator pointerAnimator;

    void Update()
    {
        TextMeshProUGUI interactTextObjectText = interactText.GetComponent<TextMeshProUGUI>();

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 3f))
        {
            if (hit.collider.gameObject.tag == "Interactable" && !shipUI.inShipMenu)
            {
                Hover();
                interactText.SetActive(true);

                if (hit.collider.gameObject.name == "Enter Ship Trigger")
                {
                    interactTextObjectText.text = "Enter Ship (E)";

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        shipUI.inShipMenu = true;
                        CursorLockManager.UnlockCursor();
                    }
                }
            }
            else
            {
                Unhover();
                interactText.SetActive(false);
                interactTextObjectText.text = "";
            }
        }
        else
        {
            Unhover();
            interactText.SetActive(false);
        }
    }

    [ContextMenu("Hover")]
    public void Hover()
    {
        pointerAnimator.Play("pointer expand");
    }

    [ContextMenu("Unhover")]
    public void Unhover()
    {
        pointerAnimator.Play("pointer shrink");
    }
}