﻿using BepInEx.Configuration;
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
        public enum NAME_TYPE {INTERNAL = 1, COMMON = 2, BOTH = 3};

        //Non-synced entries
        public ConfigEntry<bool> DefaultToVanilla { get; private set; }
        public ConfigEntry<float> LightIntensity { get; private set; }
        public ConfigEntry<bool> ShowDebugLogs { get; private set; }
        public ConfigEntry<float> GhostVolume { get; private set; }
        public ConfigEntry<bool> DisableDuplicateSounds { get; private set; }
        public ConfigEntry<bool> UseDefaultSounds { get; private set; }

        //Synced entries
        [field: SyncedEntryField] public SyncedEntry<float> MaxPower { get; private set; }
        [field: SyncedEntryField] public SyncedEntry<float> PowerRegen { get; private set; }
        [field: SyncedEntryField] public SyncedEntry<int> AliveForMax { get; private set; }
        [field: SyncedEntryField] public SyncedEntry<float> TimeForAggro { get; private set; }
        [field: SyncedEntryField] public SyncedEntry<int> HitsForAggro { get; private set; }
        [field: SyncedEntryField] public SyncedEntry<float> AudioTime { get; private set; }
        public ConfigEntry<NAME_TYPE> PesterBlacklistType { get; private set; } //Not actually a synced entry, since only the host makes pestering happen
        public ConfigEntry<string> PesterBlacklist { get; private set; } //Not actually a synced entry, since only the host makes pestering happen

        //Cost-related entries
        [field: SyncedEntryField] public SyncedEntry<float> DoorCost { get; private set; }
        [field: SyncedEntryField] public SyncedEntry<float> BigDoorCost { get; private set; }
        [field: SyncedEntryField] public SyncedEntry<float> NoisyItemCost { get; private set; }
        [field: SyncedEntryField] public SyncedEntry<float> ValveCost { get; private set; }
        [field: SyncedEntryField] public SyncedEntry<float> ShipDoorCost { get; private set; }
        [field: SyncedEntryField] public SyncedEntry<float> CompanyBellCost { get; private set; }
        [field: SyncedEntryField] public SyncedEntry<float> PesterCost { get; private set; }
        [field: SyncedEntryField] public SyncedEntry<float> ManifestCost { get; private set; }
        [field: SyncedEntryField] public SyncedEntry<float> BarkCost { get; private set; }
        [field: SyncedEntryField] public SyncedEntry<float> MiscCost { get; private set; }

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
            ShowDebugLogs = cfg.Bind(
                new ConfigDefinition("Client-Side", "ShowDebugLogs"),
                false,
                new ConfigDescription(
                    "If true, you will see debug logs."
                    )
                );
            GhostVolume = cfg.Bind(
                new ConfigDefinition("Client-Side", "Ghost Volume"),
                1f,
                new ConfigDescription(
                    "Volume of the audio ghosts make",
                    new AcceptableValueRange<float>(0, 1)
                    )
                );
            DisableDuplicateSounds = cfg.Bind(
                new ConfigDefinition("Client-Side", "Disable Duplicate Sounds"),
                false,
                new ConfigDescription(
                    "Whether or not sound files with identical names should be loaded alongside each other."
                    )
                );
            UseDefaultSounds = cfg.Bind(
                new ConfigDefinition("Client-Side", "Use Default Sounds"),
                true,
                new ConfigDescription(
                    "Whether or not the files found in Poltergeist\'s \"sounds\" folder should be included."
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
            AudioTime = cfg.BindSyncedEntry(
                new ConfigDefinition("Synced", "Audio play time"),
                5f,
                new ConfigDescription(
                    "The maximum time (in seconds) that ghost audio can play before stopping.",
                    new AcceptableValueRange<float>(0, float.MaxValue)
                    )
                );
            PesterBlacklistType = cfg.Bind( //Not actually synced, but only works for the host
                new ConfigDefinition("Synced", "Enemy Name Type"),
                NAME_TYPE.BOTH,
                new ConfigDescription(
                    "What type of name Poltergeist should look at when blacklisting enemies. Valid values are:" +
                    "\nINTERNAL: The internal name used by the game (GiantKiwi). You may need to look it up/ask a mod dev for this name." +
                    "\nCOMMON:   The name that appears when the enemy is scanned (Giant Sapsucker). This may not work for all enemies." +
                    "\nBOTH:     Both of the other name types.\n"
                    )
                );
            PesterBlacklist = cfg.Bind( //Not actually synced, but only works for the host
                new ConfigDefinition("Synced", "Pester Blacklist"),
                "",
                new ConfigDescription(
                    "A comma-separated list of monster names. Monsters who's names contain values from here will not be able to be pestered." +
                    "\nExample: \"Bracken,mask\" will disable pestering for the \"BRACKEN\", the \"MASKed\", and the \"MASK Hornets\"."
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
                    "The power required to open/close larger doors and mess with the mineshaft elevator.",
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
                60f,
                new ConfigDescription(
                    "The power required to manifest in the realm of the living.",
                    new AcceptableValueRange<float>(0, float.MaxValue)
                    )
                );
            BarkCost = cfg.BindSyncedEntry(
                new ConfigDefinition("Synced: Costs", "Audio playing cost"),
                40f,
                new ConfigDescription(
                    "The power required to play audio that the living can hear.",
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
