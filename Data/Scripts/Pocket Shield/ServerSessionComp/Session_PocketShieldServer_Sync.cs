// ;

using ExShared;
using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using VRage;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;

namespace PocketShield
{
    public partial class Session_PocketShieldServer
    {
        private SyncObject m_SyncObject = new SyncObject();
        
        private void SyncShieldDataToPlayers()
        {
            foreach (IMyPlayer player in m_Players)
            {
                ++m_SyncSaved;

                if (m_ForceSyncPlayers.Contains(player.SteamUserId))
                {
                    m_ForceSyncPlayers.Remove(player.SteamUserId);
                    ServerLogger.Log("Request Sync due to: Force Sync <" + player.SteamUserId + ">", 3);
                    SendSyncDataToPlayer(player);
                }
                else if (m_PlayerShieldEmitters.ContainsKey((long)player.SteamUserId) && m_PlayerShieldEmitters[(long)player.SteamUserId].RequireSync)
                {
                    ServerLogger.Log("Request Sync due to: Shield Updated <" + player.SteamUserId + ">", 3);
                    SendSyncDataToPlayer(player);
                }
                else if (m_ShieldDamageEffects.Count > 0)
                {
                    ServerLogger.Log("Request Sync due to: Shield Effect Updated <" + player.SteamUserId + ">", 3);
                    SendSyncDataToPlayer(player);
                }
            }
        }

        /// <summary>
        /// Defined in Session_PocketShieldServer_Sync.cs.
        /// </summary>
        /// <param name="_player"></param>
        private void SendSyncDataToPlayer(IMyPlayer _player)
        {
            // TODO: add others' data;
            foreach (var value in m_ShieldDamageEffects.Values)
            {
                double distance = Vector3D.Distance(_player.GetPosition(), value.Entity.GetPosition());
                if (distance < Constants.HIT_EFFECT_SYNC_DISTANCE)
                {
                    int ticks = m_Ticks - value.Ticks;
                    if (ticks > Constants.HIT_EFFECT_TICKS)
                        value.Ticks = Constants.HIT_EFFECT_TICKS;
                    else if (ticks < 0)
                        continue;
                    else
                        value.Ticks = Constants.HIT_EFFECT_TICKS - ticks;
                    
                    m_SyncObject.m_OthersShieldData.Add(value);
                }
            }
            
            if (m_PlayerShieldEmitters.ContainsKey((long)_player.SteamUserId))
            {
                ShieldEmitter emitter = m_PlayerShieldEmitters[(long)_player.SteamUserId];
                if (emitter != null)
                {
                    m_SyncObject.m_MyShieldData.PlayerSteamUserId = _player.SteamUserId;
                    m_SyncObject.m_MyShieldData.Energy = emitter.Energy;
                    m_SyncObject.m_MyShieldData.PluginsCount = emitter.PluginsCount;
                    m_SyncObject.m_MyShieldData.MaxEnergy = emitter.MaxEnergy;
                    m_SyncObject.m_MyShieldData.Def = emitter.DefList;
                    m_SyncObject.m_MyShieldData.Res = emitter.ResList;
                    m_SyncObject.m_MyShieldData.SubtypeId = emitter.SubtypeId;
                    m_SyncObject.m_MyShieldData.OverchargeRemainingPercent = emitter.OverchargeRemainingPercent;
                    m_SyncObject.HasShield = true;

                    emitter.RequireSync = false;
                }
            }

            string data = MyAPIGateway.Utilities.SerializeToXML(m_SyncObject);
            ServerLogger.Log("Sending sync data to player " + _player.SteamUserId, 5);
            MyAPIGateway.Multiplayer.SendMessageTo(Constants.MSG_HANDLER_ID_SYNC, Encoding.Unicode.GetBytes(data), _player.SteamUserId);

            --m_SyncSaved;
            m_SyncObject.Clear();
            m_ShieldDamageEffects.Clear();
        }

    }

    public class OtherCharacterShieldData
    {
        [XmlIgnore]
        public IMyEntity Entity = null;
        [XmlIgnore]
        public bool ShouldPlaySound = false;

        public long EntityId = 0L;
        public float ShieldAmountPercent = 0.0f;
        public int Ticks = 0;
    }

    public class MyShieldData
    {
        [XmlIgnore]
        public float EnergyRemainingPercent { get { return Energy / MaxEnergy; } }
        //[XmlIgnore]
        //public int MaxDefOrResCount { get { return Math.Max(Def.Count, Res.Count); } }

        public ulong PlayerSteamUserId = 0UL; // redundancy?;

        public float Energy = 0.0f;

        public int PluginsCount = 0;
        public float MaxEnergy = 0.0f;

        public List<MyTuple<MyStringHash, float>> Def = new List<MyTuple<MyStringHash, float>>();
        public List<MyTuple<MyStringHash, float>> Res = new List<MyTuple<MyStringHash, float>>();

        public MyStringHash SubtypeId = MyStringHash.GetOrCompute("");

        public float OverchargeRemainingPercent = 0.0f;

        public void Clear()
        {

        }
    }

    public class SyncObject
    {
        public bool HasShield = false;
        public List<OtherCharacterShieldData> m_OthersShieldData = new List<OtherCharacterShieldData>();
        public MyShieldData m_MyShieldData = new MyShieldData();
        
        public void Clear()
        {
            HasShield = false;
            m_OthersShieldData.Clear();
            m_MyShieldData.Clear();
        }
    }

}
