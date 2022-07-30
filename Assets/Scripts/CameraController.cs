using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : Singleton<CameraController>
{
    public CinemachineVirtualCamera virtualCamera;
    public CinemachineTargetGroup targetGroup;
    public CinemachineTargetGroup.Target placeholderTarget;  // Swapped in for real targets when the camera is locked (to prevent drift.)

    CinemachineTargetGroup.Target[] stashedTargets;
    CinemachineTransposer virtualTransposer;

    protected override void Awake() {
        base.Awake();

        virtualTransposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        virtualCamera.Follow = targetGroup.transform;
    }

    private void Start() {
        LevelManager.instance.onTransitionBegin += StashTargets;
        LevelManager.instance.onTransitionEnd += RestoreTargets;
        
        Vector3 finalPosition = Vector3.zero;
        float totalWeight = 0.0f;
        
        // Get the initial camera position based on the weighted average position from the target group.
        foreach (CinemachineTargetGroup.Target target in targetGroup.m_Targets) {
            finalPosition += target.target.position * target.weight;
            totalWeight += target.weight;
        }
        finalPosition /= totalWeight;

        transform.position = finalPosition + virtualTransposer.m_FollowOffset;
    }

    public static void AddTarget(Transform target, float weight = 1.0f, float radius = 0.0f) {
        CameraController.instance.targetGroup.AddMember(target, weight, radius);
    }
    
    public static void RemoveTarget(Transform target) {
        CameraController.instance.targetGroup.RemoveMember(target);
    }

    private void UpdatePlaceholderTarget() {
        placeholderTarget.target.position = targetGroup.transform.position;
    }

    private void DeployPlaceholderTarget() {
        placeholderTarget.target.transform.parent = null;
        placeholderTarget.target.transform.position = targetGroup.transform.position;
    }

    private void StashTargets() {
        DeployPlaceholderTarget();

        stashedTargets = targetGroup.m_Targets;
        targetGroup.m_Targets = new CinemachineTargetGroup.Target[] { placeholderTarget };
    }

    private void RestoreTargets() {
        targetGroup.m_Targets = stashedTargets;
        stashedTargets = new CinemachineTargetGroup.Target[0];
    }
}
