using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalVariables {
    // Effect groups
    public const string LEVEL_TRANSITION_EFFECT_GROUP = "level_transition";
    public const string DIE_CLACK_EFFECT_GROUP = "dice_clack";
    public const string VOLUME_CHECK_GROUP = "volume_check";

    // Soundtrack
    public const string MAIN_SOUNDTRACK_EFFECT = "main_track";
    public const string MAIN_MENU_SOUNDTRACK_EFFECT = "menu_track";
    public const float SOUNDTRACK_CROSSFADE_DURATION = 2.0f;

    // Individual effects
    public const string LEVEL_COMPLETE_EFFECT = "level_complete";
    public const string BUTTON_SUCCESS_EFFECT = "button_success";
    public const string BUTTON_FAILURE_EFFECT = "button_failure";
    public const string DOOR_CLOSE_EFFECT = "door_close";
    public const string DOOR_OPEN_EFFECT = "door_open";
    public const string JUMP_PAD_EFFECT = "jump_pad_spring";
    public const string SYNC_EFFECT = "die_sync";
    public const string DESYNC_EFFECT = "die_desync";

    // Turn management
    public const int JUMP_PAD_TURN_ORDER = 0;
    public const int DIE_TURN_ORDER = 0;
    public const int BUTTON_TURN_ORDER = 1;
}