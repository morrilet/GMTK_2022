using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager> {
    
    private void Update() {
        if (Input.GetKeyDown(KeyCode.R))
            RestartLevel();

        if (Input.GetKeyDown(KeyCode.Escape))
            ReturnToMainMenu();
    }

    public static void LoadNextLevel() {
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings) {
            SceneManager.LoadScene(nextIndex);
        }
        else
            ReturnToMainMenu(); 
    }

    public static void ReturnToMainMenu() {
        // We're assuming that the main menu is the first scene in the index.
        SceneManager.LoadScene(0);  
    }

    public static void RestartLevel() {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    public static void LoadLevel(int buildIndex) {
        SceneManager.LoadScene(buildIndex);
    }
}