using UnityEngine;
using System.Collections;

public class PlayerDie : MonoBehaviour {
    
    private float movementDeadzone = 0.01f;

    [Header("Movement Parameters")]
    public float moveDistance = 1.0f;
    public float moveDuration = 0.25f;
    public float rotationAmount = 90.0f;
    public AnimationCurve moveSpeedCurve;

    new private BoxCollider collider;
    private Vector3 moveDirection;

    private void Awake() {
        moveDirection = Vector3.zero;
        collider = GetComponent<BoxCollider>();
    }

    private void Update() {
        // Check tick availability and tick if we're attempting to give input on the players turn.
        TryTick();
    }

    private void TryTick() {
        // Tick if it's the players turn and we have given some input.
        if (TurnManager.instance.GetCurrentTurn() == TurnManager.TICK_TYPE.PLAYER && TurnManager.instance.ReadyForNextTurn()) {

            moveDirection = GetMoveDirectionFromInput();
            if (moveDirection != Vector3.zero) {
                // TODO: Don't tick if the movement attempt is blocked.
                TurnManager.instance.actionQueue.Enqueue(AnimateMove);
                TurnManager.instance.Tick();
            }
        }
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

    /// <summary>
    /// Get the local pivot point to use for rotating the die based on the direction 
    /// of travel. Should be the bottom edge of the direction we're headed.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    private Vector3 GetPivotPointForDirection(Vector3 direction) {
        Vector3 pivot = Vector3.zero;

        // Pivot height is always the same.
        pivot.y = -collider.bounds.extents.y;
        
        // Pivot position on the XZ plane is a function of our movement direction.
        Vector3 extents = new Vector3(collider.bounds.extents.x, 0.0f, collider.bounds.extents.z);
        pivot += Vector3.Scale(extents, direction);

        return pivot;
    }

    private void RotateAround(Vector3 pivotPoint, Quaternion rotation) {
        transform.position = rotation * (transform.position - pivotPoint) + pivotPoint;
        transform.rotation = rotation * transform.rotation;
    }

    IEnumerator AnimateMove() {
        float timer = 0;

        // We want to swap the X / Z components for rotation axis. If we're moving along the X axis,
        // for example, we want to rotate along the Z axis to make it look correct.
        Vector3 rotationAxis = new Vector3(moveDirection.z, 0.0f, -moveDirection.x);

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = transform.position + (moveDirection * moveDistance);
        Quaternion targetRotation = transform.rotation * Quaternion.AngleAxis(rotationAmount, rotationAxis);

        Vector3 pivotPoint = GetPivotPointForDirection(moveDirection);

        while (timer < moveDuration) {
            timer += Time.deltaTime;

            float angleStep = (rotationAmount * Time.deltaTime) / moveDuration;
            Quaternion nextRotation = Quaternion.AngleAxis(angleStep, rotationAxis);
            RotateAround(startPosition + pivotPoint, nextRotation);

            yield return null;
        }

        transform.position = targetPosition;
        transform.rotation = targetRotation;
        moveDirection = Vector3.zero;
    }

    // Get the number on the side of the die that's currently facing upward.
    private int GetCurrentSide() {
        return -1;
    }
}