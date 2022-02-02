// ;
using ExShared;
using Sandbox.ModAPI;
using System;
using System.IO;

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
        public const float PLUGIN_POWER_BONUS = 0.0f;

        public const int SHIELD_BAS_MAX_PLUGINS = 1; // Max allowed Plugins for Basic Emitter;
        public const float SHIELD_BAS_MAX_ENERGY = 50.0f;
        public const float SHIELD_BAS_DEF = 0.25f;
        public const float SHIELD_BAS_RES = 0.0f;
        public const float SHIELD_BAS_CHARGE_RATE = 2.0f;
        public const float SHIELD_BAS_CHARGE_DELAY = 3.0f;
        public const float SHIELD_BAS_OVERCHARGE_TIME = 7.0f;
        public const float SHIELD_BAS_OVERCHARGE_DEF_BONUS = 1.0f;
        public const float SHIELD_BAS_OVERCHARGE_RES_BONUS = 0.5f;
        public const double SHIELD_BAS_POWER_CONSUMPTION = 0.001;

        public const int SHIELD_ADV_MAX_PLUGINS = 8; // Max allowed Plugins for Advanced Emitter;
        public const float SHIELD_ADV_MAX_ENERGY = 250.0f;
        public const float SHIELD_ADV_DEF = 0.65f;
        public const float SHIELD_ADV_RES = 0.15f;
        public const float SHIELD_ADV_CHARGE_RATE = 10.0f;
        public const float SHIELD_ADV_CHARGE_DELAY = 5.0f;
        public const float SHIELD_ADV_OVERCHARGE_TIME = 3.0f;
        public const float SHIELD_ADV_OVERCHARGE_DEF_BONUS = 0.5f;
        public const float SHIELD_ADV_OVERCHARGE_RES_BONUS = 0.75f;
        public const double SHIELD_ADV_POWER_CONSUMPTION = 0.005;
        #endregion

        #region Strings
        public const string DAMAGETYPE_KI = "Bullet";
        public const string DAMAGETYPE_EX = "Explosion";
        public const string SUBTYPEID_EMITTER_BAS = "PocketShield_EmitterBasic";
        public const string SUBTYPEID_EMITTER_ADV = "PocketShield_EmitterAdvanced";
        public const string SUBTYPEID_PLUGIN_CAP = "PocketShield_PluginCap";
        public const string SUBTYPEID_PLUGIN_DEF_KI = "PocketShield_PluginDefBullet";
        public const string SUBTYPEID_PLUGIN_DEF_EX = "PocketShield_PluginDefExplosion";
        public const string SUBTYPEID_PLUGIN_RES_KI = "PocketShield_PluginResBullet";
        public const string SUBTYPEID_PLUGIN_RES_EX = "PocketShield_PluginResExplosion";
        #endregion

        #region Texture
        public const int ICON_ATLAS_W = 3;
        public const int ICON_ATLAS_H = 3;

        public const float ICONS_ATLAS_UV_SIZE_X = 1.0f / ICON_ATLAS_W;
        public const float ICONS_ATLAS_UV_SIZE_Y = 1.0f / ICON_ATLAS_H;
        #endregion

    }
    
    public class Config
    {
        private const string CONFIG_FILENAME = "Config.xml";

        public static Config Static
        {
            get
            {
                if (s_Config == null)
                {
                    s_Config = new Config();
                    s_Config.Init();
                }

                return s_Config;
            }
        }

        public string ConfigVersion { get; set; }
        public int LogLevel { get; set; }

        #region Shield Common Config
        public float PluginCapacityBonus { get; set; }
        public float PluginDefenseBonus { get; set; }
        public float PluginResistanceBonus { get; set; }
        public float PluginPowerConsumption { get; set; }

        public int BasicMaxPluginsCount { get; set; }
        public float BasicShieldEnergy { get; set; }
        public float BasicDefense { get; set; }
        public float BasicResistance { get; set; }
        public float BasicChargeRate { get; set; }
        public float BasicChargeDelay { get; set; }
        public float BasicOverchargeDuration { get; set; }
        public float BasicOverchargeDefBonus { get; set; }
        public float BasicOverchargeResBonus { get; set; }
        public double BasicPowerConsumption { get; set; }

        public int AdvancedMaxPluginsCount { get; set; }
        public float AdvancedShieldEnergy { get; set; }
        public float AdvancedDefense { get; set; }
        public float AdvancedResistance { get; set; }
        public float AdvancedChargeRate { get; set; }
        public float AdvancedChargeDelay { get; set; }
        public float AdvancedOverchargeDuration { get; set; }
        public float AdvancedOverchargeDefBonus { get; set; }
        public float AdvancedOverchargeResBonus { get; set; }
        public double AdvancedPowerConsumption { get; set; }
        #endregion
        
        private static Config s_Config = null;
        
        public Config()
        {
            InitDefault();
        }

        private void Init()
        {
            Logger.Log("Loading config file...");
            if (!LoadConfigFile())
            {
                Logger.Log("  Failed to load config file, init with default values");
                InitDefault();
                Logger.Log("  Saving config...");
                SaveConfigFile();
            }
        }
        
        private void InitDefault()
        {
            ConfigVersion = Constants.CONFIG_VERSION;
            LogLevel = Constants.LOG_LEVEL;

            #region Shield Common Config
            PluginCapacityBonus = Constants.PLUGIN_CAP_BONUS;
            PluginDefenseBonus = Constants.PLUGIN_DEF_BONUS;
            PluginResistanceBonus = Constants.PLUGIN_RES_BONUS;
            PluginPowerConsumption = Constants.PLUGIN_POWER_BONUS;

            BasicMaxPluginsCount = Constants.SHIELD_BAS_MAX_PLUGINS;
            BasicShieldEnergy = Constants.SHIELD_BAS_MAX_ENERGY;
            BasicDefense = Constants.SHIELD_BAS_DEF;
            BasicResistance = Constants.SHIELD_BAS_RES;
            BasicChargeRate = Constants.SHIELD_BAS_CHARGE_RATE;
            BasicChargeDelay = Constants.SHIELD_BAS_CHARGE_DELAY;
            BasicOverchargeDuration = Constants.SHIELD_BAS_OVERCHARGE_TIME;
            BasicOverchargeDefBonus = Constants.SHIELD_BAS_OVERCHARGE_DEF_BONUS;
            BasicOverchargeResBonus = Constants.SHIELD_BAS_OVERCHARGE_RES_BONUS;
            BasicPowerConsumption = Constants.SHIELD_BAS_POWER_CONSUMPTION;

            AdvancedMaxPluginsCount = Constants.SHIELD_ADV_MAX_PLUGINS;
            AdvancedShieldEnergy = Constants.SHIELD_ADV_MAX_ENERGY;
            AdvancedDefense = Constants.SHIELD_ADV_DEF;
            AdvancedResistance = Constants.SHIELD_ADV_RES;
            AdvancedChargeRate = Constants.SHIELD_ADV_CHARGE_RATE;
            AdvancedChargeDelay = Constants.SHIELD_ADV_CHARGE_DELAY;
            AdvancedOverchargeDuration = Constants.SHIELD_ADV_OVERCHARGE_TIME;
            AdvancedOverchargeDefBonus = Constants.SHIELD_ADV_OVERCHARGE_DEF_BONUS;
            AdvancedOverchargeResBonus = Constants.SHIELD_ADV_OVERCHARGE_RES_BONUS;
            AdvancedPowerConsumption = Constants.SHIELD_ADV_POWER_CONSUMPTION;
            #endregion
        }

        private bool InvalidateConfig(Config _config)
        {
            bool versionMatch = (_config.ConfigVersion == Constants.CONFIG_VERSION);

            Config config = _config as Config;
            if (config == null)
            {
                Logger.Log("  This Config is not a Config (this should not happen)");
                return false;
            }

            ConfigVersion = config.ConfigVersion;
            LogLevel = config.LogLevel;

            #region Shield Common Config
            PluginCapacityBonus = config.PluginCapacityBonus;
            PluginDefenseBonus = config.PluginDefenseBonus;
            PluginResistanceBonus = config.PluginResistanceBonus;
            PluginPowerConsumption = config.PluginPowerConsumption;

            BasicMaxPluginsCount = config.BasicMaxPluginsCount;
            BasicShieldEnergy = config.BasicShieldEnergy;
            BasicDefense = config.BasicDefense;
            BasicResistance = config.BasicResistance;
            BasicChargeRate = config.BasicChargeRate;
            BasicChargeDelay = config.BasicChargeDelay;
            BasicOverchargeDuration = config.BasicOverchargeDuration;
            BasicOverchargeDefBonus = config.BasicOverchargeDefBonus;
            BasicOverchargeResBonus = config.BasicOverchargeResBonus;
            BasicPowerConsumption = config.BasicPowerConsumption;

            AdvancedMaxPluginsCount = config.AdvancedMaxPluginsCount;
            AdvancedShieldEnergy = config.AdvancedShieldEnergy;
            AdvancedDefense = config.AdvancedDefense;
            AdvancedResistance = config.AdvancedResistance;
            AdvancedChargeRate = config.AdvancedChargeRate;
            AdvancedChargeDelay = config.AdvancedChargeDelay;
            AdvancedOverchargeDuration = config.AdvancedOverchargeDuration;
            AdvancedOverchargeDefBonus = config.AdvancedOverchargeDefBonus;
            AdvancedOverchargeResBonus = config.AdvancedOverchargeResBonus;
            AdvancedPowerConsumption = config.AdvancedPowerConsumption;
            #endregion
            

            if (!versionMatch)
            {
                // config version mismatch;
                Logger.Log("    Config version mismatch: read " + _config.ConfigVersion + ", newest version " + Constants.CONFIG_VERSION);

                //Logger.Log("  Updating config...");
                // TODO: Updating new config here;
                return false;
            }

            ConfigVersion = config.ConfigVersion;
            return true;
        }

        /// <summary> Read the whole Config file. </summary>
        /// <returns> read config file content as string; or null if file is not existed or read operation fails.</returns>
        public string PeekConfigFile()
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(CONFIG_FILENAME, GetType()))
            {
                Logger.Log("  Config file found");
                try
                {
                    Logger.Log("  Reading config file...");
                    TextReader reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(CONFIG_FILENAME, GetType());
                    string data = reader.ReadToEnd();
                    reader.Close();
                    Logger.Log("    Read content: \n" + data, 5);

                    return data;
                }
                catch (Exception _e)
                {
                    Logger.Log("  >>> Exception <<< " + _e.Message);
                }
            }

            return null;
        }

        public bool LoadConfigFile()
        {
            string data = PeekConfigFile();
            if (string.IsNullOrEmpty(data))
            {
                return false;
            }

            Config config = null;
            try
            {
                Logger.Log("  Deserializing data...");
                config = MyAPIGateway.Utilities.SerializeFromXML<Config>(data);
                Logger.Log("  Invalidating data...");
                if (!InvalidateConfig(config))
                {
                    Logger.Log("    Invalidate failed, saving legacy data...");
                    SaveLegacyConfigFile(data);
                    Logger.Log("    Saving new config...");
                    SaveConfigFile();
                }
            }
            catch (Exception _e)
            {
                Logger.Log("  >>> Exception <<< " + _e.Message);
                Logger.Log("    Parse failed, saving legacy data..");
                SaveLegacyConfigFile(data);
                return false;
            }

            return true;
        }

        public bool SaveConfigFile()
        {
            try
            {
                string data = MyAPIGateway.Utilities.SerializeToXML(this);
                TextWriter writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(CONFIG_FILENAME, GetType());
                writer.Write(data);
                writer.Flush();
                writer.Close();

                return true;
            }
            catch (Exception _e)
            {
                Logger.Log("    Failed to save config file");
                Logger.Log(_e.Message);
            }

            return false;
        }

        public bool SaveLegacyConfigFile(string _data)
        {
            try
            {
                string filename = CONFIG_FILENAME.Insert(CONFIG_FILENAME.Length - 4, "_legacy");
                TextWriter writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(filename, GetType());
                writer.Write(_data);
                writer.Flush();
                writer.Close();
                return true;
            }
            catch (Exception)
            {
                Logger.Log("    Fallback method failed...");
            }
            return false;
        }

    }
    
}
