// ;
using ExShared;
using PocketShieldCore;
using Sandbox.Definitions;
using SpaceEngineers.Game.Entities.Blocks;
using SpaceEngineers.Game.World;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;
using VRage.Utils;
using VRageMath;

namespace PocketShield
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public partial class Session_PocketShield : MySessionComponentBase
    {
        ServerConfig m_Config = null;
        Logger m_Logger = null;

        public override void LoadData()
        {
            m_Logger = new Logger("");
            m_Config = new ServerConfig("config.ini", m_Logger);

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
                PocketShieldAPIV2.ShieldEmitterProperties creatureEmitterProp = new PocketShieldAPIV2.ShieldEmitterProperties(null)
                {
                    SubtypeId = MyStringHash.GetOrCompute(Constants.SUBTYPEID_EMITTER_CRE),
                    IsManual = false,
                    BaseMaxEnergy = m_Config.CreatureShieldConfig.MaxEnergy,
                    BaseChargeRate = m_Config.CreatureShieldConfig.ChargeRate,
                    BaseChargeDelay = m_Config.CreatureShieldConfig.ChargeDelay,
                    BaseOverchargeDuration = m_Config.CreatureShieldConfig.OverchargeTime,
                    BaseOverchargeDefBonus = m_Config.CreatureShieldConfig.OverchargeDef,
                    BaseOverchargeResBonus = m_Config.CreatureShieldConfig.OverchargeRes,
                    BasePowerConsumption = m_Config.CreatureShieldConfig.PowerCost,
                    MaxPluginsCount = m_Config.CreatureShieldConfig.MaxPlugins,
                };
                creatureEmitterProp.BaseDef[MyDamageType.Wolf] = m_Config.CreatureShieldConfig.Def;
                creatureEmitterProp.BaseDef[MyDamageType.Spider] = m_Config.CreatureShieldConfig.Def;
                creatureEmitterProp.BaseDef[MyDamageType.Fall] = 0.75f;
                creatureEmitterProp.BaseDef[MyDamageType.Environment] = 0.75f;
                creatureEmitterProp.BaseRes[MyDamageType.Wolf] = m_Config.CreatureShieldConfig.Res;
                creatureEmitterProp.BaseRes[MyDamageType.Spider] = m_Config.CreatureShieldConfig.Res;
                creatureEmitterProp.BaseRes[MyDamageType.Fall] = 0.25f;
                creatureEmitterProp.BaseRes[MyDamageType.Environment] = 0.25f;
                
                PocketShieldAPIV2.ShieldEmitterProperties kiEmitterProp = new PocketShieldAPIV2.ShieldEmitterProperties(null)
                {
                    SubtypeId = MyStringHash.GetOrCompute(Constants.SUBTYPEID_EMITTER_KI),
                    IsManual = false,
                    BaseMaxEnergy = m_Config.KineticShieldConfig.MaxEnergy,
                    BaseChargeRate = m_Config.KineticShieldConfig.ChargeRate,
                    BaseChargeDelay = m_Config.KineticShieldConfig.ChargeDelay,
                    BaseOverchargeDuration = m_Config.KineticShieldConfig.OverchargeTime,
                    BaseOverchargeDefBonus = m_Config.KineticShieldConfig.OverchargeDef,
                    BaseOverchargeResBonus = m_Config.KineticShieldConfig.OverchargeRes,
                    BasePowerConsumption = m_Config.KineticShieldConfig.PowerCost,
                    MaxPluginsCount = m_Config.KineticShieldConfig.MaxPlugins,
                };
                kiEmitterProp.BaseDef[MyDamageType.Bullet] = m_Config.KineticShieldConfig.Def;
                kiEmitterProp.BaseDef[MyDamageType.Explosion] = m_Config.KineticShieldConfig.NonDef;
                kiEmitterProp.BaseDef[MyDamageType.Fall] = 0.75f;
                kiEmitterProp.BaseDef[MyDamageType.Environment] = 0.75f;
                kiEmitterProp.BaseRes[MyDamageType.Bullet] = m_Config.KineticShieldConfig.Res;
                kiEmitterProp.BaseRes[MyDamageType.Explosion] = m_Config.KineticShieldConfig.NonRes;
                kiEmitterProp.BaseRes[MyDamageType.Fall] = 0.25f;
                kiEmitterProp.BaseRes[MyDamageType.Environment] = 0.25f;

                PocketShieldAPIV2.ShieldEmitterProperties exEmitterProp = new PocketShieldAPIV2.ShieldEmitterProperties(null)
                {
                    SubtypeId = MyStringHash.GetOrCompute(Constants.SUBTYPEID_EMITTER_EX),
                    IsManual = false,
                    BaseMaxEnergy = m_Config.ExplosionShieldConfig.MaxEnergy,
                    BaseChargeRate = m_Config.ExplosionShieldConfig.ChargeRate,
                    BaseChargeDelay = m_Config.ExplosionShieldConfig.ChargeDelay,
                    BaseOverchargeDuration = m_Config.ExplosionShieldConfig.OverchargeTime,
                    BaseOverchargeDefBonus = m_Config.ExplosionShieldConfig.OverchargeDef,
                    BaseOverchargeResBonus = m_Config.ExplosionShieldConfig.OverchargeRes,
                    BasePowerConsumption = m_Config.ExplosionShieldConfig.PowerCost,
                    MaxPluginsCount = m_Config.ExplosionShieldConfig.MaxPlugins,
                };
                exEmitterProp.BaseDef[MyDamageType.Bullet] = m_Config.ExplosionShieldConfig.NonDef;
                exEmitterProp.BaseDef[MyDamageType.Explosion] = m_Config.ExplosionShieldConfig.Def;
                exEmitterProp.BaseDef[MyDamageType.Fall] = 0.75f;
                exEmitterProp.BaseDef[MyDamageType.Environment] = 0.75f;
                exEmitterProp.BaseRes[MyDamageType.Bullet] = m_Config.ExplosionShieldConfig.NonRes;
                exEmitterProp.BaseRes[MyDamageType.Explosion] = m_Config.ExplosionShieldConfig.Res;
                exEmitterProp.BaseRes[MyDamageType.Fall] = 0.25f;
                exEmitterProp.BaseRes[MyDamageType.Environment] = 0.25f;
                #endregion

                /* You can submit properties for your ShieldEmitter.
                 * These data will be used when creating a ShieldEmitter. If no data is submit (for a certain SubtypeID),
                 * that Emitter will not be created.
                 */
                PocketShieldAPIV2.Server_RegisterEmitter(MyStringHash.GetOrCompute(Constants.SUBTYPEID_EMITTER_CRE), creatureEmitterProp);
                PocketShieldAPIV2.Server_RegisterEmitter(MyStringHash.GetOrCompute(Constants.SUBTYPEID_EMITTER_KI), kiEmitterProp);
                PocketShieldAPIV2.Server_RegisterEmitter(MyStringHash.GetOrCompute(Constants.SUBTYPEID_EMITTER_EX), exEmitterProp);

                /* You can set a Plugin's bonus value.
                 * You can even override a Plugin's bonus value with your value.
                 */
                PocketShieldAPIV2.Server_SetPluginModifier(MyStringHash.GetOrCompute(Constants.SUBTYPEID_PLUGIN_CAP), PocketShieldAPIV2.PluginModType.Capacity, Constants.PLUGIN_CAP_BONUS);
                PocketShieldAPIV2.Server_SetPluginModifier(MyStringHash.GetOrCompute(Constants.SUBTYPEID_PLUGIN_DEF_KI), PocketShieldAPIV2.PluginModType.DefBullet, Constants.PLUGIN_DEF_BONUS);
                PocketShieldAPIV2.Server_SetPluginModifier(MyStringHash.GetOrCompute(Constants.SUBTYPEID_PLUGIN_RES_KI), PocketShieldAPIV2.PluginModType.ResBullet, Constants.PLUGIN_RES_BONUS);
                PocketShieldAPIV2.Server_SetPluginModifier(MyStringHash.GetOrCompute(Constants.SUBTYPEID_PLUGIN_DEF_EX), PocketShieldAPIV2.PluginModType.DefExplosion, Constants.PLUGIN_DEF_BONUS);
                PocketShieldAPIV2.Server_SetPluginModifier(MyStringHash.GetOrCompute(Constants.SUBTYPEID_PLUGIN_RES_EX), PocketShieldAPIV2.PluginModType.ResExplosion, Constants.PLUGIN_RES_BONUS);
                //PocketShieldAPIV2.Server_SetPluginModifier(MyStringHash.GetOrCompute(Constants.SUBTYPEID_PLUGIN_DEF_RES_ENV), Constants.PLUGIN_ENV_BONUS);

                UpdateBlueprintData();

                m_Logger.WriteLine("PocketShield registered with Server");
            }

            if (_returnSide == PocketShieldAPIV2.ReturnSide.Client)
            {
                #region Local Variable
                Vector2 uvSizeShieldIcon = new Vector2(Constants.ICONS_ATLAS_UV_SIZE_X, Constants.ICONS_ATLAS_UV_SIZE_Y);
                Vector2 uvSizeStatIcon = new Vector2(2.0f * Constants.ICONS_ATLAS_UV_SIZE_X, Constants.ICONS_ATLAS_UV_SIZE_Y);

                PocketShieldAPIV2.ShieldIconDrawInfo creatureEmitterShieldIcon = new PocketShieldAPIV2.ShieldIconDrawInfo(null)
                {
                    Material = MyStringId.GetOrCompute("PocketShieldV3_ShieldIcons"),
                    SubtypeId = MyStringHash.GetOrCompute(Constants.SUBTYPEID_EMITTER_CRE),
                    UvEnabled = true,
                    UvSize = uvSizeShieldIcon,
                    UvOffset = new Vector2(0.0f * Constants.ICONS_ATLAS_UV_SIZE_X, 0.0f * Constants.ICONS_ATLAS_UV_SIZE_Y)
                };
                PocketShieldAPIV2.ShieldIconDrawInfo kiEmitterShieldIcon = new PocketShieldAPIV2.ShieldIconDrawInfo(null)
                {
                    Material = MyStringId.GetOrCompute("PocketShieldV3_ShieldIcons"),
                    SubtypeId = MyStringHash.GetOrCompute(Constants.SUBTYPEID_EMITTER_KI),
                    UvEnabled = true,
                    UvSize = uvSizeShieldIcon,
                    UvOffset = new Vector2(1.0f * Constants.ICONS_ATLAS_UV_SIZE_X, 0.0f * Constants.ICONS_ATLAS_UV_SIZE_Y)
                };
                PocketShieldAPIV2.ShieldIconDrawInfo exEmitterShieldIcon = new PocketShieldAPIV2.ShieldIconDrawInfo(null)
                {
                    Material = MyStringId.GetOrCompute("PocketShieldV3_ShieldIcons"),
                    SubtypeId = MyStringHash.GetOrCompute(Constants.SUBTYPEID_EMITTER_EX),
                    UvEnabled = true,
                    UvSize = uvSizeShieldIcon,
                    UvOffset = new Vector2(2.0f * Constants.ICONS_ATLAS_UV_SIZE_X, 0.0f * Constants.ICONS_ATLAS_UV_SIZE_Y)
                };

                PocketShieldAPIV2.StatIconDrawInfo kiEmitterStatIcon = new PocketShieldAPIV2.StatIconDrawInfo(null)
                {
                    Material = MyStringId.GetOrCompute("PocketShieldV3_ShieldIcons"),
                    DamageType = MyStringHash.GetOrCompute(Constants.DAMAGETYPE_KI),
                    UvEnabled = true,
                    UvSize = uvSizeStatIcon,
                    UvOffset = new Vector2(0.0f * Constants.ICONS_ATLAS_UV_SIZE_X, 1.0f * Constants.ICONS_ATLAS_UV_SIZE_Y),
                };
                PocketShieldAPIV2.StatIconDrawInfo exEmitterStatIcon = new PocketShieldAPIV2.StatIconDrawInfo(null)
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
                PocketShieldAPIV2.Client_RegisterShieldIcon(creatureEmitterShieldIcon);
                PocketShieldAPIV2.Client_RegisterShieldIcon(kiEmitterShieldIcon);
                PocketShieldAPIV2.Client_RegisterShieldIcon(exEmitterShieldIcon);

                /* FOR HUD DISPLAY!
                 * You can register your custom icons for Defense and Resistance stat that will be drawn on HUD Panel.
                 * This is not required. If you don't register one, nothing will be drawn there.
                 * You can register a whole texture, or a texture atlas and supply uv information.
                 */
                PocketShieldAPIV2.Client_RegisterStatIcon(kiEmitterStatIcon);
                PocketShieldAPIV2.Client_RegisterStatIcon(exEmitterStatIcon);

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
            // Creature Emitter;
            {
                string extraTooltip = "Stat:\n  Capacity: " + m_Config.CreatureShieldConfig.MaxEnergy;
                if (m_Config.CreatureShieldConfig.Def != 0.0f || m_Config.CreatureShieldConfig.Res != 0.0f)
                {
                    extraTooltip += string.Format(
                        "\n  Against Creatures: Defense {0:0.#}%, Resistance {1:0.#}%",
                        m_Config.CreatureShieldConfig.Def * 100.0f, m_Config.CreatureShieldConfig.Res * 100.0f);
                }

                cachedStats[Constants.SUBTYPEID_EMITTER_CRE] = extraTooltip;
            }

            // Bullet Emitter;
            {
                string extraTooltip = "Stat:\n  Capacity: " + m_Config.KineticShieldConfig.MaxEnergy;
                if (m_Config.KineticShieldConfig.Def != 0.0f || m_Config.KineticShieldConfig.Res != 0.0f)
                {
                    extraTooltip += string.Format(
                        "\n  Against Bullet: Defense {0:0.#}%, Resistance {1:0.#}%" +
                        "\n  Against Explosion: Defense {2:0.#}%, Resistance {3:0.#}%",
                        m_Config.KineticShieldConfig.Def * 100.0f, m_Config.KineticShieldConfig.Res * 100.0f,
                        m_Config.KineticShieldConfig.NonDef * 100.0f, m_Config.KineticShieldConfig.NonRes * 100.0f);
                }
                if (m_Config.KineticShieldConfig.MaxPlugins > 0)
                    extraTooltip += "\n  Plugin slots: " + m_Config.KineticShieldConfig.MaxPlugins;

                cachedStats[Constants.SUBTYPEID_EMITTER_KI] = extraTooltip;
            }

            // Explosion Emitter;
            {
                string extraTooltip = "Stat:\n  Capacity: " + m_Config.ExplosionShieldConfig.MaxEnergy;
                if (m_Config.ExplosionShieldConfig.Def != 0.0f || m_Config.ExplosionShieldConfig.Res != 0.0f)
                {
                    extraTooltip += string.Format(
                        "\n  Against Bullet: Defense {0:0.#}%, Resistance {1:0.#}%" +
                        "\n  Against Explosion: Defense {2:0.#}%, Resistance {3:0.#}%",
                        m_Config.ExplosionShieldConfig.Def * 100.0f, m_Config.ExplosionShieldConfig.Res * 100.0f,
                        m_Config.ExplosionShieldConfig.NonDef * 100.0f, m_Config.ExplosionShieldConfig.NonRes * 100.0f);
                }
                if (m_Config.ExplosionShieldConfig.MaxPlugins > 0)
                    extraTooltip += "\n  Plugin slots: " + m_Config.ExplosionShieldConfig.MaxPlugins;

                cachedStats[Constants.SUBTYPEID_EMITTER_EX] = extraTooltip;
            }

            // Plugins;
            {
                cachedStats[Constants.SUBTYPEID_PLUGIN_CAP] = string.Format("Stat:\n  +{0:#.#}% Capacity", m_Config.PluginCapBonus * 100.0f);
                cachedStats[Constants.SUBTYPEID_PLUGIN_DEF_KI] = string.Format("Stat:\n  +{0:#.#}% Bullet Defense", m_Config.PluginDefBonus * 100.0f);
                cachedStats[Constants.SUBTYPEID_PLUGIN_DEF_EX] = string.Format("Stat:\n  +{0:#.#}% Explosive Defense", m_Config.PluginDefBonus * 100.0f);
                cachedStats[Constants.SUBTYPEID_PLUGIN_RES_KI] = string.Format("Stat:\n  +{0:#.#}% Bullet Resistance", m_Config.PluginResBonus * 100.0f);
                cachedStats[Constants.SUBTYPEID_PLUGIN_RES_EX] = string.Format("Stat:\n  +{0:#.#}% Explosive Resistance", m_Config.PluginResBonus * 100.0f);
            }
            #endregion

            #region Cache Names 
            Dictionary<string, string> cachedNames = new Dictionary<string, string>();
            cachedNames[Constants.SUBTYPEID_EMITTER_CRE] = "PS Creature Shield Emitter";
            cachedNames[Constants.SUBTYPEID_EMITTER_KI] = "PS Projectile Shield Emitter";
            cachedNames[Constants.SUBTYPEID_EMITTER_EX] = "PS Explosion Shield Emitter";
            cachedNames[Constants.SUBTYPEID_PLUGIN_CAP] = "PS Capacity Plugin";
            cachedNames[Constants.SUBTYPEID_PLUGIN_DEF_KI] = "PS Bullet Defense Plugin";
            cachedNames[Constants.SUBTYPEID_PLUGIN_DEF_EX] = "PS Explosion Defense Plugin";
            cachedNames[Constants.SUBTYPEID_PLUGIN_RES_KI] = "PS Bullet Resistance Plugin";
            cachedNames[Constants.SUBTYPEID_PLUGIN_RES_EX] = "PS Explosion Resistance Plugin";
            #endregion

            #region Cache Descriptions
            Dictionary<string, string> cachedDescs = new Dictionary<string, string>();
            cachedDescs[Constants.SUBTYPEID_EMITTER_CRE] = "Basic PocketShield Emitter that provides protection against Creatures (Wolf, Spider).";
            cachedDescs[Constants.SUBTYPEID_EMITTER_KI] = "Basic PocketShield Emitter that provides protection against Projectile, but weak against Explosion.";
            cachedDescs[Constants.SUBTYPEID_EMITTER_EX] = "Advanced PocketShield Emitter that provides protection against Explosion, but weak against Projectile.";
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
