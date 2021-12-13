// ;
using ExShared;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace PocketShield
{
    public class SaveDataManager
    {
        private static SaveDataManager s_Instance = null;

        private Dictionary<long, float> m_PlayerData = null;
        private Dictionary<long, float> m_NpcData = null;

        public static bool LoadData()
        {
            if (s_Instance != null)
            {
                Logger.Log("Cleaning up last SaveDataManager instance...", 2);
                s_Instance = null;
            }

            s_Instance = new SaveDataManager();
            s_Instance.m_PlayerData = new Dictionary<long, float>();
            s_Instance.m_NpcData = new Dictionary<long, float>();

            Logger.Log("Loading SaveData (shield)...", 2);

            int errorCount = 0;
            TextReader reader;
            try
            {
                reader = MyAPIGateway.Utilities.ReadFileInWorldStorage("PocketShield_savedata.dat", typeof(PocketShield.SaveDataManager));
                string content = reader.ReadToEnd();
                reader.Close();

                MyIni ini = new MyIni();
                MyIniParseResult result;
                if (!ini.TryParse(content, out result))
                {
                    Logger.Log("  Ini parse failed: " + result.ToString(), 2);
                    return false;
                }

                errorCount += s_Instance.TryParseSection(ini, "PlayerData", s_Instance.m_PlayerData);
                errorCount += s_Instance.TryParseSection(ini, "NpcData", s_Instance.m_PlayerData);
            }
            catch (Exception _e)
            {
                Logger.Log("  >> Exception << " + _e.Message, 1);
                return false;
            }
            
            Logger.Log("  Loaded " + s_Instance.m_PlayerData.Keys.Count + " Player and " + s_Instance.m_NpcData.Keys.Count + "NPC shield data, got " + errorCount + " error(s)", 2);
            Logger.Log("SaveDataManager LoadData done", 1);
            return true;
        }

        public static bool UnloadData()
        {
            if (s_Instance == null)
                return false;
            
            s_Instance.m_PlayerData.Clear();
            s_Instance.m_PlayerData = null;

            s_Instance.m_NpcData.Clear();
            s_Instance.m_NpcData = null;

            Logger.Log("SaveDataManager UnloadData done", 1);
            return true;
        }

        public static bool SaveData()
        {
            if (s_Instance == null)
                return false;

            // TODO: save data;



            Logger.Log("SaveData done", 1);
            return true;
        }

        public static void UpdatePlayerData(long _playerUid, float _value)
        {
            if (s_Instance == null)
                return;

            s_Instance.m_PlayerData[_playerUid] = _value;
        }

        public static void UpdateNpcData(long _characterId, float _value)
        {
            if (s_Instance == null)
                return;

            s_Instance.m_NpcData[_characterId] = _value;
        }

        public static float GetPlayerData(long _playerUid)
        {
            if (s_Instance == null)
                return 0.0f;
            if (!s_Instance.m_PlayerData.ContainsKey(_playerUid))
                return 0.0f;

            return s_Instance.m_PlayerData[_playerUid];
        }

        public static float GetNpcData(long _characterId)
        {
            if (s_Instance == null)
                return 0.0f;
            if (!s_Instance.m_NpcData.ContainsKey(_characterId))
                return 0.0f;

            return s_Instance.m_NpcData[_characterId];
        }

        private int TryParseSection(MyIni _ini, string _section, Dictionary<long, float> _data)
        {
            if (!_ini.ContainsSection(_section))
                return 0;

            int errorCount = 0;

            List<MyIniKey> keys = new List<MyIniKey>();
            _ini.GetKeys(_section, keys);

            foreach (MyIniKey key in keys)
            {
                long pid;
                if (!long.TryParse(key.Name, out pid))
                {
                    Logger.Log("  Ignoring error key in section " + _section + ", key=" + key.Name, 2);
                }

                double val;
                MyIniValue value = _ini.Get(key);
                if (!value.TryGetDouble(out val))
                {
                    Logger.Log("  Ignored error value in section " + _section + ", key=" + key.Name + "value=" + value, 2);
                    ++errorCount;
                    continue;
                }

                _data[pid] = (float)val;
            }

            return errorCount;
        }
    }

    public struct ShieldSyncData
    {
        public long PlayerId { get; set; } // redundancy?;

        public float Energy { get; set; }
        public float MaxEnergy { get; set; }



        public ShieldSyncData(long _pid)
        {
            PlayerId = _pid;

            Energy = 0.0f;
            MaxEnergy = 0.0f;

        }

    }
}
