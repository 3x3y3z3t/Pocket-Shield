// ;
using Sandbox.ModAPI;
using System;
using System.IO;

namespace ExShared
{
    public class Config
    {
        public string ConfigVersion { get; set; }
        public int LogLevel { get; set; }

        protected string m_ConfigFileName = "";
        protected Logger m_Logger = null;

        internal bool Init()
        {
            m_Logger.Log("Loading config file...");
            if (LoadConfigFile())
            {
                m_Logger.Log("  Init done");
                return true;
            }

            m_Logger.Log("Failed to load config file, init with default values");
            InitDefault();
            m_Logger.Log("  Saving config...");
            SaveConfigFile();

            m_Logger.Log("Init done");
            return true;
        }

        protected virtual bool InitDefault()
        {
            throw new Exception("InitDefault is not implemented");
        }

        /// <summary> Read the whole Config file. </summary>
        /// <returns> read config file content as string; or null if file is not existed or read operation fails.</returns>
        public virtual string PeekConfigFile()
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(m_ConfigFileName, GetType()))
            {
                m_Logger.Log("  Config file found: " + m_ConfigFileName);
                try
                {
                    m_Logger.Log("  Reading config file...");
                    TextReader reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(m_ConfigFileName, GetType());
                    string data = reader.ReadToEnd();
                    reader.Close();
                    m_Logger.Log("    Read content: \n" + data, 5);

                    return data;
                }
                catch (Exception _e)
                {
                    m_Logger.Log("  >>> Exception <<< " + _e.Message);
                }
            }

            return null;
        }

        public virtual bool LoadConfigFile()
        {
            string data = PeekConfigFile();
            if (string.IsNullOrEmpty(data))
            {
                return false;
            }

            Config config = null;
            try
            {
                m_Logger.Log("  Deserializing data...");
                config = DeserializeData(data);
                m_Logger.Log("  Invalidating data...");
                if (!InvalidateConfig(config))
                {
                    m_Logger.Log("    Invalidate failed, saving legacy data...");
                    SaveLegacyConfigFile(data);
                    m_Logger.Log("    Saving new config...");
                    SaveConfigFile();
                }

                return true;
            }
            catch (Exception _e)
            {
                m_Logger.Log("  >>> Exception <<< " + _e.Message);
                m_Logger.Log("    Parse failed, saving legacy data..");
                SaveLegacyConfigFile(data);
            }

            return false;
        }

        public virtual bool SaveConfigFile()
        {
            //throw new Exception("SaveConfigFile is not implemented");
            try
            {
                string data = MyAPIGateway.Utilities.SerializeToXML(this);
                TextWriter writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(m_ConfigFileName, GetType());
                writer.Write(data);
                writer.Flush();
                writer.Close();

                return true;
            }
            catch (Exception _e)
            {
                m_Logger.Log("    Failed to save config file");
                m_Logger.Log(_e.Message);
            }

            return false;
        }

        public virtual bool SaveLegacyConfigFile(string _data)
        {
            try
            {
                string filename = m_ConfigFileName.Insert(m_ConfigFileName.Length - 4, "_legacy");
                TextWriter writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(filename, GetType());
                writer.Write(_data);
                writer.Flush();
                writer.Close();
                return true;
            }
            catch (Exception _e)
            {
                m_Logger.Log("    Fallback method failed...");
            }
            return false;
        }

        protected virtual Config DeserializeData(string _data)
        {
            throw new Exception("DeserializeData is not implemented");
        }

        protected virtual bool InvalidateConfig(Config _config)
        {
            throw new Exception("InvalidateConfig is not implemented");
        }
    }

}
