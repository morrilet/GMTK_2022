using UnityEngine;
using System.Collections;

public class Die : MonoBehaviour {

    [Space, Header("Movement Parameters")]
    public float moveDistance = 1.0f;
    public float moveDuration = 0.25f;
    public float rotationAmount = 90.0f;
    public AnimationCurve moveSpeedCurve;
    public LayerMask obstacleMask;
    public LayerMask floorMask;

    [Space, Header("Sides")]
    public SideData[] sides;

    [Space, Header("Effects")]
    public AK.Wwise.Event moveSoundEvent;

    [System.Serializable]
    public struct SideData {
        public int value;
        public Vector3 normal;
        public Color debugColor;
    }

    new private BoxCollider collider;
    protected Vector3 moveDirection;

    protected virtual void Awake() {
        collider = GetComponent<BoxCollider>();
    }

    /// <summary>
    /// Move the die in the current moveDirection.
    /// </summary>
    /// <returns></returns>
    protected IEnumerator Move() {
        yield return AnimateMove(moveDirection);
        moveDirection = Vector3.zero;
    }

    /// <summary>
    /// Immediately forces the die to move if able, or play the failure effects if not. This is
    /// independent of the turn system, so it happens as soon as it's called.
    /// </summary>
    /// <returns></returns>
    public IEnumerator ForceExternalMove(Vector3 direction) {
        if (isValidMoveDirection(direction))
            yield return AnimateMove(direction);
        else
            // TODO: Wiggle!
            yield return null;
    }

    /// <summary>
    /// Get the local pivot point to use for rotating the die based on the direction 
    /// of travel. Should be the bottom edge of the direction we're headed.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    protected Vector3 GetPivotPointForDirection(Vector3 direction) {
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

    protected IEnumerator AnimateMove(Vector3 direction) {
        float timer = 0;

        // We want to swap the X / Z components for rotation axis. If we're moving along the X axis,
        // for example, we want to rotate along the Z axis to make it look correct.
        Vector3 rotationAxis = new Vector3(direction.z, 0.0f, -direction.x);

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = transform.position + (direction * moveDistance);
        Quaternion targetRotation = Quaternion.AngleAxis(rotationAmount, rotationAxis) * transform.rotation;

        Vector3 pivotPoint = GetPivotPointForDirection(direction);

        while (timer < moveDuration) {
            timer += Time.deltaTime;

            float angleStep = (rotationAmount * Time.deltaTime) / moveDuration;
            Quaternion nextRotation = Quaternion.AngleAxis(angleStep, rotationAxis);
            RotateAround(startPosition + pivotPoint, nextRotation);

            yield return null;
        }

        moveSoundEvent.Post(this.gameObject);

        transform.position = targetPosition;
        transform.rotation = targetRotation;
    }

    // Get the number on the side of the die that's currently facing upward.
    public int GetCurrentSide() {
        int bestFitValue = -1;
        float bestFitDot = -Mathf.Infinity;

        foreach(SideData side in sides) {
            float sideDot = Vector3.Dot(transform.rotation * side.normal, Vector3.up);
            if(sideDot > bestFitDot) {
                bestFitValue = side.value;
                bestFitDot = sideDot;
            }

            // Debug.DrawRay(transform.position, transform.rotation * side.normal, side.debugColor);
        }

        return bestFitValue;
    }

    // Checks whether the desired move direction is both open and 
    public bool isValidMoveDirection(Vector3 direction) {
        RaycastHit lateralHit;
        RaycastHit floorHit;

        if (direction == Vector3.zero)
            return false;

        // Cast ahead of us to see if there's anything standing in our direction of travel.
        Physics.Raycast(transform.position, (direction.normalized * moveDistance), out lateralHit, moveDistance, obstacleMask);
        Debug.DrawRay(transform.position, (direction.normalized * moveDistance), Color.red, 1.0f);

        // Cast downward from our target position to make sure there's a tile to stand on.
        Physics.Raycast(transform.position + (direction.normalized * moveDistance), Vector3.down, out floorHit, 1.0f, floorMask);

        // The move is valid if we have no lateral obstacles and there is a floor tile present at the destination.
        return lateralHit.collider == null && floorHit.collider != null;
    }
}