// ;
using ExShared;
using Sandbox.ModAPI;
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

        #region Server/Client Default Config
        public const ushort MSG_HANDLER_ID_SYNC = 1351;

        public const string SERVER_CONFIG_VERSION = "2";
        public const int SERVER_LOG_LEVEL = 1;
        public const int SERVER_UPDATE_INTERVAL = 6; // Server will update 10ups;
        public const int SHIELD_UPDATE_INTERVAL = 3; // Shield will update 20ups;

        public const string CLIENT_CONFIG_VERSION = "1";
        public const int CLIENT_LOG_LEVEL = 1;
        public const int CLIENT_UPDATE_INTERVAL = 6; // Client will update 10ups;
        #endregion

        #region Shield Common Config
        public const float SHIELD_QUICKCHARGE_POWER_THRESHOLD = 0.95f;

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



        #region Hud Config
        public const bool SHOW_PANEL = true;
        public const bool SHOW_PANEL_BG = true;

        public const float PANEL_POS_X = 20.0f;
        public const float PANEL_POS_Y = 735.0f;
        public const float PANEL_WIDTH = 265.0f;
        //public const float PANEL_HEIGHT = 240.0f;
        //public const float PADDING = 6.0f;
        public const float MARGIN = 5.0f;
        public const float ITEM_SCALE = 1.0f;
        

        #endregion

        public const bool SUPPRESS_ALL_SHIELD_LOG = false;
        public const NpcInventoryOperation NPC_DEATH_INVENTORY_OPERATION = NpcInventoryOperation.RemoveEmitterAndPlugin;
        public const float NPC_DEATH_SHIELD_REFUND_RATIO = 0.1f;
        
        #region Internal Hud Config
        public const int HIT_EFFECT_TICKS = 20;
        public const double HIT_EFFECT_SYNC_DISTANCE = 2000.0;
        public const int TEXTURE_W = 4;
        public const int TEXTURE_H = 4;
        public const float TEXTURE_UV_SIZE_X = 1.0f / TEXTURE_W;
        public const float TEXTURE_UV_SIZE_Y = 1.0f / TEXTURE_H;
        public const int TEXTURE_BLANK = 0;
        public const int TEXTURE_SHIELD_BAS = 1;
        public const int TEXTURE_SHIELD_ADV = 5;
        public const int TEXTURE_ICON_DEF_KI = 2;
        public const int TEXTURE_ICON_RES_KI = 3;
        public const int TEXTURE_ICON_DEF_EX = 6;
        public const int TEXTURE_ICON_RES_EX = 7;
        /* 
        BG: 80 92 103
        FG: 187 233 246
        AnimatedSegment: 212 251 254 0.7
        */
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
        
        public const string COLOR_TAG_DEFAULT_VALUE = "<color=128,128,128,72>";
        public const string COLOR_TAG_READONLY_VALUE= "<color=32,223,223,200>";
        public const string COLOR_TAG_NUMBER= "<color=223,223,32>";
        public const string COLOR_TAG_BOOL_TRUE= "<color=32,223,32>";
        public const string COLOR_TAG_BOOL_FALSE = "<color=223,32,32>";
    }

    //public static class ShieldConfig
    //{
    //    public const float BasicMaxHealth = 1000.0f;
    //    public const float BasicChargeRate = 10.0f;
    //    public const float BasicDefense = 0.25f;
    //    public const float BasicBulletResistance = 0.0f;
    //    public const float BasicExplosionResistance = 0.0f;
    //    public const float BasicPowerConsumption = 0.001f;
    //    public const float BasicOverchargeDuration = 5.0f;
    //    public const float BasicOverchargeDefenseBonus = 1.0f;
    //    public const float BasicOverchargeResistanceBonus = 0.5f;
    //    public const int BasicShieldMaxPlugins = 0;

    //    public const float AdvancedMaxHealth = 10000.0f;
    //    public const float AdvancedChargeRate = 100.0f;
    //    public const float AdvancedDefense = 0.75f;
    //    public const float AdvancedBulletResistance = 0.0f;
    //    public const float AdvancedExplosionResistance = 0.0f;
    //    public const float AdvancedPowerConsumption = 0.005f;
    //    public const float AdvancedOverchargeDuration = 3.0f;
    //    public const float AdvancedOverchargeDefenseBonus = 0.5f;
    //    public const float AdvancedOverchargeResistanceBonus = 0.5f;
    //    public const int AdvancedShieldMaxPlugins = 8;

    //    public const float PluginCapacityBonus = 0.1f;
    //    public const float PluginDefenseBonus = 0.5f;
    //    public const float PluginBulletResBonus = 0.1f;
    //    public const float PluginExplosiveResBonus = 0.1f;
    //    public const float PluginPowerConsumption = 1.2f;

    //    public const float ChargeDelay = 0.0f;
    //}

    public class ConfigManager
    {
        public static ServerConfig ServerConfig
        {
            get
            {
                if (s_ServerConfig == null)
                {
                    s_ServerConfig = new ServerConfig();
                    s_ServerConfig.Init();
                }
                return s_ServerConfig;
            }
        }

        public static ClientConfig ClientConfig
        {
            get
            {
                if (s_ClientConfig == null)
                {
                    s_ClientConfig = new ClientConfig();
                    s_ClientConfig.Init();
                }
                return s_ClientConfig;
            }
        }

        public static void ForceInitServer()
        {
            if (s_ServerConfig == null)
            {
                s_ServerConfig = new ServerConfig();
                s_ServerConfig.Init();
            }
        }

        public static void ForceInitClient()
        {
            if (s_ClientConfig == null)
            {
                s_ClientConfig = new ClientConfig();
                s_ClientConfig.Init();
            }
        }

        private static ServerConfig s_ServerConfig = null;
        private static ClientConfig s_ClientConfig = null;
    }

    public class ServerConfig : Config
    {
        public int ServerUpdateInterval { get; set; }
        public int ShieldUpdateInterval { get; set; }

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
        
        public bool SuppressAllShieldLog { get; set; }
        public NpcInventoryOperation NpcInventoryOperationOnDeath { get; set; }
        public float NpcShieldItemToCreditRatio { get; set; }


        public ServerConfig()
        {
            m_ConfigFileName = "ServerConfig.xml";
            m_Logger = ServerLogger.Instance;
            InitDefault();
        }

        protected override bool InitDefault()
        {

            #region Server/Client Default Config
            ConfigVersion = Constants.SERVER_CONFIG_VERSION;
            LogLevel = Constants.SERVER_LOG_LEVEL;
            ServerUpdateInterval = Constants.SERVER_UPDATE_INTERVAL;
            ShieldUpdateInterval = Constants.SHIELD_UPDATE_INTERVAL;
            #endregion

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
            
            SuppressAllShieldLog = Constants.SUPPRESS_ALL_SHIELD_LOG;
            NpcInventoryOperationOnDeath = Constants.NPC_DEATH_INVENTORY_OPERATION;
            NpcShieldItemToCreditRatio = Constants.NPC_DEATH_SHIELD_REFUND_RATIO;

            return true;
        }

        protected override Config DeserializeData(string _data)
        {
            ServerConfig config = MyAPIGateway.Utilities.SerializeFromXML<ServerConfig>(_data);
            return config;
        }

        protected override bool InvalidateConfig(Config _config)
        {
            bool versionMatch = (_config.ConfigVersion == Constants.SERVER_CONFIG_VERSION);

            ServerConfig config = _config as ServerConfig;
            if (config == null)
            {
                m_Logger.Log("  This Config is not ServerConfig (this should not happen)");
                return false;
            }

            #region Server/Client Default Config
            LogLevel = config.LogLevel;
            ServerUpdateInterval = config.ServerUpdateInterval;
            ShieldUpdateInterval = config.ShieldUpdateInterval;
            #endregion

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

            SuppressAllShieldLog = config.SuppressAllShieldLog;
            NpcInventoryOperationOnDeath = config.NpcInventoryOperationOnDeath;
            NpcShieldItemToCreditRatio = config.NpcShieldItemToCreditRatio;
            
            if (!versionMatch)
            {
                // config version mismatch;
                m_Logger.Log("    Config version mismatch: read " + _config.ConfigVersion + ", newest version " + Constants.SERVER_CONFIG_VERSION);

                //Logger.Log("  Updating config...");
                // TODO: Updating new config here;
                return false;
            }

            ConfigVersion = config.ConfigVersion;
            return true;

        }
    }

    public class ClientConfig : Config
    {
        public int ClientUpdateInterval { get; set; }
        
        public bool ShowPanel { get; set; }
        public bool ShowPanelBackground { get; set; }

        public Vector2D PanelPosition { get; set; }
        //public float Padding { get; set; }
        //public float Margin { get; set; }
        //public int DisplayItemsCount { get; set; }
        public float ItemScale { get; set; }
        
        //public bool ModEnabled { get; set; }
        //public float RadarMaxRange { get; set; }
        //public float TrajectorySensitivity { get; set; }

        public ClientConfig()
        {
            m_ConfigFileName = "ClientConfig.xml";
            m_Logger = ClientLogger.Instance;
            InitDefault();
        }

        protected override bool InitDefault()
        {
            // init with default;
            ConfigVersion = Constants.CLIENT_CONFIG_VERSION;
            LogLevel = Constants.CLIENT_LOG_LEVEL;

            ClientUpdateInterval = Constants.CLIENT_UPDATE_INTERVAL;

            ShowPanel = Constants.SHOW_PANEL;
            ShowPanelBackground = Constants.SHOW_PANEL_BG;

            PanelPosition = new Vector2D(Constants.PANEL_POS_X, Constants.PANEL_POS_Y);
            //Padding = Constants.PADDING;
            //Margin = Constants.MARGIN;
            //DisplayItemsCount = Constants.DISPLAY_ITEMS_COUNT;
            ItemScale = Constants.ITEM_SCALE;

            //ModEnabled = Constants.ENABLE_MOD;
            //RadarMaxRange = Constants.RADAR_MAX_RANGE;
            //TrajectorySensitivity = Constants.TRAJECTORY_SENSITIVITY;

            return true;
        }
        
        protected override Config DeserializeData(string _data)
        {
            ClientConfig config = MyAPIGateway.Utilities.SerializeFromXML<ClientConfig>(_data);
            return config;
        }

        protected override bool InvalidateConfig(Config _config)
        {
            bool versionMatch = (_config.ConfigVersion == Constants.CLIENT_CONFIG_VERSION);
                
            ClientConfig config = _config as ClientConfig;
            if (config == null)
            {
                m_Logger.Log("  This Config is not ClientConfig (this should not happen)");
                return false;
            }

            LogLevel = config.LogLevel;

            ClientUpdateInterval = config.ClientUpdateInterval;

            ShowPanel = config.ShowPanel;
            ShowPanelBackground = config.ShowPanelBackground;

            PanelPosition = config.PanelPosition;
            //Padding = config.Padding;
            //Margin = config.Margin;
            //DisplayItemsCount = config.DisplayItemsCount;
            ItemScale = config.ItemScale;

            //ModEnabled = config.ModEnabled;
            //RadarMaxRange = config.RadarMaxRange;
            //TrajectorySensitivity = config.TrajectorySensitivity;
            

            if (!versionMatch)
            {
                // config version mismatch;
                m_Logger.Log("    Config version mismatch: read " + _config.ConfigVersion + ", newest version " + Constants.CLIENT_CONFIG_VERSION);

                //Logger.Log("  Updating config...");
                // TODO: Updating new config here;
                return false;
            }

            ConfigVersion = config.ConfigVersion;
            return true;
            
        }
    }

}
