using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager> {
    
    public static void LoadNextLevel() {
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;

        Debug.Log(SceneManager.GetActiveScene().buildIndex);

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

    // This one is low-priority and depends on time. We should probably move it into the turn manager, honestly.
    public static void Undo() {
        // TODO
    }
}