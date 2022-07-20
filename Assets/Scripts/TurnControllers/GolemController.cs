using UnityEngine;
using System.Linq;

public class GolemController : TurnController<GolemController> {

    [HideInInspector] public GolemDie[] golems;

    protected override void Awake() {
        base.Awake();
        golems = turnObjects.Cast<GolemDie>().ToArray();
    }

    /// <summary>
    /// Checks that any synced golem die in the level has a valid move in the given direction.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public bool AnySyncedGolemHasValidMove(Vector3 direction) {
        foreach(GolemDie golem in turnObjects)
            if (golem.IsSynced() && golem.isValidMoveDirection(direction))
                return true;
        return false;
    }
}