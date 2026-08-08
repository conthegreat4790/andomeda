using UnityEngine;
using UnityEngine.UI;
public class OptionsLogic : MonoBehaviour
{
    public Button volumePlusButton;
    public Button volumeMinusButton;
    public Image volumeBar;
    public float volumeChangeAmount = 5;
    public float maxVolume = 100;
    public float minVolume = 0;
    private float currentVolume = 50;
    private float GetNormalizedVolume() 
    {
        return currentVolume / maxVolume;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        volumePlusButton.onClick.AddListener(IncreaseVolume);
        volumeMinusButton.onClick.AddListener(DecreaseVolume);
        UpdateVolumeBar();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void IncreaseVolume()
    {
        currentVolume = Mathf.Min(currentVolume + volumeChangeAmount, maxVolume);
        UpdateVolumeBar();
    }

    private void DecreaseVolume()
    {
        currentVolume = Mathf.Max(currentVolume - volumeChangeAmount, minVolume);
        UpdateVolumeBar();
    }

    private void UpdateVolumeBar()
    {
        if (volumeBar != null)
        {
            volumeBar.fillAmount = GetNormalizedVolume();
            AudioListener.volume = GetNormalizedVolume();
        }
    }
}
