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

        //Components
        private Light light = null;
        private Renderer renderer = null;

        //Helps the animation
        private static float[] switchtimes = { 0, 1f, 1.4f, 1.8f, 2.2f, 3 };
        private float startTime = 0;
        private const int SWITCHLEN = 6; 
        private int switchIndex = 999;
        private bool visibleToPlayers = false;

        /**
         * On awake, grab the renderer and light
         */
        private void Awake ()
        {
            light = GetComponentInChildren<Light>();
            renderer = GetComponentInChildren<Renderer>();
        }

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
                renderer.enabled = false;
                Poltergeist.DebugLog("Assigning head to local client");
            }

            //Check the status of the flicker animation
            if(switchIndex < SWITCHLEN)
            {
                //Check if the current step is passed
                if(Time.time >= startTime + switchtimes[switchIndex])
                {
                    switchIndex++;
                    visibleToPlayers = !visibleToPlayers;
                    if(visibleToPlayers) //Make it visible to players
                    {
                        light.enabled = true;
                        renderer.gameObject.layer = 0;
                    }
                    else //Make it invisible
                    {
                        light.enabled = false;
                        renderer.gameObject.layer = 23;
                    }
                }
            }
        }

        /**
         * Deactivate the given head after client dc's
         */
        public void Deactivate()
        {
            transform.position = StartOfRound.Instance.notSpawnedPosition.position;
        }

        /**
         * Lets the server message clients about the head flickering
         */
        [ClientRpc]
        public void ManifestClientRpc()
        {
            PlayFlickerAnim();
        }

        /**
         * Lets the client tell the server to flicker the head
         */
        [ServerRpc]
        public void ManifestServerRpc()
        {
            ManifestClientRpc();
        }

        /**
         * Plays the flicker animation
         */
        public bool PlayFlickerAnim()
        {
            //Early return if already playing
            if (switchIndex < SWITCHLEN)
                return false;

            switchIndex = 0;
            startTime = Time.time;
            visibleToPlayers = false;

            return true;
        }
    }
}
