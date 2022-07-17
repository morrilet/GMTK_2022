using UnityEngine;
using System.Linq;

public class WorldController : Singleton<WorldController> {
    
    [HideInInspector] public PlayerDie player;
    [HideInInspector] public GolemDie[] golems;
    
    private ITurnObject[] worldObjects;
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
        foreach(ITurnObject worldObj in worldObjects) {
            worldObj.QueueTurn();
        }
    }
}