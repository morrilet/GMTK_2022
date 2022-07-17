using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AK.Wwise;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject levelSelectPanel;

    [Space]

    public Slider musicSlider;
    public Slider effectsSlider;

    const string MUSIC_VOLUME_RTPC_KEY = "MusicVolume";
    const string EFFECTS_VOLUME_RTPC_KEY = "EffectsVolume";

    private void Awake() {
        mainMenuPanel.SetActive(true);
        levelSelectPanel.SetActive(false);

        LoadSettings();
    }

    private void SaveSettings() {
        PlayerPrefs.SetFloat(MUSIC_VOLUME_RTPC_KEY, musicSlider.value);
        PlayerPrefs.SetFloat(EFFECTS_VOLUME_RTPC_KEY, effectsSlider.value);
        PlayerPrefs.Save();
    }

    private void LoadSettings() {
        if (PlayerPrefs.HasKey(MUSIC_VOLUME_RTPC_KEY)) 
            musicSlider.value = PlayerPrefs.GetFloat(MUSIC_VOLUME_RTPC_KEY);
        if (PlayerPrefs.HasKey(EFFECTS_VOLUME_RTPC_KEY))
            effectsSlider.value = PlayerPrefs.GetFloat(EFFECTS_VOLUME_RTPC_KEY);
        
        SetMusicVolume(musicSlider.value);
        SetEffectsVolume(effectsSlider.value);
    }

    public void StartGame() {
        // Assuming the second scene in the build index is the first level we're all good.
        SceneManager.LoadScene(1);
    }

    public void SetLevelSelectVisible(bool visible) {
        mainMenuPanel.SetActive(!visible);
        levelSelectPanel.SetActive(visible);
    }

    public void Quit() {
        Application.Quit();
    }

    public void SetMusicVolume(float volume) {
        AkSoundEngine.SetRTPCValue(MUSIC_VOLUME_RTPC_KEY, volume);
        SaveSettings();
    }

    public void SetEffectsVolume(float volume) {
        AkSoundEngine.SetRTPCValue(EFFECTS_VOLUME_RTPC_KEY, volume);
        SaveSettings();
    }
}
