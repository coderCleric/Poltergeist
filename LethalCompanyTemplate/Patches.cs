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
        //Other fields
        public static bool vanillaMode = false;
        public static GrabbableObject ignoreObj = null;
        public static bool shouldGameOver = false;
        public static bool camControllerActive = false;

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

        /**
         * Transpile to make ship monitors viewable as a ghost
         */
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ManualCameraRenderer), "MeetsCameraEnabledConditions")]
        public static IEnumerable<CodeInstruction> MonitorDontCheckPlayerLocation(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            //First, load the list of instructions
            List<CodeInstruction> code = new List<CodeInstruction>(instructions);

            //Find where we want to be inserting
            int insertIndex = -1;
            Label afterLabel = il.DefineLabel();
            for(int i = 0; i < code.Count; i++)
            {
                //Look for the "in hangar" check
                if (code[i].opcode == OpCodes.Ldfld && (FieldInfo)code[i].operand == AccessTools.Field(typeof(PlayerControllerB), nameof(PlayerControllerB.isInHangarShipRoom)))
                {
                    Poltergeist.DebugLog("Found expected structure for monitor transpiler!");
                    insertIndex = i - 1; //Actual insert is 1 line before
                    code[i + 2].labels.Add(afterLabel);
                    break;
                }
            }

            //Make the new code to inject
            List<CodeInstruction> insertion = new List<CodeInstruction>();
            insertion.Add(new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(Patches), nameof(Patches.camControllerActive))));
            insertion.Add(new CodeInstruction(OpCodes.Brtrue_S, afterLabel));

            //Insert the code
            if (insertIndex != -1)
            {
                code.InsertRange(insertIndex, insertion);
            }

            return code;
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
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                GameObject interactObject = GameObject.Instantiate(Poltergeist.propInteractibleObject, __instance.transform);
                interactObject.GetComponent<NetworkObject>().Spawn();
                interactObject.transform.parent = __instance.transform;
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
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                GameObject interactObject = GameObject.Instantiate(Poltergeist.propInteractibleObject, __instance.transform);
                interactObject.GetComponent<NetworkObject>().Spawn();
                interactObject.transform.parent = __instance.transform;
            }
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
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(EnemyAI), "Start")]
        [HarmonyPatch(typeof(MaskedPlayerEnemy), "Start")]
        public static void AddInteractorForEnemies(EnemyAI __instance)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                Poltergeist.DebugLog("Making interactor for " + __instance.name);
                GameObject interactObject = GameObject.Instantiate(Poltergeist.enemyInteractibleObject, __instance.transform);
                interactObject.GetComponent<NetworkObject>().Spawn();
                interactObject.transform.parent = __instance.transform;
            }
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

        /////////////////////////////// Networking garbage ///////////////////////////////
        /**
         * Load any network prefabs
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameNetworkManager), "Start")]
        public static void LoadNetworkPrefabs()
        {
            //Only once
            if (Poltergeist.propInteractibleObject != null)
                return;

            //Actually load things
            Poltergeist.propInteractibleObject = Poltergeist.poltergeistAssetBundle.LoadAsset<GameObject>("Assets/Prefabs/PropInteractible.prefab");
            Poltergeist.propInteractibleObject.AddComponent<PropInteractible>();
            Poltergeist.enemyInteractibleObject = Poltergeist.poltergeistAssetBundle.LoadAsset<GameObject>("Assets/Prefabs/EnemyInteractible.prefab");
            Poltergeist.enemyInteractibleObject.AddComponent<EnemyInteractible>();
            Poltergeist.ghostHeadObject = Poltergeist.poltergeistAssetBundle.LoadAsset<GameObject>("Assets/Prefabs/ghosthead.prefab");
            Poltergeist.ghostHeadObject.AddComponent<HeadTransform>();
            Poltergeist.ghostHeadObject.AddComponent<GhostHead>();

            //Register the prefabs
            NetworkManager.Singleton.AddNetworkPrefab(Poltergeist.propInteractibleObject);
            NetworkManager.Singleton.AddNetworkPrefab(Poltergeist.enemyInteractibleObject);
            NetworkManager.Singleton.AddNetworkPrefab(Poltergeist.ghostHeadObject);
        }

        /////////////////////////////// Needed for the ghost heads ///////////////////////////////
        /**
         * Make all of the player heads
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), "Awake")]
        public static void MakeGhostHeads(PlayerControllerB __instance)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) //Only do this if we're the server
            {
                //Make the head and add it to the mapping
                GameObject madeHead = GameObject.Instantiate(Poltergeist.ghostHeadObject);
                GhostHead headScript = madeHead.GetComponent<GhostHead>();
                madeHead.transform.position = __instance.playersManager.notSpawnedPosition.position;
                GhostHead.headMapping.Add(__instance, headScript);
                madeHead.GetComponent<NetworkObject>().Spawn();

                //Check if this is the host head
                headScript.isHostHead = __instance.gameObject == __instance.playersManager.allPlayerObjects[0];
            }
        }

        /**
         * When a new player joins, give them a head
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(StartOfRound), "OnClientConnect")]
        public static void AssignPlayerHead(StartOfRound __instance, ulong clientId)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) //Only do this if we're the server
            {
                //Figure out the gameobject number for the connecting player
                int objectIndex = __instance.ClientPlayerList[clientId];

                //Figure out the correct head for that player
                GhostHead head = GhostHead.headMapping[__instance.allPlayerScripts[objectIndex]];

                //Change the ownership of the head
                head.GetComponent<NetworkObject>().ChangeOwnership(clientId);
            }
        }

        /**
         * When we dc, make sure we clear the dict
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameNetworkManager), "Disconnect")]
        public static void ClearHeadDict()
        {
            GhostHead.headMapping.Clear();
            Poltergeist.DebugLog("Cleared dict after local dc");
        }

        /**
         * When another player dc's, make sure we remove them from the dict
         */
        [HarmonyPostfix]
        [HarmonyPatch(typeof(StartOfRound), "OnPlayerDC")]
        public static void HandleHeadOnDC(StartOfRound __instance, int playerObjectNumber)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) //Only do this if we're the server
            {
                PlayerControllerB playerController = __instance.allPlayerScripts[playerObjectNumber];
                GhostHead head = GhostHead.headMapping[playerController];

                Poltergeist.DebugLog("Moving head after client dc: " + head.IsOwner);
                head.Deactivate();
            }
        }
    }
}
