// ;
using ExShared;
using Sandbox.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using VRage;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;

namespace PocketShield
{
    public enum PluginType
    {
        Capacity = 1,
        DefMeta = 2,
        ResKi = 3,
        ResEx = 4,

        Last = ResEx,
    }

    public enum ShieldEmitterType
    {
        BasicEmitter = 1,
        AdvancedEmitter = 2,
    }

    internal struct DamageMod
    {
        public MyStringHash DamagtType { get; private set; }
        public float Amount { get; private set; }

        DamageMod(MyStringHash _damageType, float _amount)
        {
            DamagtType = _damageType;
            Amount = _amount;
        }
    }



    
    public abstract class ShieldEmitter
    {
        public bool RequireSync { get; set; }
        public float ShieldEnergyPercent { get { return Energy / MaxEnergy; } }
        public bool IsOverchargeActive { get { return m_OverchargeRemainingTicks > 0; } }
        public float OverchargeRemainingPercent { get { return m_OverchargeRemainingTicks / (OverchargeDuration * 60.0f); } }


        public float Energy { get; set; }

        public int PluginsCount { get { return m_Plugins.Count; } }
        public float MaxEnergy { get; private set; }
        public float ChargeRate { get; private set; } // unit: energy per second;
        public float ChargeDelay { get; private set; } // unit: second;
        public float OverchargeDuration { get; private set; } // unit: seconds;
        public float OverchargeDefBonus { get; private set; }
        public float OverchargeResBonus { get; private set; }
        public double PowerConsumption { get; private set; } // unit: 100% per second;
        public List<MyTuple<MyStringHash, float>> DefList { get; private set; }
        public List<MyTuple<MyStringHash, float>> ResList { get; private set; }
        
        public IMyCharacter Character { get; private set; }
        public MyStringHash SubtypeId { get; protected set; }

        public float MaxEnergyBonusPercent { get; private set; }



        private Dictionary<MyStringHash, float> m_Def = null;
        private Dictionary<MyStringHash, float> m_Res = null;

        private bool m_IsFirstUpdate = true;
        private bool m_IsPluginsDirty = false;
        
        private int m_OverchargeRemainingTicks = 0;
        private int m_ChargeDelayRemainingTicks = 0;

        protected int m_Ticks = 0;
        
        protected int m_MaxPluginsCount;
        protected float m_BaseMaxEnergy;
        protected float m_BaseChargeRate;
        protected float m_BaseChargeDelay;
        protected float m_BaseOverchargeDuration;
        protected float m_BaseOverchargeDefBonus;
        protected float m_BaseOverchargeResBonus;
        protected double m_BasePowerConsumption;
        protected Dictionary<MyStringHash, float> m_BaseDef = null;
        protected Dictionary<MyStringHash, float> m_BaseRes = null;
        private List<MyStringHash> m_Plugins = null;
        
        private Logger m_Logger = null;
        
        protected static VRage.Game.MyDefinitionId s_PoweKitDefinitionID;
        

        static ShieldEmitter()
        {
            bool flag = VRage.Game.MyDefinitionId.TryParse("MyObjectBuilder_ConsumableItem/Powerkit", out s_PoweKitDefinitionID);
            //ShieldLogger.Log("  TryParse returns " + flag);
        }

        protected ShieldEmitter(IMyCharacter _character)
        {
            m_BaseDef = new Dictionary<MyStringHash, float>();
            m_BaseRes = new Dictionary<MyStringHash, float>();
            m_Plugins = new List<MyStringHash>();
            DefList = new List<MyTuple<MyStringHash, float>>();
            ResList = new List<MyTuple<MyStringHash, float>>();

            m_Def = new Dictionary<MyStringHash, float>(MyStringHash.Comparer);
            m_Res = new Dictionary<MyStringHash, float>(MyStringHash.Comparer);

            m_Logger = CustomLogger.Get((ulong)_character.EntityId);

            string logString = ">> Character [" + Utils.LogCharacterName(_character) + "] <" + _character.EntityId + ">";
            if (!_character.IsPlayer)
                logString += " (Npc)";
            m_Logger.Log(logString);
            
            Character = _character;
            RequireSync = true;

            Energy = 0.0f;

            MaxEnergyBonusPercent = 0.0f;
        }

        ~ShieldEmitter()
        {
            CustomLogger.Remove(m_Logger);
        }

