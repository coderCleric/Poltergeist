using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Poltergeist
{
    [HarmonyPatch]
    public static class Patches
    {
        /////////////////////////////// Needed to suppress certain base-game systems ///////////////////////////////
        /**
         * Prevents certain manipulations of the spectate camera that would interfere with the controls
         */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerControllerB), "RaycastSpectateCameraAroundPivot")]
        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.SetSpectateCameraToGameOverMode))]
        public static bool PreventSpectateFollow()
        {
            return false;
        }

        /**
         * Before doing late update stuff, make sure that the spectate camera is set to be overridden
         * 
         * __instance The calling player controller
         */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerControllerB), "LateUpdate")]
        public static void OverrideSpectateCam(PlayerControllerB __instance)
        {
            __instance.playersManager.overrideSpectateCamera = true;
        }




        /////////////////////////////// These are needed to manage the state of the camera controller ///////////////////////////////
        /**
         * When switching the camera, disable or enable the controller
         * 
         * @param __instance The calling start of round object
         * @param newCamera The new camera
         */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.SwitchCamera))]
        public static void ManageCameraController(StartOfRound __instance, Camera newCamera)
        {
            if (newCamera == __instance.spectateCamera)
                SpectatorCamController.instance.EnableCam();
            else
                SpectatorCamController.instance.DisableCam();
        }

        /**
         * When the start of round object wakes up, make the camera controller
         * 
         * @param __instance The calling start of round component
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(StartOfRound), "Awake")]
        public static void MakeCamController(StartOfRound __instance)
        {
            __instance.spectateCamera.gameObject.AddComponent<SpectatorCamController>();
        }

        /////////////////////////////// These set up interactions that the ghost camera can have ///////////////////////////////
        /**
         * Adds a ghost interactor to valid interactibles
         * 
         * @param __instance The calling interact trigger
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(InteractTrigger), "Start")]
        public static void AddGhostInteractor(InteractTrigger __instance)
        {
            //If it's a door, add the interactible
            if(__instance.gameObject.GetComponent<DoorLock>() != null)
                __instance.gameObject.AddComponent<GhostInteractible>();
        }
    }
}
