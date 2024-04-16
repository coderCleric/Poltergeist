using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;
using static MonoMod.Cil.RuntimeILReferenceBag.FastDelegateInvokers;

namespace Poltergeist.GhostInteractibles.Specific
{
    public class BigDoorInteractible : NaiveInteractible, IGhostOnlyInteractible
    {
        private TerminalAccessibleObject bigDoorObj;

        /**
         * On awake, grab the prop
         */
        private void Awake()
        {
            bigDoorObj = transform.parent.gameObject.GetComponent<TerminalAccessibleObject>();
            IGhostOnlyInteractible.Register(this);
        }

        /**
         * Get the cost of the interaction
         */
        public override float GetCost()
        {
            return Poltergeist.Config.BigDoorCost.Value;
        }

        /**
         * Do the actual interaction
         */
        public override float Interact(Transform playerTransform)
        {
            //Don't let them interact without meeting the cost
            if (SpectatorCamController.instance.Power < GetCost())
                return 0;

            //Toggle the door
            //Why is this private, let me see!
            bool powered = (bool)typeof(TerminalAccessibleObject).GetField("isPoweredOn", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(bigDoorObj);
            if (powered)
            {
                bigDoorObj.SetDoorToggleLocalClient();
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
            //Why is this private, let me see!
            bool powered = (bool)typeof(TerminalAccessibleObject).GetField("isPoweredOn", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(bigDoorObj);
            if (powered)
                retStr = "Toggle door : [E]";
            else
                return "Door is unpowered";

            return retStr + " (" + GetCost().ToString("F0") + ")";
        }

        /**
         * Disable or enable the interactor
         */
        public void SetActivation(bool activation)
        {
            //Use activation to set the collider layer
            if (activation)
                gameObject.layer = LayerMask.NameToLayer("InteractableObject");
            else
                gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        }

        /**
         * On destroy, unregister
         */
        private void OnDestroy()
        {
            IGhostOnlyInteractible.Unregister(this);
        }
    }
}
