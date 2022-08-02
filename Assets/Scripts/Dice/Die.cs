using UnityEngine;
using System.Collections;

public class Die : MonoBehaviour {

    [Space, Header("Movement Parameters")]
    public float moveDistance = 1.0f;
    public float moveDuration = 0.25f;
    public float wiggleDuration = 0.15f;
    public float rotationAmount = 90.0f;
    public AnimationCurve moveSpeedCurve;
    public AnimationCurve wiggleCurve;
    public LayerMask obstacleMask;
    public LayerMask floorMask;

    [Space, Header("Sides")]
    public SideData[] sides;

    [Space, Header("Effects")]
    public Animator animator;
    public ParticleSystem syncParticles;
    public ParticleSystem dustParticles;
    public Vector3 dustParticleOffset = new Vector3(0.0f, -0.5f, 0.0f);

    private const string SYNCED_ANIMATOR_STATE_NAME = "isSynced";
    private Quaternion dustParticleRotation;
    private bool previousAnimateSyncState;
    private bool isWiggling;

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
        dustParticleRotation = dustParticles.transform.rotation;
    }

    /// <summary>
    /// Move the die in the current moveDirection.
    /// </summary>
    /// <returns></returns>
    protected IEnumerator Move() {
        if (isValidMoveDirection(moveDirection))
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
            yield return AnimateWiggle(direction);
    }

    /// <summary>
    /// Get the local pivot point to use for rotating the die based on the direction 
    /// of travel. Should be the bottom edge of the direction we're headed.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    protected Vector3 GetPivotPointForDirection(Vector3 direction) {
        Vector3 pivot = Vector3.zero;

        // Local pivot height is always the same.
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

        while (isWiggling) {
            yield return null;
        }

        // We want to swap the X / Z components for rotation axis. If we're moving along the X axis,
        // for example, we want to rotate along the Z axis to make it look correct.
        Vector3 rotationAxis = new Vector3(direction.z, 0.0f, -direction.x);

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = transform.position + (direction * moveDistance);
        Quaternion targetRotation = Quaternion.AngleAxis(rotationAmount, rotationAxis) * transform.rotation;

        Vector3 localPivot = GetPivotPointForDirection(direction);
        Vector3 worldPivot = localPivot;

        AudioManager.PlayRandomGroupSound(GlobalVariables.DIE_CLACK_EFFECT_GROUP);

        while (timer < moveDuration) {
            timer += Time.deltaTime;

            float angleStep = (rotationAmount * Time.deltaTime) / moveDuration;
            Quaternion nextRotation = Quaternion.AngleAxis(angleStep, rotationAxis);
            worldPivot = startPosition + localPivot;

            // Keep the world pivot locked to the bottom of the die even if it's in the air.
            worldPivot.y = transform.position.y - collider.bounds.extents.y;

            RotateAround(worldPivot, nextRotation);
            yield return null;
        }

        dustParticles.transform.position = targetPosition + dustParticleOffset;
        dustParticles.transform.rotation = dustParticleRotation;
        dustParticles.Play();

        transform.position = targetPosition;
        transform.rotation = targetRotation;
    }

    protected IEnumerator AnimateWiggle(Vector3 direction) {
        if (isWiggling)
            yield break;
        isWiggling = true;

        float timer = 0;
        float prevTimer = 0;

        // We want to swap the X / Z components for rotation axis. If we're moving along the X axis,
        // for example, we want to rotate along the Z axis to make it look correct.
        Vector3 rotationAxis = new Vector3(direction.z, 0.0f, -direction.x);

        Vector3 startPosition = transform.position;
        Vector3 startRotation = transform.rotation.eulerAngles;

        Vector3 localPivot = GetPivotPointForDirection(direction);
        Vector3 worldPivot = localPivot;

        while (timer < wiggleDuration) {
            prevTimer = timer;

            float curveStep = wiggleCurve.Evaluate(timer / wiggleDuration);// - wiggleCurve.Evaluate(prevTimer / wiggleDuration);
            float angleStep = (rotationAmount * curveStep * Time.deltaTime) / wiggleDuration;
            Quaternion nextRotation = Quaternion.AngleAxis(angleStep, rotationAxis);
            worldPivot = startPosition + localPivot;

            // Keep the world pivot locked to the bottom of the die even if it's in the air.
            worldPivot.y = transform.position.y - collider.bounds.extents.y;

            RotateAround(worldPivot, nextRotation);
            timer += Time.deltaTime;
            yield return null;
        }

        transform.position = startPosition;
        transform.rotation = Quaternion.Euler(startRotation);
        isWiggling = false;
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

    public void AnimateSync(bool synced) {
        // Play sync effects if we just synced.
        if (synced && !previousAnimateSyncState) {
            syncParticles.Play();
            AudioManager.PlaySound(GlobalVariables.SYNC_EFFECT);
        }

        // Play desync effects if we just desynced.
        if (!synced && previousAnimateSyncState) {
            AudioManager.PlaySound(GlobalVariables.DESYNC_EFFECT);
        }

        animator.SetBool(SYNCED_ANIMATOR_STATE_NAME, synced);
        previousAnimateSyncState = synced;
    }
}