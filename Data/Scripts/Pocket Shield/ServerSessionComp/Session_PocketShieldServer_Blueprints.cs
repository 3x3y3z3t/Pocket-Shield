// ;
using ExShared;
using Sandbox.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.Components;

namespace PocketShield
{
    public partial class Session_PocketShieldServer : MySessionComponentBase
    {
        private Dictionary<string, string> m_CachedStats = null;
        private Dictionary<string, string> m_CachedNames = null;
        private Dictionary<string, string> m_CachedDescs = null;

        private void InitCache()
        {
            if (m_CachedStats == null)
            {
                m_CachedStats = new Dictionary<string, string>();
                m_CachedStats[Constants.SUBTYPEID_EMITTER_BAS] = "";
                m_CachedStats[Constants.SUBTYPEID_EMITTER_ADV] = "";
                m_CachedStats[Constants.SUBTYPEID_PLUGIN_CAP] = "";
                m_CachedStats[Constants.SUBTYPEID_PLUGIN_DEF_KI] = "";
                m_CachedStats[Constants.SUBTYPEID_PLUGIN_DEF_EX] = "";
                m_CachedStats[Constants.SUBTYPEID_PLUGIN_RES_KI] = "";
                m_CachedStats[Constants.SUBTYPEID_PLUGIN_RES_EX] = "";
            }

            if (m_CachedNames == null)
            {
                m_CachedNames = new Dictionary<string, string>();
                m_CachedNames[Constants.SUBTYPEID_EMITTER_BAS] = "PocketShield Basic Emitter";
                m_CachedNames[Constants.SUBTYPEID_EMITTER_ADV] = "PocketShield Advanced Emitter";
                m_CachedNames[Constants.SUBTYPEID_PLUGIN_CAP] = "PocketShield Capacity Plugin";
                m_CachedNames[Constants.SUBTYPEID_PLUGIN_DEF_KI] = "PocketShield Bullet Defense Plugin";
                m_CachedNames[Constants.SUBTYPEID_PLUGIN_DEF_EX] = "PocketShield Explosion Defense Plugin";
                m_CachedNames[Constants.SUBTYPEID_PLUGIN_RES_KI] = "PocketShield Bullet Resistance Plugin";
                m_CachedNames[Constants.SUBTYPEID_PLUGIN_RES_EX] = "PocketShield Explosion Resistance Plugin";
            }

            if (m_CachedDescs == null)
            {
                m_CachedDescs = new Dictionary<string, string>();
                m_CachedDescs[Constants.SUBTYPEID_EMITTER_BAS] = "Basic PocketShield Emitter that provides minimal protection.";
                m_CachedDescs[Constants.SUBTYPEID_EMITTER_ADV] = "Advanced PocketShield Emitter that provides extra protection and modification capability.";
                m_CachedDescs[Constants.SUBTYPEID_PLUGIN_CAP] = "PocketShield plugin that increase max Shield Capacity.";
                m_CachedDescs[Constants.SUBTYPEID_PLUGIN_DEF_KI] = "PocketShield plugin that increase Defense against Bullet damage.";
                m_CachedDescs[Constants.SUBTYPEID_PLUGIN_DEF_EX] = "PocketShield plugin that increase Defense against Explosion damage.";
                m_CachedDescs[Constants.SUBTYPEID_PLUGIN_RES_KI] = "PocketShield plugin that increase Resistance against Bullet damage.";
                m_CachedDescs[Constants.SUBTYPEID_PLUGIN_RES_EX] = "PocketShield plugin that increase Resistance against Explosion damage.";
            }
        }

        private void UpdateItemsDescription()
        {
            ServerLogger.Log("MethodCalled");
            InitCache();

            UpdateInventoryTooltip();
            UpdateAssemblerDescription();
        }
        
