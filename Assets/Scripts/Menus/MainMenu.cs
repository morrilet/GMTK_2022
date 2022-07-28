using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
// using AK.Wwise;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject levelSelectPanel;
    public GameObject creditsPanel;

    // public AK.Wwise.Event soundtrackEvent;

    [Space]

    public Slider musicSlider;
    public Slider effectsSlider;

    const string MUSIC_VOLUME_RTPC_KEY = "MusicVolume";
    const string EFFECTS_VOLUME_RTPC_KEY = "EffectsVolume";

    private void Awake() {
        SwitchToMainMenuPanel();
        LoadSettings();
    }

    private void Start() {
        AudioManager.PlaySound(GlobalVariables.MAIN_SOUNDTRACK_EFFECT);
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
    
    public void SwitchToMainMenuPanel() {
        mainMenuPanel.SetActive(true);
        levelSelectPanel.SetActive(false);
        creditsPanel.SetActive(false);
    }
    
    public void SwitchToLevelSelectPanel() {
        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
        creditsPanel.SetActive(false);
    }
    
    public void SwitchToCreditsPanel() {
        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    public void Quit() {
        Application.Quit();
    }

    public void SetMusicVolume(float volume) {
        // AkSoundEngine.SetRTPCValue(MUSIC_VOLUME_RTPC_KEY, volume);
        SaveSettings();
    }

    public void SetEffectsVolume(float volume) {
        // AkSoundEngine.SetRTPCValue(EFFECTS_VOLUME_RTPC_KEY, volume);
        SaveSettings();
    }
}
