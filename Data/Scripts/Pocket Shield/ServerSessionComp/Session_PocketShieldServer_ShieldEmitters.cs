// ;
using ExShared;
using System;
using System.Collections.Generic;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace PocketShield
{
    public partial class Session_PocketShieldServer
    {
        public static event Action<MyStringHash, IMyCharacter> ShieldFactory_TryCreateEmitter;
        //public static event Action<ShieldEmitter, MyStringHash> ShieldFactory_PlugPlugin;
        
        private ShieldEmitter FirstEmitterFound = null;

        private void CreateEmitterBasic(MyStringHash _subtypeId, IMyCharacter _character)
        {
            if (FirstEmitterFound != null)
                return;
            if (_subtypeId.String.EndsWith("Basic"))
                FirstEmitterFound = new ShieldEmitterAdvanced(_character);
            
        }

        private void CreateEmitterAdv(MyStringHash _subtypeId, IMyCharacter _character)
        {
            if (FirstEmitterFound != null)
                return;
            if (_subtypeId.String.EndsWith("Advanced"))
                FirstEmitterFound = new ShieldEmitterAdvanced(_character);
            
        }

        private void PlugPlugin0(ShieldEmitter _emitter, MyStringHash _subtypeId)
        {
            if (_emitter == null)
                return;



        }



        private void UpdateShieldEmittersOnceBeforeSim()
        {
            ServerLogger.Log("  Updating all Emitters once before sim..");
            foreach (long key in m_PlayerShieldEmitters.Keys)
            {
                float value = SaveDataManager.GetPlayerData(key);
                m_PlayerShieldEmitters[key].Energy = value;
            }

            foreach (long key in m_NpcShieldEmitters.Keys)
            {
                float value = SaveDataManager.GetNpcData(key);
                m_NpcShieldEmitters[key].Energy = value;
            }
        }

        private void UpdateShieldEmitters(int _ticks)
        {
            foreach (long key in m_PlayerShieldEmitters.Keys)
            {
                m_PlayerShieldEmitters[key].Update(_ticks);
            }

            foreach (long key in m_NpcShieldEmitters.Keys)
            {
                m_NpcShieldEmitters[key].Update(_ticks);
            }
        }

        private ShieldEmitter GetShieldEmitter(IMyCharacter _character)
        {
            if (_character.IsPlayer)
            {
                long playerId = (long)GetPlayerSteamUid(_character);
                if (m_PlayerShieldEmitters.ContainsKey(playerId))
                    return m_PlayerShieldEmitters[playerId];
            }
            else
            {
                if (m_NpcShieldEmitters.ContainsKey(_character.EntityId))
                    return m_NpcShieldEmitters[_character.EntityId];
            }

            return null;
        }

        private void ReplaceShieldEmitter(IMyCharacter _character, ShieldEmitter _newEmitter)
        {
            if (_character == null)
                return;
            
            if (_character.IsPlayer)
            {
                long playerId = (long)GetPlayerSteamUid(_character);
                if (_newEmitter == null)
                    m_PlayerShieldEmitters.Remove(playerId);
                else 
                    m_PlayerShieldEmitters[playerId] = _newEmitter;
            }
            else
            {
                if (_newEmitter == null)
                    m_NpcShieldEmitters.Remove(_character.EntityId);
                else
                    m_NpcShieldEmitters[_character.EntityId] = _newEmitter;
            }
        }
    }
}
