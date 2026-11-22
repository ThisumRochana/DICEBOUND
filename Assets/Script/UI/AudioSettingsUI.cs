
using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    [Header("Volume Sliders")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    private void OnEnable()
    {
        if (AudioManager.Instance == null)
            return;

        // Display the current saved volume values
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.SetValueWithoutNotify(
                AudioManager.Instance.MusicVolume
            );
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.SetValueWithoutNotify(
                AudioManager.Instance.SFXVolume
            );
        }
    }

    public void ChangeMusicVolume(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
    }

    public void ChangeSFXVolume(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
    }
}
