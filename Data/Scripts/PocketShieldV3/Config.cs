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

        public const int SHIELD_BAS_MAX_PLUGINS = 2; // Max allowed Plugins for Basic Emitter;
        public const float SHIELD_BAS_MAX_ENERGY = 50.0f;
        public const float SHIELD_BAS_DEF = 0.25f;
        public const float SHIELD_BAS_RES = 0.0f;
        public const float SHIELD_BAS_CHARGE_RATE = 10.0f;
        public const float SHIELD_BAS_CHARGE_DELAY = 2.0f;
        public const float SHIELD_BAS_OVERCHARGE_TIME = 8.0f;
        public const float SHIELD_BAS_OVERCHARGE_DEF_BONUS = 1.0f;
        public const float SHIELD_BAS_OVERCHARGE_RES_BONUS = 0.5f;
        public const double SHIELD_BAS_POWER_CONSUMPTION = 0.001;

        public const int SHIELD_ADV_MAX_PLUGINS = 8; // Max allowed Plugins for Advanced Emitter;
        public const float SHIELD_ADV_MAX_ENERGY = 250.0f;
        public const float SHIELD_ADV_DEF = 0.65f;
        public const float SHIELD_ADV_RES = 0.15f;
        public const float SHIELD_ADV_CHARGE_RATE = 10.0f;
        public const float SHIELD_ADV_CHARGE_DELAY = 4.0f;
        public const float SHIELD_ADV_OVERCHARGE_TIME = 5.0f;
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

}
