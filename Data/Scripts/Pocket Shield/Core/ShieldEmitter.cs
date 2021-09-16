// ;

using Sandbox.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace PocketShield.Core
{
    public static class Config
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

    public enum ShieldItemType
    {
        None = 0,
        BasicEmitter, AdvancedEmitter,
        Plugin0, Plugin1, Plugin2, Plugin3
    }

    public class ShieldEmitter
    {
        public float MaxHealth { get; protected set; }
        public float Health { get; set; }
        public float HealthBonus { get; set; }
        public float ChargeRate { get; protected set; }
        public float Defense { get; protected set; }
        public float BulletDamageResistance { get; protected set; }
        public float ExplosiveDamageResistance { get; protected set; }
        public float PowerConsumption { get; protected set; }
        public float OverchargeDuration { get; protected set; }
        public float OverchargeDefenseBonus { get; protected set; }
        public float OverchargeResistanceBonus { get; protected set; }

        public bool IsOvercharge { get; protected set; }

        public uint CapacityPluginsCount { get; set; }
        public uint DefensePluginsCount { get; set; }
        public uint BulletResPluginsCount { get; set; }
        public uint ExplosiveResPluginsCount { get; set; }
        public uint MaxPlugins { get; protected set; }

        public IMyPlayer Player { get; set; }
        public IMyCharacter Character { get; set; }

        protected float m_BaseMaxHealth = 0.0f;
        protected float m_BaseChargeRate = 0.0f;
        protected float m_BaseDefense = 0.0f;
        protected float m_BaseBulletResistance = 0.0f;
        protected float m_BaseExplosionResistance = 0.0f;
        protected float m_BasePowerConsumption = 0.0f;

        protected bool m_IsDirty = true;
        protected int m_RemainingOverchargeTick = 0;
        protected int m_Ticks = 0;

        protected VRage.Game.MyDefinitionId s_PoweKitDefinitionID;

        public ShieldEmitter()
        {
            MaxHealth = 0.0f;
            Health = 0.0f;
            HealthBonus = 0.0f;
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

        /** Update this ShieldEmitter.
         *  Mostly used for charge and character power consume.
         */
        public void Update(int _skippedTick)
        {
            //ShieldLogger.Log("Shield Update()");
            if (m_IsDirty)
            {
                MaxHealth = m_BaseMaxHealth;
                HealthBonus = 1.0f;
                for (int i = 0; i < CapacityPluginsCount; ++i)
                {
                    MaxHealth *= (1.0f + Config.PluginCapacityBonus);
                    HealthBonus *= (1.0f + Config.PluginCapacityBonus);
                }
                HealthBonus -= 1.0f;
                
                float bypass = 1.0f - m_BaseDefense;
                //ShieldLogger.Log("bypass = " + bypass);
                for (int i = 0; i < DefensePluginsCount; ++i)
                {
                    bypass *= (1.0f - Config.PluginDefenseBonus);
                }
                //ShieldLogger.Log("bypass = " + bypass);
                
                float kiTaken = 1.0f - m_BaseBulletResistance;
                for (int i = 0; i < BulletResPluginsCount; ++i)
                {
                    kiTaken *= (1.0f - Config.PluginBulletResBonus);
                }
                
                float exTaken = 1.0f - m_BaseExplosionResistance;
                for (int i = 0; i < ExplosiveResPluginsCount; ++i)
                {
                    exTaken *= (1.0f - Config.PluginExplosiveResBonus);
                }
                
                PowerConsumption = m_BasePowerConsumption;
                float pluginsCount = CapacityPluginsCount + DefensePluginsCount + BulletResPluginsCount + ExplosiveResPluginsCount;
                for (int i = 0; i < pluginsCount; ++i)
                {
                    PowerConsumption *= Config.PluginPowerConsumption;
                }

                if (IsOvercharge)
                {
                    //ShieldLogger.Log("Bypasses: " + bypass + " " + kiTaken + " " + exTaken + " Overcharge remaining " + m_RemainingOverchargeTick / 60.0f);
                    bypass *= (1.0f - OverchargeDefenseBonus);
                    kiTaken *= (1.0f - OverchargeResistanceBonus);
                    exTaken *= (1.0f - OverchargeResistanceBonus);
                }

                Defense = 1.0f - bypass;
                //ShieldLogger.Log("bypass = " + bypass + ", Defense = " + Defense);
                BulletDamageResistance = 1.0f - kiTaken;
                ExplosiveDamageResistance = 1.0f - exTaken;

                m_IsDirty = false;
            }

            //ShieldLogger.Log("  Character energy: " + Character.SuitEnergyLevel);
            //ShieldLogger.Log("  Shield health: " + Health + "/" + MaxHealth);
            if (Character.SuitEnergyLevel > 0.0f)
            {
                float powerConsumed = PowerConsumption / 60.0f * _skippedTick;
                if (Health < MaxHealth)
                {
                    if (Character.SuitEnergyLevel > 0.90)
                        ChargeRate = m_BaseChargeRate * 2.0f;
                    else
                        ChargeRate = m_BaseChargeRate;

                    float chargePerTick = ChargeRate / 60.0f * _skippedTick;
                    Health += chargePerTick;
                    //ShieldLogger.Log("  Shield charged +" + chargePerTick);

                }
                else
                {
                    Health = MaxHealth;
                    powerConsumed *= 0.1f;
                }
                // TODO: consume energy;
                if (Player != null)
                {
                    MyVisualScriptLogicProvider.SetPlayersEnergyLevel(Player.IdentityId, Character.SuitEnergyLevel - powerConsumed);
                }
            }
            else
            {
                ChargeRate = 0.0f;
            }
            
            if (IsOvercharge)
            {
                m_RemainingOverchargeTick -= _skippedTick;
                if (m_RemainingOverchargeTick <= 0)
                {
                    IsOvercharge = false;
                    m_IsDirty = true;
                }
            }
        }

        public virtual void UpdatePlugins(uint _capacityPluginsCount, uint _defensePluginsCount, uint _bulletResPluginsCount, uint _explosiveResPluginsCount)
        {
            //ShieldLogger.Log("New Plugins Count: " + _capacityPluginsCount + " " + _defensePluginsCount + " " + _bulletResPluginsCount + " " + _explosiveResPluginsCount);
            if (CapacityPluginsCount != _capacityPluginsCount)
            {
                CapacityPluginsCount = _capacityPluginsCount;
                m_IsDirty = true;
            }

            if (DefensePluginsCount != _defensePluginsCount)
            {
                DefensePluginsCount = _defensePluginsCount;
                m_IsDirty = true;
            }

            if (BulletResPluginsCount != _bulletResPluginsCount)
            {
                BulletResPluginsCount = _bulletResPluginsCount;
                m_IsDirty = true;
            }

            if (ExplosiveResPluginsCount != _explosiveResPluginsCount)
            {
                ExplosiveResPluginsCount = _explosiveResPluginsCount;
                m_IsDirty = true;
            }
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
}
