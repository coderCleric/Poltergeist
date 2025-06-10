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
        public Transform intendedParent = null; //Only assigned on the host


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

            //Try to reparent
            //Parent is gone (was probably deleted), kill this thing
            if(intendedParent == null)
            {
                Poltergeist.LogWarning("No parent found, killing interactor!");
                GetComponent<NetworkObject>().Despawn(true);
                return;
            }

            //Parent is there, but doesn't have the NetworkObject or isn't spawned, kill this thing
            NetworkObject parentNO = intendedParent.GetComponent<NetworkObject>();
            if(parentNO == null || !parentNO.IsSpawned)
            {
                Poltergeist.LogWarning("Parent has incorrect network seup, killing interactor!");
                GetComponent<NetworkObject>().Despawn(true);
                return;
            }

            //If the parent looks good, try to reparent to fix the client
            transform.parent = intendedParent;
        }
    }
}
