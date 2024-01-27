using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Poltergeist
{
    public class GhostInteractible : MonoBehaviour
    {
        private InteractTrigger trigger = null;

        /**
         * When made, grab certain important parts
         */
        private void Awake()
        {
            trigger = GetComponent<InteractTrigger>();
        }

        /**
         * Attempt to interact
         */
        public void Interact(Transform playerTransform)
        {
            if (trigger.interactable)
                trigger.Interact(playerTransform);
        }

        /**
         * Give the text to display for the interaction
         */
        public string GetTipText()
        {
            if (!trigger.interactable)
                return trigger.disabledHoverTip;
            else
            {
                StringBuilder builder = new StringBuilder(trigger.hoverTip);
                return builder.Replace("[LMB]", "[E]").ToString();
            }
        }
    }
}
