// ;
using ExShared;
using Sandbox.ModAPI;
using System;
using System.IO;
using VRageMath;

namespace PocketShield
{
    internal class Constants
    {
        public const ulong MOD_ID = 2656470280UL;
        public const string LOG_PREFIX = "PocketShield";

        #region Server/Client Default Config
        public const string SERVER_CONFIG_VERSION = "1";
        public const int SERVER_LOG_LEVEL = 1;
        public const int SERVER_UPDATE_INTERVAL = 6; // Server will update 10ups;
        public const int SHIELD_UPDATE_INTERVAL = 3; // Shield will update 20ups;

        public const string CLIENT_CONFIG_VERSION = "1";
        public const int CLIENT_LOG_LEVEL = 1;
        public const int CLIENT_UPDATE_INTERVAL = 6; // Client will update 10ups;
        #endregion

        public const int MAX_PLUGINS = 8; // Max allowed Plugins count;

    }

    public static class ShieldConfig
    {
        public const float BasicMaxHealth = 1000.0f;
        public const float BasicChargeRate = 10.0f;
        public const float BasicDefense = 0.25f;
        public const float BasicBulletResistance = 0.0f;
        public const float BasicExplosionResistance = 0.0f;
        public const float BasicPowerConsumption = 0.001f;
        public const float BasicOverchargeDuration = 5.0f;
        public const float BasicOverchargeDefenseBonus = 1.0f;
        public const float BasicOverchargeResistanceBonus = 0.5f;
        public const int BasicShieldMaxPlugins = 0;

        public const float AdvancedMaxHealth = 10000.0f;
        public const float AdvancedChargeRate = 100.0f;
        public const float AdvancedDefense = 0.75f;
        public const float AdvancedBulletResistance = 0.0f;
        public const float AdvancedExplosionResistance = 0.0f;
        public const float AdvancedPowerConsumption = 0.005f;
        public const float AdvancedOverchargeDuration = 3.0f;
        public const float AdvancedOverchargeDefenseBonus = 0.5f;
        public const float AdvancedOverchargeResistanceBonus = 0.5f;
        public const int AdvancedShieldMaxPlugins = 8;

        public const float PluginCapacityBonus = 0.1f;
        public const float PluginDefenseBonus = 0.5f;
        public const float PluginBulletResBonus = 0.1f;
        public const float PluginExplosiveResBonus = 0.1f;
        public const float PluginPowerConsumption = 1.2f;

        public const float ChargeDelay = 0.0f;
    }

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

