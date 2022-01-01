// ;
using ExShared;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using VRage;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Utils;
using VRageMath;

namespace PocketShield
{
    public class SaveDataManager
    {
        private const string c_SavedataFilename = "PocketShield_savedata.dat";
        private const string c_SectionPlayerData = "PlayerData";
        private const string c_SectionNpcData = "NpcData";

        private static SaveDataManager s_Instance = null;

        private Dictionary<long, float> m_PlayerData = null;
        private Dictionary<long, float> m_NpcData = null;

        public static bool LoadData()
        {
            if (s_Instance != null)
            {
                ServerLogger.Log("Cleaning up last SaveDataManager instance...", 2);
                s_Instance = null;
            }

            s_Instance = new SaveDataManager();
            s_Instance.m_PlayerData = new Dictionary<long, float>();
            s_Instance.m_NpcData = new Dictionary<long, float>();

            ServerLogger.Log("Loading SaveData (shield)...", 1);

            if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(c_SavedataFilename, typeof(PocketShield.SaveDataManager)))
            {
                ServerLogger.Log("  Couldn't find savedata file (" + c_SavedataFilename + ") in World Storage", 1);
                //ServerLogger.Log("SaveDataManager LoadData done", 1);
                return false;
            }

            int errorCount = 0;
            try
            {
                TextReader reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(c_SavedataFilename, typeof(PocketShield.SaveDataManager));
                string content = reader.ReadToEnd();
                reader.Close();

                MyIni ini = new MyIni();
                MyIniParseResult result;
                if (!ini.TryParse(content, out result))
                {
                    ServerLogger.Log("  Ini parse failed: " + result.ToString(), 2);
                    return false;
                }

                errorCount += s_Instance.TryParseSection(ini, c_SectionPlayerData, s_Instance.m_PlayerData);
                errorCount += s_Instance.TryParseSection(ini, c_SectionNpcData, s_Instance.m_PlayerData);
            }
            catch (Exception _e)
            {
                ServerLogger.Log("  >> Exception << " + _e.Message, 1);
                return false;
            }
            
            ServerLogger.Log("  Loaded " + s_Instance.m_PlayerData.Keys.Count + " Player, " + s_Instance.m_NpcData.Keys.Count + " NPC shield data, got " + errorCount + " error(s)", 2);
            //ServerLogger.Log("SaveDataManager LoadData done", 1);
            return true;
        }

        public static bool UnloadData()
        {
            if (s_Instance == null)
                return false;

            ServerLogger.Log("Unloading SaveData (shield)...", 1);

            s_Instance.m_PlayerData.Clear();
            s_Instance.m_PlayerData = null;

            s_Instance.m_NpcData.Clear();
            s_Instance.m_NpcData = null;

            //Logger.Log("SaveDataManager UnloadData done", 1);
            return true;
        }

        public static bool SaveData()
        {
            if (s_Instance == null)
                return false;

            ServerLogger.Log("Saving SaveData (shield)...", 1);

            MyIni ini = new MyIni();

            ini.AddSection(c_SectionPlayerData);
            foreach (KeyValuePair<long, float> pair in s_Instance.m_PlayerData)
            {
                ini.Set(c_SectionPlayerData, pair.Key.ToString(), pair.Value);
            }

            ini.AddSection(c_SectionNpcData);
            foreach (KeyValuePair<long, float> pair in s_Instance.m_NpcData)
            {
                ini.Set(c_SectionNpcData, pair.Key.ToString(), pair.Value);
            }

            string data = ini.ToString();
            try
            {
                TextWriter writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(c_SavedataFilename, typeof(SaveDataManager));
                writer.WriteLine(data);
                writer.Flush();
                writer.Close();
            }
            catch (Exception _e)
            {
                ServerLogger.Log("  >> Exception << " + _e.Message, 1);
                return false;
            }

            ServerLogger.Log("SaveData done", 1);
            return true;
        }

        public static void UpdatePlayerData(long _playerUid, float _value)
        {
            if (s_Instance == null)
                LoadData();

            s_Instance.m_PlayerData[_playerUid] = _value;
        }

        public static void UpdateNpcData(long _characterId, float _value)
        {
            if (s_Instance == null)
                LoadData();

            s_Instance.m_NpcData[_characterId] = _value;
        }

        public static float GetPlayerData(long _playerUid)
        {
            if (s_Instance == null)
                LoadData();

            if (!s_Instance.m_PlayerData.ContainsKey(_playerUid))
                return 0.0f;

            return s_Instance.m_PlayerData[_playerUid];
        }

        public static float GetNpcData(long _characterId)
        {
            if (s_Instance == null)
                LoadData();

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
                    ServerLogger.Log("  Ignoring error key in section " + _section + ", key=" + key.Name, 2);
                }

                double val;
                MyIniValue value = _ini.Get(key);
                if (!value.TryGetDouble(out val))
                {
                    ServerLogger.Log("  Ignored error value in section " + _section + ", key=" + key.Name + "value=" + value, 2);
                    ++errorCount;
                    continue;
                }

                _data[pid] = (float)val;
            }

            return errorCount;
        }
    }
}
