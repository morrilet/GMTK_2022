using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LevelManager : Singleton<LevelManager> {
    
    [Space, Header("Scene Transitions")]
    public Vector2 transitionSlideDirection;
    public float transitionSlideSpeed;
    public float transitionSlideOffset;  // TODO: Consider making this automatic by finding the furthest object in the slide direction.
    public float transitionTileEffectDuration;

    [Space, Header("Scene Transitions (Raise)")]
    public float transitionRaiseStartHeight;
    public AnimationCurve transitionRaiseCurve;
    
    [Space, Header("Scene Transitions (Fall)")]
    public float transitionDropStartHeight;
    public AnimationCurve transitionDropCurve;

    private List<LevelTransitionObject> levelRaiseObjects;
    private List<LevelTransitionObject> levelDropObjects;

    public delegate void TransitionCallback();
    public event TransitionCallback onTransitionBegin;
    public event TransitionCallback onTransitionEnd;

    public enum LEVEL_TRANSITION_TYPE {
        RAISE,
        DROP
    }

    private void Start() {
        levelRaiseObjects = GetLevelTransitionObjects(LEVEL_TRANSITION_TYPE.RAISE);
        levelDropObjects = GetLevelTransitionObjects(LEVEL_TRANSITION_TYPE.DROP);

        // TODO: Only do this when we're not starting from a restart request.
        StartCoroutine(PlayLevelStartEffects());
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.R))
            RestartLevel();

        if (Input.GetKeyDown(KeyCode.Escape))
            ReturnToMainMenu();
    }

    /// <summary>
    /// Get an array of level transition objects of the given type.
    /// </summary>
    /// <param name="transitionType"></param>
    /// <returns></returns>
    private List<LevelTransitionObject> GetLevelTransitionObjects(LEVEL_TRANSITION_TYPE transitionType) {
        return GameObject.FindObjectsOfType<LevelTransitionObject>().Where(
            obj => obj.transitionType == transitionType
        ).ToList();
    }

    public static void LoadNextLevel() {
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings) {
            SceneManager.LoadScene(nextIndex);
        }
        else
            ReturnToMainMenu(); 
    }

    public static void ReturnToMainMenu() {
        // We're assuming that the main menu is the first scene in the index.
        SceneManager.LoadScene(0);  
    }

    public static void RestartLevel() {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    public static void LoadLevel(int buildIndex) {
        SceneManager.LoadScene(buildIndex);
    }

    private bool TransitionComplete() {
        bool raiseComplete = !levelRaiseObjects.Any(obj => obj.isAnimating == true);
        bool dropComplete = !levelDropObjects.Any(obj => obj.isAnimating == true);
        return raiseComplete && dropComplete;
    }

    /// <summary>
    /// Triggers a level transition object if the transition slider is past it's position.
    /// </summary>
    /// <returns>Whether or not the object was triggered.</returns>
    public bool TryTriggerLevelTransitionObject(LevelTransitionObject tileObj, Vector3 transitionSlidePosition, float tileStartHeight, AnimationCurve tileCurve) {
        Vector3 slideDirection = new Vector3(transitionSlideDirection.normalized.x, 0.0f, transitionSlideDirection.normalized.y);
        Vector3 fromSlidePosToTile = new Vector3(tileObj.transform.position.x, 0.0f, tileObj.transform.position.z) - transitionSlidePosition;

        // If the slide position is past the object position and it's not already transitioning, trigger it.
        if (Vector3.Dot(slideDirection, fromSlidePosToTile) <= 0.0f && !tileObj.hasTriggered) {
            StartCoroutine(tileObj.AnimateLevelStart(
                tileStartHeight,
                transitionTileEffectDuration,
                tileCurve
            ));
            return true;
        }
        return false;
    }

    public IEnumerator PlayLevelStartEffects() {
        Vector3 slideDirection = new Vector3(transitionSlideDirection.normalized.x, 0.0f, transitionSlideDirection.normalized.y);
        Vector3 slidePosition = slideDirection * transitionSlideOffset;

        // TODO: Fix the potential bug where one set of tiles completes before the next ones and `TransitionComplete` resolves to true.
        //       Fix for this is probably to control the transition state from here, but then that messes with the animate trigger.

        List<LevelTransitionObject> objectsToTransition = new List<LevelTransitionObject>();
        objectsToTransition.AddRange(levelRaiseObjects);
        objectsToTransition.AddRange(levelDropObjects);

        int numObjectsToTrigger = levelRaiseObjects.Count + levelDropObjects.Count;
        int countTriggered = 0;

        // Fire the start transition event so listeners know what's up.
        if (onTransitionBegin != null)
            onTransitionBegin();

        // Set the initial tile positions.
        foreach(LevelTransitionObject obj in levelRaiseObjects) {
            obj.transform.position = new Vector3(obj.transform.position.x, transitionRaiseStartHeight, obj.transform.position.z);
        }
        foreach(LevelTransitionObject obj in levelDropObjects) {
            obj.transform.position = new Vector3(obj.transform.position.x, transitionDropStartHeight, obj.transform.position.z);
        }


        while (countTriggered < numObjectsToTrigger) {
            foreach(LevelTransitionObject obj in levelRaiseObjects) {
                if (TryTriggerLevelTransitionObject(obj, slidePosition, transitionRaiseStartHeight, transitionRaiseCurve))
                    countTriggered++;
            }
            foreach(LevelTransitionObject obj in levelDropObjects) {
                if (TryTriggerLevelTransitionObject(obj, slidePosition, transitionDropStartHeight, transitionDropCurve))
                    countTriggered++;
            }

            Debug.DrawLine(slideDirection * transitionSlideOffset, slidePosition, Color.black);

            slidePosition += slideDirection * Time.deltaTime * transitionSlideSpeed;
            yield return null;
        }

        while(!TransitionComplete()) {
            // Wait for the remaining tiles to finish their transitions.
            yield return null;
        }

        // Reset the trigger for future level transitions.
        foreach(LevelTransitionObject obj in objectsToTransition)
            obj.ResetTrigger();

        // Fire the start transition event so listeners know we're done.
        if (onTransitionEnd != null)
            onTransitionEnd();
    }
}