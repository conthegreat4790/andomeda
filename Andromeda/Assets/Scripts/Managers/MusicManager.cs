using UnityEngine;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour
{
    public AudioSource musicManager;
    public Button musicToggle;
    public AudioClip[] musicClips;
    public GameObject mainMenu;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        musicManager = GetComponent<AudioSource>();
        musicManager.playOnAwake = false;
    }
    void Start()
    {
        musicToggle.onClick.AddListener(ToggleMusic);
    }

    // Update is called once per frame
    void Update()
    {
        if (mainMenu.activeSelf && !musicManager.isPlaying)
        {
            musicManager.clip = musicClips[0];
            musicManager.Play();
        }
        if (!mainMenu.activeSelf && musicManager.isPlaying)
        {
            musicManager.Stop();
        }
    }
    void ToggleMusic()
{
    if (musicManager.isPlaying)
    {
        musicManager.Stop();
    }
    else
    {
        musicManager.Play();
    }
}
}
