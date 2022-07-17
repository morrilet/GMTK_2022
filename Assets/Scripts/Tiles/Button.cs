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
    public AK.Wwise.Event successEvent;
    public AK.Wwise.Event failEvent;
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

    private float triggerDistance = 0.1f;
    private bool triggered = false;

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
                // failEvent.Post(this.gameObject);  // Play the audio cue.
                return false;
            }
            if (requireSpecificValue && hitDie.GetCurrentSide() != requiredValue) {
                // failEvent.Post(this.gameObject);  // Play the audio cue.
                return false;
            }
            return true;
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

        SetModel();
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
        successEvent.Post(this.gameObject);
    }

    protected virtual void extraTriggerActions() {
        // Implemented by children.
    }

    protected virtual void extraReleaseActions() {
        // Implemented by children.
    }

    public TurnManager.TICK_TYPE GetTurnType() { return TurnManager.TICK_TYPE.WORLD; }
}
