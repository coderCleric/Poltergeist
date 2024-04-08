using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Poltergeist.GhostInteractibles
{
    public interface IGhostInteractible
    {
        //Get the cost of the interaction
        public float GetCost();

        //Allows interactions with the interactible, returning the cost
        public float Interact(Transform playerTransform);

        //Gives the tip text for this interactible
        public string GetTipText();
    }
}
