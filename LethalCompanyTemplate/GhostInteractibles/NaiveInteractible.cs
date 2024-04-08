using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Poltergeist.GhostInteractibles
{
    //Just need this to be a mono class so it can be found via getcomponent
    public abstract class NaiveInteractible : MonoBehaviour, IGhostInteractible
    {
        public abstract float Interact(Transform playerTransform);
        public abstract float GetCost();
        public abstract string GetTipText();
    }
}
