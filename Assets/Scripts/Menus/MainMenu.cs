using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject levelSelectPanel;

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

    // TODO: Volume sliders
}
