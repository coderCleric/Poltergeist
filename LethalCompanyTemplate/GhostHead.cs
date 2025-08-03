using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Poltergeist
{
    public class GhostHead : NetworkBehaviour
    {
        //Mapping between player controllers and head objects, only need to maintain on the host
        public static Dictionary<PlayerControllerB, GhostHead> headMapping = new Dictionary<PlayerControllerB, GhostHead> ();

        private bool initialized = false;
        public bool isActive = false;
        public bool isHostHead = false;
        private float playTime = 0;

        //Components
        private Light light = null;
        private Renderer renderer = null;
        private AudioSource manifestSource = null;
        private AudioSource barkSource = null;

        //Poor man's animation curve
        private const int KEYFRAMES = 6;
        private static float[] keyTimes = { 0, 1f, 1.4f, 1.8f, 2.2f, 3 };
        private static float[] visibilities = { 0, 1, 1, 0.5f, 1f, 0 };
        private int keyIndex = 999;
        private float startTime = 0;

        //Material handling
        public static string[] matNames = ["ace_mat", "bi_mat", "lesbian_mat", "pan_mat", "pride_mat", "trans_mat", "nb_mat", "fluid_mat", "aro_mat"];
        private static float randMatChance = 0.15f;
        private static Material[] sharedMats = null;
        private static Material sharedDefaultMat = null;
        private Material[] materials = null;
        private Material defaultMat = null;
        private Material matInstance = null;
        private DunGen.RandomStream matRNG = null;

        //Bounds for the animation
        private ColorAdjustments colorAdj = null;
        private float maxOpacity = 1;
        private float maxIntensity = 1;
        private Color filterCol = Color.white;

        /**
         * On awake, grab the renderer and light
         */
        private void Awake ()
        {
            //Grab a bunch of different attributes
            light = GetComponentInChildren<Light>();
            renderer = GetComponentInChildren<Renderer>();
            matInstance = renderer.material;
            maxOpacity = matInstance.color.a;
            maxIntensity = light.intensity;
            manifestSource = transform.Find("manifest_audio").GetComponent<AudioSource>();
            manifestSource.volume = Poltergeist.Config.GhostVolume.Value;
            barkSource = transform.Find("bark_audio").GetComponent<AudioSource>();
            barkSource.volume = Poltergeist.Config.GhostVolume.Value;

            //Load the material instances
            materials = new Material[sharedMats.Length];
            for (int i = 0; i < sharedMats.Length; i++)
            {
                materials[i] = Instantiate(sharedMats[i]);
            }
            defaultMat = Instantiate(sharedDefaultMat);
        }

        /**
         * Handle the frame-by-frame things
         */
        private void LateUpdate ()
        {
            //Check if we should initialize
            if(!initialized && SpectatorCamController.instance != null && IsOwner && (!base.IsServer || isHostHead))
            {
                //Setup flags
                initialized = true;
                SpectatorCamController.instance.head = this;
                renderer.enabled = false;

                //Make and grab the postprocessing vol
                GameObject volObj = Instantiate(Poltergeist.colorVolObject);
                VolumeProfile colorProfile = volObj.GetComponent<Volume>().profile;
                colorProfile.TryGet<ColorAdjustments>(out colorAdj);
                colorAdj.colorFilter.overrideState = true;
                filterCol = colorAdj.colorFilter.value;
                colorAdj.colorFilter.value = Color.white;
                Poltergeist.DebugLog("Assigning head to local client");

                //Send the default clip to the audio manager
                AudioManager.defaultClip = manifestSource.clip;

                //Make it so that neither source is spatial
                manifestSource.spatialBlend = 0;
                barkSource.spatialBlend = 0;
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
                    if(colorAdj != null)
                        colorAdj.colorFilter.value = Color.white;
                    manifestSource.Stop();
                }

                //Otherwise, actually do the animation
                else
                {
                    //Determine where we are in this keyframe
                    float duration = keyTimes[keyIndex] - keyTimes[keyIndex - 1];
                    float timeInFrame = Time.time - (startTime + keyTimes[keyIndex - 1]);
                    float progress = timeInFrame / duration;

                    //Interpolate
                    float curIntensity = Mathf.Lerp(visibilities[keyIndex - 1], visibilities[keyIndex], progress);
                    Color matCol = matInstance.color;
                    matInstance.color = new Color(matCol.r, matCol.g, matCol.b, curIntensity * maxOpacity);
                    light.intensity = curIntensity * maxIntensity;
                    if (colorAdj != null)
                        colorAdj.colorFilter.value = Color.Lerp(Color.white, filterCol, curIntensity);
                }
            }

            //Handle the audio timer
            if(playTime > 0)
            {
                playTime -= Time.deltaTime;
                if (playTime <= 0) 
                    barkSource.Stop();
            }
        }

        /**
         * Applies a random material to the head
         */
        public void UpdateHeadMat()
        {
            //Make the rng thing, if needed
            if(matRNG == null)
                matRNG = new DunGen.RandomStream();

            //Select then apply a random material
            double randomRoll = matRNG.NextDouble();
            Poltergeist.DebugLog($"Head random mat roll: {randomRoll}");
            int index;
            if (randomRoll > randMatChance)
                index = -1;
            else
                index = matRNG.Next() % materials.Length;
            ApplyMatServerRPC(index);
        }

        /**
         * Deactivate the given head after client dc's
         */
        public void Deactivate()
        {
            transform.position = StartOfRound.Instance.notSpawnedPosition.position;

            //Stop the animation
            keyIndex = KEYFRAMES;
            light.enabled = false;
            renderer.gameObject.layer = 23;
            light.intensity = maxIntensity;
            matInstance.color = new Color(matInstance.color.r, matInstance.color.g, matInstance.color.b, maxOpacity);
            if (colorAdj != null)
                colorAdj.colorFilter.value = Color.white;
            manifestSource.Stop();
        }

        /**
         * Load the materials from the assetbundle into the shared mats
         */
        public static void LoadMats(AssetBundle bundle)
        {
            sharedMats = new Material[matNames.Length];

            //Load each material from the bundle
            for(int i = 0; i < sharedMats.Length; i++)
            {
                sharedMats[i] = bundle.LoadAsset<Material>($"Assets/Materials/{matNames[i]}.mat");
            }

            //Load the default
            sharedDefaultMat = bundle.LoadAsset<Material>("Assets/Materials/ghost_mat.mat");
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
         * Tells if the head is currently manifesting
         */
        public bool IsManifesting()
        {
            return keyIndex < KEYFRAMES;
        }

        /**
         * Plays the flicker animation
         */
        public bool PlayFlickerAnim()
        {
            //Early return if already playing
            if (IsManifesting())
                return false;

            //Start the animation
            keyIndex = 1;
            startTime = Time.time;
            light.enabled = true;
            renderer.gameObject.layer = 0;

            //Play the audio
            manifestSource.Play();

            return true;
        }

        /**
         * Lets the server message clients about the head flickering
         */
        [ClientRpc]
        public void BarkClientRpc(int index)
        {
            PlayBarkAudio(index);
        }

        /**
         * Lets the client tell the server to flicker the head
         */
        [ServerRpc]
        public void BarkServerRpc(int index)
        {
            BarkClientRpc(index);
        }

        /**
         * Tells if bark audio is currently playing
         */
        public bool IsBarking()
        {
            return barkSource.isPlaying;
        }

        /**
         * Plays the given bark audio
         */
        public void PlayBarkAudio(int index)
        {
            //Make sure it's not already playing
            if (IsBarking())
                return;

            Poltergeist.DebugLog("Playing a bark locally");
            playTime = Poltergeist.Config.AudioTime.Value;
            barkSource.clip = AudioManager.GetClip(index);
            barkSource.Play();
        }

        /**
         * Let clients tell the server to change their mat
         */
        [ServerRpc]
        private void ApplyMatServerRPC(int index)
        {
            ApplyMatClientRPC(index);
        }

        /**
         * Let the server tell clients about a mat change
         */
        [ClientRpc]
        private void ApplyMatClientRPC(int index)
        {
            ApplyMatLocally(index);
        }

        /**
         * Applies the given material index to the head locally
         */
        private void ApplyMatLocally(int index)
        {
            if (index < 0)
                matInstance = defaultMat;
            else
                matInstance = materials[index];

            renderer.sharedMaterial = matInstance;
        }
    }
}
