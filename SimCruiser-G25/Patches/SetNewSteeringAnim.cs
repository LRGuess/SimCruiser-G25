using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.Versioning;
using HarmonyLib;
using UnityEngine;

namespace SimCruiser_G25.Patches;

[HarmonyPatch(typeof(VehicleController), "SetCarEffects")]
public class SetNewSteeringAnim
{
    static bool Prefix(VehicleController __instance, float setSteering)
    {
	    __instance.steeringWheelAnimFloat = Mathf.Clamp(
		    __instance.steeringWheelAnimFloat 
		    + setSteering * __instance.steeringWheelTurnSpeed * Time.deltaTime / 6f,
		    -2.4f, 2.4f
	    );

	    float normalized = Mathf.Clamp(
		    (__instance.steeringWheelAnimFloat + 2.4f) / 4.8f,
		    0f, 1f
	    );
	    float num = Mathf.Clamp((__instance.steeringWheelAnimFloat + 2.4f) / 4.8f, 0.01f, 0.99f) - __instance.steeringWheelAnimator.GetFloat("steeringWheelTurnSpeed");
	    __instance.steeringWheelAnimator.SetFloat("steeringWheelTurnSpeed", normalized);
		if (__instance.currentDriver != null)
		{
			__instance.currentDriver.playerBodyAnimator.SetFloat("animationSpeed", __instance.currentDriver.playerBodyAnimator.GetFloat("animationSpeed") + num * 2f);
		}
		__instance.leftWheelMesh.transform.localEulerAngles = new Vector3(__instance.leftWheelMesh.transform.localEulerAngles.x, __instance.steeringWheelAnimFloat * 50f, 0f);
		__instance.MatchWheelMeshToCollider(__instance.leftWheelMesh, __instance.FrontLeftWheel);
		__instance.rightWheelMesh.transform.localEulerAngles = new Vector3(__instance.rightWheelMesh.transform.localEulerAngles.x, __instance.steeringWheelAnimFloat * 50f, 0f);
		__instance.MatchWheelMeshToCollider(__instance.rightWheelMesh, __instance.FrontRightWheel);
		__instance.MatchWheelMeshToCollider(__instance.backLeftWheelMesh, __instance.BackLeftWheel);
		__instance.MatchWheelMeshToCollider(__instance.backRightWheelMesh, __instance.BackRightWheel);
		if (__instance.gear == CarGearShift.Reverse)
		{
			__instance.gearStickAnimValue = Mathf.MoveTowards(__instance.gearStickAnimValue, 0.5f, 15f * Time.deltaTime * (Time.realtimeSinceStartup - __instance.timeAtLastGearShift));
		}
		else if (__instance.gear == CarGearShift.Park)
		{
			__instance.gearStickAnimValue = Mathf.MoveTowards(__instance.gearStickAnimValue, 1f, 15f * Time.deltaTime * (Time.realtimeSinceStartup - __instance.timeAtLastGearShift));
		}
		else
		{
			__instance.gearStickAnimValue = Mathf.MoveTowards(__instance.gearStickAnimValue, 0f, 15f * Time.deltaTime * (Time.realtimeSinceStartup - __instance.timeAtLastGearShift));
		}
		__instance.gearStickAnimator.SetFloat("gear", Mathf.Clamp(__instance.gearStickAnimValue, 0.01f, 0.99f));
		if (__instance.EngineRPM < -5f)
		{
			if (!__instance.backLightsOn)
			{
				__instance.backLightsOn = true;
				__instance.backLightsMesh.material = __instance.backLightOnMat;
				__instance.backLightsContainer.SetActive(value: true);
			}
		}
		else if (__instance.backLightsOn)
		{
			__instance.backLightsOn = false;
			__instance.backLightsMesh.material = __instance.headlightsOffMat;
			__instance.backLightsContainer.SetActive(value: false);
		}
		__instance.SetVehicleAudioProperties(__instance.extremeStressAudio, __instance.underExtremeStress, 0.2f, 1f, 3f, useVolumeInsteadOfPitch: true);
		if (__instance.IsOwner)
		{
			if (!__instance.syncedExtremeStress && __instance.underExtremeStress && __instance.extremeStressAudio.volume > 0.35f)
			{
				__instance.syncedExtremeStress = true;
				__instance.SyncExtremeStressServerRpc(__instance.underExtremeStress);
			}
			else if (__instance.syncedExtremeStress && !__instance.underExtremeStress && __instance.extremeStressAudio.volume < 0.5f)
			{
				__instance.syncedExtremeStress = false;
				__instance.SyncExtremeStressServerRpc(__instance.underExtremeStress);
			}
		}
		__instance.SetRadioValues();
		float num2 = Vector3.Dot(Vector3.Normalize(__instance.mainRigidbody.velocity * 1000f), __instance.transform.forward);
		bool audioActive = num2 > -0.6f && num2 < 0.4f && (__instance.averageVelocity.magnitude > 4f || __instance.EngineRPM > 400f);
		if ((!__instance.FrontLeftWheel.isGrounded && !__instance.FrontRightWheel.isGrounded) || (!__instance.BackLeftWheel.isGrounded && !__instance.BackRightWheel.isGrounded))
		{
			audioActive = false;
		}
		if (__instance.FrontLeftWheel.motorTorque > 900f && __instance.FrontRightWheel.motorTorque > 900f)
		{
			audioActive = true;
			num2 = Mathf.Max(num2, 0.8f);
			if (__instance.averageVelocity.magnitude > 8f && !__instance.tireSparks.isPlaying)
			{
				__instance.tireSparks.Play(withChildren: true);
			}
		}
		else
		{
			__instance.tireSparks.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
		}
		__instance.SetVehicleAudioProperties(__instance.skiddingAudio, audioActive, 0f, num2, 3f, useVolumeInsteadOfPitch: true);
		__instance.carEngine2AudioActive = __instance.ignitionStarted;
		float num3 = Mathf.Abs(__instance.EngineRPM);
		float highest = Mathf.Clamp(num3 / __instance.engineIntensityPercentage, 0.7f, 1.5f);
		__instance.SetVehicleAudioProperties(__instance.engineAudio2, __instance.carEngine2AudioActive, 0.7f, highest, 3f, useVolumeInsteadOfPitch: false, 0.5f);
		highest = Mathf.Clamp(num3 / __instance.engineIntensityPercentage, 0.65f, 1.15f);
		float highest2 = highest;
		if (!__instance.ignitionStarted)
		{
			highest2 = 1f;
		}
		__instance.SetVehicleAudioProperties(__instance.engineAudio1, __instance.carEngine1AudioActive, 0.7f, highest2, 2f, useVolumeInsteadOfPitch: false, 0.7f);
		if (__instance.engineAudio1.volume > 0.3f && __instance.engineAudio1.isPlaying && Time.realtimeSinceStartup - __instance.timeAtLastEngineAudioPing > 2f)
		{
			__instance.timeAtLastEngineAudioPing = Time.realtimeSinceStartup;
			if (__instance.EngineRPM > 130f)
			{
				RoundManager.Instance.PlayAudibleNoise(__instance.engineAudio1.transform.position, 32f, 0.75f, 0, noiseIsInsideClosedShip: false, 2692);
			}
			if (__instance.EngineRPM > 60f)
			{
				RoundManager.Instance.PlayAudibleNoise(__instance.engineAudio1.transform.position, 25f, 0.6f, 0, noiseIsInsideClosedShip: false, 2692);
			}
			else if (!__instance.ignitionStarted)
			{
				RoundManager.Instance.PlayAudibleNoise(__instance.engineAudio1.transform.position, 15f, 0.6f, 0, noiseIsInsideClosedShip: false, 2692);
			}
			else
			{
				RoundManager.Instance.PlayAudibleNoise(__instance.engineAudio1.transform.position, 11f, 0.5f, 0, noiseIsInsideClosedShip: false, 2692);
			}
		}
		__instance.carRollingAudioActive = num3 > 10f;
		highest = Mathf.Clamp(num3 / (__instance.engineIntensityPercentage * 0.35f), 0f, 1f);
		__instance.SetVehicleAudioProperties(__instance.rollingAudio, __instance.carRollingAudioActive, 0f, highest, 5f, useVolumeInsteadOfPitch: true);
		__instance.turbulenceAudio.volume = Mathf.Lerp(__instance.turbulenceAudio.volume, Mathf.Min(1f, __instance.turbulenceAmount), 10f * Time.deltaTime);
		__instance.turbulenceAmount = Mathf.Max(__instance.turbulenceAmount - Time.deltaTime, 0f);
		if (__instance.turbulenceAudio.volume > 0.02f)
		{
			if (!__instance.turbulenceAudio.isPlaying)
			{
				__instance.turbulenceAudio.Play();
			}
		}
		else if (__instance.turbulenceAudio.isPlaying)
		{
			__instance.turbulenceAudio.Stop();
		}
        
        return false;
    }
}