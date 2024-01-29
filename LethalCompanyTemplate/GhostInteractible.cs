using HarmonyLib;
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
            //Make the noise and suppress the duplicate
            if (type == GhostInteractType.NOISE_PROP)
            {
                Patches.ignoreObj = noiseProp;
                noiseProp.UseItemOnClient();
            }
            else if (type == GhostInteractType.BOOMBOX)
            {
                Patches.ignoreObj = boombox;
                boombox.UseItemOnClient();
            }
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
