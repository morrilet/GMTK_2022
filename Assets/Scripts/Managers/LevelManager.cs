using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LevelManager : Singleton<LevelManager> {
    
    [Space, Header("Scene Transitions")]
    public Vector2 transitionSlideDirection;
    public float transitionSlideSpeed;
    public float transitionTileEffectDuration;

    [Space, Header("Scene Transitions (Raise)")]
    public float transitionRaiseStartHeight;
    public AnimationCurve transitionRaiseCurve;
    
    [Space, Header("Scene Transitions (Fall)")]
    public float transitionDropStartHeight;
    public AnimationCurve transitionDropCurve;
    
    [Space, Header("Scene Transitions (Level Exit)")]
    public AnimationCurve transitionLevelExitCurve;

    private List<LevelTransitionObject> levelRaiseObjects;
    private List<LevelTransitionObject> levelDropObjects;
    private float transitionSlideOffset;

    public delegate void TransitionCallback();
    public event TransitionCallback onTransitionBegin;
    public event TransitionCallback onTransitionEnd;

    private bool loadingNextLevel = false;

    public enum LEVEL_TRANSITION_TYPE {
        RAISE,
        DROP
    }

    private void Start() {
        levelRaiseObjects = GetLevelTransitionObjects(LEVEL_TRANSITION_TYPE.RAISE);
        levelDropObjects = GetLevelTransitionObjects(LEVEL_TRANSITION_TYPE.DROP);

        // Get the slide offset based on the most distant object in the scene.
        Vector3 furthestObjPosition = GetFurthestLevelTransitionObjectInDirection(-transitionSlideDirection);
        float offsetSign = Mathf.Sign(Vector3.Dot(furthestObjPosition, transitionSlideDirection));
        transitionSlideOffset = Vector3.Project(furthestObjPosition, transitionSlideDirection).magnitude * offsetSign;

        // TODO: Only do this when we're not starting from a restart request.
        if (PersistentVariableStore.instance.useLevelTransitionEffects)
            StartCoroutine(PlayLevelStartEffects());
            
        PersistentVariableStore.instance.useLevelTransitionEffects = true;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.R))
            RestartLevel();

        if (Input.GetKeyDown(KeyCode.Escape))
            ReturnToMainMenu();
    }

    private Vector3 GetFurthestLevelTransitionObjectInDirection(Vector3 direction) {
        LevelTransitionObject[] allTransitionObjects = GameObject.FindObjectsOfType<LevelTransitionObject>();
        Vector3 nearestPosition = Vector3.zero;
        float nearestDot = -Mathf.Infinity;

        // Using the dot product on non-normalized vectors also includes distance, so it works here.
        foreach (LevelTransitionObject obj in allTransitionObjects) {
            float dot = Vector3.Dot(direction, obj.transform.position);
            if (dot > nearestDot) {
                nearestDot = dot;
                nearestPosition = obj.transform.position;
            }
        }

        return nearestPosition;
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
        LevelManager.instance.StartCoroutine(LevelManager.instance.LoadNextLevelCoroutine());
    }

    public IEnumerator LoadNextLevelCoroutine() {
        if (loadingNextLevel)
            yield break;

        loadingNextLevel = true;
        yield return PlayLevelEndEffects();
        loadingNextLevel = false;
        
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(nextIndex);
        else
            ReturnToMainMenu(); 
    }

    public static void ReturnToMainMenu() {
        // We're assuming that the main menu is the first scene in the index.
        SceneManager.LoadScene(0);  
    }

    public static void RestartLevel() {
        Scene currentScene = SceneManager.GetActiveScene();
        PersistentVariableStore.instance.useLevelTransitionEffects = false;
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    public static void LoadLevel(int buildIndex) {
        SceneManager.LoadScene(buildIndex);
    }

    private bool TransitionComplete(List<LevelTransitionObject> transitionObjects) {
        return !transitionObjects.Any(obj => obj.isAnimating == true);
    }

    /// <summary>
    /// Triggers a level transition object if the transition slider is past it's position.
    /// </summary>
    /// <returns>Whether or not the object was triggered.</returns>
    public bool TryTriggerLevelTransitionObject(LevelTransitionObject tileObj, Vector3 transitionSlidePosition, float finalTileHeight, AnimationCurve tileCurve, bool start=true) {
        Vector3 slideDirection = new Vector3(transitionSlideDirection.normalized.x, 0.0f, transitionSlideDirection.normalized.y);
        Vector3 fromSlidePosToTile = new Vector3(tileObj.transform.position.x, 0.0f, tileObj.transform.position.z) - transitionSlidePosition;

        // If the slide position is past the object position and it's not already transitioning, trigger it.
        if (Vector3.Dot(slideDirection, fromSlidePosToTile) <= 0.0f && !tileObj.hasTriggered) {
            if (start) {
                StartCoroutine(tileObj.AnimateLevelStart(
                    finalTileHeight,
                    transitionTileEffectDuration,
                    tileCurve
                ));
            } else {
                StartCoroutine(tileObj.AnimateLevelEnd(
                    finalTileHeight,
                    transitionTileEffectDuration,
                    tileCurve
                ));
            }
            return true;
        }
        return false;
    }

    // TODO: Clean this up. I wrote it when I was so, so tired.
    public IEnumerator PlayLevelStartEffects() {
        Vector3 slideDirection = new Vector3(transitionSlideDirection.normalized.x, 0.0f, transitionSlideDirection.normalized.y);
        Vector3 slidePosition = slideDirection * transitionSlideOffset;

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
                if (TryTriggerLevelTransitionObject(obj, slidePosition, transitionRaiseStartHeight, transitionRaiseCurve, true))
                    countTriggered++;
            }
            foreach(LevelTransitionObject obj in levelDropObjects) {
                if (TryTriggerLevelTransitionObject(obj, slidePosition, transitionDropStartHeight, transitionDropCurve, true))
                    countTriggered++;
            }

            Debug.DrawLine(slideDirection * transitionSlideOffset, slidePosition, Color.black);

            slidePosition += slideDirection * Time.deltaTime * transitionSlideSpeed;
            yield return null;
        }

        while(!TransitionComplete(objectsToTransition)) {
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

    public IEnumerator PlayLevelEndEffects() {
        Vector3 slideDirection = new Vector3(transitionSlideDirection.normalized.x, 0.0f, transitionSlideDirection.normalized.y);
        Vector3 slidePosition = slideDirection * transitionSlideOffset;

        List<LevelTransitionObject> objectsToTransition = GameObject.FindObjectsOfType<LevelTransitionObject>().ToList();
        int numObjectsToTrigger = objectsToTransition.Count;
        int countTriggered = 0;

        // Fire the start transition event so listeners know what's up.
        if (onTransitionBegin != null)
            onTransitionBegin();

        while (countTriggered < numObjectsToTrigger) {
            foreach(LevelTransitionObject obj in objectsToTransition) {
                if (TryTriggerLevelTransitionObject(obj, slidePosition, transitionRaiseStartHeight, transitionLevelExitCurve, false))
                    countTriggered++;
            }

            Debug.DrawLine(slideDirection * transitionSlideOffset, slidePosition, Color.black);

            slidePosition += slideDirection * Time.deltaTime * transitionSlideSpeed;
            yield return null;
        }

        while(!TransitionComplete(objectsToTransition)) {
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