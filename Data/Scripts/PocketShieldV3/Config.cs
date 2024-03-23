// ;
using ExShared;
using Sandbox.ModAPI;
using System;
using System.IO;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;

namespace PocketShield
{
    public enum NpcInventoryOperation
    {
        NoTouch = 1 << 0,
        RemoveEmitterOnly = 1 << 1,
        RemovePluginOnly = 1 << 2,
        RemoveEmitterAndPlugin = RemoveEmitterOnly | RemovePluginOnly,
    }

    internal static class Constants
    {
        public const ulong MOD_ID = 2656470280UL;
        public const string LOG_PREFIX = "PocketShield";

        public const string CONFIG_VERSION = "1";
        public const int LOG_LEVEL = 1;

        #region Shield Common Config
        public const float PLUGIN_CAP_BONUS = 0.15f;
        public const float PLUGIN_DEF_BONUS = 0.5f;
        public const float PLUGIN_RES_BONUS = 0.5f;

        // Creature Shield;
        public const float SHIELD_CRE_MAX_ENERGY = 100.0f;
        public const float SHIELD_CRE_DEF = 0.80f;
        public const float SHIELD_CRE_RES = -0.2f;
        public const float SHIELD_CRE_CHARGE_RATE = 20.0f;
        public const float SHIELD_CRE_CHARGE_DELAY = 1.0f;
        public const double SHIELD_CRE_POWER_CONSUMPTION = 0.0005;

        // Projectile Shield;
        public const float SHIELD_KI_MAX_ENERGY = 250.0f;
        public const float SHIELD_KI_DEF = 0.65f;
        public const float SHIELD_KI_RES = 0.15f;
        public const float SHIELD_KI_NON_DEF = 0.35f;
        public const float SHIELD_KI_NON_RES = -0.15f;
        public const float SHIELD_KI_CHARGE_RATE = 10.0f;
        public const float SHIELD_KI_CHARGE_DELAY = 4.0f;
        public const float SHIELD_KI_OVERCHARGE_TIME = 10.0f;
        public const float SHIELD_KI_OVERCHARGE_DEF_BONUS = 0.5f;
        public const float SHIELD_KI_OVERCHARGE_RES_BONUS = 0.75f;
        public const double SHIELD_KI_POWER_CONSUMPTION = 0.001;
        public const int SHIELD_KI_MAX_PLUGINS = 8;

        // Explosion Shield;
        public const float SHIELD_EX_MAX_ENERGY = 250.0f;
        public const float SHIELD_EX_DEF = 0.65f;
        public const float SHIELD_EX_RES = 0.15f;
        public const float SHIELD_EX_NON_DEF = 0.35f;
        public const float SHIELD_EX_NON_RES = -0.15f;
        public const float SHIELD_EX_CHARGE_RATE = 10.0f;
        public const float SHIELD_EX_CHARGE_DELAY = 4.0f;
        public const float SHIELD_EX_OVERCHARGE_TIME = 10.0f;
        public const float SHIELD_EX_OVERCHARGE_DEF_BONUS = 0.5f;
        public const float SHIELD_EX_OVERCHARGE_RES_BONUS = 0.75f;
        public const double SHIELD_EX_POWER_CONSUMPTION = 0.001;
        public const int SHIELD_EX_MAX_PLUGINS = 8;



        #endregion

        #region Strings
        public const string DAMAGETYPE_KI = "Bullet";
        public const string DAMAGETYPE_EX = "Explosion";
        public const string SUBTYPEID_EMITTER_CRE = "PocketShield_EmitterCreature";
        public const string SUBTYPEID_EMITTER_KI = "PocketShield_EmitterProjectile";
        public const string SUBTYPEID_EMITTER_EX = "PocketShield_EmitterExplosion";
        public const string SUBTYPEID_PLUGIN_CAP = "PocketShield_PluginCap";
        public const string SUBTYPEID_PLUGIN_DEF_KI = "PocketShield_PluginDefBullet";
        public const string SUBTYPEID_PLUGIN_DEF_EX = "PocketShield_PluginDefExplosion";
        public const string SUBTYPEID_PLUGIN_RES_KI = "PocketShield_PluginResBullet";
        public const string SUBTYPEID_PLUGIN_RES_EX = "PocketShield_PluginResExplosion";
        #endregion

        #region Texture
        public const int TEXTURE_ICON_1 = 1;

        public const int ICON_ATLAS_W = 3;
        public const int ICON_ATLAS_H = 3;