        public static void ForceInit()
        {
            if (s_ServerConfig == null)
            {
                s_ServerConfig = new ServerConfig();
                s_ServerConfig.Init();
            }
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

        #region ShieldEmitter Config
        public float BasicShieldEnergy { get; set; }
        public float BasicDefense { get; set; }
        public float BasicBulletResistance { get; set; }
        public float BasicExplosionResistance { get; set; }
        public float BasicPowerConsumption { get; set; }
        public float BasicChargeRate { get; set; }
        public float BasicChargeDelay { get; set; }
        public float BasicOverchargeDuration { get; set; }
        public float BasicOverchargeDefBonus { get; set; }
        public float BasicOverchargeResBonus { get; set; }
        public uint BasicMaxPluginsCount { get; set; }

        public float AdvancedShieldEnergy { get; set; }
        public float AdvancedDefense { get; set; }
        public float AdvancedBulletResistance { get; set; }
        public float AdvancedExplosionResistance { get; set; }
        public float AdvancedPowerConsumption { get; set; }
        public float AdvancedChargeRate { get; set; }
        public float AdvancedChargeDelay { get; set; }
        public float AdvancedOverchargeDuration { get; set; }
        public float AdvancedOverchargeDefBonus { get; set; }
        public float AdvancedOverchargeResBonus { get; set; }
        public uint AdvancedMaxPluginsCount { get; set; }

        public float PluginCapacityBonus { get; set; }
        public float PluginDefenseBonus { get; set; }
        public float PluginBulletResBonus { get; set; }
        public float PluginExplosionResBonus { get; set; }
        public float PluginPowerConsumption { get; set; }
        #endregion

        public ServerConfig()
        {
            m_ConfigFileName = "ServerConfig.xml";
            InitDefault();
        }

        protected override bool InitDefault()
        {
            ConfigVersion = Constants.SERVER_CONFIG_VERSION;
            LogLevel = Constants.SERVER_LOG_LEVEL;
            ServerUpdateInterval = Constants.SERVER_UPDATE_INTERVAL;
            ShieldUpdateInterval = Constants.SHIELD_UPDATE_INTERVAL;

            // TODO: Init default (server);

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
                Logger.Log("  This Config is not ServerConfig (this should not happen)");
                return false;
            }

            LogLevel = config.LogLevel;

            ServerUpdateInterval = config.ServerUpdateInterval;

            if (PanelWidth < 0.0f)
                PanelWidth = 0.0f;
            if (DisplayItemsCount < 1)
                DisplayItemsCount = 1;
            if (DisplayItemsCount > 5)
                DisplayItemsCount = 5;
            if (ItemScale < 0.0f)
                ItemScale = 0.0f;

            if (!versionMatch)
            {
                // config version mismatch;
                Logger.Log("    Config version mismatch: read " + _config.ConfigVersion + ", newest version " + Constants.SERVER_CONFIG_VERSION);

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
        public bool ShowMaxRangeIcon { get; set; }
        public bool ShowSignalName { get; set; }

        public Vector2D PanelPosition { get; set; }
        public float PanelWidth { get; set; }
        public float Padding { get; set; }
        public float Margin { get; set; }
        public int DisplayItemsCount { get; set; }
        public float ItemScale { get; set; }
        
        public bool ModEnabled { get; set; }
        public float RadarMaxRange { get; set; }
        public float TrajectorySensitivity { get; set; }

        public ClientConfig()
        {
            m_ConfigFileName = "ClientConfig.xml";
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
            ShowMaxRangeIcon = Constants.SHOW_MAX_RANGE_ICON;
            ShowSignalName = Constants.SHOW_SIGNAL_NAME;

            PanelPosition = new Vector2D(Constants.PANEL_POS_X, Constants.PANEL_POS_Y);
            PanelWidth = Constants.PANEL_WIDTH;
            Padding = Constants.PADDING;
            Margin = Constants.MARGIN;
            DisplayItemsCount = Constants.DISPLAY_ITEMS_COUNT;
            ItemScale = Constants.ITEM_SCALE;

            ModEnabled = Constants.ENABLE_MOD;
            RadarMaxRange = Constants.RADAR_MAX_RANGE;
            TrajectorySensitivity = Constants.TRAJECTORY_SENSITIVITY;

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
                Logger.Log("  This Config is not ClientConfig (this should not happen)");
                return false;
            }

            LogLevel = config.LogLevel;

            ClientUpdateInterval = config.ClientUpdateInterval;

            ShowPanel = config.ShowPanel;
            ShowPanelBackground = config.ShowPanelBackground;
            ShowMaxRangeIcon = config.ShowMaxRangeIcon;
            ShowSignalName = config.ShowSignalName;

            PanelPosition = config.PanelPosition;
            PanelWidth = config.PanelWidth;
            Padding = config.Padding;
            Margin = config.Margin;
            DisplayItemsCount = config.DisplayItemsCount;
            ItemScale = config.ItemScale;

            ModEnabled = config.ModEnabled;
            RadarMaxRange = config.RadarMaxRange;
            TrajectorySensitivity = config.TrajectorySensitivity;

            if (PanelWidth < 0.0f)
                PanelWidth = 0.0f;
            if (DisplayItemsCount < 1)
                DisplayItemsCount = 1;
            if (DisplayItemsCount > 5)
                DisplayItemsCount = 5;
            if (ItemScale < 0.0f)
                ItemScale = 0.0f;

            if (!versionMatch)
            {
                // config version mismatch;
                Logger.Log("    Config version mismatch: read " + _config.ConfigVersion + ", newest version " + Constants.CLIENT_CONFIG_VERSION);

                //Logger.Log("  Updating config...");
                // TODO: Updating new config here;
                return false;
            }

            ConfigVersion = config.ConfigVersion;
            return true;
            
        }
    }

}
