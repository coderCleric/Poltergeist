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
        public enum GhostInteractType {GENERAL, NOISE_PROP, BOOMBOX}

        private InteractTrigger trigger = null;
        private NoisemakerProp noiseProp = null;
        private BoomboxItem boombox = null;
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

            else if (GetComponent<BoomboxItem>() != null)
            {
                boombox = GetComponent<BoomboxItem>();
                type = GhostInteractType.BOOMBOX;
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
                case GhostInteractType.BOOMBOX:
                    MakeNoise();
                    break;
            }
        }

        /**
         * Honk the horn
         */
        private void MakeNoise()
        {
            //Grab the network object
            NetworkObject netObj = null;
            if (type == GhostInteractType.NOISE_PROP)
                netObj = noiseProp.NetworkObject;
            else if (type == GhostInteractType.BOOMBOX)
                netObj = boombox.NetworkObject;
            else
                return;

            //Error check
            if (netObj == null || !netObj.IsSpawned)
            {
                Poltergeist.DebugLog("could not remote honk; netobj was bad");
                return;
            }

            //Change the ownership to the ghost
            Patches.doGhostGrab = true;
            MethodInfo method = SpectatorCamController.instance.ClientPlayer.GetType().GetMethod("GrabObjectServerRpc", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(SpectatorCamController.instance.ClientPlayer, new object[] { new NetworkObjectReference(netObj) });

            //Make the noise
            if(type == GhostInteractType.NOISE_PROP)
                noiseProp.UseItemOnClient();
            else if (type == GhostInteractType.BOOMBOX)
                boombox.UseItemOnClient();
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

                //It's the boombox
                case GhostInteractType.BOOMBOX:
                    return "Toggle music : [E]";
            }

            return "Unknown Interaction";
        }
    }
}
