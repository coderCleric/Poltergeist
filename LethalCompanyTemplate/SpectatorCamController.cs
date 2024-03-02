using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Reflection;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.HighDefinition;

namespace Poltergeist
{
    public class SpectatorCamController : MonoBehaviour
    {
        //Config things
        public static float lightIntensity = 5;

        //Other fields
        public static SpectatorCamController instance = null;

        private float camMoveSpeed = 5f;
        private Light light = null;

        private float maxPower = 100;
        private float powerRecover = 5;
        private float power = 0;
        public float Power => power;

        private PlayerControllerB clientPlayer = null;
        public PlayerControllerB ClientPlayer => clientPlayer;
        private GhostInteractible currentGhostInteractible = null;
        private Transform hintPanelRoot = null;
        private Transform hintPanelOrigParent = null;
        private Transform deathUIRoot = null;
        private float accelTime = -1;
        private float decelTime = -1;
        public static List<MaskedPlayerEnemy> masked = new List<MaskedPlayerEnemy>();

        /**
         * On awake, make and grab the light
         */
        private void Awake()
        {
            instance = this;
            GameObject lightObj = new GameObject("GhostLight");
            light = lightObj.AddComponent<Light>();
            lightObj.AddComponent<HDAdditionalLightData>();
            lightObj.transform.eulerAngles = new Vector3 (90f, 0f, 0f);
            light.type = LightType.Directional;
            light.shadows = LightShadows.None;
            light.intensity = lightIntensity;

            DisableCam();
        }

        /**
         * Enables the spectator camera
         */
        public void EnableCam()
        {
            if (!enabled)
            {
                enabled = true;

                //Move the camera
                if (!Patches.vanillaMode)
                {
                    transform.parent = null;
                    Transform oldCam = StartOfRound.Instance.activeCamera.transform;
                    transform.position = oldCam.position;
                    transform.rotation = oldCam.rotation;
                }

                //If we don't have them, need to grab certain objects
                if (hintPanelRoot == null)
                {
                    hintPanelRoot = HUDManager.Instance.tipsPanelAnimator.transform.parent;
                    hintPanelOrigParent = hintPanelRoot.parent;
                    deathUIRoot = HUDManager.Instance.SpectateBoxesContainer.transform.parent;
                }

                //Move the hint panel to the death UI
                hintPanelRoot.parent = deathUIRoot;

                //Zero the power
                power = 0;

                //Enable ghost-only interactables
                GhostInteractible.SetGhostActivation(true);
            }
        }

        /**
         * Disables the spectator camera
         */
        public void DisableCam()
        {
            if (enabled)
            {
                //Basics
                enabled = false;
                light.enabled = false;
                Patches.vanillaMode = Patches.defaultMode;

                //If these aren't null, we moved them and need to put them back
                if (hintPanelRoot != null)
                {
                    hintPanelRoot.parent = hintPanelOrigParent;
                }

                //Disable ghost-only interactables
                GhostInteractible.SetGhostActivation(false);
            }
        }

        /**
         * When the left mouse is clicked, switch the light
         */
        private void SwitchLight(InputAction.CallbackContext context)
        {
            //Cancel if this isn't a "performed" action
            if (!context.performed || Patches.vanillaMode)
            {
                return;
            }

            //If in the right conditions, switch the light
            if (clientPlayer.isPlayerDead && !clientPlayer.isTypingChat && !clientPlayer.quickMenuManager.isMenuOpen)
            {
                light.enabled = !light.enabled;
            }
        }

