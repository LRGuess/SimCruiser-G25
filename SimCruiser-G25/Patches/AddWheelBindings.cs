using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;
    
namespace SimCruiser_G25.Patches;

[HarmonyPatch(typeof(StartOfRound), "Update")]
class AddWheelBindings
{
    private static bool added;

    static void Postfix()
    {
        if (added) return;

        var player = StartOfRound.Instance?.localPlayerController;
        if (player == null) return;

        // Get base game's playerActions
        var actionsBase = player.playerActions.asset;
        if (actionsBase == null)
        {
            Debug.LogError("Fatal PlayerActions is null :(");
            return;
        }

        // Base game's configurable inputs
        var actionsConfigurable = InputSystem.actions;
        if (actionsConfigurable == null)
        {
            Debug.LogError("Fatal PlayerActions is null :(");
            return;
        }
        
        ApplyBindings(actionsBase);
        ApplyBindings(actionsConfigurable);
        added = true;

        Debug.Log("Added G25's input bindings! :D");
    }

    static void ApplyBindings(InputActionAsset actions)
    {
        // ---------------- Getting Actions ---------------
        
        // Get the base game's move input action
        InputAction move = actions.FindAction("Move");
        if (move == null)
        {
            Debug.LogError("Fatal Move Action in PlayerActions is null :(");
            return;
        }
        
        // Get the base game's jump input action (for boosting)
        InputAction jump = actions.FindAction("Jump");
        if (jump == null)
        {
            Debug.LogError("Fatal Jump Action in PlayerActions is null :(");
            return;
        }
        
        // Get the base game's interact action
        InputAction interact = actions.FindAction("Interact");
        if (interact == null)
        {
            Debug.LogError("Fatal Interact Action in PlayerActions is null :(");
            return;
        }
        
        // Get the base game's look input action
        InputAction look = actions.FindAction("Look");
        if (look == null)
        {
            Debug.LogError("Fatal Look Action in PlayerActions is null :(");
            return;
        }
        
        // ----------------- Devices ----------------------
        
        // Find device paths
        foreach (var device in InputSystem.devices)
            Debug.Log(device.path);

        foreach (var device in InputSystem.devices)
        {
            if (device.displayName.ToLower().Contains("g25"))
            {
                Debug.Log("Found g25: "+ device.name);

                foreach (var control in device.allControls)
                {
                    Debug.Log(control.path);
                }
            }
        }
        
        // --------------- Adding Bindings ----------------------
        // ------ Move -------
        move.AddBinding("<HID::G25 Racing Wheel>/stick").WithProcessor("scaleVector2(x=1,y=1)");
        
        // ------Jump -------
        jump.AddBinding("<HID::G25 Racing Wheel>/button8").WithInteraction("press");
        
        // --------- Move but for boosts -----------
        move.AddCompositeBinding("2DVector")
            .With("Up", "<HID::G25 Racing Wheel>/button16")
            .With("Down", "<HID::G25 Racing Wheel>/button18")
            .With("Left", "<HID::G25 Racing Wheel>/button17")
            .With("Right", "<HID::G25 Racing Wheel>/button19");
        
        // ------ Interact ---------
        interact.AddBinding("<HID::G25 Racing Wheel>/button7").WithInteraction("press");
        
        // ------- Look ---------
        look.AddBinding("<HID::G25 Racing Wheel>/hat").WithProcessor("scaleVector2(x=100,y=100)");
    }
}