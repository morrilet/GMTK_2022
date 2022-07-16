using System.Collections;

public interface ITriggerable {
    public IEnumerator triggerAction();
    public IEnumerator releaseTriggerAction();
}