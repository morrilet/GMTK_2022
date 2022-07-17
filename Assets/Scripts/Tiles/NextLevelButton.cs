using UnityEngine;

public class NextLevelButton : Button {
    protected override void extraTriggerActions() {
        LevelManager.LoadNextLevel();
    }
}