using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, ITriggerable
{
    public GameObject modelObject;
    public LayerMask blockingMask;
 
    // Close the door.
    public IEnumerator releaseTriggerAction() {
        if (CanClose()) {
            modelObject.SetActive(true);
            AudioManager.PlaySound(GlobalVariables.DOOR_CLOSE_EFFECT);
        }
        else  // We could try to be smart here and not re-queue if the door should open again, but since requeues happen before real actions we're fine. It's fine.
            WorldController.instance.RequeueActionForNextTurn(releaseTriggerAction);
        yield return null;
    }

    // Open the door.
    public IEnumerator triggerAction() {
        modelObject.SetActive(false);
        AudioManager.PlaySound(GlobalVariables.DOOR_OPEN_EFFECT);
        yield return null;
    }

    /// <summary>
    /// Check whether or not there is something blocking us from closing the door. If there is, we 
    /// need to re-register our trigger action on the next world turn.
    /// </summary>
    /// <returns></returns>
    private bool CanClose() {
        RaycastHit dieHit;

        // Cast from above us down to our current position - this will make sure we don't start the cast inside a collider.
        Physics.Raycast(transform.position + Vector3.up, Vector3.down, out dieHit, 1.0f, blockingMask);

        return dieHit.collider == null;
    }
}
