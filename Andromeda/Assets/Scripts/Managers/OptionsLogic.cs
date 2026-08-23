using UnityEngine;
using UnityEngine.UI;
public class OptionsLogic : MonoBehaviour
{
    public Slider volumeSlider;
    public float currentVolume;
    public Button resetToDefault;

    void Start()
    {
        resetToDefault.onClick.AddListener(ResetToDefault);
    }
    void Update()
    {
        AudioListener.volume = currentVolume / 100f;
        currentVolume = volumeSlider.value;
    }
    void ResetToDefault()
    {
        currentVolume = 50f;
        volumeSlider.value = currentVolume;
    }
}
