// ;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Utils;

namespace PocketShield.API
{
    class PocketShieldAPI
    {
        public const ulong MOD_ID = 2656470280UL;


        public void RegisterDamageTypeBypassAmount(MyStringHash _damageType, float _amount)
        {

            //MyAPIGateway.Utilities.SendModMessage()



        }

        public void RegisterDamageTypeBypassAmount(string _damageType, float _amount)
        {
            RegisterDamageTypeBypassAmount(MyStringHash.GetOrCompute(_damageType), _amount);
        }

        public void RegisterDamageTypeBaseResistanceAmount(MyStringHash _damageType, float _amount)
        {

        }

        public void RegisterDamageTypeBaseResistanceAmount(string _damageType, float _amount)
        {
            RegisterDamageTypeBaseResistanceAmount(MyStringHash.GetOrCompute(_damageType), _amount);
        }


    }
}
