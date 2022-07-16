using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class Button : MonoBehaviour, ITurnObject
{
    [Space, Header("Events")]
    public GameObject[] triggerObjects;

    [Space, Header("Metadata")]
    public int requiredValue;
    public LayerMask triggerMask;
    public bool requireHeldDown;  // Whether the button needs to stay held down or not.

    private float triggerDistance = 0.1f;
    private bool triggered = false;

    private bool CheckTrigger() {
        RaycastHit hit;
        Physics.Raycast(transform.position, Vector3.up, out hit, triggerDistance, triggerMask);
        Debug.DrawRay(transform.position, Vector3.up * triggerDistance, Color.blue);

        if (hit.collider) {
            Die hitDie = hit.collider.gameObject.GetComponent<Die>();
            return hitDie.GetCurrentSide() == requiredValue;
        }
        return false;
    }

    public void QueueTurn() {
        // Queue the trigger objects if we've been triggered.
        if (CheckTrigger() && !triggered) {
            Trigger(true);
        }
        
        // For buttons that need to be held down, queue the reverse trigger action if the die leaves the button.
        if (!CheckTrigger() && triggered && requireHeldDown) {
            Trigger(false);
            triggered = false;
        }

        // Only check triggered if it's not already set. This allows us to leave it 
        // set for buttons that don't get unset when the die moves off of it.
        if (!triggered)
            triggered = CheckTrigger();
    }

    private void Trigger(bool isHeld) {
        foreach(GameObject obj in triggerObjects) {
            ITriggerable triggerObj = obj.GetComponent<ITriggerable>();

            if (isHeld)
                TurnManager.QueueAction(triggerObj.triggerAction);
            else
                TurnManager.QueueAction(triggerObj.releaseTriggerAction);
        }
    }

    public TurnManager.TICK_TYPE GetTurnType() { return TurnManager.TICK_TYPE.WORLD; }
}
