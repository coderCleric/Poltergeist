using BepInEx.Configuration;
using CSync.Extensions;
using CSync.Lib;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Poltergeist
{
    public class PoltergeistConfig : SyncedConfig<PoltergeistConfig>
    {
        //Non-synced entries
        public ConfigEntry<bool> DefaultToVanilla { get; private set; }
        public ConfigEntry<float> LightIntensity { get; private set; }

        //Synced entries
        [field: DataMember] public SyncedEntry<float> MaxPower { get; private set; }

        //Cost entries (also synced)

        /**
         * Make an instance of the config
         */
        public PoltergeistConfig(ConfigFile cfg) : base(Poltergeist.MOD_GUID)
        {
            //Register the config
            ConfigManager.Register(this);

            //Bind the non-synced stuff
            DefaultToVanilla = cfg.Bind(
                new ConfigDefinition("Client-Side", "DefaultToVanilla"),
                false,
                new ConfigDescription(
                    "If true, you will be placed into the default spectate mode when you die."
                    )
                );
            LightIntensity = cfg.Bind(
                new ConfigDefinition("Client-Side", "LightIntensity"),
                5f,
                new ConfigDescription(
                    "The intensity of the ghost light.\n" +
                    "(WARNING: This game has a lot of fog, so setting this too high can actually make it harder to see.)",
                    new AcceptableValueRange<float>(0, float.MaxValue)
                    )
                );

            //Bind the regular synced stuff
            MaxPower = cfg.BindSyncedEntry(
                new ConfigDefinition("Synced", "MaxPower"),
                100f,
                new ConfigDescription(
                    "The maximum amount of power that will be available to the ghosts.",
                    new AcceptableValueRange<float>(0, float.MaxValue)
                    )
                );
        }
    }
}
