using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;
    
namespace SimCruiser_G25.Patches;

[HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Start))]
class AddWheelBindings
{
    private static bool added;

    static void Postfix(PlayerControllerB __instance)
    {
        if (added) return;

        if (!__instance.IsOwner) return; // Ensure local player

        // Get base game's playerActions
        var actions = __instance.playerActions;
        if (actions == null)
        {
            Debug.LogError("Fatal PlayerActions is null :(");
            return;
        }
        
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
        
        // --------------- Adding Bindings ----------------------
        // ------ Move -------
        move.AddCompositeBinding("2DVector")
            .With("Up", "<HID::G25 Racing Wheel>/stick/up")
            .With("Down", "<HID::G25 Racing Wheel>/stick/down")
            .With("Left", "<HID::G25 Racing Wheel>/left")
            .With("Right", "<HID::G25 Racing Wheel>/right");
        
        // ------Jump -------
        jump.AddBinding("<HID::G25 Racing Wheel>/button7");
        jump.Disable();
        jump.Enable();
        
        // --------- Move but for boosts -----------
        move.AddCompositeBinding("2DVector")
            .With("Up", "<HID::G25 Racing Wheel>/button16")
            .With("Down", "<HID::G25 Racing Wheel>/button18")
            .With("Left", "<HID::G25 Racing Wheel>/button17")
            .With("Right", "<HID::G25 Racing Wheel>/button19");
        move.Disable();
        move.Enable();
        
        // ------ Interact ---------
        interact.AddBinding("<HID::G25 Racing Wheel>/button8");
        interact.Disable();
        interact.Enable();
        
        // ------- Look ---------
        look.AddBinding("<HID::G25 Racing Wheel>/hat").WithProcessor("scaleVector2(x=100,y=100)").WithProcessor("invertVector2(invertX=false,invertY=true)");
        look.Disable();
        look.Enable();
        
        // ------------------- Done ----------------
        // Don't let this run again
        added = true;

        Debug.Log("Added G25's input bindings! :D");
    }
}