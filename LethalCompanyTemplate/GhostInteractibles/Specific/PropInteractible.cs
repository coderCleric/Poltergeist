using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Poltergeist.GhostInteractibles.Specific
{
    public class PropInteractible : NaiveInteractible
    {
        private GrabbableObject prop;

        /**
         * On awake, grab the prop
         */
        private void Awake()
        {
            prop = GetComponent<GrabbableObject>();
        }

        /**
         * Get the cost of the interaction
         */
        public override float GetCost()
        {
            return 5;
        }

        /**
         * Do the actual interaction
         */
        public override float Interact(Transform playerTransform)
        {
            //Don't let them interact without meeting the cost
            if (SpectatorCamController.instance.Power < GetCost())
                return 0;

            //Make the noise
            Patches.ignoreObj = prop;
            prop.UseItemOnClient();

            return GetCost();
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
    }
}
