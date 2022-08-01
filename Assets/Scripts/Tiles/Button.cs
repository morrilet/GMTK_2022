using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class Button : MonoBehaviour, ITurnObject
{
    [Space, Header("Events")]
    public UnityEvent triggerMetaEvents;
    public UnityEvent releaseMetaEvents;
    public GameObject[] triggerObjects;

    [Space, Header("Effects")]
    public Material[] valueMaterials;  // Materials for every required value.
    public Material[] valueMaterialsToggle;  // Materials for every required value of toggle button.
    public GameObject buttonObject;
    public GameObject buttonObjectPressed;
    public bool allowModelSwitch;

    [Space, Header("Metadata")]
    public int requiredValue;
    public LayerMask triggerMask;
    public bool requireHeldDown;  // Whether the button needs to stay held down or not.
    public bool requirePlayer;
    public bool requireSpecificValue = true;

    private float triggerDistance = 0.25f;
    private bool triggered = false;
    private bool playedRejectAudio = false;  // Track this so we don't spam the audio cue if a rejected die stays on the button.

    private void Awake() {
        SetMaterial();
        SetModel();
    }

    private void SetModel() {
        if (allowModelSwitch) {
            buttonObject.SetActive(!triggered);
            buttonObjectPressed.SetActive(triggered);
        }
    }

    private void SetMaterial() {
        // Note that pressed is the last in the list and none is the first.

        Renderer buttonRenderer = buttonObject.GetComponentInChildren<Renderer>();
        Renderer buttonPressedRenderer = null;
        Material[] materialArray = requireHeldDown ? valueMaterials : valueMaterialsToggle;

        if (buttonObjectPressed != null) {
            buttonPressedRenderer = buttonObjectPressed.GetComponentInChildren<Renderer>();
            buttonPressedRenderer.materials = new Material[] { materialArray[materialArray.Length - 1] };
        }

        if (requireSpecificValue)
            buttonRenderer.materials = new Material[] { materialArray[requiredValue] };
        else
            buttonRenderer.materials = new Material[] { materialArray[0] };
    }

    private bool CheckTrigger() {
        RaycastHit hit;
        Physics.Raycast(transform.position, Vector3.up, out hit, triggerDistance, triggerMask);
        Debug.DrawRay(transform.position, Vector3.up * triggerDistance, Color.blue);

        if (hit.collider) {
            Die hitDie = hit.collider.gameObject.GetComponent<Die>();

            if (requirePlayer && hitDie != WorldController.instance.player) {
                if (!playedRejectAudio) {
                    AudioManager.PlaySound(GlobalVariables.BUTTON_FAILURE_EFFECT);  // Play the audio cue.
                    playedRejectAudio = true;
                }
                return false;
            }
            if (requireSpecificValue && hitDie.GetCurrentSide() != requiredValue) {
                if (!playedRejectAudio) {
                    AudioManager.PlaySound(GlobalVariables.BUTTON_FAILURE_EFFECT);  // Play the audio cue.
                    playedRejectAudio = true;
                }
                return false;
            }
            return true;
        } else{
            playedRejectAudio = false;
        }
        return false;
    }

    public void QueueTurn() {
        TurnManager.QueueAction(TryTrigger);
    }

    public IEnumerator TryTrigger() {
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

        SetModel();
        yield return null;
    }

    private void Trigger(bool isHeld) {

        // Trigger our objects.
        foreach(GameObject obj in triggerObjects) {
            ITriggerable triggerObj = obj.GetComponent<ITriggerable>();

            if (isHeld)
                TurnManager.QueueAction(triggerObj.triggerAction);
            else
                TurnManager.QueueAction(triggerObj.releaseTriggerAction);
        }

        // Apply meta events and extra actions.
        if (isHeld) {
            triggerMetaEvents.Invoke();
            extraTriggerActions();
        }
        else {
            releaseMetaEvents.Invoke();
            extraReleaseActions();
        }

        // Play the click effect.
        AudioManager.PlaySound(GlobalVariables.BUTTON_SUCCESS_EFFECT);
    }

    protected virtual void extraTriggerActions() {
        // Implemented by children.
    }

    protected virtual void extraReleaseActions() {
        // Implemented by children.
    }

    public TurnManager.TURN_TYPE GetTurnType() { return TurnManager.TURN_TYPE.WORLD; }

    public int GetTurnOrder() {
        return GlobalVariables.BUTTON_TURN_ORDER;
    }
}
