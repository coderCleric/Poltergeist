using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace Poltergeist.GhostInteractibles
{
    public abstract class NetworkedInteractible : NetworkBehaviour, IGhostInteractible
    {
        private float waitTime = 2;

        public abstract float Interact(Transform playerTransform);
        public abstract float GetCost();
        public abstract string GetTipText();
        protected abstract void DoSetup();

        /**
         * Waits a certain amount of time, then does setup
         */
        private void Update()
        {
            if (waitTime > 0)
            {
                waitTime -= Time.deltaTime;
                if(waitTime <= 0)
                    DoSetup();
            }
        }
    }
}
