using System.Collections;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RaycastManager : MonoBehaviour
{
    public Camera cam;
    public GameObject interactText;
    public Animator pointerAnimator;

    void Update()
    {
        TextMeshProUGUI interactTextObjectText = interactText.GetComponent<TextMeshProUGUI>();

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 3f))
        {
            if (hit.collider.gameObject.tag == "Interactable")
            {
                Hover();

                if (hit.collider.gameObject.name == "Enter Ship Trigger")
                {
                    interactTextObjectText.text = "Enter Ship (E)";

                    if (Input.GetKey(KeyCode.E))
                    {

                    }
                }
            }
            else
            {
                Unhover();
            }
        }
        else
        {
            Unhover();
        }
    }

    [ContextMenu("Hover")]
    void Hover()
    {
        pointerAnimator.Play("pointer expand");
        interactText.SetActive(true);
    }

    [ContextMenu("Unhover")]
    void Unhover()
    {
        pointerAnimator.Play("pointer shrink");
        interactText.SetActive(false);
    }
}