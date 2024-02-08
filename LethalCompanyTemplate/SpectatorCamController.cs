using GameNetcodeStuff;
using System;
using System.Collections.Generic;
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

        private PlayerControllerB clientPlayer = null;
        public PlayerControllerB ClientPlayer => clientPlayer;
        private GhostInteractible currentGhostInteractible = null;
        private Transform hintPanelRoot = null;
        private Transform hintPanelOrigParent = null;
        private Transform deathUIRoot = null;
        private float accelTime = -1;
        private float decelTime = -1;

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
                transform.parent = null;

                //Move the camera
                Transform oldCam = StartOfRound.Instance.activeCamera.transform;
                transform.position = oldCam.position;
                transform.rotation = oldCam.rotation;

                //If we don't have them, need to grab certain objects
                if (hintPanelRoot == null)
                {
                    hintPanelRoot = HUDManager.Instance.tipsPanelAnimator.transform.parent;
                    hintPanelOrigParent = hintPanelRoot.parent;
                    deathUIRoot = HUDManager.Instance.SpectateBoxesContainer.transform.parent;
                }

                //Move the hint panel to the death UI
                hintPanelRoot.parent = deathUIRoot;
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

                //If these aren't null, we moved them and need to put them back
                if(hintPanelRoot != null)
                {
                    hintPanelRoot.parent = hintPanelOrigParent;
                }
            }
        }

        /**
         * When the left mouse is clicked, switch the light
         */
        private void SwitchLight(InputAction.CallbackContext context)
        {
            //Cancel if this isn't a "performed" action
            if (!context.performed)
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
            if(player == clientPlayer)
            {
                //tipMethod.Invoke(HUDManager.Instance, new object[] {"Specified player is you!"});
                HUDManager.Instance.DisplayTip("Can't Teleport", "Specified player is you!", true);
                return;
            }
            if(player.isPlayerDead)
            {
                //tipMethod.Invoke(HUDManager.Instance, new object[] { "Specified player is dead!" });
                HUDManager.Instance.DisplayTip("Can't Teleport", "Specified player is dead!", true);
                return;
            }
            if(!player.isPlayerControlled)
            {
                //tipMethod.Invoke(HUDManager.Instance, new object[] { "Specified player is not connected!" });
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
            if (!context.performed)
            {
                return;
            }

            //If not null, use the interactible
            if (currentGhostInteractible != null)
            {
                Poltergeist.DebugLog("Attempting to use interactible");
                currentGhostInteractible.Interact(clientPlayer.transform);
            }
            else
                Poltergeist.DebugLog("No interactible found");
        }

        /**
         * When scrolling is done, set the camera up to change speed
         */
        private void HandleScroll(InputAction.CallbackContext context)
        {
            if(context.ReadValue<float>() > 0)
            {
                accelTime = Time.time + 0.3f;
                decelTime = -1;
            }
            else
            {

                decelTime = Time.time + 0.3f;
                accelTime = -1;
            }
        }

        /**
         * Add and remove the switch light listener as needed
         */
        private void OnEnable()
        {
            IngamePlayerSettings.Instance.playerInput.actions.FindAction("ActivateItem").performed += SwitchLight;
            IngamePlayerSettings.Instance.playerInput.actions.FindAction("Interact").performed += DoInteract;
            IngamePlayerSettings.Instance.playerInput.actions.FindAction("SwitchItem").performed += HandleScroll;
        }
        private void OnDisable()
        {
            IngamePlayerSettings.Instance.playerInput.actions.FindAction("ActivateItem").performed -= SwitchLight;
            IngamePlayerSettings.Instance.playerInput.actions.FindAction("Interact").performed -= DoInteract;
            IngamePlayerSettings.Instance.playerInput.actions.FindAction("SwitchItem").performed -= HandleScroll;
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

            //If the player is in the menu, don't do update stuff
            if (clientPlayer.isTypingChat || clientPlayer.quickMenuManager.isMenuOpen)
                return;

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
