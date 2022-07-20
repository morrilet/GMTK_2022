using UnityEngine;

public class GolemDie : Die, ITurnObject
{

    public void QueueTurn() {

        // If we have a move direction and it's valid, move the die.
        if (isValidMoveDirection(moveDirection)) {
            TurnManager.QueueAction(Move);
        }
    }

    // TODO: Animate sync
    // TODO: What if the game starts with the dice synced? We won't have a world turn to sync before the player can move. Add a setup phase, maybe?

    /// <summary>
    /// Golem dice sync with the player when they have the same side up.
    /// </summary>
    /// <returns></returns>
    public bool IsSynced() {
        return GetCurrentSide() == WorldController.instance.player.GetCurrentSide();
    }

    /// <summary>
    /// Sets the golem move direction, enabling it to move on the next world turn.
    /// </summary>
    /// <param name="direction"></param>
    public void QueueMove(Vector3 direction) {
        moveDirection = direction;
    }

    public TurnManager.TURN_TYPE GetTurnType() { return TurnManager.TURN_TYPE.GOLEM; }
}