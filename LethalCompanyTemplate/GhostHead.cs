using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace Poltergeist
{
    public class GhostHead : NetworkBehaviour
    {
        //Mapping between player controllers and head objects, only need to maintain on the host
        public static Dictionary<PlayerControllerB, GhostHead> headMapping = new Dictionary<PlayerControllerB, GhostHead> ();

        private bool initialized = false;
        public bool isActive = false;
        public bool isHostHead = false;

        /**
         * Handle the frame-by-frame things
         */
        private void LateUpdate ()
        {
            //Check if we should initialize
            if(!initialized && SpectatorCamController.instance != null && IsOwner && (!base.IsServer || isHostHead))
            {
                initialized = true;
                SpectatorCamController.instance.head = this;
                Poltergeist.DebugLog("Assigning head to local client");
            }

            //If we aren't initialized, return
            if (!initialized)
                return;

            //If we're active (AKA the player is dead), teleport the ghost head to the spectator cam
            if(isActive)
            {
                transform.position = SpectatorCamController.instance.transform.position;
                transform.rotation = SpectatorCamController.instance.transform.rotation;
            }
        }
    }
}
