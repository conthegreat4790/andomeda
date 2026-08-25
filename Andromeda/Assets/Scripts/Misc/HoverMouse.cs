using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverMouse : MonoBehaviour
{
    public static HoverMouse Instance { get; private set; }

    public AudioSource audioSource;
    public AudioClip hoverSound;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Start()
    {
        ApplyHoverSoundToAllButtons();
    }

    public void ApplyHoverSoundToAllButtons()
    {
#if UNITY_2023_1_OR_NEWER
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
        Button[] buttons = FindObjectsOfType<Button>(true);
#endif

        Debug.Log($"[HoverMouse] Found {buttons.Length} button(s). Attaching HoverSound.");

        foreach (Button button in buttons)
        {
            AddHoverSound(button.gameObject);
        }
    }

    public void AddHoverSound(GameObject target)
    {
        if (target.GetComponent<HoverSound>() == null)
        {
            target.AddComponent<HoverSound>().hoverSound = hoverSound;
        }
    }

    public void PlayHoverSound()
    {
        if (audioSource == null)
        {
            Debug.LogWarning("[HoverMouse] audioSource is null.");
            return;
        }
        if (hoverSound == null)
        {
            Debug.LogWarning("[HoverMouse] hoverSound is null.");
            return;
        }
        audioSource.PlayOneShot(hoverSound);
    }
}

public class HoverSound : MonoBehaviour, IPointerEnterHandler
{
    public AudioClip hoverSound;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null)
        {
            HoverMouse.Instance?.audioSource.PlayOneShot(hoverSound);
        }
        else
        {
            HoverMouse.Instance?.PlayHoverSound();
        }
    }
}
