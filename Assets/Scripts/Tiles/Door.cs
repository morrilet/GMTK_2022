using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, ITriggerable
{
    public GameObject modelObject;

    public IEnumerator releaseTriggerAction() {
        modelObject.SetActive(true);
        yield return null;
    }

    public IEnumerator triggerAction() {
        modelObject.SetActive(false);
        yield return null;
    }
}
