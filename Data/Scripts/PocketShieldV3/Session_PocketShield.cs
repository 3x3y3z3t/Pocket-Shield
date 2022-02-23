// ;
using ExShared;
using PocketShieldCore;
using Sandbox.Definitions;
using System.Collections.Generic;
using VRage.Game.Components;
using VRage.Utils;
using VRageMath;

namespace PocketShield
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public partial class Session_PocketShield : MySessionComponentBase
    {

        Logger m_Logger = null;

        public override void LoadData()
        {
            m_Logger = new Logger("");

        }

        protected override void UnloadData()
        {
            /* REQUIRED!
             * You need to call PocketShieldAPIV2.Close() when you are done using it (typically on mod unload).
             * You can check core mod (PocketShieldCore)'s log for information on which mod forgot to Close.
             */
            PocketShieldAPIV2.Close();

            m_Logger.Close();
        }

        public override void BeforeStart()
        {
            string modInfo = ModContext.ModId + "." + ModContext.ModName;
            m_Logger.WriteLine("ModInfo: " + modInfo);

            /* REQUIRED!
             * You need to call PocketShieldAPIV2.Init() and pass in an unique string to identify your mod.
             * Wait until PocketShieldAPI.Ready == true, or pass in a callback.
             * Do not use the API when it is not Ready, or the callback has not been called.
             */
            PocketShieldAPIV2.Init(modInfo, RegisterFinishedCallback, Logger_WriteLine);
        }

        private void RegisterFinishedCallback(PocketShieldAPIV2.ReturnSide _returnSide)
        {
            if (_returnSide == PocketShieldAPIV2.ReturnSide.Server)
            {
                #region Local Variable
                PocketShieldAPIV2.ShieldEmitterProperties basicEmitterProp = new PocketShieldAPIV2.ShieldEmitterProperties(null)
                {
                    SubtypeId = MyStringHash.GetOrCompute(Constants.SUBTYPEID_EMITTER_BAS),
                    IsManual = true,

                    BaseMaxEnergy = Constants.SHIELD_BAS_MAX_ENERGY,
                    BaseChargeRate = Constants.SHIELD_BAS_CHARGE_RATE,
                    BaseChargeDelay = Constants.SHIELD_BAS_CHARGE_DELAY,
                    BaseOverchargeDuration = Constants.SHIELD_BAS_OVERCHARGE_TIME,
                    BaseOverchargeDefBonus = Constants.SHIELD_BAS_OVERCHARGE_DEF_BONUS,
                    BaseOverchargeResBonus = Constants.SHIELD_BAS_OVERCHARGE_RES_BONUS,
                    BasePowerConsumption = Constants.SHIELD_BAS_POWER_CONSUMPTION,
                    MaxPluginsCount = Constants.SHIELD_BAS_MAX_PLUGINS
                };
                basicEmitterProp.BaseDef[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_KI)] = Constants.SHIELD_BAS_DEF;
                basicEmitterProp.BaseDef[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_EX)] = Constants.SHIELD_BAS_DEF;
                basicEmitterProp.BaseRes[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_KI)] = Constants.SHIELD_BAS_RES;
                basicEmitterProp.BaseRes[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_EX)] = Constants.SHIELD_BAS_RES;
                PocketShieldAPIV2.ShieldEmitterProperties advancedEmitterProp = new PocketShieldAPIV2.ShieldEmitterProperties(null)
                {
                    SubtypeId = MyStringHash.GetOrCompute(Constants.SUBTYPEID_EMITTER_ADV),
                    IsManual = false,

                    MaxPluginsCount = Constants.SHIELD_ADV_MAX_PLUGINS,
                    BaseMaxEnergy = Constants.SHIELD_ADV_MAX_ENERGY,
                    BaseChargeRate = Constants.SHIELD_ADV_CHARGE_RATE,
                    BaseChargeDelay = Constants.SHIELD_ADV_CHARGE_DELAY,
                    BaseOverchargeDuration = Constants.SHIELD_ADV_OVERCHARGE_TIME,
                    BaseOverchargeDefBonus = Constants.SHIELD_ADV_OVERCHARGE_DEF_BONUS,
                    BaseOverchargeResBonus = Constants.SHIELD_ADV_OVERCHARGE_RES_BONUS,
                    BasePowerConsumption = Constants.SHIELD_ADV_POWER_CONSUMPTION
                };
                advancedEmitterProp.BaseDef[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_KI)] = Constants.SHIELD_ADV_DEF;
                advancedEmitterProp.BaseDef[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_EX)] = Constants.SHIELD_ADV_DEF;
                advancedEmitterProp.BaseRes[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_KI)] = Constants.SHIELD_ADV_RES;
                advancedEmitterProp.BaseRes[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_EX)] = Constants.SHIELD_ADV_RES;
                #endregion
                
                /* You can submit properties for your ShieldEmitter.
                 * These data will be used when creating a ShieldEmitter. If no data is submit (for a certain SubtypeID),
                 * that Emitter will not be created.
                 */
                PocketShieldAPIV2.Server_RegisterEmitter(MyStringHash.GetOrCompute(Constants.SUBTYPEID_EMITTER_BAS), basicEmitterProp);
                PocketShieldAPIV2.Server_RegisterEmitter(MyStringHash.GetOrCompute(Constants.SUBTYPEID_EMITTER_ADV), advancedEmitterProp);

                /* You can set a Plugin's bonus value.
                 * You can even override a Plugin's bonus value with your value.
                 */
                PocketShieldAPIV2.Server_SetPluginModifier(MyStringHash.GetOrCompute(Constants.SUBTYPEID_PLUGIN_CAP), Constants.PLUGIN_CAP_BONUS);
                PocketShieldAPIV2.Server_SetPluginModifier(MyStringHash.GetOrCompute(Constants.SUBTYPEID_PLUGIN_DEF_KI), Constants.PLUGIN_DEF_BONUS);
                PocketShieldAPIV2.Server_SetPluginModifier(MyStringHash.GetOrCompute(Constants.SUBTYPEID_PLUGIN_DEF_EX), Constants.PLUGIN_DEF_BONUS);
                PocketShieldAPIV2.Server_SetPluginModifier(MyStringHash.GetOrCompute(Constants.SUBTYPEID_PLUGIN_RES_KI), Constants.PLUGIN_RES_BONUS);
                PocketShieldAPIV2.Server_SetPluginModifier(MyStringHash.GetOrCompute(Constants.SUBTYPEID_PLUGIN_RES_EX), Constants.PLUGIN_RES_BONUS);

                UpdateBlueprintData();

                m_Logger.WriteLine("PocketShield registered with Server");
            }

            if (_returnSide == PocketShieldAPIV2.ReturnSide.Client)
            {
                #region Local Variable
                Vector2 uvSizeShieldIcon = new Vector2(Constants.ICONS_ATLAS_UV_SIZE_X, Constants.ICONS_ATLAS_UV_SIZE_Y);
                Vector2 uvSizeStatIcon = new Vector2(2.0f * Constants.ICONS_ATLAS_UV_SIZE_X, Constants.ICONS_ATLAS_UV_SIZE_Y);

                PocketShieldAPIV2.ShieldIconDrawInfo basicEmitterShieldIcon = new PocketShieldAPIV2.ShieldIconDrawInfo(null)
                {
                    Material = MyStringId.GetOrCompute("PocketShieldV3_ShieldIcons"),
                    SubtypeId = MyStringHash.GetOrCompute(Constants.SUBTYPEID_EMITTER_BAS),
                    UvEnabled = true,
                    UvSize = uvSizeShieldIcon,
                    UvOffset = new Vector2(1.0f * Constants.ICONS_ATLAS_UV_SIZE_X, 0.0f * Constants.ICONS_ATLAS_UV_SIZE_Y)
                };
                PocketShieldAPIV2.ShieldIconDrawInfo advancedEmitterShieldIcon = new PocketShieldAPIV2.ShieldIconDrawInfo(null)
                {
                    Material = MyStringId.GetOrCompute("PocketShieldV3_ShieldIcons"),
                    SubtypeId = MyStringHash.GetOrCompute(Constants.SUBTYPEID_EMITTER_ADV),
                    UvEnabled = true,
                    UvSize = uvSizeShieldIcon,
                    UvOffset = new Vector2(2.0f * Constants.ICONS_ATLAS_UV_SIZE_X, 0.0f * Constants.ICONS_ATLAS_UV_SIZE_Y)
                };

                PocketShieldAPIV2.StatIconDrawInfo basicEmitterStatIcon = new PocketShieldAPIV2.StatIconDrawInfo(null)
                {
                    Material = MyStringId.GetOrCompute("PocketShieldV3_ShieldIcons"),
                    DamageType = MyStringHash.GetOrCompute(Constants.DAMAGETYPE_KI),
                    UvEnabled = true,
                    UvSize = uvSizeStatIcon,
                    UvOffset = new Vector2(0.0f * Constants.ICONS_ATLAS_UV_SIZE_X, 1.0f * Constants.ICONS_ATLAS_UV_SIZE_Y),
                };
                PocketShieldAPIV2.StatIconDrawInfo advancedEmitterStatIcon = new PocketShieldAPIV2.StatIconDrawInfo(null)
                {
                    Material = MyStringId.GetOrCompute("PocketShieldV3_ShieldIcons"),
                    DamageType = MyStringHash.GetOrCompute(Constants.DAMAGETYPE_EX),
                    UvEnabled = true,
                    UvSize = uvSizeStatIcon,
                    UvOffset = new Vector2(0.0f * Constants.ICONS_ATLAS_UV_SIZE_X, 2.0f * Constants.ICONS_ATLAS_UV_SIZE_Y),
                };
                #endregion

                /* FOR HUD DISPLAY!
                 * You can register custom icon for your shield that will be drawn on HUD Panel.
                 * This is not required. If you don't register one, a default icon will be drawn.
                 * You can register a whole texture, or a texture atlas and supply uv information.
                 */
                PocketShieldAPIV2.Client_RegisterShieldIcon(basicEmitterShieldIcon);
                PocketShieldAPIV2.Client_RegisterShieldIcon(advancedEmitterShieldIcon);

                /* FOR HUD DISPLAY!
                 * You can register your custom icons for Defense and Resistance stat that will be drawn on HUD Panel.
                 * This is not required. If you don't register one, nothing will be drawn there.
                 * You can register a whole texture, or a texture atlas and supply uv information.
                 */
                PocketShieldAPIV2.Client_RegisterStatIcon(basicEmitterStatIcon);
                PocketShieldAPIV2.Client_RegisterStatIcon(advancedEmitterStatIcon);

                m_Logger.WriteLine("PocketShield registered with Client");
            }

            m_Logger.WriteLine("Status: Server = " + PocketShieldAPIV2.ServerReady + ", Client = " + PocketShieldAPIV2.ClientReady);
        }










        // ================================================================================;
        // All Methods below are not required.
        // Because of that, they are written as short as possible and may be hard to read.
        // 
        // You can keep them, or create one that suits your needs.
        // ================================================================================;

        #region Non-important Stuff
        /// <summary> A wrapper for Logger's WriteLine method, used for passing to API. </summary>
        private void Logger_WriteLine(string _message)
        {
            m_Logger.WriteLine(_message);
        }

        /// <summary>
        /// Update Items and Blueprints data to show item's stat dynamically.
        /// </summary>
        private void UpdateBlueprintData()
        {
            #region Cache Stats
            Dictionary<string, string> cachedStats = new Dictionary<string, string>();
            // Basic Emitter;
            {
                string extraTooltip = "Stat:\n  Capacity: " + Constants.SHIELD_BAS_MAX_ENERGY;
                if (Constants.SHIELD_BAS_DEF != 0.0f || Constants.SHIELD_BAS_RES != 0.0f)
                {
                    extraTooltip += string.Format(
                        "\n  Against Bullet: Defense {0:0.#}%, Resistance {1:0.#}%" +
                        "\n  Against Explosion: Defense {0:0.#}%, Resistance {1:0.#}%",
                        Constants.SHIELD_BAS_DEF * 100.0f, Constants.SHIELD_BAS_RES * 100.0f);
                }
                if (Constants.PLUGIN_CAP_BONUS > 0)
                    extraTooltip += "\n  Plugin slots: " + Constants.SHIELD_BAS_MAX_PLUGINS;

                cachedStats[Constants.SUBTYPEID_EMITTER_BAS] = extraTooltip;
            }

            // Advanced Emitter;
            {
                string extraTooltip = "Stat:\n  Capacity: " + Constants.SHIELD_ADV_MAX_ENERGY;
                if (Constants.SHIELD_ADV_DEF != 0.0f || Constants.SHIELD_ADV_RES != 0.0f)
                {
                    extraTooltip += string.Format(
                        "\n  Against Bullet: Defense {0:0.#}%, Resistance {1:0.#}%" +
                        "\n  Against Explosion: Defense {0:0.#}%, Resistance {1:0.#}%",
                        Constants.SHIELD_ADV_DEF * 100.0f, Constants.SHIELD_ADV_RES * 100.0f);
                }
                if (Constants.PLUGIN_CAP_BONUS > 0)
                    extraTooltip += "\n  Plugin slots: " + Constants.SHIELD_ADV_MAX_PLUGINS;

                cachedStats[Constants.SUBTYPEID_EMITTER_ADV] = extraTooltip;
            }

            // Plugins;
            {
                cachedStats[Constants.SUBTYPEID_PLUGIN_CAP] = string.Format("Stat:\n  +{0:#.#}% Capacity", Constants.PLUGIN_CAP_BONUS * 100.0f);
                cachedStats[Constants.SUBTYPEID_PLUGIN_DEF_KI] = string.Format("Stat:\n  +{0:#.#}% Bullet Defense", Constants.PLUGIN_DEF_BONUS * 100.0f);
                cachedStats[Constants.SUBTYPEID_PLUGIN_DEF_EX] = string.Format("Stat:\n  +{0:#.#}% Explosive Defense", Constants.PLUGIN_DEF_BONUS * 100.0f);
                cachedStats[Constants.SUBTYPEID_PLUGIN_RES_KI] = string.Format("Stat:\n  +{0:#.#}% Bullet Resistance", Constants.PLUGIN_RES_BONUS * 100.0f);
                cachedStats[Constants.SUBTYPEID_PLUGIN_RES_EX] = string.Format("Stat:\n  +{0:#.#}% Explosive Resistance", Constants.PLUGIN_RES_BONUS * 100.0f);
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
        #endregion


    }
}