        public const float ICONS_ATLAS_UV_SIZE_X = 1.0f / ICON_ATLAS_W;
        public const float ICONS_ATLAS_UV_SIZE_Y = 1.0f / ICON_ATLAS_H;
        #endregion

    }



    public class ShieldConfig
    {
        public float MaxEnergy { get; set; } = 0.0f;
        public float Def { get; set; } = 0.0f;
        public float Res { get; set; } = 0.0f;
        public float NonDef { get; set; } = 0.0f;
        public float NonRes { get; set; } = 0.0f;
        public float ChargeRate { get; set; } = 0.0f;
        public float ChargeDelay { get; set; } = 0.0f;
        public float OverchargeTime { get; set; } = 0.0f;
        public float OverchargeDef { get; set; } = 0.0f;
        public float OverchargeRes { get; set; } = 0.0f;
        public double PowerCost { get; set; } = 0.0;
        public int MaxPlugins { get; set; } = 0;
    }

    public class ServerConfig : Config
    {
        protected const string c_SectionPlugins = "Plugins";
        protected const string c_SectionShieldCreature = "Creature Shield (Protect against Spider/Wolf)";
        protected const string c_SectionShieldBullet = "Projectile Shield (Protect against Projectiles)";
        protected const string c_SectionShieldExplosion = "Explosion Shield (Protect against Explosion)";

        protected const string c_NamePluginCap = "Plugin Capacity Bonus";
        protected const string c_NamePluginDef = "Plugin Defense Bonus";
        protected const string c_NamePluginRes = "Plugin Resistance Bonus";

        protected const string c_NameShieldEnergy = "Max Energy";
        protected const string c_NameShieldDef = "Defense";
        protected const string c_NameShieldRes = "Resistance";
        protected const string c_NameShieldChargeRate = "Charge Rate";
        protected const string c_NameShieldChargeDelay = "Charge Delay";
        protected const string c_NameShieldOverTime = "Overcharge Duration";
        protected const string c_NameShieldOverDef = "Overcharge Defense Bonus";
        protected const string c_NameShieldOverRes = "Overcharge Resistance Bonus";
        protected const string c_NameShieldPowerCost = "Power Consumption";
        protected const string c_NameShieldMaxPlugins = "Max Plugins Accepts";

        public ShieldConfig CreatureShieldConfig { get; set; } = new ShieldConfig();
        public ShieldConfig KineticShieldConfig { get; set; } = new ShieldConfig();
        public ShieldConfig ExplosionShieldConfig { get; set; } = new ShieldConfig();

        public float PluginCapBonus { get; set; } = 0.0f;
        public float PluginDefBonus { get; set; } = 0.0f;
        public float PluginResBonus { get; set; } = 0.0f;

        public ServerConfig(string _filename, Logger _logger) : base(_filename, _logger)
        {

        }

