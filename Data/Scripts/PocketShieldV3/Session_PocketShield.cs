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

        Logger m_Logger = null;

        public override void LoadData()
        {
            m_Logger = new Logger("");

        }

        protected override void UnloadData()
        {
            /* REQUIRED!
             * You need to call PocketShieldAPI.Close() when you are done using it (typically on mod unload).
             * You can check core mod (PocketShieldCore)'s log for information on which mod forgot to Close.
             */
            PocketShieldAPI.Close();
            
            m_Logger.Close();
        }

        public override void BeforeStart()
        {
            string modInfo = ModContext.ModId + "." + ModContext.ModName;
            m_Logger.WriteLine("ModInfo: " + modInfo);
            
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
                PocketShieldAPI.SetPluginModifier(MyStringHash.GetOrCompute(Constants.SUBTYPEID_PLUGIN_CAP), Constants.PLUGIN_CAP_BONUS);
                PocketShieldAPI.SetPluginModifier(MyStringHash.GetOrCompute(Constants.SUBTYPEID_PLUGIN_DEF_KI), Constants.PLUGIN_DEF_BONUS);
                PocketShieldAPI.SetPluginModifier(MyStringHash.GetOrCompute(Constants.SUBTYPEID_PLUGIN_DEF_EX), Constants.PLUGIN_DEF_BONUS);
                PocketShieldAPI.SetPluginModifier(MyStringHash.GetOrCompute(Constants.SUBTYPEID_PLUGIN_RES_KI), Constants.PLUGIN_RES_BONUS);
                PocketShieldAPI.SetPluginModifier(MyStringHash.GetOrCompute(Constants.SUBTYPEID_PLUGIN_RES_EX), Constants.PLUGIN_RES_BONUS);

                UpdateBlueprintData();

                m_Logger.WriteLine("PocketShield registered with Server");
            }

            if (_returnSide == PocketShieldAPI.ReturnSide.Client)
            {
                Vector2 uvSizeShieldIcon = new Vector2(Constants.ICONS_ATLAS_UV_SIZE_X, Constants.ICONS_ATLAS_UV_SIZE_Y);
                Vector2 uvSizeStatIcon = new Vector2(2.0f * Constants.ICONS_ATLAS_UV_SIZE_X, Constants.ICONS_ATLAS_UV_SIZE_Y);

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
                    UvSize = uvSizeShieldIcon,
                    UvOffset = new Vector2(1.0f * Constants.ICONS_ATLAS_UV_SIZE_X, 0.0f * Constants.ICONS_ATLAS_UV_SIZE_Y)
                });
                PocketShieldAPI.RegisterShieldIcon(new PocketShieldAPI.ShieldIconDrawInfo(null)
                {
                    Material = MyStringId.GetOrCompute("PocketShieldV3_ShieldIcons"),
                    SubtypeId = MyStringHash.GetOrCompute(Constants.SUBTYPEID_EMITTER_ADV),
                    UvEnabled = true,
                    UvSize = uvSizeShieldIcon,
                    UvOffset = new Vector2(2.0f * Constants.ICONS_ATLAS_UV_SIZE_X, 0.0f * Constants.ICONS_ATLAS_UV_SIZE_Y)
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
                    UvSize = uvSizeStatIcon,
                    UvOffset = new Vector2(0.0f * Constants.ICONS_ATLAS_UV_SIZE_X, 1.0f * Constants.ICONS_ATLAS_UV_SIZE_Y),
                });
                PocketShieldAPI.RegisterStatIcons(new PocketShieldAPI.ItemCardDrawInfo(null)
                {
                    Material = MyStringId.GetOrCompute("PocketShieldV3_ShieldIcons"),
                    DamageType = MyStringHash.GetOrCompute(Constants.DAMAGETYPE_EX),
                    UvEnabled = true,
                    UvSize = uvSizeStatIcon,
                    UvOffset = new Vector2(0.0f * Constants.ICONS_ATLAS_UV_SIZE_X, 2.0f * Constants.ICONS_ATLAS_UV_SIZE_Y),
                });

                m_Logger.WriteLine("PocketShield registered with Client");
            }

            m_Logger.WriteLine("Status: Server = " + PocketShieldAPI.ServerReady + ", Client = " + PocketShieldAPI.ClientReady);
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

            m_CachedProperties.MaxPluginsCount = Constants.SHIELD_BAS_MAX_PLUGINS;
            m_CachedProperties.BaseMaxEnergy = Constants.SHIELD_BAS_MAX_ENERGY;
            m_CachedProperties.BaseChargeRate = Constants.SHIELD_BAS_CHARGE_RATE;
            m_CachedProperties.BaseChargeDelay = Constants.SHIELD_BAS_CHARGE_DELAY;
            m_CachedProperties.BaseOverchargeDuration = Constants.SHIELD_BAS_OVERCHARGE_TIME;
            m_CachedProperties.BaseOverchargeDefBonus = Constants.SHIELD_BAS_OVERCHARGE_DEF_BONUS;
            m_CachedProperties.BaseOverchargeResBonus = Constants.SHIELD_BAS_OVERCHARGE_RES_BONUS;
            m_CachedProperties.BasePowerConsumption = Constants.SHIELD_BAS_POWER_CONSUMPTION;

            m_CachedProperties.BaseDef[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_KI)] = Constants.SHIELD_BAS_DEF;
            m_CachedProperties.BaseDef[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_EX)] = Constants.SHIELD_BAS_DEF;
            m_CachedProperties.BaseRes[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_KI)] = Constants.SHIELD_BAS_RES;
            m_CachedProperties.BaseRes[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_EX)] = Constants.SHIELD_BAS_RES;

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

            m_CachedProperties.MaxPluginsCount = Constants.SHIELD_ADV_MAX_PLUGINS;
            m_CachedProperties.BaseMaxEnergy = Constants.SHIELD_ADV_MAX_ENERGY;
            m_CachedProperties.BaseChargeRate = Constants.SHIELD_ADV_CHARGE_RATE;
            m_CachedProperties.BaseChargeDelay = Constants.SHIELD_ADV_CHARGE_DELAY;
            m_CachedProperties.BaseOverchargeDuration = Constants.SHIELD_ADV_OVERCHARGE_TIME;
            m_CachedProperties.BaseOverchargeDefBonus = Constants.SHIELD_ADV_OVERCHARGE_DEF_BONUS;
            m_CachedProperties.BaseOverchargeResBonus = Constants.SHIELD_ADV_OVERCHARGE_RES_BONUS;
            m_CachedProperties.BasePowerConsumption = Constants.SHIELD_ADV_POWER_CONSUMPTION;

            m_CachedProperties.BaseDef[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_KI)] = Constants.SHIELD_ADV_DEF;
            m_CachedProperties.BaseDef[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_EX)] = Constants.SHIELD_ADV_DEF;
            m_CachedProperties.BaseRes[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_KI)] = Constants.SHIELD_ADV_RES;
            m_CachedProperties.BaseRes[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_EX)] = Constants.SHIELD_ADV_RES;

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
