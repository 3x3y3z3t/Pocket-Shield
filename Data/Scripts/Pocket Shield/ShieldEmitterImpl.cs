// ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace PocketShield
{
    public class ShieldEmitterBasic : ShieldEmitter
    {

        public ShieldEmitterBasic(IMyCharacter _character) : base(_character)
        {
            ServerConfig config = ConfigManager.ServerConfig;
            m_MaxPluginsCount = config.BasicMaxPluginsCount;
            m_BaseMaxEnergy = config.BasicShieldEnergy;
            m_BaseChargeRate = config.BasicChargeRate;
            m_BaseChargeDelay = config.BasicChargeDelay;
            m_BaseOverchargeDuration = config.BasicOverchargeDuration;
            m_BaseOverchargeDefBonus = config.BasicOverchargeDefBonus;
            m_BaseOverchargeResBonus = config.BasicOverchargeResBonus;
            m_BasePowerConsumption = config.BasicPowerConsumption;

            m_BaseDef[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_KI)] = config.BasicDefense;
            m_BaseDef[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_EX)] = config.BasicDefense;

            m_BaseRes[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_KI)] = config.BasicResistance;
            m_BaseRes[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_EX)] = config.BasicResistance;

            SubtypeId = MyStringHash.GetOrCompute(Constants.SUBTYPEID_EMITTER_BAS);
        }



    }

    public class ShieldEmitterAdvanced : ShieldEmitter
    {

        public ShieldEmitterAdvanced(IMyCharacter _character) : base(_character)
        {
            ServerConfig config = ConfigManager.ServerConfig;
            m_MaxPluginsCount = config.BasicMaxPluginsCount;
            m_BaseMaxEnergy = config.BasicShieldEnergy;
            m_BaseChargeRate = config.BasicChargeRate;
            m_BaseChargeDelay = config.BasicChargeDelay;
            m_BaseOverchargeDuration = config.BasicOverchargeDuration;
            m_BaseOverchargeDefBonus = config.BasicOverchargeDefBonus;
            m_BaseOverchargeResBonus = config.BasicOverchargeResBonus;
            m_BasePowerConsumption = config.BasicPowerConsumption;

            m_BaseDef[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_KI)] = config.BasicDefense;
            m_BaseDef[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_EX)] = config.BasicDefense;

            m_BaseRes[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_KI)] = config.BasicResistance;
            m_BaseRes[MyStringHash.GetOrCompute(Constants.DAMAGETYPE_EX)] = config.BasicResistance;

            SubtypeId = MyStringHash.GetOrCompute(Constants.SUBTYPEID_EMITTER_ADV);
        }



    }
}