        public void Update(int _ticks)
        {
            if (m_IsFirstUpdate)
                FirstUpdate();

            m_Ticks += _ticks;
            if (m_Ticks >= 2000000000)
                m_Ticks -= 2000000000;

            UpdatePlugins();

            m_Logger.Log("Updating shield (skip " + _ticks + " ticks) (internal ticks: " + m_Ticks + ")", 5);
            m_Logger.LogNoBreak(string.Format("  Energy: {0:0}/{1:0}", (int)Energy, (int)MaxEnergy), 5);

            // recharge shield;
            if (IsOverchargeActive)
                m_ChargeDelayRemainingTicks = 0;
            int chargeTicks = _ticks - m_ChargeDelayRemainingTicks;
            if (chargeTicks <= 0)
            {
                m_ChargeDelayRemainingTicks -= _ticks;
                m_Logger.Inline(string.Format(" (delay {0:0.##}s)", m_ChargeDelayRemainingTicks / 60.0f), 5, true);
            }
            else
            {
                m_ChargeDelayRemainingTicks = 0;
                if (Character.SuitEnergyLevel > 0.0f)
                {
                    double powerCost = PowerConsumption * (chargeTicks / 60.0f);
                    if (Energy < MaxEnergy)
                    {
                        float chargeAmount = ChargeRate * (chargeTicks / 60.0f);
                        if (Character.SuitEnergyLevel > Constants.SHIELD_QUICKCHARGE_POWER_THRESHOLD)
                            chargeAmount *= 2.0f;

                        Energy += chargeAmount;
                        m_Logger.Inline(string.Format(" (+{0:0.##})", chargeAmount), 5, true);
                        if (Energy > MaxEnergy)
                            Energy = MaxEnergy;

                        RequireSync = true;
                    }
                    else
                    {
                        Energy = MaxEnergy;
                        powerCost *= 0.01;
                        m_Logger.BreakLine(5);
                    }

                    MyVisualScriptLogicProvider.SetPlayersEnergyLevel(Character.ControllerInfo.ControllingIdentityId, Character.SuitEnergyLevel - (float)powerCost);
                }
                else
                {
                    m_Logger.Inline(" (no power)", 5, true);
                }
            }

            if (IsOverchargeActive)
            {
                int overchargeTicks = _ticks - m_OverchargeRemainingTicks;
                if (overchargeTicks <= 0)
                    m_OverchargeRemainingTicks -= _ticks;
                else 
                {
                    m_OverchargeRemainingTicks = 0;
                    // TODO: (maybe) handle undercharge :))
                    DeactiveOvercharge();
                }

                RequireSync = true;
            }

        }

        private void UpdatePlugins()
        {
            if (!m_IsPluginsDirty)
                return;

            ServerConfig config = ConfigManager.ServerConfig;

            m_Logger.Log("Plugin List: ");
            foreach (var plugin in m_Plugins)
            {
                m_Logger.Log("  " + plugin.String, 4);
            }
            
            float capMod = 1.0f;
            float defKIMod = 1.0f;
            float defEXMod = 1.0f;
            float resKIMod = 1.0f;
            float resEXMod = 1.0f;

            foreach (MyStringHash subtypeId in m_Plugins)
            {
                if (subtypeId == MyStringHash.GetOrCompute(Constants.SUBTYPEID_PLUGIN_CAP))
                    capMod *= (1.0f + config.PluginCapacityBonus);
                else if (subtypeId == MyStringHash.GetOrCompute(Constants.SUBTYPEID_PLUGIN_DEF_KI))
                    defKIMod *= (1.0f - config.PluginDefenseBonus);
                else if (subtypeId == MyStringHash.GetOrCompute(Constants.SUBTYPEID_PLUGIN_DEF_EX))
                    defEXMod *= (1.0f - config.PluginDefenseBonus);
                else if (subtypeId == MyStringHash.GetOrCompute(Constants.SUBTYPEID_PLUGIN_RES_KI))
                    resKIMod *= (1.0f - config.PluginResistanceBonus);
                else if (subtypeId == MyStringHash.GetOrCompute(Constants.SUBTYPEID_PLUGIN_RES_EX))
                    resEXMod *= (1.0f - config.PluginResistanceBonus);
            }
            
            MaxEnergy = m_BaseMaxEnergy * capMod;
            MaxEnergyBonusPercent = 1.0f - capMod;
            
            {
                float bypass;
                // defKI
                bypass = (1.0f - m_BaseDef[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_KI)]) * defKIMod;
                if (IsOverchargeActive)
                    bypass *= OverchargeDefBonus;
                m_Def[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_KI)] = 1.0f - bypass;

