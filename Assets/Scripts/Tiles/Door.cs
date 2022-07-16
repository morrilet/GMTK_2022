using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, ITriggerable
{
    public GameObject modelObject;

    public IEnumerator releaseTriggerAction() {
        yield return new WaitForSeconds(1.0f);
        modelObject.SetActive(true);
    }

    public IEnumerator triggerAction() {
        yield return new WaitForSeconds(1.0f);
        modelObject.SetActive(false);
    }
}
