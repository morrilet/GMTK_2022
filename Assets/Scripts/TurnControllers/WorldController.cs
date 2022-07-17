using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class WorldController : Singleton<WorldController> {
    
    [HideInInspector] public PlayerDie player;
    [HideInInspector] public GolemDie[] golems;
    
    private ITurnObject[] worldObjects;
    private List<TurnManager.Action> retryActions;  // A list of actions that failed to complete and need to be re-queued next turn.
    private Die[] dice;

    protected override void Awake() {
        base.Awake();
        worldObjects = GameObject.FindObjectsOfType<MonoBehaviour>()
            .OfType<ITurnObject>()
            .Where(obj => obj.GetTurnType() == TurnManager.TICK_TYPE.WORLD)
            .ToArray();

        dice = GameObject.FindObjectsOfType<Die>();
        player = GameObject.FindObjectOfType<PlayerDie>();
        golems = GameObject.FindObjectsOfType<GolemDie>();

        retryActions = new List<TurnManager.Action>();
    }

    private void Update() {
        if (TurnManager.instance.GetCurrentTurn() == TurnManager.TICK_TYPE.WORLD && TurnManager.instance.ReadyForNextTurn()) {
            QueueActions();
            TurnManager.TakeTurn();
        }
    }

    /// <summary>
    /// Queue all world object actions within the turn manager.
    /// </summary>
    private void QueueActions() {
        // Attempt any failed actions from last turn.
        foreach(TurnManager.Action action in retryActions) {
            TurnManager.QueueAction(action);
        }
        retryActions.Clear();

        // Queue each world objects turn.
        foreach(ITurnObject worldObj in worldObjects) {
            worldObj.QueueTurn();
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