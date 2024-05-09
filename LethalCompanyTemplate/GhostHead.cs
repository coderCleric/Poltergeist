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

        //Poor man's animation curve
        private const int KEYFRAMES = 6;
        private static float[] keyTimes = { 0, 1f, 1.4f, 1.8f, 2.2f, 3 };
        private static float[] visibilities = { 0, 1, 1, 0.5f, 1f, 0 };
        private int keyIndex = 999;
        private float startTime = 0;

        //Bounds for the animation
        private Material matInstance = null;
        private float maxOpacity = 1;
        private float maxIntensity = 1;

        /**
         * On awake, grab the renderer and light
         */
        private void Awake ()
        {
            light = GetComponentInChildren<Light>();
            renderer = GetComponentInChildren<Renderer>();
            matInstance = renderer.material;
            maxOpacity = matInstance.color.a;
            maxIntensity = light.intensity;
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
            if(keyIndex < KEYFRAMES)
            {
                //Check if we need to move to the next frame
                if (Time.time >= startTime + keyTimes[keyIndex])
                    keyIndex++;

                //If we're at the end, disable the light and make us invisible to the living
                if(keyIndex ==  KEYFRAMES)
                {
                    light.enabled = false;
                    renderer.gameObject.layer = 23;
                    light.intensity = maxIntensity;
                    matInstance.color = new Color(matInstance.color.r, matInstance.color.g, matInstance.color.b, maxOpacity);
                }

                //Otherwise, actually do the animation
                else
                {
                    //Determine where we are in this keyframe
                    float duration = keyTimes[keyIndex] - keyTimes[keyIndex - 1];
                    float timeInFrame = Time.time - (startTime + keyTimes[keyIndex - 1]);
                    float progress = timeInFrame / duration;

                    //Interpolate
                    Color matCol = matInstance.color;
                    matInstance.color = new Color(matCol.r, matCol.g, matCol.b, 
                        Mathf.Lerp(visibilities[keyIndex - 1] * maxOpacity, visibilities[keyIndex] * maxOpacity, progress));
                    light.intensity = Mathf.Lerp(visibilities[keyIndex - 1] * maxIntensity, visibilities[keyIndex] * maxIntensity, progress);
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
            if (keyIndex < KEYFRAMES)
                return false;

            //Start the animation
            keyIndex = 1;
            startTime = Time.time;
            light.enabled = true;
            renderer.gameObject.layer = 0;

            return true;
        }
    }
}
