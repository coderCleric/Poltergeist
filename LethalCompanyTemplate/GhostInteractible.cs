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
        public enum GhostInteractType {UNKNOWN, GENERAL, NOISE_PROP, BOOMBOX, BIGDOOR}

        //Needed to facilitate the different types of interaction
        private InteractTrigger trigger = null;
        private NoisemakerProp noiseProp = null;
        private BoomboxItem boombox = null;
        private TerminalAccessibleObject bigDoorObj = null;

        //Fundamental info on the interaction
        private GhostInteractType type = GhostInteractType.UNKNOWN;
        public float cost = 10f;

        //Useful for the global list of items
        private static List<GhostInteractible> managedInteractibles = new List<GhostInteractible>();
        private bool ghostOnly = false;
        private int indexInList = -1;

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

            else if(transform.parent.gameObject.GetComponent<TerminalAccessibleObject>() != null && transform.parent.name.Contains("BigDoor"))
            {
                type = GhostInteractType.BIGDOOR;
                bigDoorObj = transform.parent.gameObject.GetComponent<TerminalAccessibleObject>();
            }
        }

        /**
         * Sets whether or not this is a ghost-only interactible
         */
        public void SetGhostOnly(bool ghostOnly)
        {
            //Don't do anything if it's already set to that
            if(ghostOnly == this.ghostOnly) 
                return;

            this.ghostOnly = ghostOnly;

            if (ghostOnly)
            {
                //Add it to the list
                indexInList = managedInteractibles.Count;
                managedInteractibles.Add(this);
            }
            else //Remove it from the list
                RemoveFromManaged();
        }

        /**
         * Removes this item from the list of managed interactibles quickly
         */
        private void RemoveFromManaged()
        {
            //-1 indicates it's not in the list
            if (indexInList == -1)
                return;

            //Special case for only having 1 element
            if(managedInteractibles.Count == 1)
            {
                managedInteractibles.Clear();
                indexInList = -1;
                return;
            }

            //Override with the last element
            managedInteractibles[indexInList] = managedInteractibles[managedInteractibles.Count - 1];

            //Give that element its new index
            managedInteractibles[indexInList].indexInList = indexInList;

            //Remove the duplicate at the end
            managedInteractibles.RemoveAt(managedInteractibles.Count - 1);
            indexInList = -1;
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

                //It's a big door
                case GhostInteractType.BIGDOOR:
                    //Why is this private, let me see!
                    bool powered = (bool) typeof(TerminalAccessibleObject).GetField("isPoweredOn", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(bigDoorObj);
                    if(powered)
                    {
                        bigDoorObj.SetDoorToggleLocalClient();
                        retCost = cost;
                    }
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
                    retStr = "Make noise : [E]";
                    break;

                //It's the boombox
                case GhostInteractType.BOOMBOX:
                    retStr = "Toggle music : [E]";
                    break;

                //It's a big door
                case GhostInteractType.BIGDOOR:
                    //Why is this private, let me see!
                    bool powered = (bool)typeof(TerminalAccessibleObject).GetField("isPoweredOn", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(bigDoorObj);
                    if (powered)
                        retStr = "Toggle door : [E]";
                    else
                        return "Door is unpowered";
                    break;
            }

            return retStr + " (" + cost.ToString("F0") + ")";
        }

        /**
         * Toggles the raycast colliders on all of the ghost interactibles
         */
        public static void SetGhostActivation(bool active)
        {
            //Loop through each registered interacible
            foreach(GhostInteractible interactible in managedInteractibles)
            {
                //Skip if it has no collider
                if (interactible.gameObject.GetComponent<Collider> == null)
                    continue;

                //Otherwise, use activation to set the collider layer
                if (active)
                    interactible.gameObject.layer = LayerMask.NameToLayer("InteractableObject");
                else
                    interactible.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }
        }

        /**
         * On destroy, remove from the list
         */
        private void OnDestroy()
        {
            RemoveFromManaged();
        }
    }
}
