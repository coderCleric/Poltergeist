using BepInEx.Configuration;
using CSync.Extensions;
using CSync.Lib;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Poltergeist
{
    public class PoltergeistConfig : SyncedConfig2<PoltergeistConfig>
    {
        //Non-synced entries
        public ConfigEntry<bool> DefaultToVanilla { get; private set; }
        public ConfigEntry<float> LightIntensity { get; private set; }

        //Synced entries
        [field: DataMember] public SyncedEntry<float> MaxPower { get; private set; }
        [field: DataMember] public SyncedEntry<float> PowerRegen { get; private set; }
        [field: DataMember] public SyncedEntry<int> AliveForMax { get; private set; }
        [field: DataMember] public SyncedEntry<float> TimeForAggro { get; private set; }
        [field: DataMember] public SyncedEntry<int> HitsForAggro { get; private set; }

        //Cost-related entries
        [field: DataMember] public SyncedEntry<float> DoorCost { get; private set; }
        [field: DataMember] public SyncedEntry<float> BigDoorCost { get; private set; }
        [field: DataMember] public SyncedEntry<float> NoisyItemCost { get; private set; }
        [field: DataMember] public SyncedEntry<float> ValveCost { get; private set; }
        [field: DataMember] public SyncedEntry<float> ShipDoorCost { get; private set; }
        [field: DataMember] public SyncedEntry<float> CompanyBellCost { get; private set; }
        [field: DataMember] public SyncedEntry<float> PesterCost { get; private set; }
        [field: DataMember] public SyncedEntry<float> ManifestCost { get; private set; }
        [field: DataMember] public SyncedEntry<float> MiscCost { get; private set; }

        /**
         * Make an instance of the config
         */
        public PoltergeistConfig(ConfigFile cfg) : base(Poltergeist.MOD_GUID)
        {
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
                8f,
                new ConfigDescription(
                    "The intensity of the ghost light.\n",
                    new AcceptableValueRange<float>(0, float.MaxValue)
                    )
                );

            //Bind the regular synced stuff
            MaxPower = cfg.BindSyncedEntry(
                new ConfigDefinition("Synced", "Max power"),
                100f,
                new ConfigDescription(
                    "The maximum amount of power that will be available to the ghosts.",
                    new AcceptableValueRange<float>(0, float.MaxValue)
                    )
                );
            PowerRegen = cfg.BindSyncedEntry(
                new ConfigDefinition("Synced", "Power regen"),
                5f,
                new ConfigDescription(
                    "How much power the ghosts regenerate per second.",
                    new AcceptableValueRange<float>(0, float.MaxValue)
                    )
                );
            AliveForMax = cfg.BindSyncedEntry(
                new ConfigDefinition("Synced", "Alive for max power"),
                1,
                new ConfigDescription(
                    "The maximum number of players that can be alive for the ghosts to have max power.\n" + 
                    "(As soon as this number or fewer players are left alive, ghosts will be at max power.)",
                    new AcceptableValueRange<int>(0, int.MaxValue)
                    )
                );
            TimeForAggro = cfg.BindSyncedEntry(
                new ConfigDefinition("Synced", "Pester aggro timespan"),
                3f,
                new ConfigDescription(
                    "How many seconds can be between pesterings for an enemy to get mad at a nearby player.",
                    new AcceptableValueRange<float>(0, float.MaxValue)
                    )
                );
            HitsForAggro = cfg.BindSyncedEntry(
                new ConfigDefinition("Synced", "Aggro hit requirement"),
                2,
                new ConfigDescription(
                    "How many times an enemy has to be pestered in the timespan in order to get mad at a nearby player.",
                    new AcceptableValueRange<int>(1, int.MaxValue)
                    )
                );

            //Bind the cost-related configs
            DoorCost = cfg.BindSyncedEntry(
                new ConfigDefinition("Synced: Costs", "Door cost"),
                10f,
                new ConfigDescription(
                    "The power required to open/close regular doors.",
                    new AcceptableValueRange<float>(0, float.MaxValue)
                    )
                );
            BigDoorCost = cfg.BindSyncedEntry(
                new ConfigDefinition("Synced: Costs", "Big door cost"),
                50f,
                new ConfigDescription(
                    "The power required to open/close larger doors.",
                    new AcceptableValueRange<float>(0, float.MaxValue)
                    )
                );
            NoisyItemCost = cfg.BindSyncedEntry(
                new ConfigDefinition("Synced: Costs", "Noisy item cost"),
                5f,
                new ConfigDescription(
                    "The power required to use noisy items.",
                    new AcceptableValueRange<float>(0, float.MaxValue)
                    )
                );
            ValveCost = cfg.BindSyncedEntry(
                new ConfigDefinition("Synced: Costs", "Valve cost"),
                20f,
                new ConfigDescription(
                    "The power required to turn valves.",
                    new AcceptableValueRange<float>(0, float.MaxValue)
                    )
                );
            ShipDoorCost = cfg.BindSyncedEntry(
                new ConfigDefinition("Synced: Costs", "Ship door cost"),
                30f,
                new ConfigDescription(
                    "The power required to use the ship doors.",
                    new AcceptableValueRange<float>(0, float.MaxValue)
                    )
                );
            CompanyBellCost = cfg.BindSyncedEntry(
                new ConfigDefinition("Synced: Costs", "Company bell cost"),
                15f,
                new ConfigDescription(
                    "The power required to ring the bell at the company building.",
                    new AcceptableValueRange<float>(0, float.MaxValue)
                    )
                );
            PesterCost = cfg.BindSyncedEntry(
                new ConfigDefinition("Synced: Costs", "Pester cost"),
                20f,
                new ConfigDescription(
                    "The power required to pester enemies.",
                    new AcceptableValueRange<float>(0, float.MaxValue)
                    )
                );
            ManifestCost = cfg.BindSyncedEntry(
                new ConfigDefinition("Synced: Costs", "Manifest cost"),
                80f,
                new ConfigDescription(
                    "The power required to manifest in the realm of the living.",
                    new AcceptableValueRange<float>(0, float.MaxValue)
                    )
                );
            MiscCost = cfg.BindSyncedEntry(
                new ConfigDefinition("Synced: Costs", "Misc cost"),
                10f,
                new ConfigDescription(
                    "The power required to do any interactions not covered by another section.",
                    new AcceptableValueRange<float>(0, float.MaxValue)
                    )
                );

            //Register the config
            ConfigManager.Register(this);
        }
    }
}
