// ;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using VRage.Game.ModAPI;

namespace PocketShield.Core
{
    public class PlayerShieldData
    {
        public ulong SteamUserId { get; set; }

        public float Health { get; set; }

        public PlayerShieldData()
        {
            SteamUserId = 0;
            Health = 0.0f;
        }

        public PlayerShieldData(ShieldEmitter _emitter)
        {
            SteamUserId = 0;
            Health = _emitter.Health;
        }
    }

    public class PlayerShieldDataSync : PlayerShieldData
    {
        public float MaxHealth { get; set; }
        public float HealthBonus { get; set; }
        public float ChargeRate { get; set; }

        public float Defense { get; set; }

        public float BulletDamageResistance { get; set; }
        public float ExplosiveDamageResistance { get; set; }

        public PlayerShieldDataSync() : base()
        {
            MaxHealth = 0.0f;
            HealthBonus = 0.0f;
            ChargeRate = 0.0f;
            Defense = 0.0f;
            BulletDamageResistance = 0.0f;
            ExplosiveDamageResistance = 0.0f;
        }
    }

    public class PlayerShieldDataManager
    {
        public Dictionary<ulong, PlayerShieldData> PlayerData { get; private set; }
        private string m_Filename = null;

        public PlayerShieldDataManager()
        {
            PlayerData = new Dictionary<ulong, PlayerShieldData>();
            m_Filename = "PlayerShieldData.xml";
        }
        
        public PlayerShieldDataSync GetData(IMyPlayer _player)
        {

            if (PlayerData.ContainsKey(_player.SteamUserId))
                return PlayerData[_player.SteamUserId] as PlayerShieldDataSync;

            PlayerShieldDataSync data = new PlayerShieldDataSync
            {
                SteamUserId = _player.SteamUserId
            };
            PlayerData.Add(_player.SteamUserId, data);
            return data;
        }

        public void ModifyData(IMyPlayer _player, PlayerShieldData _newData)
        {

            if (PlayerData.ContainsKey(_player.SteamUserId))
            {
                PlayerData[_player.SteamUserId] = _newData;
                return;
            }

            _newData.SteamUserId = _player.SteamUserId;
            PlayerData.Add(_player.SteamUserId, _newData);
        }

        public void SaveData()
        {
            try
            {
                PlayerShieldData[] datas = new PlayerShieldData[PlayerData.Count];
                PlayerData.Values.CopyTo(datas, 0);
                TextWriter writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(m_Filename, typeof(PlayerShieldDataManager));
                writer.Write(MyAPIGateway.Utilities.SerializeToXML(datas));
                writer.Flush();
                writer.Close();
            }
            catch (Exception _e)
            { }
        }

        public void LoadData()
        {
            try
            {
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(m_Filename, typeof(PlayerShieldDataManager)))
                    return;

                Clear();

                TextReader reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(m_Filename, typeof(PlayerShieldDataManager));
                string xmlText = reader.ReadToEnd();
                reader.Close();

                PlayerShieldData[] datas = MyAPIGateway.Utilities.SerializeFromXML<PlayerShieldData[]>(xmlText);
                foreach (PlayerShieldData data in datas)
                {
                    PlayerData.Add(data.SteamUserId, data);
                }



            }
            catch (Exception _e)
            { }
        }

        public void Clear()
        {
            PlayerData.Clear();
        }
    }

}