        /**
         * Attempts to teleport to the specified player
         */
        public void TeleportToPlayer(PlayerControllerB player)
        {
            //Display errors depending on circumstance
            MethodInfo tipMethod = HUDManager.Instance.GetType().GetMethod("DisplaySpectatorTip", BindingFlags.NonPublic | BindingFlags.Instance);
            
            //Player is dead, check for body/masked
            if(player.isPlayerDead)
            {
                //If some registered masked is mimicking this player, go there
                MaskedPlayerEnemy targetMasked = null;
                foreach (MaskedPlayerEnemy enemy in masked)
                {
                    if (enemy.mimickingPlayer == player)
                    {
                        targetMasked = enemy;
                        break;
                    }
                }
                if (targetMasked != null)
                {
                    //If alive, move to the face
                    if(!targetMasked.isEnemyDead)
                    {
                        Transform target = targetMasked.transform.Find("ScavengerModel/metarig/spine/spine.001/spine.002/spine.003/spine.004");
                        transform.position = target.position + (target.up * 0.2f);
                        transform.eulerAngles = new Vector3(target.eulerAngles.x, target.eulerAngles.y, 0);
                    }

                    //Otherwise, go over the body
                    else
                    {
                        transform.position = targetMasked.transform.position + Vector3.up;
                    }
                }

                //If the player has a corpse, move to it
                else if (player.deadBody != null && !player.deadBody.deactivated)
                {
                    //Move to the corpse
                    transform.position = player.deadBody.transform.position + Vector3.up;
                }

                //No corpse or masked, can't do anything
                else
                    HUDManager.Instance.DisplayTip("Can't Teleport", "Specified player is dead with no body!", true);
                return;
            }

            //Player is not connected, can't teleport
            if(!player.isPlayerControlled)
            {
                HUDManager.Instance.DisplayTip("Can't Teleport", "Specified player is not connected!", true);
                return;
            }

            //Otherwise, move the camera to that player
            transform.position = player.gameplayCamera.transform.position;
            transform.rotation = player.gameplayCamera.transform.rotation;

            //Apply the effects
            clientPlayer.spectatedPlayerScript = player;
            clientPlayer.SetSpectatedPlayerEffects(false);
        }

        /**
         * When the interact key is pressed, try to use the current ghost interactible
         */
        private void DoInteract(InputAction.CallbackContext context)
        {
            //Cancel if this isn't a "performed" action
            if (!context.performed || Patches.vanillaMode)
            {
                return;
            }

            //If not null, use the interactible
            if (currentGhostInteractible != null)
            {
                Poltergeist.DebugLog("Attempting to use interactible");
                power -= currentGhostInteractible.Interact(clientPlayer.transform);
            }
            else
                Poltergeist.DebugLog("No interactible found");
        }

        /**
         * When scrolling up is done, set the camera up to change speed
         */
        private void Accelerate(InputAction.CallbackContext context)
        {
            if (Patches.vanillaMode)
                return;
            accelTime = Time.time + 0.3f;
            decelTime = -1;
        }

        /**
         * When scrolling down is done, set the camera up to change speed
         */
        private void Decelerate(InputAction.CallbackContext context)
        {
            if (Patches.vanillaMode)
                return;
            decelTime = Time.time + 0.3f;
            accelTime = -1;
        }

        /**
         * Switches modes between the vanilla and modded spectate
         */
        private void SwitchModes(InputAction.CallbackContext context)
        {
            //Only do it if performing
            if (!context.performed)
                return;

            //Change the flag
            Patches.vanillaMode = !Patches.vanillaMode;

            //Handle switching to vanilla
            if(Patches.vanillaMode)
            {
                light.enabled = false;
                clientPlayer.spectatedPlayerScript = null;
                currentGhostInteractible = null;
                clientPlayer.cursorTip.text = "";
                StartOfRound.Instance.SetSpectateCameraToGameOverMode(Patches.shouldGameOver, clientPlayer);
            }

            //Handle switching to modded
            else
            {
                transform.parent = null;
            }
        }

        /**
         * Add and remove the different control listeners as needed
         */
        private void OnEnable()
        {
            PoltergeistCustomInputs.instance.SwitchLightButton.performed += SwitchLight;
            IngamePlayerSettings.Instance.playerInput.actions.FindAction("Interact").performed += DoInteract;
            PoltergeistCustomInputs.instance.AccelerateButton.performed += Accelerate;
            PoltergeistCustomInputs.instance.DecelerateButton.performed += Decelerate;
            PoltergeistCustomInputs.instance.ToggleButton.performed += SwitchModes;
        }
        private void OnDisable()
        {
            PoltergeistCustomInputs.instance.SwitchLightButton.performed -= SwitchLight;
            IngamePlayerSettings.Instance.playerInput.actions.FindAction("Interact").performed -= DoInteract;
            PoltergeistCustomInputs.instance.AccelerateButton.performed -= Accelerate;
            PoltergeistCustomInputs.instance.DecelerateButton.performed -= Decelerate;
            PoltergeistCustomInputs.instance.ToggleButton.performed -= SwitchModes;
        }

