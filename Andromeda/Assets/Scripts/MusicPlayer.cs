using UnityEngine;
using UnityEngine.UI;

public class MusicPlayer : MonoBehaviour
{
    public Button musicToggle;
    public AudioSource musicSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        musicToggle.onClick.AddListener(ToggleMusic);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void ToggleMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Pause();
        }
        else
        {
            musicSource.Play();
        }
    }
}
