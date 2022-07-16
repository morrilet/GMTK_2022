using UnityEngine;
using System.Linq;

// TODO: Create a TurnController base class that contains the turn type and the queue actions function. This and WorldController can use it.

public class GolemController : Singleton<GolemController> {
    private ITurnObject[] golems;

    protected override void Awake() {
        base.Awake();
        golems = GameObject.FindObjectsOfType<MonoBehaviour>()
            .OfType<ITurnObject>()
            .Where(obj => obj.GetTurnType() == TurnManager.TICK_TYPE.GOLEM)
            .ToArray();
    }

    private void Update() {
        if (TurnManager.instance.GetCurrentTurn() == TurnManager.TICK_TYPE.GOLEM && TurnManager.instance.ReadyForNextTurn()) {
            QueueActions();
            TurnManager.TakeTurn();
        }
    }

    /// <summary>
    /// Checks that any synced golem die in the level has a valid move in the given direction.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public bool AnySyncedGolemHasValidMove(Vector3 direction) {

        foreach(GolemDie golem in golems)
            if (golem.IsSynced() && golem.isValidMoveDirection(direction))
                return true;
        return false;
    }

    /// <summary>
    /// Queue all world object actions within the turn manager.
    /// </summary>
    private void QueueActions() {
        foreach(ITurnObject obj in golems) {
            obj.QueueTurn();
        }
    }
}