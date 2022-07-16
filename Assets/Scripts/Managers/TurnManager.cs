using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TurnManager : Singleton<TurnManager> {

    // public delegate void OnTickAction(TICK_TYPE tickType);
    // public event OnTickAction onTick;

    public delegate IEnumerator Action();
    public Queue<Action> actionQueue;  // The queue of actions to run on the next tick.
    private Task currentAction;

    public enum TICK_TYPE {
        PLAYER,
        WORLD,
    }

    // Determines the order of the turns. We progress through this list from start to finish and then
    // loop back to the start. Every tick moves the current turn state to the next item in the list.
    private TICK_TYPE[] turnOrder = new TICK_TYPE[] {
        TICK_TYPE.PLAYER,
        TICK_TYPE.WORLD
    };
    private int currentTickPointer;

    protected override void Awake() {
        base.Awake();
        actionQueue = new Queue<Action>();
    }

    private void StartNextAction() {
        // If there's nothing in the queue we're all done.
        if (QueueComplete()) {
            currentAction = null;
            return;
        }

        // If there's something in the queue, fire it off and tell it to fire the next in the chain afterwards.
        Action nextAction = actionQueue.Dequeue();
        currentAction = new Task(nextAction.Invoke());
        currentAction.Finished += delegate (bool manual) { StartNextAction(); };
    }

    public bool ReadyForNextTurn() {
        return QueueComplete() && currentAction == null;
    }

    public bool QueueComplete() {
        return actionQueue.Count == 0;
    }

    public void Tick() {
        currentTickPointer = (currentTickPointer + 1) % turnOrder.Length;
        Debug.Log($"Tick: {turnOrder[currentTickPointer]}");

        StartNextAction();

        // if (onTick != null)
        //     onTick(turnOrder[currentTickPointer]);
    }

    public TICK_TYPE GetCurrentTurn() {
        return turnOrder[currentTickPointer];
    }
}