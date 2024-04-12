﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Poltergeist.GhostInteractibles.Specific
{
    public enum CostType { DOOR, VALVE, SHIPDOOR, COMPANYBELL, HANGARDOOR, MISC}
    public class BasicInteractible : NaiveInteractible
    {
        public CostType costType = CostType.MISC;
        private InteractTrigger trigger;

        /**
         * On awake, grab the trigger
         */
        private void Awake()
        {
            trigger = GetComponent<InteractTrigger>();
        }

        /**
         * Get the cost of the interaction
         */
        public override float GetCost()
        {
            switch(costType)
            {
                case CostType.DOOR:
                    return 10;
                case CostType.VALVE:
                    return 20;
                case CostType.SHIPDOOR:
                    return 30;
                case CostType.COMPANYBELL:
                    return 15;
                case CostType.HANGARDOOR:
                    return 50;
                default: //Should only be misc
                    return 5;
            }
        }

        /**
         * Do the actual interaction
         */
        public override float Interact(Transform playerTransform)
        {
            //Don't let them interact without meeting the cost
            if (SpectatorCamController.instance.Power < GetCost())
                return 0;

            //Check to see if the interactor will let us do it
            if (trigger.interactable && (!trigger.interactCooldown || trigger.currentCooldownValue <= 0))
            {
                trigger.Interact(playerTransform);
                return GetCost();
            }

            return 0;
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
            if (!trigger.interactable)
                return trigger.disabledHoverTip; //Display no cost if you can't interact
            else
            {
                StringBuilder builder = new StringBuilder(trigger.hoverTip);
                retStr = builder.Replace("[LMB]", "[E]").ToString();
            }

            return retStr + " (" + GetCost().ToString("F0") + ")";
        }
    }
}