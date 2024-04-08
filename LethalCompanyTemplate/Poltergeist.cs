using BepInEx;
using HarmonyLib;
using System.Reflection;

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

        //Other things
        private static Poltergeist instance = null;

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

            // All done!
            Logger.LogInfo($"Plugin {MOD_GUID} is loaded!");
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