        protected override bool Invalidate(MyIni _iniData)
        {
            ConfigVersion = _iniData.Get(c_SectionCommon, c_NameConfigVersion).ToString(Constants.CONFIG_VERSION);
            LogLevel = _iniData.Get(c_SectionCommon, c_NameLogLevel).ToInt32(Constants.LOG_LEVEL);

            PluginCapBonus = (float)_iniData.Get(c_SectionPlugins, c_NamePluginCap).ToDouble(Constants.PLUGIN_CAP_BONUS);
            PluginDefBonus = (float)_iniData.Get(c_SectionPlugins, c_NamePluginDef).ToDouble(Constants.PLUGIN_DEF_BONUS);
            PluginResBonus = (float)_iniData.Get(c_SectionPlugins, c_NamePluginRes).ToDouble(Constants.PLUGIN_RES_BONUS);

            CreatureShieldConfig.MaxEnergy = (float)_iniData.Get(c_SectionShieldCreature, c_NameShieldEnergy).ToDouble(Constants.SHIELD_CRE_MAX_ENERGY);
            CreatureShieldConfig.Def = (float)_iniData.Get(c_SectionShieldCreature, c_NameShieldDef).ToDouble(Constants.SHIELD_CRE_DEF);
            CreatureShieldConfig.Res = (float)_iniData.Get(c_SectionShieldCreature, c_NameShieldRes).ToDouble(Constants.SHIELD_CRE_RES);
            CreatureShieldConfig.ChargeRate = (float)_iniData.Get(c_SectionShieldCreature, c_NameShieldChargeRate).ToDouble(Constants.SHIELD_CRE_CHARGE_RATE);
            CreatureShieldConfig.ChargeDelay = (float)_iniData.Get(c_SectionShieldCreature, c_NameShieldChargeDelay).ToDouble(Constants.SHIELD_CRE_CHARGE_DELAY);
            //CreatureShieldConfig.OverchargeTime = (float)_iniData.Get(c_SectionShieldCreature, c_NameShieldOverTime).ToDouble(0.0);
            //CreatureShieldConfig.OverchargeDef = (float)_iniData.Get(c_SectionShieldCreature, c_NameShieldOverDef).ToDouble(0.0);
            //CreatureShieldConfig.OverchargeRes = (float)_iniData.Get(c_SectionShieldCreature, c_NameShieldOverRes).ToDouble(0.0);
            CreatureShieldConfig.PowerCost = _iniData.Get(c_SectionShieldCreature, c_NameShieldPowerCost).ToDouble(Constants.SHIELD_CRE_POWER_CONSUMPTION);
            //CreatureShieldConfig.MaxPlugins = _iniData.Get(c_SectionShieldCreature, c_NameShieldMaxPlugins).ToInt32(0);

            KineticShieldConfig.MaxEnergy = (float)_iniData.Get(c_SectionShieldBullet, c_NameShieldEnergy).ToDouble(Constants.SHIELD_KI_MAX_ENERGY);
            KineticShieldConfig.Def = (float)_iniData.Get(c_SectionShieldBullet, c_NameShieldDef).ToDouble(Constants.SHIELD_KI_DEF);
            KineticShieldConfig.Res = (float)_iniData.Get(c_SectionShieldBullet, c_NameShieldRes).ToDouble(Constants.SHIELD_KI_RES);
            KineticShieldConfig.NonDef = Constants.SHIELD_KI_NON_DEF;
            KineticShieldConfig.NonRes = Constants.SHIELD_KI_NON_RES;
            KineticShieldConfig.ChargeRate = (float)_iniData.Get(c_SectionShieldBullet, c_NameShieldChargeRate).ToDouble(Constants.SHIELD_KI_CHARGE_RATE);
            KineticShieldConfig.ChargeDelay = (float)_iniData.Get(c_SectionShieldBullet, c_NameShieldChargeDelay).ToDouble(Constants.SHIELD_KI_CHARGE_DELAY);
            KineticShieldConfig.OverchargeTime = (float)_iniData.Get(c_SectionShieldBullet, c_NameShieldOverTime).ToDouble(Constants.SHIELD_KI_OVERCHARGE_TIME);
            KineticShieldConfig.OverchargeDef = (float)_iniData.Get(c_SectionShieldBullet, c_NameShieldOverDef).ToDouble(Constants.SHIELD_KI_OVERCHARGE_DEF_BONUS);
            KineticShieldConfig.OverchargeRes = (float)_iniData.Get(c_SectionShieldBullet, c_NameShieldOverRes).ToDouble(Constants.SHIELD_KI_OVERCHARGE_RES_BONUS);
            KineticShieldConfig.PowerCost = _iniData.Get(c_SectionShieldBullet, c_NameShieldPowerCost).ToDouble(Constants.SHIELD_KI_POWER_CONSUMPTION);
            KineticShieldConfig.MaxPlugins = _iniData.Get(c_SectionShieldBullet, c_NameShieldMaxPlugins).ToInt32(Constants.SHIELD_KI_MAX_PLUGINS);

            ExplosionShieldConfig.MaxEnergy = (float)_iniData.Get(c_SectionShieldExplosion, c_NameShieldEnergy).ToDouble(Constants.SHIELD_EX_MAX_ENERGY);
            ExplosionShieldConfig.Def = (float)_iniData.Get(c_SectionShieldExplosion, c_NameShieldDef).ToDouble(Constants.SHIELD_EX_DEF);
            ExplosionShieldConfig.Res = (float)_iniData.Get(c_SectionShieldExplosion, c_NameShieldRes).ToDouble(Constants.SHIELD_EX_RES);
            ExplosionShieldConfig.NonDef = Constants.SHIELD_EX_NON_DEF;
            ExplosionShieldConfig.NonRes = Constants.SHIELD_EX_NON_RES;
            ExplosionShieldConfig.ChargeRate = (float)_iniData.Get(c_SectionShieldExplosion, c_NameShieldChargeRate).ToDouble(Constants.SHIELD_EX_CHARGE_RATE);
            ExplosionShieldConfig.ChargeDelay = (float)_iniData.Get(c_SectionShieldExplosion, c_NameShieldChargeDelay).ToDouble(Constants.SHIELD_EX_CHARGE_DELAY);
            ExplosionShieldConfig.OverchargeTime = (float)_iniData.Get(c_SectionShieldExplosion, c_NameShieldOverTime).ToDouble(Constants.SHIELD_EX_OVERCHARGE_TIME);
            ExplosionShieldConfig.OverchargeDef = (float)_iniData.Get(c_SectionShieldExplosion, c_NameShieldOverDef).ToDouble(Constants.SHIELD_EX_OVERCHARGE_DEF_BONUS);
            ExplosionShieldConfig.OverchargeRes = (float)_iniData.Get(c_SectionShieldExplosion, c_NameShieldOverRes).ToDouble(Constants.SHIELD_EX_OVERCHARGE_RES_BONUS);
            ExplosionShieldConfig.PowerCost = _iniData.Get(c_SectionShieldExplosion, c_NameShieldPowerCost).ToDouble(Constants.SHIELD_EX_POWER_CONSUMPTION);
            ExplosionShieldConfig.MaxPlugins = _iniData.Get(c_SectionShieldExplosion, c_NameShieldMaxPlugins).ToInt32(Constants.SHIELD_EX_MAX_PLUGINS);

            if (ConfigVersion != Constants.CONFIG_VERSION)
            {
                m_Logger.WriteLine("  Config version mismatch: read " + ConfigVersion + ", newest version " + Constants.CONFIG_VERSION);
                return false;
            }
            else
            {
                return true;
            }
        }

