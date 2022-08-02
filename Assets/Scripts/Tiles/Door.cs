using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, ITriggerable
{
    public GameObject modelObject;
    public LayerMask blockingMask;
    public bool isInverted;

    private void Awake() {
        if (isInverted)
            modelObject.SetActive(false);
    }
 
    // Close the door.
    public IEnumerator releaseTriggerAction() {
        if (!isInverted)
            Close();
        else 
            Open();
        yield return null;
    }

    // Open the door.
    public IEnumerator triggerAction() {
        if (!isInverted)
            Open();
        else 
            Close();
        yield return null;
    }

    private void Open() {
        modelObject.SetActive(false);
        AudioManager.PlaySound(GlobalVariables.DOOR_OPEN_EFFECT);
    }   
    
    private void Close() {
        if (CanClose()) {
            modelObject.SetActive(true);
            AudioManager.PlaySound(GlobalVariables.DOOR_CLOSE_EFFECT);
        }
        else  // We could try to be smart here and not re-queue if the door should open again, but since requeues happen before real actions we're fine. It's fine.
            WorldController.instance.RequeueActionForNextTurn(releaseTriggerAction);
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
