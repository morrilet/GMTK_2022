using UnityEngine;

public class GolemDie : Die, ITurnObject
{
    private void Start() {
        CameraController.AddTarget(transform);
    }

    public void QueueTurn() {
        // If we have a move direction and it's valid, move the die.
        if (isValidMoveDirection(moveDirection)) {
            TurnManager.QueueAction(Move);
        }
        // If the movement is invalid clear the direction so we don't try to use it next turn. 
        else {
            moveDirection = Vector3.zero;
        }
    }

    /// <summary>
    /// Golem dice sync with the player when they have the same side up.
    /// </summary>
    /// <returns></returns>
    public bool IsSynced() {
        bool synced = GetCurrentSide() == WorldController.instance.player.GetCurrentSide();
        return synced;
    }

    /// <summary>
    /// Sets the golem move direction, enabling it to move on the next world turn.
    /// </summary>
    /// <param name="direction"></param>
    public void QueueMove(Vector3 direction) {
        moveDirection = direction;
    }

    public TurnManager.TURN_TYPE GetTurnType() { return TurnManager.TURN_TYPE.GOLEM; }
    
    public int GetTurnOrder() {
        return GlobalVariables.DIE_TURN_ORDER;
    }
}