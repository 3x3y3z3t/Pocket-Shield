// ;
using ExShared;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;

namespace PocketShield
{
    public partial class Session_PocketShieldServer : MySessionComponentBase
    {
        private Dictionary<string, float> m_CachedPrice = new Dictionary<string, float>();

        private void UpdateBlueprintData()
        {
            ServerConfig config = ConfigManager.ServerConfig;

            #region Cache Stats
            Dictionary<string, string> cachedStats = new Dictionary<string, string>();
            // Basic Emitter;
            {
                string extraTooltip = "Stat:\n  Capacity: " + config.BasicShieldEnergy;
                if (config.BasicDefense != 0.0f || config.BasicResistance != 0.0f)
                {
                    extraTooltip += string.Format(
                        "\n  Against Bullet: Defense {0:0.#}%, Resistance {1:0.#}%" +
                        "\n  Against Explosion: Defense {0:0.#}%, Resistance {1:0.#}%",
                        config.BasicDefense * 100.0f, config.BasicResistance * 100.0f);
                }
                if (config.PluginCapacityBonus > 0)
                    extraTooltip += "\n  Plugin slots: " + config.BasicMaxPluginsCount;

                cachedStats[Constants.SUBTYPEID_EMITTER_BAS] = extraTooltip;
            }

            // Advanced Emitter;
            {
                string extraTooltip = "Stat:\n  Capacity: " + config.AdvancedShieldEnergy;
                if (config.AdvancedDefense != 0.0f || config.AdvancedResistance != 0.0f)
                {
                    extraTooltip += string.Format(
                        "\n  Against Bullet: Defense {0:0.#}%, Resistance {1:0.#}%" +
                        "\n  Against Explosion: Defense {0:0.#}%, Resistance {1:0.#}%",
                        config.AdvancedDefense * 100.0f, config.AdvancedResistance * 100.0f);
                }
                if (config.PluginCapacityBonus > 0)
                    extraTooltip += "\n  Plugin slots: " + config.AdvancedMaxPluginsCount;

                cachedStats[Constants.SUBTYPEID_EMITTER_ADV] = extraTooltip;
            }

            // Plugins;
            {
                cachedStats[Constants.SUBTYPEID_PLUGIN_CAP] = string.Format("Stat:\n  +{0:#.#}% Capacity", config.PluginCapacityBonus * 100.0f);
                cachedStats[Constants.SUBTYPEID_PLUGIN_DEF_KI] = string.Format("Stat:\n  +{0:#.#}% Bullet Defense", config.PluginDefenseBonus * 100.0f);
                cachedStats[Constants.SUBTYPEID_PLUGIN_DEF_EX] = string.Format("Stat:\n  +{0:#.#}% Explosive Defense", config.PluginDefenseBonus * 100.0f);
                cachedStats[Constants.SUBTYPEID_PLUGIN_RES_KI] = string.Format("Stat:\n  +{0:#.#}% Bullet Resistance", config.PluginResistanceBonus * 100.0f);
                cachedStats[Constants.SUBTYPEID_PLUGIN_RES_EX] = string.Format("Stat:\n  +{0:#.#}% Explosive Resistance", config.PluginResistanceBonus * 100.0f);
            }
            #endregion

            #region Cache Names 
            Dictionary<string, string> cachedNames = new Dictionary<string, string>();
            cachedNames[Constants.SUBTYPEID_EMITTER_BAS] = "PocketShield Basic Emitter";
            cachedNames[Constants.SUBTYPEID_EMITTER_ADV] = "PocketShield Advanced Emitter";
            cachedNames[Constants.SUBTYPEID_PLUGIN_CAP] = "PocketShield Capacity Plugin";
            cachedNames[Constants.SUBTYPEID_PLUGIN_DEF_KI] = "PocketShield Bullet Defense Plugin";
            cachedNames[Constants.SUBTYPEID_PLUGIN_DEF_EX] = "PocketShield Explosion Defense Plugin";
            cachedNames[Constants.SUBTYPEID_PLUGIN_RES_KI] = "PocketShield Bullet Resistance Plugin";
            cachedNames[Constants.SUBTYPEID_PLUGIN_RES_EX] = "PocketShield Explosion Resistance Plugin";
            #endregion

            #region Cache Descriptions
            Dictionary<string, string> cachedDescs = new Dictionary<string, string>();
            cachedDescs[Constants.SUBTYPEID_EMITTER_BAS] = "Basic PocketShield Emitter that provides minimal protection.";
            cachedDescs[Constants.SUBTYPEID_EMITTER_ADV] = "Advanced PocketShield Emitter that provides extra protection and modification capability.";
            cachedDescs[Constants.SUBTYPEID_PLUGIN_CAP] = "PocketShield plugin that increase max Shield Capacity.";
            cachedDescs[Constants.SUBTYPEID_PLUGIN_DEF_KI] = "PocketShield plugin that increase Defense against Bullet damage.";
            cachedDescs[Constants.SUBTYPEID_PLUGIN_DEF_EX] = "PocketShield plugin that increase Defense against Explosion damage.";
            cachedDescs[Constants.SUBTYPEID_PLUGIN_RES_KI] = "PocketShield plugin that increase Resistance against Bullet damage.";
            cachedDescs[Constants.SUBTYPEID_PLUGIN_RES_EX] = "PocketShield plugin that increase Resistance against Explosion damage.";
            #endregion

            #region Cache Prices
            MyDefinitionId id;
            foreach (var key in cachedStats.Keys)
            {
                if (MyDefinitionId.TryParse("MyObjectBuilder_PhysicalObject/" + key, out id))
                {
                    m_CachedPrice[key] = CalculateItemMinimalPrice(id) * 0.5f;
                }
                else
                {
                    ServerLogger.Log("> ================================================== <");
                    ServerLogger.Log(">   Could not parse DefID for                        <");
                    ServerLogger.Log(string.Format(">   {0,-48} <", key));
                    ServerLogger.Log("> ================================================== <");
                }
            }

            foreach (var key in m_CachedPrice.Keys)
            {
                ServerLogger.Log("Price for " + key + " is " + m_CachedPrice[key], 4);
            }
            #endregion

            var physItemDef = MyDefinitionManager.Static.GetPhysicalItemDefinitions();
            foreach (var def in physItemDef)
            {
                foreach (var key in cachedStats.Keys)
                {
                    if (def.Id.SubtypeName.Contains(key))
                    {
                        def.DisplayNameString = def.DisplayNameString.Replace(key + "_DisplayName", cachedNames[key]);
                        def.ExtraInventoryTooltipLine = def.ExtraInventoryTooltipLine.Replace(key + "_Stat", cachedStats[key]);
                        def.MinimalPricePerUnit = (int)m_CachedPrice[key];
                        break;
                    }
                }
            }

            var bpDef = MyDefinitionManager.Static.GetBlueprintDefinitions();
            foreach (var def in bpDef)
            {
                foreach (string key in cachedStats.Keys)
                {
                    if (def.Id.SubtypeName.Contains(key))
                    {
                        string displayName = cachedNames[key] + "\n  " + cachedDescs[key] + "\n" + cachedStats[key];
                        def.DisplayNameString = def.DisplayNameString.Replace(key + "_DisplayName", displayName);
                        break;
                    }
                }
            }

        }

