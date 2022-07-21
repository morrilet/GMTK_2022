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
    public AnimationCurve travelHeightCurve;

    [Space, Header("Landing Parameters")]
    public int landingRollTileCount;

    private float triggerDistance = 0.25f;
    private Transform projectileTransform;

    public TurnManager.TURN_TYPE GetTurnType() {
        return TurnManager.TURN_TYPE.WORLD;
    }

    // Same as the button objects - let's consider consolidating this into some utility class or somesuch.
    private bool CheckTrigger() {
        RaycastHit hit;
        Physics.Raycast(transform.position, Vector3.up, out hit, triggerDistance, triggerMask);
        Debug.DrawRay(transform.position, Vector3.up * triggerDistance, Color.blue);

        if (hit.collider)
            projectileTransform = hit.collider.transform;
        else
            projectileTransform = null;
        return hit.collider;
    }

    public void QueueTurn() {
        if (CheckTrigger())
            TurnManager.QueueAction(Jump);
    }

    private void Update() {
        Debug.DrawRay(transform.position, GetLandingDirection(), Color.blue);
    }

    /// <summary>
    /// Get the direction that the projectile should move once it has landed. This is based on the 
    /// direction of travel from the jump pad to the target tile.
    /// </summary>
    /// <returns></returns>
    public Vector3 GetLandingDirection() {
        Vector3 startPosition = new Vector3(transform.position.x, 0.0f, transform.position.z);
        Vector3 endPosition = new Vector3(targetTile.transform.position.x, 0.0f, targetTile.transform.position.z);
        Vector3 travelDirection = endPosition - startPosition;
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
    
    private IEnumerator Jump() {
        Vector3 endPosition = targetTile.position;
        Vector3 startPosition = projectileTransform.position;
        Vector3 nextPosition;
        float timer = 0.0f;
        
        // Ignore the vertical component of the target tile - we work on a single plane.
        endPosition.y = projectileTransform.position.y;

        // Play the audio effect.
        // audioEvent.Post(this.gameObject);

        while (timer < travelDuration) {

            // Handle the travel on both the XZ plane and the Y axis.
            nextPosition = Vector3.Lerp(startPosition, endPosition, timer / travelDuration);
            nextPosition.y = startPosition.y + (travelHeightCurve.Evaluate(timer / travelDuration) * travelMaxHeight);
            projectileTransform.position = nextPosition;

            timer += Time.deltaTime;
            yield return null;
        }

        // Set the position to the exact target position before we start moving the die. Errors will accumulate.
        projectileTransform.position = endPosition;

        Vector3 landingDirection = GetLandingDirection();
        Die projectileDie = projectileTransform.GetComponent<Die>();
        int landingTilesMoved = 0;
        while(landingTilesMoved < landingRollTileCount) {
            yield return projectileDie.ForceExternalMove(landingDirection);
            landingTilesMoved += 1;
        }
    }
}
