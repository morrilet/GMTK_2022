using UnityEngine;
using System.Linq;
using System.Collections;

public class GolemController : TurnController<GolemController> {

    [HideInInspector] public GolemDie[] golems;

    protected override void Awake() {
        base.Awake();
        golems = turnObjects.Cast<GolemDie>().ToArray();
    }

    private void Start() {
        StartCoroutine(HandleSyncForAllDice());  // Animate the sync state for all dice at the start of the level.
    }

    protected override void QueueActions() {
        base.QueueActions();

        // After all golems have taken their turn we re-check the sync state in order to perform any animations.
        TurnManager.QueueAction(HandleSyncForAllDice);
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

    /// <summary>
    /// Checks that any golem is synced.
    /// </summary>
    /// <returns></returns>
    public bool AnyGolemIsSynced() {
        foreach(GolemDie golem in turnObjects)
            if (golem.IsSynced())
                return true;
        return false;
    }

    /// <summary>
    /// Move the die in the current moveDirection.
    /// </summary>
    /// <returns></returns>
    public IEnumerator HandleSyncForAllDice() {
        foreach(GolemDie golem in turnObjects) {
            golem.AnimateSync(golem.IsSynced());
        }
        WorldController.instance.player.AnimateSync(AnyGolemIsSynced());

        yield return null;
    }
}