        public override void PackIniData(ref MyIni _iniData)
        {
            base.PackIniData(ref _iniData);

            _iniData.Set(c_SectionCommon, c_NameConfigVersion, ConfigVersion);
            _iniData.Set(c_SectionCommon, c_NameLogLevel, LogLevel);

            _iniData.Set(c_SectionShieldCreature, c_NameShieldEnergy, CreatureShieldConfig.MaxEnergy);
            _iniData.Set(c_SectionShieldCreature, c_NameShieldDef, CreatureShieldConfig.Def);
            _iniData.Set(c_SectionShieldCreature, c_NameShieldRes, CreatureShieldConfig.Res);
            _iniData.Set(c_SectionShieldCreature, c_NameShieldChargeRate, CreatureShieldConfig.ChargeRate);
            _iniData.Set(c_SectionShieldCreature, c_NameShieldChargeDelay, CreatureShieldConfig.ChargeDelay);
            //_iniData.Set(c_SectionShieldCreature, c_NameShieldOverTime, CreatureShieldConfig.OverchargeTime);
            //_iniData.Set(c_SectionShieldCreature, c_NameShieldOverDef, CreatureShieldConfig.OverchargeDef);
            //_iniData.Set(c_SectionShieldCreature, c_NameShieldOverRes, CreatureShieldConfig.OverchargeRes);
            _iniData.Set(c_SectionShieldCreature, c_NameShieldPowerCost, CreatureShieldConfig.PowerCost);
            //_iniData.Set(c_SectionShieldCreature, c_NameShieldMaxPlugins, CreatureShieldConfig.MaxPlugins);

            _iniData.Set(c_SectionShieldBullet, c_NameShieldEnergy, KineticShieldConfig.MaxEnergy);
            _iniData.Set(c_SectionShieldBullet, c_NameShieldDef, KineticShieldConfig.Def);
            _iniData.Set(c_SectionShieldBullet, c_NameShieldRes, KineticShieldConfig.Res);
            _iniData.Set(c_SectionShieldBullet, c_NameShieldChargeRate, KineticShieldConfig.ChargeRate);
            _iniData.Set(c_SectionShieldBullet, c_NameShieldChargeDelay, KineticShieldConfig.ChargeDelay);
            _iniData.Set(c_SectionShieldBullet, c_NameShieldOverTime, KineticShieldConfig.OverchargeTime);
            _iniData.Set(c_SectionShieldBullet, c_NameShieldOverDef, KineticShieldConfig.OverchargeDef);
            _iniData.Set(c_SectionShieldBullet, c_NameShieldOverRes, KineticShieldConfig.OverchargeRes);
            _iniData.Set(c_SectionShieldBullet, c_NameShieldPowerCost, KineticShieldConfig.PowerCost);
            _iniData.Set(c_SectionShieldBullet, c_NameShieldMaxPlugins, KineticShieldConfig.MaxPlugins);

            _iniData.Set(c_SectionShieldExplosion, c_NameShieldEnergy, ExplosionShieldConfig.MaxEnergy);
            _iniData.Set(c_SectionShieldExplosion, c_NameShieldDef, ExplosionShieldConfig.Def);
            _iniData.Set(c_SectionShieldExplosion, c_NameShieldRes, ExplosionShieldConfig.Res);
            _iniData.Set(c_SectionShieldExplosion, c_NameShieldChargeRate, ExplosionShieldConfig.ChargeRate);
            _iniData.Set(c_SectionShieldExplosion, c_NameShieldChargeDelay, ExplosionShieldConfig.ChargeDelay);
            _iniData.Set(c_SectionShieldExplosion, c_NameShieldOverTime, ExplosionShieldConfig.OverchargeTime);
            _iniData.Set(c_SectionShieldExplosion, c_NameShieldOverDef, ExplosionShieldConfig.OverchargeDef);
            _iniData.Set(c_SectionShieldExplosion, c_NameShieldOverRes, ExplosionShieldConfig.OverchargeRes);
            _iniData.Set(c_SectionShieldExplosion, c_NameShieldPowerCost, ExplosionShieldConfig.PowerCost);
            _iniData.Set(c_SectionShieldExplosion, c_NameShieldMaxPlugins, ExplosionShieldConfig.MaxPlugins);



        }

    }
}

