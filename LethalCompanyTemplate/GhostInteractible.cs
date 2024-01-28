using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace Poltergeist
{
    public class GhostInteractible : MonoBehaviour
    {
        public enum GhostInteractType {GENERAL, NOISE_PROP}

        private InteractTrigger trigger = null;
        private NoisemakerProp noiseProp = null;
        private GhostInteractType type = GhostInteractType.GENERAL;

        /**
         * When made, grab certain important parts
         */
        private void Awake()
        {
            if (GetComponent<InteractTrigger>() != null)
            {
                trigger = GetComponent<InteractTrigger>();
                type = GhostInteractType.GENERAL;
            }

            else if (GetComponent<NoisemakerProp>() != null)
            {
                noiseProp = GetComponent<NoisemakerProp>();
                type = GhostInteractType.NOISE_PROP;
            }
        }

        /**
         * Attempt to interact
         */
        public void Interact(Transform playerTransform)
        {
            switch (type)
            {
                //It's some generic interactible
                case GhostInteractType.GENERAL:
                    if (trigger.interactable)
                        trigger.Interact(playerTransform);
                    break;

                //It's some sort of horn
                case GhostInteractType.NOISE_PROP:
                    HonkHorn();
                    break;
            }
        }

        /**
         * Honk the horn
         */
        private void HonkHorn()
        {
            NetworkObject netObj = noiseProp.NetworkObject;

            //Error check
            if (netObj == null || !netObj.IsSpawned)
            {
                Poltergeist.DebugLog("could not remote honk; netobj was bad");
                return;
            }

            //Actually do stuff
            Patches.doGhostGrab = true;
            MethodInfo method = SpectatorCamController.instance.ClientPlayer.GetType().GetMethod("GrabObjectServerRpc", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(SpectatorCamController.instance.ClientPlayer, new object[] { new NetworkObjectReference(netObj) });
            noiseProp.UseItemOnClient();
        }

        /**
         * Give the text to display for the interaction
         */
        public string GetTipText()
        {
            switch (type)
            {
                //It's some generic interactible
                case GhostInteractType.GENERAL:
                    if (!trigger.interactable)
                        return trigger.disabledHoverTip;
                    else
                    {
                        StringBuilder builder = new StringBuilder(trigger.hoverTip);
                        return builder.Replace("[LMB]", "[E]").ToString();
                    }

                //It's some sort of horn
                case GhostInteractType.NOISE_PROP:
                    return "Honk horn : [E]";
            }

            return "Unknown Interaction";
        }
    }
}