                // defEX
                bypass = (1.0f - m_BaseDef[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_EX)]) * defEXMod;
                if (IsOverchargeActive)
                    bypass *= OverchargeDefBonus;
                m_Def[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_EX)] = 1.0f - bypass;

                // resKI
                bypass = (1.0f - m_BaseRes[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_KI)]) * resKIMod;
                if (IsOverchargeActive)
                    bypass *= OverchargeResBonus;
                m_Res[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_KI)] = 1.0f - bypass;

                // resEX
                bypass = (1.0f - m_BaseRes[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_EX)]) * resEXMod;
                if (IsOverchargeActive)
                    bypass *= OverchargeResBonus;
                m_Res[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_EX)] = 1.0f - bypass;
            }

            // construct DefList, ResList;
            {
                DefList.Clear();
                ResList.Clear();

                foreach (var def in m_Def)
                    DefList.Add(new MyTuple<MyStringHash, float>(def.Key, def.Value));

                foreach (var res in m_Res)
                    ResList.Add(new MyTuple<MyStringHash, float>(res.Key, res.Value));
            }

            RequireSync = true;

            m_IsPluginsDirty = false;
        }
        
        public bool TakeDamage(ref MyDamageInformation _damageInfo)
        {
            m_Logger.Log("Incoming " + _damageInfo.Amount + " " + _damageInfo.Type.String + " damage", 1);
            if (Character == null)
            {
                m_Logger.Log("  There is no Character to take damage (this should not happen)", 1);
                return false;
            }

            m_ChargeDelayRemainingTicks = (int)(ChargeDelay * 60.0f); // 60 ticks per second;

            if (Energy <= 0.0f)
            {
                m_Logger.Log("  Shield depleted", 2);
                return false;
            }

            // TODO: process damage;
            float beforeDamageEnergy = Energy;
            float totalShieldDamage = 0.0f;
            //float totalHealthDamage = 0.0f;

            float defRate = GetDefenseAgainst(_damageInfo.Type);
            float resRate = GetResistanceAgainst(_damageInfo.Type);
            float shieldDamage = _damageInfo.Amount * defRate;
            float healthDamage = _damageInfo.Amount - shieldDamage;
            shieldDamage *= (1.0f - resRate);
            
            if (shieldDamage > 0.0f)
            {
                if (Energy >= shieldDamage)
                {
                    Energy -= shieldDamage;
                    totalShieldDamage = shieldDamage;
                    //totalHealthDamage = healthDamge;
                }
                else
                {
                    healthDamage += (shieldDamage - Energy);
                    totalShieldDamage = Energy;
                    //totalHealthDamage = healthDamge;
                    Energy = 0.0f;
                }
            }

            _damageInfo.Amount = healthDamage;

            m_Logger.Log(string.Format("  Shield damage: {0:0.##} ({1:0.##}%) (res {2:0.##}%), health damage: {3:0.##}", totalShieldDamage, defRate * 100.0f, resRate * 100.0f, healthDamage), 2);
            m_Logger.Log(string.Format("  Shield energy: {0:0.##} -> {1:0.##}", beforeDamageEnergy, Energy), 2);

            return totalShieldDamage != 0.0f;
        }

        private void DeactiveOvercharge()
        {
            // TODO: deactive overcharge;


        }

        private bool TryActiveOvercharge()
        {
            MyInventory inventory = Character.GetInventory() as MyInventory;
            if (inventory == null)
                return false;

            var item = inventory.FindItem(s_PoweKitDefinitionID);
            if (item.HasValue)
            {
                inventory.RemoveItems(item.Value.ItemId, 1, false);

                Energy = MaxEnergy;
                m_OverchargeRemainingTicks = (int)(OverchargeDuration * 60.0f);
                RequireSync = true;

                return true;
            }

            return false;
        }

        private void FirstUpdate()
        {
            m_Logger.Log("First Update called", 1);
            MaxEnergy = m_BaseMaxEnergy;
            ChargeRate = m_BaseChargeRate;
            ChargeDelay = m_BaseChargeDelay;
            OverchargeDuration = m_BaseOverchargeDuration;
            OverchargeDefBonus = m_BaseOverchargeDefBonus;
            OverchargeResBonus = m_BaseOverchargeResBonus;
            PowerConsumption = m_BasePowerConsumption;

            m_IsFirstUpdate = false;
        }

        public void CleanPluginsList()
        {
            m_Plugins.Clear();
            m_IsPluginsDirty = true;
        }

        public void AddPlugins(IEnumerable<MyStringHash> _collection)
        {
            m_Plugins.AddRange(_collection);
            if (m_Plugins.Count > m_MaxPluginsCount)
                m_Plugins.RemoveRange(m_MaxPluginsCount, m_Plugins.Count - m_MaxPluginsCount);
            m_IsPluginsDirty = true;
        }

        public void AddPlugins(MyStringHash _plugin)
        {
            if (m_Plugins.Count >= m_MaxPluginsCount)
                return;
            m_Plugins.Add(_plugin);
            m_IsPluginsDirty = true;
        }
        
        public float GetDefenseAgainst(MyStringHash _damageType)
        {
            if (m_Def.ContainsKey(_damageType))
                return m_Def[_damageType];
            return 0.0f;
        }

        public float GetResistanceAgainst(MyStringHash _damageType)
        {
            if (m_Res.ContainsKey(_damageType))
                return m_Res[_damageType];
            return 0.0f;
        }

        
    }


}
