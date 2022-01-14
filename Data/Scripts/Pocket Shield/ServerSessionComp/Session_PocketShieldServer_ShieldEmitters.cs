// ;
using ExShared;
using System;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace PocketShield
{
    public partial class Session_PocketShieldServer
    {
        public static event Action<MyStringHash, IMyCharacter> ShieldFactory_TryCreateEmitter;
        
        private ShieldEmitter FirstEmitterFound = null;

        private void CreateEmitterBasic(MyStringHash _subtypeId, IMyCharacter _character)
        {
            if (FirstEmitterFound != null)
                return;

            ServerLogger.Log("      Creating Basic Emitter", 4);
            if (_subtypeId.String.EndsWith("Basic"))
            {
                FirstEmitterFound = new ShieldEmitterBasic(_character);
                ServerLogger.Log("        Basic Emitter created", 4);
            }
        }

        private void CreateEmitterAdv(MyStringHash _subtypeId, IMyCharacter _character)
        {
            if (FirstEmitterFound != null)
                return;

            ServerLogger.Log("      Creating Advanced Emitter", 4);
            if (_subtypeId.String.EndsWith("Advanced"))
            {
                FirstEmitterFound = new ShieldEmitterAdvanced(_character);
                ServerLogger.Log("        Advanced Emitter created", 4);
            }
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
            if (GetPlayerSteamUid(_character) == 0U)
            {
                if (m_NpcShieldEmitters.ContainsKey(_character.EntityId))
                    return m_NpcShieldEmitters[_character.EntityId];
            }
            else
            {
                long playerId = (long)GetPlayerSteamUid(_character);
                if (m_PlayerShieldEmitters.ContainsKey(playerId))
                    return m_PlayerShieldEmitters[playerId];
            }

            return null;
        }

        private void ReplaceShieldEmitter(IMyCharacter _character, ShieldEmitter _newEmitter)
        {
            if (_character == null)
                return;
            
            if (GetPlayerSteamUid(_character) == 0U)
            {
                if (_newEmitter == null)
                    m_NpcShieldEmitters.Remove(_character.EntityId);
                else
                    m_NpcShieldEmitters[_character.EntityId] = _newEmitter;
            }
            else
            {
                long playerId = (long)GetPlayerSteamUid(_character);
                if (_newEmitter == null)
                    m_PlayerShieldEmitters.Remove(playerId);
                else 
                    m_PlayerShieldEmitters[playerId] = _newEmitter;
            }
        }
    }
}
