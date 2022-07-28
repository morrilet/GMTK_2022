using UnityEngine;

public class NextLevelButton : Button {
    protected override void extraTriggerActions() {
        AudioManager.PlaySound(GlobalVariables.LEVEL_COMPLETE_EFFECT);
        LevelManager.LoadNextLevel();
    }
}