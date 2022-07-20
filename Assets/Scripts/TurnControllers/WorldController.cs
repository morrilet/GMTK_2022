using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class WorldController : TurnController<WorldController> {
    
    // TODO: Maybe move this to a more appropriate class.
    [HideInInspector] public PlayerDie player;
    
    private Die[] dice;

    protected override void Awake() {
        base.Awake();
        player = GameObject.FindObjectOfType<PlayerDie>();
    }
}