        // HACK: copied from Sandbox.Game.World.Generator.MyMinimalPriceCalculator.CalculateItemMinimalPrice(VRage.Game.MyDefinitionId, float, ref int);
        private float CalculateItemMinimalPrice(MyDefinitionId _id)
        {
            MyPhysicalItemDefinition physItemDef = null;
            if (MyDefinitionManager.Static.TryGetPhysicalItemDefinition(_id, out physItemDef) && physItemDef.MinimalPricePerUnit != -1)
            {
                return physItemDef.MinimalPricePerUnit;
            }

            MyBlueprintDefinitionBase bpDefBase = null;
            if (!MyDefinitionManager.Static.TryGetBlueprintDefinitionByResultId(_id, out bpDefBase))
            {
                return 0.0f;
            }

            float price = 0.0f;
            float efficiencyMod = physItemDef.IsIngot ? 1.0f : MyAPIGateway.Session.AssemblerEfficiencyMultiplier;
            foreach (var item in bpDefBase.Prerequisites)
            {
                price += CalculateItemMinimalPrice(item.Id) * (float)item.Amount / efficiencyMod;
            }

            float speedMod = physItemDef.IsIngot ? MyAPIGateway.Session.RefinerySpeedMultiplier : MyAPIGateway.Session.AssemblerSpeedMultiplier;
            for (int i = 0; i < bpDefBase.Results.Length; ++i)
            {
                var item = bpDefBase.Results[i];
                if (item.Id == _id && (float)item.Amount > 0.0f)
                {
                    // this is the item we want to get;

                    float number = 1.0f + (float)Math.Log(bpDefBase.BaseProductionTimeInSeconds + 1.0f) / speedMod;
                    price *= (1.0f / (float)item.Amount) * number;
                    return price;
                }
            }

            return 0.0f;
        }
        

    }
}
