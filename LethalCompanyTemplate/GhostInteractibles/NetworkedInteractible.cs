using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace Poltergeist.GhostInteractibles
{
    public abstract class NetworkedInteractible : NetworkBehaviour, IGhostInteractible
    {
        protected float waitTime = 2;
        protected bool wasBugged = false;


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

        /**
         * Allows clients to report issues to the server, for better warning messages
         */
        [ServerRpc(RequireOwnership = false)]
        public void SendWarningServerRpc()
        {
            Poltergeist.LogWarning($"A player is having an issue with interactible {gameObject.name}!");
        }
    }
}
