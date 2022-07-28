using UnityEngine;
using System.Collections;

public class LevelTransitionObject : MonoBehaviour {
    
    public LevelManager.LEVEL_TRANSITION_TYPE transitionType;  // Used for tagging.

    public bool hasTriggered { get; private set; }
    public bool isAnimating { get; private set; }

    private float initialPositionY;

    private void Awake() {
        initialPositionY = transform.position.y;
    }

    public IEnumerator AnimateLevelStart(float startPosY, float duration, AnimationCurve curve) {
        yield return Animate(startPosY, initialPositionY, duration, curve);
    }
        
    public IEnumerator AnimateLevelEnd(float endPosY, float duration, AnimationCurve curve) {
        yield return Animate(initialPositionY, endPosY, duration, curve);
    }
    
    private IEnumerator Animate(float startHeight, float endHeight, float duration, AnimationCurve curve) {
        float timer = 0.0f;
        hasTriggered = true;
        isAnimating = true;

        Vector3 startPosition = new Vector3(transform.position.x, startHeight, transform.position.z);
        Vector3 endPosition = new Vector3(transform.position.x, endHeight, transform.position.z);

        AudioManager.PlayRandomGroupSound(GlobalVariables.LEVEL_TRANSITION_EFFECT_GROUP);
        transform.position = startPosition;

        while(timer < duration) {
            transform.position = Vector3.Lerp(startPosition, endPosition, curve.Evaluate(timer / duration));
            timer += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;
        isAnimating = false;
    }

    public void ResetTrigger() {
        hasTriggered = false;
    }
}