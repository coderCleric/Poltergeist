using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Reflection.Emit;
using Unity.Netcode;
using System.Reflection;
using Poltergeist.GhostInteractibles;
using Poltergeist.GhostInteractibles.Specific;

namespace Poltergeist
{
    [HarmonyPatch]
    public static class Patches
    {
        //Config things
        public static bool defaultMode = false;

        //Other fields
        public static bool vanillaMode = false;
        public static GrabbableObject ignoreObj = null;
        public static bool shouldGameOver = false;

        /////////////////////////////// Misc ///////////////////////////////
        /**
         * Patch the spiders to not explode if hit by a null player
         */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SandSpiderAI), nameof(SandSpiderAI.TriggerChaseWithPlayer))]
        public static bool PreventSpiderBug(PlayerControllerB playerScript)
        {
            return playerScript != null;
        }

        /////////////////////////////// Needed to suppress certain base-game systems ///////////////////////////////
        /**
         * Prevents certain manipulations of the spectate camera that would interfere with the controls
         */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.SetSpectateCameraToGameOverMode))]
        public static bool PreventSpectateFollow(bool enableGameOver)
        {
            shouldGameOver = enableGameOver;
            return vanillaMode;
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
            if (!vanillaMode)
                __instance.playersManager.overrideSpectateCamera = true;
        }

        /**
         * If this is the object we're ignoring, ignore it
         * 
         * __instance The calling grabbable
         */
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GrabbableObject), "ActivateItemClientRpc")]
        public static bool SuppressDuplicateHonk(GrabbableObject __instance) { 
            if(__instance == ignoreObj)
            {
                ignoreObj = null;
                return __instance.NetworkManager.IsServer || __instance.NetworkManager.IsHost;
            }
            return true;
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
            if (__instance.gameObject.GetComponent<DoorLock>() != null) {
                BasicInteractible interactible = __instance.gameObject.AddComponent<BasicInteractible>();
                interactible.costType = CostType.DOOR;
                return;
            }

            //If its a lightswitch or a storage locker, add one
            if (__instance.name.Equals("LightSwitch") || (__instance.transform.parent != null && __instance.transform.parent.name.Contains("storage")))
            {
                BasicInteractible interactible = __instance.gameObject.AddComponent<BasicInteractible>();
                return;
            }

            //If it's a ship decoration, add it (can't figure out a better way than checking name)
            Transform parent = __instance.transform.parent;
            if(parent != null)
            {
                if(parent.name.Contains("Pumpkin") || parent.name.Contains("Television") || parent.name.Contains("Record") || parent.name.Contains("Romantic")
                     || parent.name.Contains("Shower") || parent.name.Contains("Toilet") || parent.name.Contains("Plushie"))
                {
                    __instance.gameObject.AddComponent<BasicInteractible>();
                    return;
                }
            }

            //If it's one of the ship buttons, add one
            if(parent != null)
            {
                if(parent.name.Equals("StartButton") || parent.name.Equals("StopButton"))
                {
                    BasicInteractible interactible = __instance.gameObject.AddComponent<BasicInteractible>();
                    interactible.costType = CostType.SHIPDOOR;
                    return;
                }
            }

            //If it's a steam valve, add one
            if(__instance.gameObject.GetComponent<SteamValveHazard>() != null)
            {
                BasicInteractible interactible = __instance.gameObject.AddComponent<BasicInteractible>();
                interactible.costType = CostType.VALVE;
                return;
            }

            //If it's the company bell, add one
            if (parent != null)
            {
                if (parent.name.Equals("BellDinger"))
                {
                    BasicInteractible interactible = __instance.gameObject.AddComponent<BasicInteractible>();
                    interactible.costType = CostType.COMPANYBELL;
                    return;
                }
            }

            //If it's the lever for the big hangar, add one
            if(__instance.name.Contains("LeverSwitchHandle"))
            {
                BasicInteractible interactible = __instance.gameObject.AddComponent<BasicInteractible>();
                interactible.costType = CostType.HANGARDOOR;
                return;
            }
        }

        /**
         * Add ghost interactor objects to airhorns and clownhorns and any other prop
         * 
         * @param __instance The calling noise prop
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(NoisemakerProp), "Start")]
        public static void AddInteractorForHorns(NoisemakerProp __instance)
        {
            PropInteractible interactible = __instance.gameObject.AddComponent<PropInteractible>();
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
            PropInteractible interactible = __instance.gameObject.AddComponent<PropInteractible>();
        }

        /**
         * Add ghost interactor for pneumatic doors
         * 
         * @param __instance The calling script
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(TerminalAccessibleObject), "Start")]
        public static void AddInteractorForBigDoors(TerminalAccessibleObject __instance)
        {
            //Only add if it's a big door
            if(__instance.name.Contains("BigDoor"))
            {
                //Make the gameobject on the door
                GameObject interactObj = new GameObject();
                interactObj.transform.parent = __instance.transform;
                interactObj.transform.localPosition = new Vector3(0, 2.4f, 0);
                interactObj.transform.localEulerAngles = Vector3.zero;
                interactObj.transform.localScale = Vector3.one;
                interactObj.layer = LayerMask.NameToLayer("Ignore Raycast");
                interactObj.name = "GhostInteractable";

                //Make the box collider
                BoxCollider col = interactObj.AddComponent<BoxCollider>();
                col.size = new Vector3(0.7f, 4, 3);
                col.isTrigger = true; //Just need the player to not walk into it

                //Make the ghost interactor
                BigDoorInteractible interactor = interactObj.AddComponent<BigDoorInteractible>();
            }
        }

        /**
         * Add ghost interactor to enemies
         *
        [HarmonyPostfix]
        [HarmonyPatch(typeof(EnemyAI), "Start")]
        public static void AddInteractorForEnemies(EnemyAI __instance)
        {
            Poltergeist.DebugLog("Making interactor for " + __instance.name);
            EnemyAICollisionDetect detector = __instance.GetComponentInChildren<EnemyAICollisionDetect>();
            if (detector != null)
            {
                GhostInteractible interactible = detector.gameObject.AddComponent<GhostInteractible>();
                interactible.cost = 20;
                Poltergeist.DebugLog("Success for " + __instance.name);
            }
        }/


        /////////////////////////////// Transpile grabbed object behaviour to facilitate ground use ///////////////////////////////
        /**
         * Make items usable by any client when not held
         * 
         * @param __instance The calling input action
         * @param __result The resulting value
         */
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.UseItemOnClient))]
        public static IEnumerable<CodeInstruction> AllowGroundUse(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            //First, load the list of instructions
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            //Next, find where we need to insert
            int insertIndex = -1;
            object successLabel = null;
            for (int i = 0; i < code.Count - 1; i++)
            {
                //Count it as a good spot when we recognize the string being loaded
                if (code[i].opcode == OpCodes.Ldstr && ((string)code[i].operand).Equals("Can't use item; not owner"))
                {
                    Poltergeist.DebugLog("Found expected structure for transpiler of UseItemOnClient");

                    //Save the index to insert into
                    insertIndex = i;

                    //Save the label to jump to
                    successLabel = code[i - 1].operand;

                    break;
                }
            }

            //Construct the code to insert (check if the object is held)
            List<CodeInstruction> insertion = new List<CodeInstruction>();
            insertion.Add(new CodeInstruction(OpCodes.Ldarg_0));
            insertion.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(GrabbableObject), nameof(GrabbableObject.isHeld))));
            insertion.Add(new CodeInstruction(OpCodes.Brfalse_S, successLabel));

            //Insert the code
            if (insertIndex != -1)
            {
                code.InsertRange(insertIndex, insertion);
            }

            return code;
        }

        /**
         * Make items on the ground not forbid hearing with ownership
         * 
         * @param __instance The calling input action
         * @param __result The resulting value
         */
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(GrabbableObject), "ActivateItemClientRpc")]
        public static IEnumerable<CodeInstruction> AllowGroundHearing(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            //First, load the list of instructions
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            //Next, find where we need to insert
            int insertIndex = -1;
            object successLabel = null;
            for (int i = 0; i < code.Count - 1; i++)
            {
                //Count it as a good spot when we recognize the string being loaded
                if (code[i].opcode == OpCodes.Call && ((MethodInfo)code[i].operand).Name.Equals("get_IsOwner"))
                {
                    Poltergeist.DebugLog("Found expected structure for transpiler of ActivateItemClientRpc");

                    //Save the index to insert into
                    insertIndex = i - 1;

                    //Save the label to jump to
                    successLabel = code[i + 1].operand;

                    break;
                }
            }

            //Construct the code to insert (check if the object is held)
            List<CodeInstruction> insertion = new List<CodeInstruction>();
            insertion.Add(new CodeInstruction(OpCodes.Ldarg_0));
            insertion.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(GrabbableObject), nameof(GrabbableObject.isHeld))));
            insertion.Add(new CodeInstruction(OpCodes.Brfalse_S, successLabel));

            //Insert the code
            if (insertIndex != -1)
            {
                code.InsertRange(insertIndex, insertion);
            }

            return code;
        }

        /////////////////////////////// Keeping track of masked ///////////////////////////////
        /**
         * When a masked is spawned mimicking a player, register them
         * 
         * @param __instance The calling enemy
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MaskedPlayerEnemy), "Start")]
        public static void RegisterMasked(MaskedPlayerEnemy __instance)
        {
            if(__instance.mimickingPlayer != null)
                SpectatorCamController.masked.Add(__instance);
        }

        /**
         * When a masked is destroyed, remove them from the list
         * 
         * @param __instance The calling enemy
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MaskedPlayerEnemy), "OnDestroy")]
        public static void DeregisterMasked(MaskedPlayerEnemy __instance)
        {
            if(__instance.mimickingPlayer != null)
                SpectatorCamController.masked.Remove(__instance);
        }
    }
}
