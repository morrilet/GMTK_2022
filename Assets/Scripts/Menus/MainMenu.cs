using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject levelSelectPanel;
    public GameObject creditsPanel;

    [Space]

    public Slider musicSlider;
    public Slider effectsSlider;

    const string MUSIC_VOLUME_PREFS_KEY = "MusicVolume";
    const string EFFECTS_VOLUME_PREFS_KEY = "EffectsVolume";

    private void Awake() {
        SwitchToMainMenuPanel();
        LoadSettings();
    }

    private void Start() {
        // Start the menu music, including any necessary transitions.
        AudioManager.TryStartMenuMusic();
    }

    private void SaveSettings() {
        PlayerPrefs.SetFloat(MUSIC_VOLUME_PREFS_KEY, musicSlider.value);
        PlayerPrefs.SetFloat(EFFECTS_VOLUME_PREFS_KEY, effectsSlider.value);
        PlayerPrefs.Save();
    }

    private void LoadSettings() {
        if (PlayerPrefs.HasKey(MUSIC_VOLUME_PREFS_KEY)) 
            musicSlider.value = PlayerPrefs.GetFloat(MUSIC_VOLUME_PREFS_KEY);
        if (PlayerPrefs.HasKey(EFFECTS_VOLUME_PREFS_KEY))
            effectsSlider.value = PlayerPrefs.GetFloat(EFFECTS_VOLUME_PREFS_KEY);
        
        SetMusicVolume(musicSlider.value);
        SetEffectsVolume(effectsSlider.value);
    }

    public void StartGame() {
        // Switch to the main soundtrack after we finish our current loop.
        // TODO: Remove this, probably. See below.
        SceneManager.sceneLoaded += MainMenu.OnLoadLevel;

        // Assuming the second scene in the build index is the first level we're all good.
        SceneManager.LoadScene(1);
    }

    // The switch between menu music and in-game music is proving challenging. Where should we run the code to switch? What if the level
    // is completed before the first loop is finished and we can switch tracks?
    // I think the answer is to track whether or not we're playing menu / game music in the audio manager and run the switch in the LevelManager
    // since that doesn't exist in the menu and it'll always run `Start()` when a new level starts.

    // TODO: Remove this, probably. See above.
    public static UnityEngine.Events.UnityAction<Scene, LoadSceneMode> OnLoadLevel = (scene, mode) => {
        AudioManager.SwitchAfterLoops(GlobalVariables.MAIN_MENU_SOUNDTRACK_EFFECT, GlobalVariables.MAIN_SOUNDTRACK_EFFECT, 1);
        SceneManager.sceneLoaded -= MainMenu.OnLoadLevel;  // Don't switch to the main soundtrack again on level load!
    };
    
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
        AudioManager.instance.SetMusicVolume(volume);
        SaveSettings();
    }

    public void SetEffectsVolume(float volume) {
        AudioManager.instance.SetEffectsVolume(volume);
        SaveSettings();
    }

    public void PlayVolumeCheck() {
        AudioManager.PlayRandomGroupSound(GlobalVariables.VOLUME_CHECK_GROUP);
    }
}
