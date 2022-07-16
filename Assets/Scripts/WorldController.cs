using UnityEngine;

public class WorldController : MonoBehaviour {
    private void Update() {
        if (TurnManager.instance.GetCurrentTurn() == TurnManager.TICK_TYPE.WORLD && TurnManager.instance.ReadyForNextTurn()) {
            QueueActions();
            TurnManager.instance.Tick();
        }
    }

    private void QueueActions() {
        // TODO: Queue button checks, door checks, jump pad handling, etc.
    }
}