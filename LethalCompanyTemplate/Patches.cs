using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Reflection.Emit;
using Unity.Netcode;

namespace Poltergeist
{
    [HarmonyPatch]
    public static class Patches
    {
        public static bool doGhostGrab = false;

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

            //If it's the lightswitch, add one there too
            if(__instance.name.Equals("LightSwitch"))
                __instance.gameObject.AddComponent<GhostInteractible>();
        }

        /**
         * Add ghost interactor objects to airhorns and clownhorns
         * 
         * @param __instance The calling noise prop
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(NoisemakerProp), "Start")]
        public static void AddInteractorForHorns(NoisemakerProp __instance)
        {
            if(__instance.name.Contains("Airhorn") || __instance.name.Contains("Clownhorn"))
            {
                __instance.gameObject.AddComponent<GhostInteractible>();
            }
        }

        /**
         * Add ghost interactor objects to boomboxes
         * 
         * @param __instance The calling boombox
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(BoomboxItem), "Start")]
        public static void AddInteractorForBoombox(BoomboxItem __instance)
        {
            __instance.gameObject.AddComponent<GhostInteractible>();
        }


        /////////////////////////////// Make it so horns can be used by any client when on the ground ///////////////////////////////
        /**
         * Make items usable by any client when not held
         * 
         * @param __instance The calling input action
         * @param __result The resulting value
         */
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(PlayerControllerB), "GrabObjectServerRpc")]
        public static IEnumerable<CodeInstruction> AllowGroundUse(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            //First, load the list of instructions
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            //Next, find where we need to insert
            int insertIndex = -1;
            for (int i = 0; i < code.Count - 1; i++)
            {
                //Count it as a good spot when we recognize the first check of "flag"
                if (code[i].opcode == OpCodes.Ldloc_0)
                {
                    Poltergeist.DebugLog("Found expected structure for transpiler");

                    //Save the index to insert into
                    insertIndex = i;

                    break;
                }
            }

            //Make the labels that we'll need
            Label endLabel = il.DefineLabel();
            Label retLabel = il.DefineLabel();

            //Construct the code to insert
            code[insertIndex].opcode = OpCodes.Ldsfld;
            code[insertIndex].operand = AccessTools.Field(typeof(Patches), nameof(Patches.doGhostGrab));
            List<CodeInstruction> insertion = new List<CodeInstruction>();
            insertion.Add(new CodeInstruction(OpCodes.Brtrue_S, endLabel));
            insertion.Add(new CodeInstruction(OpCodes.Ldloc_0));

            //Insert the code
            if (insertIndex != -1)
            {
                code.InsertRange(insertIndex + 1, insertion);
            }

            //Add the last bit
            CodeInstruction tmp = new CodeInstruction(OpCodes.Ldc_I4_0);
            tmp.labels.Add(endLabel);
            code.Add(tmp);
            code.Add(new CodeInstruction(OpCodes.Stsfld, AccessTools.Field(typeof(Patches), nameof(Patches.doGhostGrab))));
            code.Add(new CodeInstruction(OpCodes.Ldloc_0));
            code.Add(new CodeInstruction(OpCodes.Brfalse, retLabel));

            //This changes the ownership
            code.Add(new CodeInstruction(OpCodes.Ldloc_1));
            code.Add(new CodeInstruction(OpCodes.Ldarg_0));
            code.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PlayerControllerB), nameof(PlayerControllerB.actualClientId))));
            code.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(NetworkObject), nameof(NetworkObject.ChangeOwnership))));

            //Finally, return
            tmp = new CodeInstruction(OpCodes.Ret);
            tmp.labels.Add(retLabel);
            code.Add(tmp);

            return code;
        }
    }
}
