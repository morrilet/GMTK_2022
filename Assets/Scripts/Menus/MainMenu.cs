using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject levelSelectPanel;

    [Space]

    public Slider musicSlider;
    public Slider effectsSlider;

    private void Awake() {
        mainMenuPanel.SetActive(true);
        levelSelectPanel.SetActive(false);
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
        // TODO: Change Wwise music volume
    }

    public void SetEffectsVolume(float volume) {
        // TODO: Change Wwise effects volume
    }

    // TODO: Volume sliders
}
