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

    public enum TURN_TYPE {
        PLAYER,
        GOLEM,
        WORLD,
    }

    // Determines the order of the turns. We progress through this list from start to finish and then
    // loop back to the start. Every tick moves the current turn state to the next item in the list.
    private TURN_TYPE[] turnOrder = new TURN_TYPE[] {
        TURN_TYPE.WORLD,
        TURN_TYPE.PLAYER,
        TURN_TYPE.GOLEM
    };
    private int currentTurnPointer;

    protected override void Awake() {
        base.Awake();
        actionQueue = new Queue<Action>();
    }

    protected void StartNextAction() {
        // If there's nothing in the queue we're all done.
        if (QueueComplete()) {
            currentTurnPointer = (currentTurnPointer + 1) % turnOrder.Length;  // Increment the turn counter once we're done with the turn.
            currentAction = null;
            return;
        }

        // If there's something in the queue, fire it off and tell it to fire the next in the chain afterwards.
        Action nextAction = actionQueue.Peek();

        currentAction = new Task(nextAction.Invoke());
        currentAction.Finished += delegate (bool manual) {
            actionQueue.Dequeue(); // Clear the action from the queue once it's finished.
            StartNextAction(); 
        };
    }

    public void LogQueue() {
        Debug.Log("-----");
        foreach(TurnManager.Action act in actionQueue) {
            Debug.Log(act.Method.Name);
        }
        Debug.Log("-----");
    }

    public bool ReadyForNextTurn() {
        return QueueComplete() && currentAction == null;
    }

    public bool QueueComplete() {
        return actionQueue.Count == 0;
    }

    public TURN_TYPE GetCurrentTurn() {
        return turnOrder[currentTurnPointer];
    }

    public static void QueueAction(Action action) {
        TurnManager.instance.actionQueue.Enqueue(action);
    }

    public static void TakeTurn() {
        // Kick off the first action in the list, causing a chain reaction until all actions are resolved.
        TurnManager.instance.StartNextAction();
    }
}