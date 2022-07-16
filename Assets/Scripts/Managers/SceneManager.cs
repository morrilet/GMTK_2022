using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : Singleton<SceneManager> {
    
    public static void LoadNextLevel() {
        // TODO: Get the next level in the build index and load it, or kick back to the main menu if we're at the end.
    }

    public static void ReturnToMainMenu() {
        // TODO
    }

    public static void RestartLevel() {
        // TODO
    }

    // This one is low-priority and depends on time. We should probably move it into the turn manager, honestly.
    public static void Undo() {
        // TODO
    }
}