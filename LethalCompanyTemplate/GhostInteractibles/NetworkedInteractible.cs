using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace Poltergeist.GhostInteractibles
{
    public abstract class NetworkedInteractible : NetworkBehaviour, IGhostInteractible
    {
        public abstract float Interact(Transform playerTransform);
        public abstract float GetCost();
        public abstract string GetTipText();
    }
}
