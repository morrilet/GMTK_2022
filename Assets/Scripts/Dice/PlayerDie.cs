using UnityEngine;
using System.Collections;

public class PlayerDie : Die {
    
    private float movementDeadzone = 0.01f;
    private bool allowInput = false;

    protected override void Awake() {
        base.Awake();
        moveDirection = Vector3.zero;
    }

    private void Start() {
        LevelManager.instance.onTransitionBegin += () => allowInput = false;
        LevelManager.instance.onTransitionEnd += () => allowInput = true;
        CameraController.AddTarget(transform);
    }

    public Vector3 GetMoveDirection() {
        return moveDirection;
    }

    private void Update() {
        // Check tick availability and tick if we're attempting to give input on the players turn.
        TryProcessTurn();
    }

    private void TryProcessTurn() {
        if (!allowInput)
            return;

        // Tick if it's the players turn and we have given some input.
        if (TurnManager.instance.GetCurrentTurn() == TurnManager.TURN_TYPE.PLAYER && TurnManager.instance.ReadyForNextTurn()) {
            Vector3 desiredMoveDirection = GetMoveDirectionFromInput();
            moveDirection = desiredMoveDirection;

            // TODO: Consider moving this into a `TryMove(dir)` function so that we can use it in `ForceExternalMove()`
            // If our desired direction is blocked, don't move that way.
            if (!isValidMoveDirection(moveDirection)) {
                // TODO: Wiggle!
                moveDirection = Vector3.zero;
            }
            
            if (MovementShouldTakeTurn(desiredMoveDirection)) {
                TurnManager.QueueAction(Move);
                foreach(GolemDie golem in GolemController.instance.golems) {
                    if (golem.IsSynced())
                        golem.QueueMove(desiredMoveDirection);
                }
                TurnManager.TakeTurn();
            }
        }
    }

    /// <summary>
    /// Checks whether or not the desired movement input should require us to take our turn. This is only the case when the player
    /// die can move in the desired direction or one of the currently synced golems can move in the desired direction.
    /// </summary>
    /// <param name="desiredMovement"></param>
    /// <returns></returns>
    private bool MovementShouldTakeTurn(Vector3 desiredMovement) {
        return isValidMoveDirection(desiredMovement) || GolemController.instance.AnySyncedGolemHasValidMove(desiredMovement);
    }

    private Vector3 GetMoveDirectionFromInput() {
        Vector3 input = new Vector3(
            Mathf.Abs(Input.GetAxisRaw("Horizontal")) > movementDeadzone ? Input.GetAxisRaw("Horizontal") : 0.0f,
            0.0f,
            Mathf.Abs(Input.GetAxisRaw("Vertical")) > movementDeadzone ? Input.GetAxisRaw("Vertical") : 0.0f
        );
        
        // We can only move in one direction at a time, so only worry about the direction with the most input.
        if (Mathf.Abs(input.x) > Mathf.Abs(input.z))
            input.z = 0.0f;
        else
            input.x = 0.0f;

        return input.normalized;
    }

    // TODO: ForceMove(Vector3 direction) -> Allows an external source to move the player using the AnimateMove() function.
}