using UnityEngine;
using System.Collections;

public class PlayerDie : Die {
    
    private float movementDeadzone = 0.01f;
    private Vector3 moveDirection;

    protected override void Awake() {
        base.Awake();

        moveDirection = Vector3.zero;
    }

    private void Update() {
        // Check tick availability and tick if we're attempting to give input on the players turn.
        TryProcessTurn();
    }

    private void TryProcessTurn() {
        // Tick if it's the players turn and we have given some input.
        if (TurnManager.instance.GetCurrentTurn() == TurnManager.TICK_TYPE.PLAYER && TurnManager.instance.ReadyForNextTurn()) {

            moveDirection = GetMoveDirectionFromInput();

            // If our desired direction is blocked, don't move that way.
            if (!isValidMoveDirection(moveDirection)) {
                // TODO: Wiggle!
                moveDirection = Vector3.zero;
            }

            // If we still have a target direction, fire away!
            if (moveDirection != Vector3.zero) {
                TurnManager.instance.actionQueue.Enqueue(Move);
                TurnManager.instance.Tick();
            }
        }
    }

    private IEnumerator Move() {
        yield return AnimateMove(moveDirection);
        moveDirection = Vector3.zero;
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