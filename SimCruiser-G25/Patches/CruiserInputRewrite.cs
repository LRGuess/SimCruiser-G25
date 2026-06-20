using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SimCruiser_G25.Patches;

[HarmonyPatch(typeof(VehicleController), "GetVehicleInput")]
public class CruiserInputRewrite
{
    static bool Prefix(VehicleController __instance)
    {
        if (__instance.localPlayerInControl)
        {
            if (__instance.testingVehicleInEditor)
            {
                __instance.moveInputVector = __instance.input.actions.FindAction("Move").ReadValue<Vector2>();
            }
            else
            {
                __instance.moveInputVector = InputSystem.actions.FindAction("Move").ReadValue<Vector2>();
            }
            //float num = __instance.steeringWheelTurnSpeed;
            //__instance.steeringInput = Mathf.Clamp(__instance.steeringInput + __instance.moveInputVector.x * num * Time.deltaTime, -3f, 3f);
            float steer = __instance.moveInputVector.x;
            __instance.steeringInput = Mathf.Clamp(steer * 3f, -3f, 3f);
            Debug.Log(__instance.steeringInput);
            if (Mathf.Abs(__instance.moveInputVector.x) > 0.1f)
            {
                __instance.steeringWheelAudio.volume = Mathf.Lerp(__instance.steeringWheelAudio.volume, Mathf.Abs(__instance.moveInputVector.x), 5f * Time.deltaTime);
            }
            else
            {
                __instance.steeringWheelAudio.volume = Mathf.Lerp(__instance.steeringWheelAudio.volume, 0f, 5f * Time.deltaTime);
            }
            __instance.steeringAnimValue = __instance.moveInputVector.x;
            __instance.drivePedalPressed = __instance.moveInputVector.y > 0.1f;
            __instance.brakePedalPressed = __instance.moveInputVector.y < -0.1f;
        }
        
        return false;
    }
}