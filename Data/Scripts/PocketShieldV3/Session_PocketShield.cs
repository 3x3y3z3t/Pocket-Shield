// ;
using ExShared;
using PocketShieldCore;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using VRage.Game.Components;
using VRage.Utils;
using VRageMath;

namespace PocketShield
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public partial class Session_PocketShield : MySessionComponentBase
    {
        private PocketShieldAPI.ShieldEmitterProperties m_CachedProperties = new PocketShieldAPI.ShieldEmitterProperties(null);

        public override void LoadData()
        {

        }

        protected override void UnloadData()
        {
            /* REQUIRED!
             * You need to call PocketShieldAPI.Close() when you are done using it (typically on mod unload).
             * You can check core mod (PocketShieldCore)'s log for information on which mod forgot to Close.
             */
            PocketShieldAPI.Close();

            MyAPIGateway.Utilities.MessageEntered -= Utilities_MessageEntered;

            Logger.DeInit();

        }

        public override void BeforeStart()
        {
            string modInfo = ModContext.ModId + "." + ModContext.ModName;
            Logger.Log("ModInfo: " + modInfo);

            MyAPIGateway.Utilities.MessageEntered += Utilities_MessageEntered;

            /* REQUIRED!
             * You need to call PocketShieldAPI.Init() and pass in a unique string to identify your mod.
             * Wait until PocketShieldAPI.Ready == true, or pass in a callback.
             * Do not use the API when it is not Ready, or the callback has not been called.
             */
            PocketShieldAPI.Init(modInfo, RegisterFinishedCallback);

        }

        private void RegisterFinishedCallback(PocketShieldAPI.ReturnSide _returnSide)
        {
            if (_returnSide == PocketShieldAPI.ReturnSide.Server)
            {
                /* You can register callbacks to compile data for ShieldEmitter construction.
                 * When a ShieldEmitter item is found in inventory, it will go through these callbacks
                 * to get ShieldEmitterProperties data to construct a ShieldEmitter.
                 * An Emitter will be constructed or not depends on the data these callbacks return.
                 */
                PocketShieldAPI.RegisterCompileEmitterPropertiesCallback(CompileBasicEmitterProperties);
                PocketShieldAPI.RegisterCompileEmitterPropertiesCallback(CompileAdvancedEmitterProperties);

                /* You can set a Plugin's bonus value.
                 * You can even override a Plugin's bonus value with your value.
                 */
                PocketShieldAPI.SetPluginModifier(MyStringHash.GetOrCompute(Constants.SUBTYPEID_PLUGIN_CAP), Config.Static.PluginCapacityBonus);
                PocketShieldAPI.SetPluginModifier(MyStringHash.GetOrCompute(Constants.SUBTYPEID_PLUGIN_DEF_KI), Config.Static.PluginDefenseBonus);
                PocketShieldAPI.SetPluginModifier(MyStringHash.GetOrCompute(Constants.SUBTYPEID_PLUGIN_DEF_EX), Config.Static.PluginDefenseBonus);
                PocketShieldAPI.SetPluginModifier(MyStringHash.GetOrCompute(Constants.SUBTYPEID_PLUGIN_RES_KI), Config.Static.PluginResistanceBonus);
                PocketShieldAPI.SetPluginModifier(MyStringHash.GetOrCompute(Constants.SUBTYPEID_PLUGIN_RES_EX), Config.Static.PluginResistanceBonus);

                UpdateBlueprintData();

                Logger.Log("PocketShield registered with Server");
            }

            if (_returnSide == PocketShieldAPI.ReturnSide.Client)
            {
                Vector2 uvSize = new Vector2(Constants.ICONS_ATLAS_UV_SIZE_X, Constants.ICONS_ATLAS_UV_SIZE_Y);

                /* FOR HUD DISPLAY!
                 * You can register custom icon for your shield that will be drawn on HUD Panel.
                 * This is not required. If you don't register one, a default icon will be drawn.
                 * You can register a whole texture, or a texture atlas and supply uv information.
                 */
                PocketShieldAPI.RegisterShieldIcon(new PocketShieldAPI.ShieldIconDrawInfo(null)
                {
                    Material = MyStringId.GetOrCompute("PocketShieldV3_ShieldIcons"),
                    SubtypeId = MyStringHash.GetOrCompute(Constants.SUBTYPEID_EMITTER_BAS),
                    UvEnabled = true,
                    UvSize = uvSize,
                    UvOffset = new Vector2(0.0f * Constants.ICONS_ATLAS_UV_SIZE_X, 0.0f * Constants.ICONS_ATLAS_UV_SIZE_Y)
                });
                PocketShieldAPI.RegisterShieldIcon(new PocketShieldAPI.ShieldIconDrawInfo(null)
                {
                    Material = MyStringId.GetOrCompute("PocketShieldV3_ShieldIcons"),
                    SubtypeId = MyStringHash.GetOrCompute(Constants.SUBTYPEID_EMITTER_ADV),
                    UvEnabled = true,
                    UvSize = uvSize,
                    UvOffset = new Vector2(1.0f * Constants.ICONS_ATLAS_UV_SIZE_X, 0.0f * Constants.ICONS_ATLAS_UV_SIZE_Y)
                });

                /* FOR HUD DISPLAY!
                 * You can register your custom icons for Defense and Resistance stat that will be drawn on HUD Panel.
                 * This is not required. If you don't register one, nothing will be drawn there.
                 * You can register a whole texture, or a texture atlas and supply uv information.
                 */
                PocketShieldAPI.RegisterStatIcons(new PocketShieldAPI.ItemCardDrawInfo(null)
                {
                    Material = MyStringId.GetOrCompute("PocketShieldV3_ShieldIcons"),
                    DamageType = MyStringHash.GetOrCompute(Constants.DAMAGETYPE_KI),
                    UvEnabled = true,
                    UvSize = uvSize,
                    UvOffset = new Vector2(0.0f * Constants.ICONS_ATLAS_UV_SIZE_X, 1.0f * Constants.ICONS_ATLAS_UV_SIZE_Y),
                });
                PocketShieldAPI.RegisterStatIcons(new PocketShieldAPI.ItemCardDrawInfo(null)
                {
                    Material = MyStringId.GetOrCompute("PocketShieldV3_ShieldIcons"),
                    DamageType = MyStringHash.GetOrCompute(Constants.DAMAGETYPE_EX),
                    UvEnabled = true,
                    UvSize = uvSize,
                    UvOffset = new Vector2(0.0f * Constants.ICONS_ATLAS_UV_SIZE_X, 2.0f * Constants.ICONS_ATLAS_UV_SIZE_Y),
                });

                Logger.Log("PocketShield registered with Client");
            }

            Logger.Log("Status: Server = " + PocketShieldAPI.ServerReady + ", Client = " + PocketShieldAPI.ClientReady);
        }

        /// <summary>
        /// Compiles ShieldEmitterProperties used to create a ShieldEmitter. 
        /// PocketShield will pass in current item's SubtypeId so you can know which item to create.
        /// </summary>
        /// <param name="_subtypeId">The SubtypeId of current inventory item to check</param>
        /// <returns>ShieldEmitterProperties used to create a ShieldEmitter, or null to not creating anything</returns>
        public ImmutableList<object> CompileBasicEmitterProperties(MyStringHash _subtypeId)
        {
            if (_subtypeId != MyStringHash.GetOrCompute(Constants.SUBTYPEID_EMITTER_BAS))
                return null;

            m_CachedProperties.Clear();

            m_CachedProperties.SubtypeId = _subtypeId;

            m_CachedProperties.MaxPluginsCount = Config.Static.BasicMaxPluginsCount;
            m_CachedProperties.BaseMaxEnergy = Config.Static.BasicShieldEnergy;
            m_CachedProperties.BaseChargeRate = Config.Static.BasicChargeRate;
            m_CachedProperties.BaseChargeDelay = Config.Static.BasicChargeDelay;
            m_CachedProperties.BaseOverchargeDuration = Config.Static.BasicOverchargeDuration;
            m_CachedProperties.BaseOverchargeDefBonus = Config.Static.BasicOverchargeDefBonus;
            m_CachedProperties.BaseOverchargeResBonus = Config.Static.BasicOverchargeResBonus;
            m_CachedProperties.BasePowerConsumption = Config.Static.BasicPowerConsumption;

            m_CachedProperties.BaseDef[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_KI)] = Config.Static.BasicDefense;
            m_CachedProperties.BaseDef[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_EX)] = Config.Static.BasicDefense;
            m_CachedProperties.BaseRes[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_KI)] = Config.Static.BasicResistance;
            m_CachedProperties.BaseRes[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_EX)] = Config.Static.BasicResistance;

            return m_CachedProperties.Data;
        }

        /// <summary>
        /// Compiles ShieldEmitterProperties used to create a ShieldEmitter. 
        /// PocketShield will pass in current item's SubtypeId so you can know which item to create.
        /// </summary>
        /// <param name="_subtypeId">The SubtypeId of current inventory item to check</param>
        /// <returns>ShieldEmitterProperties used to create a ShieldEmitter, or null to not creating anything</returns>
        public ImmutableList<object> CompileAdvancedEmitterProperties(MyStringHash _subtypeId)
        {
            if (_subtypeId != MyStringHash.GetOrCompute(Constants.SUBTYPEID_EMITTER_ADV))
                return null;

            m_CachedProperties.Clear();

            m_CachedProperties.SubtypeId = _subtypeId;

            m_CachedProperties.MaxPluginsCount = Config.Static.AdvancedMaxPluginsCount;
            m_CachedProperties.BaseMaxEnergy = Config.Static.AdvancedShieldEnergy;
            m_CachedProperties.BaseChargeRate = Config.Static.AdvancedChargeRate;
            m_CachedProperties.BaseChargeDelay = Config.Static.AdvancedChargeDelay;
            m_CachedProperties.BaseOverchargeDuration = Config.Static.AdvancedOverchargeDuration;
            m_CachedProperties.BaseOverchargeDefBonus = Config.Static.AdvancedOverchargeDefBonus;
            m_CachedProperties.BaseOverchargeResBonus = Config.Static.AdvancedOverchargeResBonus;
            m_CachedProperties.BasePowerConsumption = Config.Static.AdvancedPowerConsumption;

            m_CachedProperties.BaseDef[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_KI)] = Config.Static.AdvancedDefense;
            m_CachedProperties.BaseDef[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_EX)] = Config.Static.AdvancedDefense;
            m_CachedProperties.BaseRes[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_KI)] = Config.Static.AdvancedResistance;
            m_CachedProperties.BaseRes[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_EX)] = Config.Static.AdvancedResistance;

            return m_CachedProperties.Data;
        }

        // ================================================================================;
        // All Methods below are not required.
        // Because of that, they are written as short as possible and hard to read.
        // 
        // You can keep them, or create one that suits your needs.
        // ================================================================================;







        #region Non-important Stuff
        /// <summary>
        /// Update Items and Blueprints data to show item's stat dynamically.
        /// </summary>
        private void UpdateBlueprintData()
        {
            #region Cache Stats
            Dictionary<string, string> cachedStats = new Dictionary<string, string>();
            // Basic Emitter;
            {
                string extraTooltip = "Stat:\n  Capacity: " + Config.Static.BasicShieldEnergy;
                if (Config.Static.BasicDefense != 0.0f || Config.Static.BasicResistance != 0.0f)
                {
                    extraTooltip += string.Format(
                        "\n  Against Bullet: Defense {0:0.#}%, Resistance {1:0.#}%" +
                        "\n  Against Explosion: Defense {0:0.#}%, Resistance {1:0.#}%",
                        Config.Static.BasicDefense * 100.0f, Config.Static.BasicResistance * 100.0f);
                }
                if (Config.Static.PluginCapacityBonus > 0)
                    extraTooltip += "\n  Plugin slots: " + Config.Static.BasicMaxPluginsCount;

                cachedStats[Constants.SUBTYPEID_EMITTER_BAS] = extraTooltip;
            }

            // Advanced Emitter;
            {
                string extraTooltip = "Stat:\n  Capacity: " + Config.Static.AdvancedShieldEnergy;
                if (Config.Static.AdvancedDefense != 0.0f || Config.Static.AdvancedResistance != 0.0f)
                {
                    extraTooltip += string.Format(
                        "\n  Against Bullet: Defense {0:0.#}%, Resistance {1:0.#}%" +
                        "\n  Against Explosion: Defense {0:0.#}%, Resistance {1:0.#}%",
                        Config.Static.AdvancedDefense * 100.0f, Config.Static.AdvancedResistance * 100.0f);
                }
                if (Config.Static.PluginCapacityBonus > 0)
                    extraTooltip += "\n  Plugin slots: " + Config.Static.AdvancedMaxPluginsCount;

                cachedStats[Constants.SUBTYPEID_EMITTER_ADV] = extraTooltip;
            }

            // Plugins;
            {
                cachedStats[Constants.SUBTYPEID_PLUGIN_CAP] = string.Format("Stat:\n  +{0:#.#}% Capacity", Config.Static.PluginCapacityBonus * 100.0f);
                cachedStats[Constants.SUBTYPEID_PLUGIN_DEF_KI] = string.Format("Stat:\n  +{0:#.#}% Bullet Defense", Config.Static.PluginDefenseBonus * 100.0f);
                cachedStats[Constants.SUBTYPEID_PLUGIN_DEF_EX] = string.Format("Stat:\n  +{0:#.#}% Explosive Defense", Config.Static.PluginDefenseBonus * 100.0f);
                cachedStats[Constants.SUBTYPEID_PLUGIN_RES_KI] = string.Format("Stat:\n  +{0:#.#}% Bullet Resistance", Config.Static.PluginResistanceBonus * 100.0f);
                cachedStats[Constants.SUBTYPEID_PLUGIN_RES_EX] = string.Format("Stat:\n  +{0:#.#}% Explosive Resistance", Config.Static.PluginResistanceBonus * 100.0f);
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

            var physItemDef = MyDefinitionManager.Static.GetPhysicalItemDefinitions();
            foreach (var def in physItemDef)
            {
                foreach (var key in cachedStats.Keys)
                {
                    if (def.Id.SubtypeName.Contains(key))
                    {
                        def.DisplayNameString = def.DisplayNameString.Replace(key + "_DisplayName", cachedNames[key]);
                        def.ExtraInventoryTooltipLine = def.ExtraInventoryTooltipLine.Replace(key + "_Stat", cachedStats[key]);
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
        
        private void Utilities_MessageEntered(string _messageText, ref bool _sendToOthers)
        {
            const string c_ChatCmdPrefix = "/PShield";

            Logger.Log(">> Ultilities_MessageEntered triggered <<", 5);
            if (MyAPIGateway.Session.Player == null)
                return;

            if (!_messageText.StartsWith(c_ChatCmdPrefix))
                return;

            Logger.Log("  Chat Command captured: " + _messageText, 1);
            ProcessCommands(_messageText);

            _sendToOthers = false;
        }

        private bool ProcessCommands(string _commands)
        {
            string[] commands = _commands.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (commands.Length <= 1)
            {
                MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] You didn't specify any command", 2000);
                return false;
            }

            for (int i = 1; i < commands.Length; ++i)
            {
                string cmd = commands[i].Trim();
                Logger.Log("    Processing command " + i + ": " + cmd, 1);
                if (ProcessSingleCommand(cmd))
                    MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] Command executed.", 2000);
                else
                    MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] Command execution failed. See log for more info.", 2000);
            }

            return true;
        }

        private bool ProcessSingleCommand(string _command)
        {
            if (_command == "ReloadCfg")
            {
                Logger.Log("      Executing reload command", 1);
                if (Config.Static.LoadConfigFile())
                {
                    MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] Config reloaded", 2000);
                    UpdateBlueprintData();
                }
                else
                    MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] Config reload failed", 2000);
                return true;
            }

            if (_command == "SaveCfg")
            {
                Logger.Log("      Executing save command", 1);
                if (Config.Static.SaveConfigFile())
                    MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] Config saved", 2000);
                else
                    MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] Config saving failed", 2000);
                return true;
            }

            if (_command == "LoadedCfg")
            {
                Logger.Log("  Executing LoadedCfg command");
                string configs = MyAPIGateway.Utilities.SerializeToXML(Config.Static);
                //configs += "\nViewport Size = " + s_ViewportSize.ToString();
                MyAPIGateway.Utilities.ShowMissionScreen(
                    screenTitle: "Loaded Configs",
                    currentObjectivePrefix: "",
                    currentObjective: "ClientConfig.xml",
                    screenDescription: configs,
                    okButtonCaption: "Close"
                );
                return true;
            }

            if (_command == "PeekCfg")
            {
                Logger.Log("  Executing PeekCfg command");
                //MyAPIGateway.Utilities.ShowNotification("[Pantenna] PeekCfg Command", 3000);
                string configs = Config.Static.PeekConfigFile();
                MyAPIGateway.Utilities.ShowMissionScreen(
                    screenTitle: "Raw Config File",
                    currentObjectivePrefix: "",
                    currentObjective: "ClientConfig.xml",
                    screenDescription: configs,
                    okButtonCaption: "Close"
                );
                return true;
            }

            MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] Unknown Command [" + _command + "]", 2000);
            Logger.Log("      Unknown command [" + _command + "]", 1);
            return false;
        }
        #endregion


    }
}
