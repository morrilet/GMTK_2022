using UnityEngine;
using System.Linq;

public class WorldController : Singleton<WorldController> {
    
    private ITurnObject[] worldObjects;

    protected override void Awake() {
        base.Awake();
        worldObjects = GameObject.FindObjectsOfType<MonoBehaviour>().OfType<ITurnObject>().ToArray();
    }

    private void Update() {
        if (TurnManager.instance.GetCurrentTurn() == TurnManager.TICK_TYPE.WORLD && TurnManager.instance.ReadyForNextTurn()) {
            QueueActions();
            TurnManager.instance.Tick();
        }
    }

    /// <summary>
    /// Queue all world object actions within the turn manager.
    /// </summary>
    private void QueueActions() {
        foreach(ITurnObject worldObj in worldObjects) {
            worldObj.QueueTurn();
        }
    }
}