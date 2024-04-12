using BepInEx;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace Poltergeist
{
    [BepInPlugin(MOD_GUID, MOD_NAME, MOD_VERSION)]
    [BepInDependency("com.rune580.LethalCompanyInputUtils")]
    public class Poltergeist : BaseUnityPlugin
    {
        //Plugin info
        public const string MOD_GUID = "coderCleric.Poltergeist";
        public const string MOD_NAME = "Poltergeist";
        public const string MOD_VERSION = "0.4";

        //Network prefabs
        public static GameObject propInteractibleObject;

        //Other things
        private static Poltergeist instance = null;
        public static AssetBundle poltergeistAssetBundle;

        private void Awake()
        {
            instance = this;

            //Handle the config
            Patches.defaultMode = Config.Bind<bool>("General",
                "DefaultToVanilla",
                false,
                "If true, the vanilla spectate system will be used by default on death.").Value;
            SpectatorCamController.lightIntensity = Config.Bind<float>("General",
                "GhostLightIntensity",
                5,
                "The intensity of the global light when dead.\n" +
                "WARNING: This game has a lot of fog, so excessively high values can decrease visibility.").Value;

            //Make the patches
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            //Make the input instance
            new PoltergeistCustomInputs();

            //Load the assetbundle
            string dllFolderPath = System.IO.Path.GetDirectoryName(Info.Location);
            string assetBundleFilePath = System.IO.Path.Combine(dllFolderPath, "bundles", "poltergeist");
            poltergeistAssetBundle = AssetBundle.LoadFromFile(assetBundleFilePath);

            //Patch netcode
            NetcodePatcher();

            // All done!
            Logger.LogInfo($"Plugin {MOD_GUID} is loaded!");
        }

        /**
         * The wiki says to do this
         */
        private static void NetcodePatcher()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }

        /**
         * Simple debug logging
         */
        public static void DebugLog(string msg)
        {
            instance.Logger.LogInfo(msg);
        }
    }
}