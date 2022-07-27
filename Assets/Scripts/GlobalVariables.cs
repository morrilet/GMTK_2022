using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class GlobalVariables : Singleton<GlobalVariables>
{
    [HideInInspector] public bool useLevelTransitionEffects = true;

    protected override void Awake() {
        base.Awake();
        DontDestroyOnLoad(this.gameObject);
    }
}
