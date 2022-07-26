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

    protected override void Awake() {
        base.Awake();
        virtualCamera.Follow = targetGroup.transform;
    }

    private void Start() {
        LevelManager.instance.onTransitionBegin += StashTargets;
        LevelManager.instance.onTransitionEnd += RestoreTargets;
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
        targetGroup.m_Targets = new CinemachineTargetGroup.Target[] {placeholderTarget};
    }

    private void RestoreTargets() {
        targetGroup.m_Targets = stashedTargets;
        stashedTargets = new CinemachineTargetGroup.Target[0];
    }
}
