using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        public float cost = 10f;

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
        public float Interact(Transform playerTransform)
        {
            float retCost = 0;

            //Don't let them interact without meeting the cost
            if(SpectatorCamController.instance.Power < cost)
                return retCost;

            switch (type)
            {
                //It's some generic interactible
                case GhostInteractType.GENERAL:
                    if (trigger.interactable)
                    {
                        trigger.Interact(playerTransform);
                        retCost = cost;
                    }
                    break;

                //It's some sort of horn
                case GhostInteractType.NOISE_PROP:
                case GhostInteractType.BOOMBOX:
                    MakeNoise();
                    retCost = cost;
                    break;
            }

            return retCost;
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
            //Display message for not having enough power
            if(SpectatorCamController.instance.Power < cost)
                return "Not Enough Power (" + cost.ToString("F0") + ")";

            //When you do have enough power
            string retStr = "Unknown Interaction";
            switch (type)
            {
                //It's some generic interactible
                case GhostInteractType.GENERAL:
                    if (!trigger.interactable)
                        return trigger.disabledHoverTip; //Display no cost if you can't interact
                    else
                    {
                        StringBuilder builder = new StringBuilder(trigger.hoverTip);
                        retStr = builder.Replace("[LMB]", "[E]").ToString();
                    }
                    break;

                //It's some sort of horn
                case GhostInteractType.NOISE_PROP:
                    retStr = "Honk horn : [E]";
                    break;

                //It's the boombox
                case GhostInteractType.BOOMBOX:
                    retStr = "Toggle music : [E]";
                    break;
            }

            return retStr + " (" + cost.ToString("F0") + ")";
        }
    }
}
