using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TurnController<T> : Singleton<T> where T : Component {
    
    public TurnManager.TURN_TYPE turnType;

    protected ITurnObject[] turnObjects;
    protected List<TurnManager.Action> retryActions;  // A list of actions that failed to complete and need to be re-queued next turn.
    
    protected override void Awake() {
        base.Awake();
        GetTurnObjects();

        retryActions = new List<TurnManager.Action>();
    }

    protected virtual void Update() {
        if (TurnManager.instance.GetCurrentTurn() == turnType && TurnManager.instance.ReadyForNextTurn()) {
            QueueActions();
            TurnManager.TakeTurn();
        }
    }

    /// <summary>
    /// Get the turn objects relevant to this turn controller.
    /// </summary>
    protected virtual void GetTurnObjects() {
        turnObjects = GameObject.FindObjectsOfType<MonoBehaviour>()
            .OfType<ITurnObject>()
            .Where(obj => obj.GetTurnType() == turnType)
            .ToArray();
    }

    /// <summary>
    /// Queue all world object actions within the turn manager.
    /// </summary>
    protected virtual void QueueActions() {
        // Attempt any failed actions from last turn.
        foreach(TurnManager.Action action in retryActions) {
            TurnManager.QueueAction(action);
        }
        retryActions.Clear();

        // Queue the turn for all turn objects.
        foreach(ITurnObject obj in turnObjects) {
            obj.QueueTurn();
        }
    }
    
    /// <summary>
    /// Allows us to repeat actions that failed, such as a door closing.
    /// </summary>
    /// <param name="action"></param>
    public void RequeueActionForNextTurn(TurnManager.Action action) {
        retryActions.Add(action);
    }
}