        /**
         * Just before rendering, handle camera input
         */
        private void LateUpdate ()
        {
            //Need to wait for the player controller to be registered
            if(clientPlayer == null)
            {
                clientPlayer = StartOfRound.Instance.localPlayerController;
                if (clientPlayer == null)
                    return;
            }

            //Calculate the max power based on # of connected players dead
            float connected = -1; //Negative 1 because we want max power at 1 living player
            float dead = 0;
            foreach(PlayerControllerB player in StartOfRound.Instance.allPlayerScripts) //First, count them
            {
                if (player.isPlayerDead)
                {
                    connected++;
                    dead++;
                }
                else if (player.isPlayerControlled)
                    connected++;
            }
            dead = Mathf.Min(dead, connected); //Make sure we don't go above 100 power
            if (connected == 0) //Edge case for only 1 player
                maxPower = 100f;
            else
                maxPower = (dead / connected) * 100f;

            //If dead, player should always be gaining power
            power = Mathf.Min(maxPower, power + (powerRecover * Time.deltaTime));

            //If the player is in the menu (or we're in vanilla mode), don't do update stuff
            if (clientPlayer.isTypingChat || clientPlayer.quickMenuManager.isMenuOpen || Patches.vanillaMode)
            {
                currentGhostInteractible = null;
                return;
            }

            //Take raw inputs
            Vector2 moveInput = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Move").ReadValue<Vector2>();
            Vector2 lookInput = clientPlayer.playerActions.Movement.Look.ReadValue<Vector2>() * 0.008f * IngamePlayerSettings.Instance.settings.lookSensitivity;
            if (!IngamePlayerSettings.Instance.settings.invertYAxis)
            {
                lookInput.y *= -1f;
            }
            bool sprint = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Sprint").ReadValue<float>() > 0.3f;

            //Rotate the camera
            transform.Rotate(0, lookInput.x, 0, Space.World);

            //Need to correct the rotation to not allow looking too high or low
            float newX = (transform.eulerAngles.x % 360) + lookInput.y;
            if (newX < 270 && newX > 90)
            {
                if (270 - newX < newX - 90)
                    transform.eulerAngles = new Vector3(270, transform.eulerAngles.y, 0);
                else
                    transform.eulerAngles = new Vector3(90, transform.eulerAngles.y, 0);
            }
            else
                transform.eulerAngles = new Vector3(newX, transform.eulerAngles.y, 0);

            //Move the camera
            float curMoveSpeed = camMoveSpeed;
            if (sprint)
                curMoveSpeed *= 5;
            Vector3 rightMove = transform.transform.right * moveInput.x * curMoveSpeed * Time.deltaTime;
            Vector3 forwardMove = transform.transform.forward * moveInput.y * curMoveSpeed * Time.deltaTime;
            transform.position += rightMove + forwardMove;

            //Actually do the speed change
            if(accelTime > Time.time)
            {
                camMoveSpeed += Time.deltaTime * camMoveSpeed;
                camMoveSpeed = Mathf.Clamp(camMoveSpeed, 0, 100);
            }
            else if(decelTime > Time.time)
            {
                camMoveSpeed -= Time.deltaTime * camMoveSpeed;
                camMoveSpeed = Mathf.Clamp(camMoveSpeed, 0, 100);
            }

            //Display the current power
            HUDManager.Instance.spectatingPlayerText.text = "Power: " + power.ToString("F0") + " / " + maxPower.ToString("F0");

            //Lets the player teleport to other players
            int teleIndex = -1;
            for(Key i = Key.Digit1; i <= Key.Digit0; i++)
            {
                if (Keyboard.current[i].wasPressedThisFrame) 
                {
                    teleIndex = (i - Key.Digit1);
                    break;
                }
            }
            if(teleIndex != -1)
            {
                PlayerControllerB[] playerList = StartOfRound.Instance.allPlayerScripts;
                if(teleIndex >= playerList.Length)
                    HUDManager.Instance.DisplayTip("Cannot Teleport", "Specified player index is invalid!", isWarning: true);
                else
                    TeleportToPlayer(playerList[teleIndex]);
            }

            //Lets the player detect ghost interactibles
            RaycastHit hit;
            currentGhostInteractible = null;
            clientPlayer.cursorTip.text = "";
            if (Physics.Raycast(transform.position, transform.forward, out hit, 5, 832) && hit.collider.gameObject.layer != 8)
            {
                GhostInteractible ghostInteractible = hit.collider.gameObject.GetComponent<GhostInteractible>();
                if (ghostInteractible != null)
                {
                    currentGhostInteractible = ghostInteractible;
                    clientPlayer.cursorTip.text = ghostInteractible.GetTipText();
                }
            }
        }
    }
}
