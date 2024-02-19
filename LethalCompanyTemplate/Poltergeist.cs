using System;
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
            Patches.defaultMode = Config.Bind<bool>("General",
                "DefaultToVanilla",
                false,
                "If true, the vanilla spectate system will be used by default on death.").Value;
            SpectatorCamController.lightIntensity = Config.Bind<float>("General",
                "GhostLightIntensity",
                5,
                "The intensity of the global light when dead.\n" +
                "WARNING: This game has a lot of fog, so excessively high values can decrease visibility.").Value;
            GhostInteractible.interactCoolDown = (long)TimeSpan.FromSeconds(Config.Bind<int>("General",
                "InteractCoolDown",
                1,
                "The cooldown until a dead player can interact with an object again.").Value).TotalMilliseconds;

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