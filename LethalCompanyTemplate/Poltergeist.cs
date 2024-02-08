using BepInEx;
using HarmonyLib;
using System.Reflection;

namespace Poltergeist
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Poltergeist : BaseUnityPlugin
    {
        private static Poltergeist instance = null;

        private void Awake()
        {
            instance = this;

            //Handle the config
            Patches.runBarebones = Config.Bind<bool>("General",
                "RunBarebones",
                false,
                "If true, most mod functionality will be disabled, preserving the vanilla spectate behavior.\n" +
                "(Good for those who want to spectate normally, but don't want things to break if others are using the mod)").Value;
            SpectatorCamController.lightIntensity = Config.Bind<float>("General",
                "GhostLightIntensity",
                5,
                "The intensity of the global light when dead.\n" +
                "WARNING: This game has a lot of fog, so excessively high values can decrease visibility.").Value;

            //Make the patches
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            // All done!
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
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