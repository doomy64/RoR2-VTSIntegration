using BepInEx.Configuration;
using Newtonsoft.Json;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace VTSIntegration
{
    internal class VTSConfig
    {
        public struct PinData
        {
            public PinData(string mesh, List<int> vertexIDs, List<float> vertexWeights)
            {
                this.mesh = mesh;
                this.vertexIDs = vertexIDs;
                this.vertexWeights = vertexWeights;
            }

            public static PinData Null()
            {
                return new PinData("null", null, null);
            }

            [JsonProperty]
            public string mesh;
            [JsonProperty]
            public List<int> vertexIDs;
            [JsonProperty]
            public List<float> vertexWeights;
        }

        public struct ItemConfig
        {
            public ItemConfig(ConfigEntry<string> displayName, ConfigEntry<string> imageOverride, ConfigEntry<Vector2> position, ConfigEntry<int> rotation, ConfigEntry<float> size, ConfigEntry<string> pinInfo)
            {
                this.displayName = displayName;
                this.imageOverride = imageOverride;
                this.position = position;
                this.rotation = rotation;
                this.size = size;
                this.pinInfo = pinInfo;
            }

            public PinData GetPinData()
            {
                return pinInfo.Value.Equals("null") ? new PinData("null", null, null) : JsonConvert.DeserializeObject<PinData>(pinInfo.Value);
            }

            public void SetPinData(PinData pinData)
            {
                pinInfo.Value = JsonConvert.SerializeObject(pinData);
            }

            public ConfigEntry<string> displayName { get; set; }
            public ConfigEntry<string> imageOverride { get; set; }
            public ConfigEntry<Vector2> position { get; set; }
            public ConfigEntry<int> rotation { get; set; }
            public ConfigEntry<float> size { get; set; }
            public ConfigEntry<string> pinInfo { get; set; }

        }

        public struct ListConfigEntry
        {
            public ConfigEntry<string> entry { get; set; }
            public List<string> values { get; set; }

            public ListConfigEntry(ConfigFile file, string section, string key, List<string> defaultValue, string description)
            {
                entry = file.Bind<string>(section, key, string.Join(",", defaultValue), description);
                values = new List<string>();
                values.AddRange(entry.Value.Split(","));
            }


            public void Add(string s)
            {
                if (!values.Contains(s))
                {
                    values.Add(s);
                    UpdateConfig();
                }
            }

            public void Remove(string s)
            {
                if(values.Remove(s))
                    UpdateConfig();
                
            }

            public void Set(string s, bool v)
            {
                if (values.Contains(s) && !v)
                {
                    values.Remove(s);
                }
                else if (v && !values.Contains(s))
                {
                    values.Add(s);
                }
                else
                    return;

                UpdateConfig();
            }
            public bool Contains(string s)
            { 
                return values.Contains(s); 
            }
            private void UpdateConfig()
            {
                entry.Value = string.Join(",", values);
            }

        }

        public static ConfigFile modelConfig;
        public static ConfigEntry<string> address;
        public static ConfigEntry<string> authToken;
        public static ConfigEntry<float> brightness;
        public static ConfigEntry<float> contrast;
        public static ConfigEntry<float> gamma;
        public static ListConfigEntry blockedItems;
        public static ListConfigEntry flippedItems;
        public static ListConfigEntry backgroundItems;
        public static ListConfigEntry forceEntries;
        private static List<string> defaultBlacklist = new List<string>
        {
            //Items that can be obtained in normal gameplay, but don't show in the player's inventory
            "ConvertCritChanceToCritDamage",
            "DrizzlePlayerHelper",
            "MonsoonPlayerHelper",
            "VoidmanPassiveItem",
            //Items that don't have a proper texture, probably can't be obtained normally
            "AACannon",
            "AdaptiveArmor",
            "BoostAttackSpeed",
            "BoostDamage",
            "BoostEquipmentRecharge",
            "BoostHp",
            "CrippleWardOnLevel",
            "CritHeal", 
            "CutHp",
            "DroneWeaponsBoost",
            "DroneWeaponsDisplay1",
            "DroneWeaponsDisplay2",
            "EmpowerAlways",
            "Ghost",
            "GummyCloneIdentifier",
            "HealthDecay",
            "InvadingDoppelganger",
            "LemurianHarness",
            "LevelBonus",
            "MageAttunement",
            "MinHealthPercentage",
            "MinionLeash",
            "PlantOnHit",
            "PlasmaCore",
            "TeamSizeDamageBonus",
            "TempestOnKill",
            "UseAmbientLevel",
            "WarCryOnCombat"
        };
        private static List<string> defaultForceEntries = new List<string>
        {
            "FragileDamageBonusConsumed", //Broken Watch
            "ExtraLifeConsumed", //Dio's Best Friend (Consumed)
            "ExtraLifeVoidConsumed", //Pluripotent Larva (Consumed)
            "HealingPotionConsumed", //Empty Bottle
            "LowerPricedChestsConsumed", //Sale Star (Consumed)
            "RegeneratingScrapConsumed", //Regenerating Scrap (Consumed)
            "BossHunterConsumed", //Trophy Hunter's Tricorn (Consumed)
            "HealAndReviveConsumed", //Seed of Life (Consumed)
            "TonicAffliction", //Tonic Affliction
            "QuestVolatileBattery", //Fuel Array
            "EliteAurelioniteEquipment", //Aurelionite's Blessing
            "EliteBeadEquipment", //His Spiteful Boon
            "EliteEarthEquipment", //His Reassurance
            "EliteFireEquipment", //Ifrit's Indistinction
            "EliteHauntedEquipment", //Spectral Circlet
            "EliteIceEquipment", //Her Biting Embrace
            "EliteLightningEquipment", //Silence Between Two Strikes
            "EliteLunarEquipment", //Shared Design
            "ElitePoisonEquipment", //N'kuhana's Retort
        };
        private static VTSIntegration plugin;
        public static Dictionary<string, ItemConfig> itemConfig = new Dictionary<string, ItemConfig>();
        
        public static void Init(VTSIntegration p)
        {
            plugin = p;
            //Placeholder while we wait for the API to give us a model
            modelConfig = plugin.Config;
            address = plugin.Config.Bind<string>("General", "Address", "ws://localhost:8001", "Address for VTS API. Usually unnecessary to change this");
            authToken = plugin.Config.Bind<string>("General", "Token", "null", "Plugin Authentication for VTS API. Do not change this");
            brightness = plugin.Config.Bind<float>("General", "Brightness", 1.1f, "Change brightness for items sent to VTS");
            contrast = plugin.Config.Bind<float>("General", "Contrast", 2.0f, "Change contrast for items sent to VTS");
            gamma = plugin.Config.Bind<float>("General", "Gamma", 1.0f, "Change gamma for items sent to VTS");

            forceEntries = new ListConfigEntry(plugin.Config, "General", "ForcedLogbookEntries", defaultForceEntries, "Items to force to appear in the logbook, for configuration in VTS");
            GenerateModelConfigLists(plugin.Config);
            
            ItemCatalog.availability.onAvailable += ItemCatalog_onAvailable;
            EquipmentCatalog.availability.onAvailable += EquipmentCatalog_onAvailable;
        }

        public static void LoadModelConfig(string modelID)
        {
            modelConfig = new ConfigFile($"{BepInEx.Paths.ConfigPath}/VTSmodels/{modelID}.cfg", true);
            if (ItemCatalog.availability.available)
            {
                GenerateItemConfigs();
            }
            if (EquipmentCatalog.availability.available)
            {
                GenerateEquipmentConfigs();
            }
            GenerateModelConfigLists(modelConfig);
        }

        private static void GenerateModelConfigLists(ConfigFile file)
        {
            blockedItems = new ListConfigEntry(file, "General", "ItemBlacklist", defaultBlacklist, "Items that won't be sent to VTS. Add items by deleting the item in VTS. Remove items by pressing F2 while hovering over its logbook entry");
            flippedItems = new ListConfigEntry(file, "General", "FlippedItems", new List<string>(), "Items that will be flipped when sent to VTS. Add/remove items by flipping the item in VTS");
            backgroundItems = new ListConfigEntry(file, "General", "BackgroundItems", new List<string>(), "Items that will appear behind your model in VTS. Add/remove items by pressing F3 while hovering the item in your logbook");
        }

        private static void ItemCatalog_onAvailable()
        {
            GenerateItemConfigs();
        }

        private static void EquipmentCatalog_onAvailable()
        {
            GenerateEquipmentConfigs();
        }

        private static void GenerateItemConfigs()
        {
            modelConfig.SaveOnConfigSet = false;
            foreach (ItemDef item in ItemCatalog.allItemDefs)
            {
                GenerateConfig(item);
            }
            modelConfig.SaveOnConfigSet = true;
            modelConfig.Save();
        }

        private static void GenerateEquipmentConfigs()
        {
            modelConfig.SaveOnConfigSet = false;
            foreach (EquipmentDef def in EquipmentCatalog.equipmentDefs)
            {
                GenerateConfig(def);
            }
            modelConfig.SaveOnConfigSet = true;
            modelConfig.Save();
        }
        private static void GenerateConfig(ItemDef item)
        {
            //This item isn't used afaik, and its name conflicts with Hellfire Tincture
            if (item.name.ToLower().Equals("burnnearby"))
                return;

            string name = "Item." + item.name;
            ConfigEntry<string> displayName = modelConfig.Bind<string>(name, "display_name", Language.GetString(item.nameToken), "Display name for this item, used to make finding specific items easier. Changing this does nothing");
            ConfigEntry<string> imageOverride = modelConfig.Bind<string>(name, "image", "null", "VTS item to use instead of the in-game sprite");
            ConfigEntry<Vector2> position = modelConfig.Bind<Vector2>(name, "position", new Vector2(0.0f, 0.0f), "Stored position for this item. Can be changed by moving the item in VTS");
            ConfigEntry<int> rotation = modelConfig.Bind<int>(name, "rotation", 0, "Stored rotation for this item. With the item active in VTS, change with CTRL + Scroll Up/Down while hovering the item in your logbook");
            ConfigEntry<float> size = modelConfig.Bind<float>(name, "size", 0.32f, "Stored size for this item. With the item active in VTS, change with Scroll Up/Down while hovering the item in your logbook");
            ConfigEntry<string> pinnedModel = modelConfig.Bind<string>(name, "pinData", "{model:'null',mesh:'null'}", "Information for how this item is pinned to a model");
            itemConfig.Remove(item.name);
            itemConfig.Add(item.name, new ItemConfig(displayName, imageOverride, position, rotation, size, pinnedModel));
        }

        private static void GenerateConfig(EquipmentDef def)
        {
            string name = "ItemEquipment." + def.name;
            ConfigEntry<string> displayName = modelConfig.Bind<string>(name, "display_name", Language.GetString(def.nameToken), "Display name for this item, used to make finding specific items easier. Changing this does nothing");
            ConfigEntry<string> imageOverride = modelConfig.Bind<string>(name, "image", "null", "VTS item to use instead of the in-game sprite");
            ConfigEntry<Vector2> position = modelConfig.Bind<Vector2>(name, "position", new Vector2(0.0f, 0.0f), "Stored position for this item. Can be changed by moving the item in VTS");
            ConfigEntry<int> rotation = modelConfig.Bind<int>(name, "rotation", 0, "Stored rotation for this item. With the item active in VTS, change with CTRL + Scroll Up/Down while hovering the item in your logbook");
            ConfigEntry<float> size = modelConfig.Bind<float>(name, "size", 0.32f, "Stored size for this item. With the item active in VTS, change with Scroll Up/Down while hovering the item in your logbook");
            ConfigEntry<string> pinnedModel = modelConfig.Bind<string>(name, "pinData", "{model:'null',mesh:'null'}", "Information for how this item is pinned to a model");
            itemConfig.Remove(def.name);
            itemConfig.Add(def.name, new ItemConfig(displayName, imageOverride, position, rotation, size, pinnedModel));
        }
    }
}
