using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour, ITurnObject
{
    public LayerMask triggerMask;
    public Transform targetTile;

    [Space, Header("Jump Parameters")]
    public float travelDuration;
    public float travelMaxHeight;
    public int travelRotations = 1;  // The number of sides to turn when we fling the die.
    public AnimationCurve travelHeightCurve;

    [Space, Header("Landing Parameters")]
    public int landingRollTileCount;
    public float bounceMaxHeight;
    public AnimationCurve bounceHeightCurve;

    private float triggerDistance = 0.25f;
    private Transform projectileTransform;
    private Animator animator;
    private Vector3 travelDirection;

    private const string ANIMATION_TRIGGER_NAME = "Spring";
    private const float ANIMATION_DELAY = 0.075f;

    private void Awake() {
        animator = GetComponent<Animator>();
        travelDirection = GetTravelDirection();

        // Make sure we're pointing the right way so our animations make sense.
        SetInitialRotation();
    }

    private void SetInitialRotation() {
        float angle = Vector3.SignedAngle(Vector3.forward, travelDirection.normalized, Vector3.up);
        transform.rotation = Quaternion.Euler(0.0f, angle - 180.0f, 0.0f);
    }

    public TurnManager.TURN_TYPE GetTurnType() {
        return TurnManager.TURN_TYPE.WORLD;
    }

    private bool CheckTrigger() {
        Collider collider = GetTriggerTarget(transform.position);
        if (collider)
            projectileTransform = collider.transform;
        else
            projectileTransform = null;
        return collider != null;
    }
    
    private bool IsLandingTileOccupied() {
        return GetTriggerTarget(targetTile.transform.position) != null;
    }

    private Collider GetTriggerTarget(Vector3 origin) {
        RaycastHit hit;
        Physics.Raycast(origin, Vector3.up, out hit, triggerDistance, triggerMask);
        return hit.collider;
    }

    public void QueueTurn() {
        if (CheckTrigger())
            TurnManager.QueueAction(Jump);
    }

    public Vector3 GetTravelDirection() {
        Vector3 startPosition = new Vector3(transform.position.x, 0.0f, transform.position.z);
        Vector3 endPosition = new Vector3(targetTile.transform.position.x, 0.0f, targetTile.transform.position.z);
        return endPosition - startPosition;
    }

    /// <summary>
    /// Get the direction that the projectile should move once it has landed. This is based on the 
    /// direction of travel from the jump pad to the target tile.
    /// </summary>
    /// <returns></returns>
    public Vector3 GetLandingDirection() {
        float xAmount, zAmount;

        xAmount = Vector3.Dot(travelDirection, Vector3.right);
        zAmount = Vector3.Dot(travelDirection, Vector3.forward);

        Vector3 result = Vector3.zero;
        if (Mathf.Abs(xAmount) > Mathf.Abs(zAmount)) {
            result = Vector3.right * Mathf.Sign(xAmount);
        } else {
            result = Vector3.forward * Mathf.Sign(zAmount);
        }
        return result;
    }

    /// <summary>
    /// Bounces the target and moves it forward using the dice movement system.
    /// </summary>
    /// <param name="direction"></param>
    /// /// <param name="yBaseline"></param>
    /// <returns></returns>
    private IEnumerator BounceAndMoveDie(Die target, Vector3 direction, float yBaseline) {
        float timer = 0.0f;
        float currentY = yBaseline;

        // Asynchronously move the die while we operate on the bounce aspect of the effect.
        StartCoroutine(target.ForceExternalMove(direction));

        // Start bouncing the die.
        float deltaY = 0.0f;
        while (timer < target.moveDuration) {

            deltaY = yBaseline + (bounceHeightCurve.Evaluate(timer / target.moveDuration) * bounceMaxHeight) - currentY;
            currentY = yBaseline + (bounceHeightCurve.Evaluate(timer / target.moveDuration) * bounceMaxHeight);
            
            // target.transform.position = new Vector3(target.transform.position.x, currentY, target.transform.position.z);
            target.transform.position += Vector3.up * deltaY;

            timer += Time.deltaTime;
            yield return null;
        }
        
        // Set the Y position of the target back to baseline.
        target.transform.position = new Vector3(target.transform.position.x, yBaseline, target.transform.position.z);
    }

    private Vector3 GetAirRotationAxis() {
        float dotRight = Vector3.Dot(travelDirection.normalized, Vector3.right);
        float dotForward = Vector3.Dot(travelDirection.normalized, Vector3.forward);
        
        Debug.Log($"RIGHT: {dotRight}");
        Debug.Log($"FORWARD: {dotForward}");

        if (Mathf.Abs(dotForward) > Mathf.Abs(dotRight)) {
            return Vector3.right * -Mathf.Sign(dotForward);
        }
        return Vector3.forward * Mathf.Sign(dotRight);
    }
    
    private IEnumerator Jump() {
        if (IsLandingTileOccupied()) {
            AudioManager.PlaySound(GlobalVariables.BUTTON_FAILURE_EFFECT);
            yield break;
        }

        Vector3 endPosition = targetTile.position;
        Vector3 startPosition = projectileTransform.position;
        Vector3 nextPosition;

        // Get the axis along which to rotate the die in midair.
        Vector3 rotationAxis = GetAirRotationAxis();

        Quaternion endRotation = Quaternion.AngleAxis(travelRotations * -90.0f, rotationAxis) * projectileTransform.rotation;
        Quaternion startRotation = projectileTransform.rotation;
        Quaternion nextRotation;

        float timer = 0.0f;

        // Play the audio effect.
        AudioManager.PlaySound(GlobalVariables.JUMP_PAD_EFFECT);

        // Play the animation and wait for a point during it where it's appropriate to start moving the die.
        animator.SetTrigger(ANIMATION_TRIGGER_NAME);
        yield return new WaitForSeconds(ANIMATION_DELAY);
        
        // Ignore the vertical component of the target tile - we work on a single plane.
        endPosition.y = projectileTransform.position.y;

        while (timer < travelDuration) {

            // Handle the travel on both the XZ plane and the Y axis.
            nextPosition = Vector3.Lerp(startPosition, endPosition, timer / travelDuration);
            nextPosition.y = startPosition.y + (travelHeightCurve.Evaluate(timer / travelDuration) * travelMaxHeight);
            projectileTransform.position = nextPosition;

            // Handle the rotation.
            nextRotation = Quaternion.Lerp(startRotation, endRotation, timer / travelDuration);
            projectileTransform.rotation = nextRotation;

            timer += Time.deltaTime;
            yield return null;
        }

        // Set the position to the exact target position before we start moving the die. Errors will accumulate.
        projectileTransform.position = endPosition;

        // Bounce movement.
        Vector3 landingDirection = GetLandingDirection();
        Die projectileDie = projectileTransform.GetComponent<Die>();
        int landingTilesMoved = 0;
        while(landingTilesMoved < landingRollTileCount) {
            if (projectileDie.isValidMoveDirection(landingDirection)) {
                yield return BounceAndMoveDie(projectileDie, landingDirection, endPosition.y);
                landingTilesMoved += 1;
            } else {
                break;
            }
        }

        // Set the final position after all effects have been applied to be sure we're still on-grid and won't accumulate errors.
        projectileTransform.position = endPosition + (projectileDie.moveDistance * landingDirection * landingTilesMoved);
    }
    
    public int GetTurnOrder() {
        return GlobalVariables.JUMP_PAD_TURN_ORDER;
    }
}
