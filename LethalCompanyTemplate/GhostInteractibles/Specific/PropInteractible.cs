using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace Poltergeist.GhostInteractibles.Specific
{
    public class PropInteractible : NetworkedInteractible
    {
        private GrabbableObject prop;

        /**
         * On start, grab the prop
         */
        private void Start()
        {
            prop = transform.parent.GetComponent<GrabbableObject>();
        }

        /**
         * Get the cost of the interaction
         */
        public override float GetCost()
        {
            return 5;
        }

        /**
         * Do the actual interaction with networked stuff
         */
        public override float Interact(Transform playerTransform)
        {
            //Don't let them interact without meeting the cost
            if (SpectatorCamController.instance.Power < GetCost())
                return 0;

            //Don't let them interact if the item is restricting it
            if(prop.RequireCooldown() || !prop.UseItemBatteries(!prop.itemProperties.holdButtonUse))
                return 0;

            //Make the noise
            Patches.ignoreObj = prop;
            InteractServerRpc((int)SpectatorCamController.instance.ClientPlayer.playerClientId, prop.isBeingUsed);
            InteractLocallyOnly();

            return GetCost();
        }

        /**
         * Do only the local interaction stuff
         */
        public void InteractLocallyOnly()
        {
            Poltergeist.DebugLog("Interacting locally with " + prop.gameObject.name);
            prop.ItemActivate(prop.isBeingUsed);
        }

        /**
         * Gives the tip text
         */
        public override string GetTipText()
        {
            string retStr = "";

            //Display message for not having enough power
            if (SpectatorCamController.instance.Power < GetCost())
                return "Not Enough Power (" + GetCost().ToString("F0") + ")";

            //Set up the actual text
            retStr = "Make noise : [E]";

            return retStr + " (" + GetCost().ToString("F0") + ")";
        }

        /**
         * Lets the server message clients about an item being used by a ghost
         */
        [ClientRpc]
        public void InteractClientRpc(int playerID, bool isBeingUsed)
        {
            //Do nothing if we're the originator
            if (SpectatorCamController.instance.ClientPlayer != null && playerID == (int)SpectatorCamController.instance.ClientPlayer.playerClientId)
                return;

            //Just need to use the item locally
            prop.isBeingUsed = isBeingUsed;
            InteractLocallyOnly();
        }

        /**
         * Lets the client tell the server we're activating it
         */
        [ServerRpc(RequireOwnership = false)]
        public void InteractServerRpc(int playerID, bool isBeingUsed)
        {
            InteractClientRpc(playerID, isBeingUsed);
        }
    }
}