#region Shared
namespace ExShared
{
    public abstract class Config
    {
        protected const string c_SectionCommon = "Common";

        protected const string c_NameConfigVersion = "Config Version (Do not touch this plz)";
        protected const string c_NameLogLevel = "Log Level";

        protected const string c_CommentLogLevel = "Log Level scales from -1 (disable logging) to 5 (log everything)";

        public string ConfigVersion { get; set; }
        public int LogLevel { get; set; }


        public string Filename { get; protected set; }
        protected Logger m_Logger = null;

        protected Config(string _filename, Logger _logger)
        {
            Filename = _filename;
            m_Logger = _logger;
            if (!LoadConfigFile())
            {
                //SaveConfigFile();
            }
        }

        protected abstract bool Invalidate(MyIni _iniData);

        public virtual void PackIniData(ref MyIni _iniData)
        {
            _iniData.Set(c_SectionCommon, c_NameConfigVersion, ConfigVersion);
            _iniData.Set(c_SectionCommon, c_NameLogLevel, LogLevel);

            _iniData.Set(c_SectionCommon, c_NameLogLevel, c_CommentLogLevel);
        }

        public string PeekConfigFile()
        {
            try
            {
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(Filename, typeof(Config)))
                {
                    TextReader reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(Filename, typeof(Config));
                    string data = reader.ReadToEnd();
                    reader.Close();

                    return data;
                }
                else
                {
                    m_Logger.WriteLine("  Config file not found");
                    return null;
                }
            }
            catch (Exception _e)
            {
                m_Logger.WriteLine("  > Exception < Error during loading Config: " + _e.Message);
                return null;
            }
        }

        public bool LoadConfigFile()
        {
            m_Logger.WriteLine("Loading config..");

            string strData = PeekConfigFile();
            //if (string.IsNullOrEmpty(strData))
            //    return false;

            MyIni iniData = new MyIni();
            MyIniParseResult parseResult;
            if (!iniData.TryParse(strData, out parseResult))
            {
                m_Logger.WriteLine("  Data loaded successfully, but could not be parsed");
                m_Logger.WriteLine("    " + parseResult.ToString());
            }
            if (!Invalidate(iniData) || string.IsNullOrEmpty(strData))
            {
                SaveConfigFile();
                if (!string.IsNullOrEmpty(strData))
                {
                    m_Logger.WriteLine("  Saving backup data..");
                    SaveBackupConfigFile(strData);
                }
            }



            m_Logger.WriteLine("Loading done");
            return true;
        }

        public bool SaveConfigFile()
        {
            m_Logger.WriteLine("Saving config..");

            MyIni iniData = new MyIni();
            PackIniData(ref iniData);
            string data = iniData.ToString();
            try
            {
                TextWriter writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(Filename, typeof(Config));
                writer.WriteLine(data);
                writer.Close();
            }
            catch (Exception _e)
            {
                m_Logger.WriteLine("  > Exception < Error during saving Config: " + _e.Message);
                return false;
            }

            m_Logger.WriteLine("Saving done");
            return true;
        }

        private bool SaveBackupConfigFile(string _data)
        {
            string filename = Filename.Insert(Filename.Length - 4, "_old");
            try
            {
                TextWriter writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(filename, typeof(Config));
                writer.WriteLine(_data);
                writer.Close();
            }
            catch (Exception _e)
            {
                m_Logger.WriteLine("  > Exception < Error during saving backup file (you are out of luck): " + _e.Message);
                return false;
            }

            return true;
        }


    }
}
#endregion