        private void UpdateInventoryTooltip()
        {
            ServerConfig config = ConfigManager.ServerConfig;

            var definitions = MyDefinitionManager.Static.GetPhysicalItemDefinitions();

            foreach (var definition in definitions)
            {
                if (definition.Id.SubtypeName.Contains(Constants.SUBTYPEID_EMITTER_BAS))
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
                    {
                        extraTooltip += "\n  Plugin slots: " + config.BasicMaxPluginsCount;
                    }
                    m_CachedStats[Constants.SUBTYPEID_EMITTER_BAS] = extraTooltip;

                    definition.DisplayNameString = definition.DisplayNameString.Replace(Constants.SUBTYPEID_EMITTER_BAS + "_DisplayName", m_CachedNames[Constants.SUBTYPEID_EMITTER_BAS]);
                    definition.ExtraInventoryTooltipLine.Replace(Constants.SUBTYPEID_EMITTER_BAS + "_Stat", extraTooltip);
                }
                else if (definition.Id.SubtypeName.Contains(Constants.SUBTYPEID_EMITTER_ADV))
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
                    {
                        extraTooltip += "\n  Plugin slots: " + config.AdvancedMaxPluginsCount;
                    }
                    m_CachedStats[Constants.SUBTYPEID_EMITTER_ADV] = extraTooltip;

                    definition.DisplayNameString = definition.DisplayNameString.Replace(Constants.SUBTYPEID_EMITTER_ADV + "_DisplayName", m_CachedNames[Constants.SUBTYPEID_EMITTER_ADV]);
                    definition.ExtraInventoryTooltipLine.Replace(Constants.SUBTYPEID_EMITTER_ADV + "_Stat", extraTooltip);
                }
                else if (definition.Id.SubtypeName.Contains(Constants.SUBTYPEID_PLUGIN_CAP))
                {
                    string extraTooltip = string.Format("Stat:\n  +{0:#.#}% Capacity", config.PluginCapacityBonus * 100.0f);
                    m_CachedStats[Constants.SUBTYPEID_PLUGIN_CAP] = extraTooltip;

                    definition.DisplayNameString = definition.DisplayNameString.Replace(Constants.SUBTYPEID_PLUGIN_CAP + "_DisplayName", m_CachedNames[Constants.SUBTYPEID_PLUGIN_CAP]);
                    definition.ExtraInventoryTooltipLine.Replace(Constants.SUBTYPEID_PLUGIN_CAP + "_Stat", extraTooltip);
                }
                else if (definition.Id.SubtypeName.Contains(Constants.SUBTYPEID_PLUGIN_DEF_KI))
                {
                    string extraTooltip = string.Format("Stat:\n  +{0:#.#}% Bullet Defense", config.PluginDefenseBonus * 100.0f);
                    m_CachedStats[Constants.SUBTYPEID_PLUGIN_DEF_KI] = extraTooltip;

                    definition.DisplayNameString = definition.DisplayNameString.Replace(Constants.SUBTYPEID_PLUGIN_DEF_KI + "_DisplayName", m_CachedNames[Constants.SUBTYPEID_PLUGIN_DEF_KI]);
                    definition.ExtraInventoryTooltipLine.Replace(Constants.SUBTYPEID_PLUGIN_DEF_KI + "_Stat", extraTooltip);
                }
                else if (definition.Id.SubtypeName.Contains(Constants.SUBTYPEID_PLUGIN_DEF_EX))
                {
                    string extraTooltip = string.Format("Stat:\n  +{0:#.#}% Explosive Defense", config.PluginDefenseBonus * 100.0f);
                    m_CachedStats[Constants.SUBTYPEID_PLUGIN_DEF_EX] = extraTooltip;

                    definition.DisplayNameString = definition.DisplayNameString.Replace(Constants.SUBTYPEID_PLUGIN_DEF_EX + "_DisplayName", m_CachedNames[Constants.SUBTYPEID_PLUGIN_DEF_EX]);
                    definition.ExtraInventoryTooltipLine.Replace(Constants.SUBTYPEID_PLUGIN_DEF_EX + "_Stat", extraTooltip);
                }
                else if (definition.Id.SubtypeName.Contains(Constants.SUBTYPEID_PLUGIN_RES_KI))
                {
                    string extraTooltip = string.Format("Stat:\n  +{0:#.#}% Bullet Resistance", config.PluginResistanceBonus * 100.0f);
                    m_CachedStats[Constants.SUBTYPEID_PLUGIN_RES_KI] = extraTooltip;

                    definition.DisplayNameString = definition.DisplayNameString.Replace(Constants.SUBTYPEID_PLUGIN_RES_KI + "_DisplayName", m_CachedNames[Constants.SUBTYPEID_PLUGIN_RES_KI]);
                    definition.ExtraInventoryTooltipLine.Replace(Constants.SUBTYPEID_PLUGIN_RES_KI + "_Stat", extraTooltip);
                }
                else if (definition.Id.SubtypeName.Contains(Constants.SUBTYPEID_PLUGIN_RES_EX))
                {
                    string extraTooltip = string.Format("Stat:\n  +{0:#.#}% Explosive Resistance", config.PluginResistanceBonus * 100.0f);
                    m_CachedStats[Constants.SUBTYPEID_PLUGIN_RES_EX] = extraTooltip;

                    definition.DisplayNameString = definition.DisplayNameString.Replace(Constants.SUBTYPEID_PLUGIN_RES_EX + "_DisplayName", m_CachedNames[Constants.SUBTYPEID_PLUGIN_RES_EX]);
                    definition.ExtraInventoryTooltipLine.Replace(Constants.SUBTYPEID_PLUGIN_RES_EX + "_Stat", extraTooltip);
                }
            }


        }

        private void UpdateAssemblerDescription()
        {
            ServerConfig config = ConfigManager.ServerConfig;

            var definitions = MyDefinitionManager.Static.GetBlueprintDefinitions();

            foreach (var definition in definitions)
            {
                foreach (string key in m_CachedNames.Keys)
                {
                    if (definition.Id.SubtypeName.Contains(key))
                    {
                        string displayName =
                            m_CachedNames[key] + "\n  " +
                            m_CachedDescs[key] + "\n" +
                            m_CachedStats[key];

                        definition.DisplayNameString = definition.DisplayNameString.Replace(key + "_DisplayName", displayName);
                        break;
                    }
                }
            }



        }



    }
}
