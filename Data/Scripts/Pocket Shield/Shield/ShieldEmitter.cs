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

    internal struct DamageDistribution
    {
        public float RawDamage;
        public string DamageType;

        public float HealthDamage;
        public float ShieldDamage;
        public float ResistedDamage;
    }

    internal struct DamageResistance
    {
        public MyStringHash DamagtType { get; private set; }
        public float ResAmount { get; private set; }

        DamageResistance(MyStringHash _damageType, float _resistanceAmount)
        {
            DamagtType = _damageType;
            ResAmount = _resistanceAmount;
        }
    }



    
    public abstract class ShieldEmitter
    {
        public bool RequireSync { get; set; }
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
        public double PowerConsumption { get; private set; } // unit: 0.01% per second;
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

        //public static Dictionary<long, ShieldEmitter> s_GlobalPlayerShieldEmitters = null;
        //public static Dictionary<long, ShieldEmitter> s_GlobalNpcShieldEmitters = null;

            

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
            
            Character = _character;
            RequireSync = true;

            Energy = 0.0f;

            MaxEnergyBonusPercent = 0.0f;
        }

        public void Update(int _ticks)
        {
            if (m_IsFirstUpdate)
                FirstUpdate();

            m_Ticks += _ticks;
            if (m_Ticks >= 2000000000)
                m_Ticks -= 2000000000;

            UpdatePlugins();

            m_Logger.Log("Updating shield (skip " + _ticks + " ticks) (internal ticks: " + m_Ticks + ")", 4);
            m_Logger.LogNoBreak(string.Format("  Energy: {0:0}/{1:0}", (int)Energy, (int)MaxEnergy), 4);

            // recharge shield;
            int chargeTicks = _ticks - m_ChargeDelayRemainingTicks;
            if (chargeTicks <= 0)
            {
                m_ChargeDelayRemainingTicks -= _ticks;
                m_Logger.Inline(string.Format(" (delay {0:0.##}s)", m_ChargeDelayRemainingTicks / 60.0f), _breakNow: true);
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
                        m_Logger.Inline(string.Format(" (+{0:0.##})", chargeAmount), _breakNow: true);
                        if (Energy > MaxEnergy)
                            Energy = MaxEnergy;

                        RequireSync = true;
                    }
                    else
                    {
                        Energy = MaxEnergy;
                        powerCost *= 0.01;
                        m_Logger.BreakLine();
                    }

                    MyVisualScriptLogicProvider.SetPlayersEnergyLevel(Character.ControllerInfo.ControllingIdentityId, Character.SuitEnergyLevel - (float)powerCost);
                }
                else
                {
                    m_Logger.Inline(" (no power)", _breakNow: true);
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

            float capMod = 1.0f;
            float defKIMod = 1.0f;
            float defEXMod = 1.0f;
            float resKIMod = 1.0f;
            float resEXMod = 1.0f;
            
            foreach (MyStringHash subtypeId in m_Plugins)
            {
                if (subtypeId.String == Constants.SUBTYPEID_PLUGIN_CAP)
                    capMod *= (1.0f + config.PluginCapacityBonus);
                else if (subtypeId.String == Constants.SUBTYPEID_PLUGIN_DEF_KI)
                    defKIMod *= (1.0f + config.PluginDefenseBonus);
                else if (subtypeId.String == Constants.SUBTYPEID_PLUGIN_DEF_EX)
                    defEXMod *= (1.0f + config.PluginDefenseBonus);
                else if (subtypeId.String == Constants.SUBTYPEID_PLUGIN_RES_KI)
                    resKIMod *= (1.0f + config.PluginResistanceBonus);
                else if (subtypeId.String == Constants.SUBTYPEID_PLUGIN_RES_EX)
                    resEXMod *= (1.0f + config.PluginResistanceBonus);
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
        
        public void TakeDamage(ref MyDamageInformation _damageInfo)
        {
            m_Logger.Log("Shield is taking " + _damageInfo.Amount + " " + _damageInfo.Type.String + " damage", 1);
            if (Character == null)
            {
                m_Logger.Log("  There is no Character to take damage (this should not happen)", 1);
            }

            m_ChargeDelayRemainingTicks = (int)(ChargeDelay * 60.0f); // 60 ticks per second;

            if (Energy <= 0.0f)
            {
                m_Logger.Log("  Shield depleted", 2);
                return;
            }

            // TODO: process damage;
            float beforeDamageEnergy = Energy;
            float totalShieldDamage = 0.0f;
            float totalHealthDamage = 0.0f;

            float defRate = GetDefenseAgainst(_damageInfo.Type);
            float resRate = GetResistanceAgainst(_damageInfo.Type);
            float shieldDamage = _damageInfo.Amount * defRate;
            float healthDamge = _damageInfo.Amount - shieldDamage;



            // TODO: process shield Resistance;
            if (Energy >= shieldDamage)
            {
                Energy -= shieldDamage;
                totalShieldDamage = shieldDamage;
                totalHealthDamage = healthDamge;
            }
            else
            {
                healthDamge += (shieldDamage - Energy);
                totalShieldDamage = Energy;
                totalHealthDamage = healthDamge;
                Energy = 0.0f;
            }

            _damageInfo.Amount = totalHealthDamage;

            m_Logger.Log(string.Format("  Shield damage: {0:0.##} ({1:0.##}%) (res {2:0.##}%), health damage: {3:0.##}", totalShieldDamage, defRate * 100.0f, resRate * 100.0f, totalHealthDamage), 2);
            m_Logger.Log(string.Format("  Shield energy: {0:0.##} -> {1:0.##}", beforeDamageEnergy, Energy), 1);
        }

        private void ProcessDamageTaken(ref DamageDistribution _damage)
        {
            // TODO: process damage;



        }

        private void DeactiveOvercharge()
        {
            // TODO: deactive overcharge;


        }

        private void FirstUpdate()
        {
            m_Logger.Log("First Update called");
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

        void dummy()
        {

        }


















































#if false
        public float MaxHealth { get; protected set; }
        public float Health { get; set; }
        public float ChargeRate { get; protected set; }
        public float Defense { get; protected set; }
        public float BulletDamageResistance { get; protected set; }
        public float ExplosiveDamageResistance { get; protected set; }
        public float OverchargeDefenseBonus { get; protected set; }
        public float OverchargeResistanceBonus { get; protected set; }

        public bool IsOvercharge { get; protected set; }

        public uint CapacityPluginsCount { get; set; }
        public uint DefensePluginsCount { get; set; }
        public uint BulletResPluginsCount { get; set; }
        public uint ExplosiveResPluginsCount { get; set; }
        public uint MaxPlugins { get; protected set; }

        public IMyPlayer Player { get; set; }

        protected float m_BaseMaxHealth = 0.0f;
        protected float m_BaseChargeRate = 0.0f;
        protected float m_BaseBulletResistance = 0.0f;
        protected float m_BaseExplosionResistance = 0.0f;
        protected float m_BasePowerConsumption = 0.0f;

        protected bool m_IsDirty = true;
        protected int m_RemainingOverchargeTick = 0;

        protected VRage.Game.MyDefinitionId s_PoweKitDefinitionID;

        public ShieldEmitter()
        {
            MaxHealth = 0.0f;
            Health = 0.0f;
            ChargeRate = 0.0f;
            Defense = 0.0f;
            BulletDamageResistance = 0.0f;
            ExplosiveDamageResistance = 0.0f;
            PowerConsumption = 0.0f;
            OverchargeDuration = 0.0f;
            OverchargeDefenseBonus = 0.0f;
            OverchargeResistanceBonus = 0.0f;

            CapacityPluginsCount = 0;
            DefensePluginsCount = 0;
            BulletResPluginsCount = 0;
            ExplosiveResPluginsCount = 0;

            Character = null;
            Player = null;


            bool flag = VRage.Game.MyDefinitionId.TryParse("MyObjectBuilder_ConsumableItem/Powerkit", out s_PoweKitDefinitionID);
            //ShieldLogger.Log("  TryParse returns " + flag);
        }
        

        public void TakeDamage(ref MyDamageInformation _damageInfo)
        {
            ShieldLogger.Log("Shield taking damage...");
            if (Character == null)
            {
                ShieldLogger.Log("  Just kidding, there is no character to take damage.");
                return;
            }
            
            if (_damageInfo.Type == MyStringHash.GetOrCompute("Bullet"))
            {
                ShieldLogger.Log("  Taking " + _damageInfo.Amount + " bullet damage:");
                _damageInfo.Amount = CalculateDamageToHealth(_damageInfo.Amount, BulletDamageResistance);
            }
            else if (_damageInfo.Type == MyStringHash.GetOrCompute("Explosion"))
            {
                ShieldLogger.Log("  Taking " + _damageInfo.Amount + " explosion damage:");
                _damageInfo.Amount = CalculateDamageToHealth(_damageInfo.Amount, ExplosiveDamageResistance);
            }
            else if (_damageInfo.Type == MyStringHash.GetOrCompute("Fall") || _damageInfo.Type == MyStringHash.GetOrCompute("Environment"))
            {
                ShieldLogger.Log("  Taking " + _damageInfo.Amount + " fall/environment damage:");
                _damageInfo.Amount = CalculateDamageToHealth(_damageInfo.Amount, 0.0f);
            }

            ShieldLogger.Log("  Taking " + _damageInfo.Amount + " [other damge type] damage (shield won't take damage).");
        }

        private float CalculateDamageToHealth(float _damageAmount, float _resist)
        {
            ShieldLogger.Log(string.Format("    Damage info: {0:0} (bypass {1:F1}%, resist {2:F1}%)", _damageAmount, (1.0f - Defense) * 100.0f, _resist * 100.0f));
            float dmgToShield = _damageAmount * Defense;
            float dmgToHealth = _damageAmount - dmgToShield;

            float dmgShieldWillTake = dmgToShield * (1.0f - _resist);
            //ShieldLogger.Log("    Damage to shield: " + dmgToShield + " -> " + dmgShieldWillTake + ", damage to health: " + dmgToHealth);
            //ShieldLogger.Log("    Remaining Shield health: " + Health);
            
            while(dmgShieldWillTake >= Health)
            {
                // damage is greater than shield health
                dmgShieldWillTake -= Health; // shield takes a portion of damage;
                //ShieldLogger.Log("      Shield took " + Health + "dmg. Remaining damage: " + dmgShieldWillTake + ", remaining shield: 0");
                
                if (TryActiveOvercharge())
                {
                    // overcharge active succesfully;
                    // now health is full;
                    continue;
                }
                
                Health = 0.0f;

                dmgShieldWillTake /= (1.0f - _resist); // damage amount is revert back to before-resistance;
                //ShieldLogger.Log("      " + dmgShieldWillTake + "dmg will be added back to dmgToHealth.");
                dmgToHealth += dmgShieldWillTake; // that amount is added to health damage;
                break;
            }
            
            if (dmgShieldWillTake < Health)
            {
                // damage is less than shield health
                Health -= dmgShieldWillTake; // shield takes a portion of damage (full);
                ShieldLogger.Log("      Shield took " + dmgShieldWillTake + "dmg. Remaining damage: 0, remaining shield: " + Health);
                //dmgToShield = 0.0f;
            }

            //ShieldLogger.Log("    Damage to health: " + dmgToHealth);
            return dmgToHealth;
        }

        private bool TryActiveOvercharge()
        {
            if (Character == null)
                return false;

            MyInventory inventory = Character.GetInventory() as MyInventory;
            if (inventory == null)
                return false;

            if (inventory.GetItemAmount(s_PoweKitDefinitionID) >= 1)
            {
                MyPhysicalInventoryItem item = inventory.FindItem(s_PoweKitDefinitionID).Value;
                inventory.RemoveItems(item.ItemId, 1);
                Health = MaxHealth;
                IsOvercharge = true;
                m_RemainingOverchargeTick = (int)(OverchargeDuration * 60.0f);
                m_IsDirty = true;
                //ShieldLogger.Log("Overcharge Activated!!! Duration = " + m_RemainingOverchargeTick);
                return true;
            }

            return false;
        }

    }

    public class BasicShieldEmitter : ShieldEmitter
    {
        public BasicShieldEmitter() : base()
        {
            m_BaseMaxHealth = Config.BasicMaxHealth;
            m_BaseChargeRate = Config.BasicChargeRate;
            m_BaseDefense = Config.BasicDefense;
            m_BaseBulletResistance = Config.BasicBulletResistance;
            m_BaseExplosionResistance = Config.BasicExplosionResistance;
            m_BasePowerConsumption = Config.BasicPowerConsumption;

            OverchargeDuration = Config.BasicOverchargeDuration;
            OverchargeDefenseBonus = Config.BasicOverchargeDefenseBonus;
            OverchargeResistanceBonus = Config.BasicOverchargeResistanceBonus;
            MaxPlugins = Config.BasicShieldMaxPlugins;

            MaxHealth = m_BaseMaxHealth;
            ChargeRate = m_BaseChargeRate;
            Defense = m_BaseDefense;
            BulletDamageResistance = m_BaseBulletResistance;
            ExplosiveDamageResistance = m_BaseExplosionResistance;
            PowerConsumption = m_BasePowerConsumption;

        }

        public override void UpdatePlugins(uint _capacityPluginsCount, uint _defensePluginsCount, uint _bulletResPluginsCount, uint _explosiveResPluginsCount)
        {
            return;
        }
    }

    public class AdvancedShieldEmitter : ShieldEmitter
    {
        public AdvancedShieldEmitter() : base()
        {
            m_BaseMaxHealth = Config.AdvancedMaxHealth;
            m_BaseChargeRate = Config.AdvancedChargeRate;
            m_BaseDefense = Config.AdvancedDefense;
            m_BaseBulletResistance = Config.AdvancedBulletResistance;
            m_BaseExplosionResistance = Config.AdvancedExplosionResistance;
            m_BasePowerConsumption = Config.AdvancedPowerConsumption;

            OverchargeDuration = Config.AdvancedOverchargeDuration;
            OverchargeDefenseBonus = Config.AdvancedOverchargeDefenseBonus;
            OverchargeResistanceBonus = Config.AdvancedOverchargeResistanceBonus;
            MaxPlugins = Config.AdvancedShieldMaxPlugins;

            MaxHealth = m_BaseMaxHealth;
            ChargeRate = m_BaseChargeRate;
            Defense = m_BaseDefense;
            BulletDamageResistance = m_BaseBulletResistance;
            ExplosiveDamageResistance = m_BaseExplosionResistance;
            PowerConsumption = m_BasePowerConsumption;

        }

    }
#endif


    }


}
