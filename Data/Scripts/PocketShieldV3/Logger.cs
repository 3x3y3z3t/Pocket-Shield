// ;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using VRage.Utils;

namespace ExShared
{
    public class Logger
    {
        private const string LOG_FILENAME = "debug.log";

        public const string c_LogPrefix = "[" + PocketShield.Constants.LOG_PREFIX + "]";
        
        public static Logger Static { get { return s_Instance; } }

        public static bool Suppressed
        {
            get { return Static.m_Suppressed; }
            set
            {
                if (value == Static.m_Suppressed)
                    return;

                if (value)
                {
                    Static.m_Suppressed = false;
                    Static.LogInternal(">> Log Suppressed <<");
                    Static.m_Suppressed = true;
                }
                else
                {
                    Static.m_Suppressed = false;
                    Static.LogInternal(">> Log Unsuppressed <<");
                }
            }
        }
        public int LogLevel { get; set; }

        private bool m_Suppressed = false;
        private TextWriter m_TextWriter = null;

        private static readonly Logger s_Instance = new Logger();
        
        public Logger()
        {
            LogLevel = 5;
            
            try
            {
                m_TextWriter = MyAPIGateway.Utilities.WriteFileInWorldStorage(LOG_FILENAME, typeof(Logger));
            }
            catch (Exception _e)
            {
                MyLog.Default.WriteLine(c_LogPrefix + " > Exception < Problem encountered while initializing logger '" + LOG_FILENAME + "': " + _e.Message);
            }

            LogInternal(">> Log Begin <<");
        }
        
        public static void DeInit()
        {
            if (Static.m_TextWriter != null)
            {
                Static.m_Suppressed = false;
                Static.LogInternal(">> Log End <<");
                Static.m_TextWriter.Close();
            }
        }

        public static void Log(string _message, int _level = 0)
        {
            Static.LogInternal(_message, _level);
        }

        public void LogInternal(string _message, int _level = 0)
        {
            if (m_Suppressed)
                return;
            if (_level > LogLevel)
                return;

            try
            {
                m_TextWriter.WriteLine("[" + DateTime.Now.ToString("yy.MM.dd HH:mm:ss.ff") + "][" + _level + "]: " + _message);
                m_TextWriter.Flush();
            }
            catch (Exception _e)
            {
                MyLog.Default.WriteLine(c_LogPrefix + " > Exception < Problem encountered while logging: " + _e.Message);
                MyLog.Default.WriteLine(c_LogPrefix + "   Msg: " + _message);
            }
        }
        
    }
}
