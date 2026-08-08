using UnityEngine;
using UnityEngine.UI;
public class OptionsLogic : MonoBehaviour
{
    public Slider volumeSlider;
    public float currentVolume;

    void Update()
    {
        AudioListener.volume = currentVolume / 100f;
    }

    public void OnValueChangedVolumeSlider()
    {
        currentVolume = volumeSlider.value;
    }
}
