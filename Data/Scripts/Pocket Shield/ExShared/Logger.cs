// ;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using VRage.Utils;

namespace ExShared
{
    public class ServerLogger
    {
        public static Logger Instance
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = new Logger(Logger.c_ServerLoggerName);
                return s_Instance;
            }
        }

        private static Logger s_Instance = null;

        public static void DeInit()
        {
            if (s_Instance == null)
                return;

            s_Instance.DeInit();
            MyLog.Default.WriteLine(Logger.c_LogPrefix + " > Info < Logger destroyed (" + Logger.DestroyedLoggers + "/" + Logger.InstantiatedLoggers + ")");
        }

        public static void Log(string _message, int _level = 0)
        {
            Instance.Log(_message, _level);
        }
    }

    public class ClientLogger
    {
        public static Logger Instance
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = new Logger(Logger.c_ClientLoggerName);
                return s_Instance;
            }
        }

        private static Logger s_Instance = null;

        public static void DeInit()
        {
            if (s_Instance == null)
                return;

            s_Instance.DeInit();
            MyLog.Default.WriteLine(Logger.c_LogPrefix + " > Info < Logger destroyed (" + Logger.DestroyedLoggers + "/" + Logger.InstantiatedLoggers + ")");
        }

        public static void Log(string _message, int _level = 0)
        {
            Instance.Log(_message, _level);
        }
    }

    public class CustomLogger
    {
        public static string Prefix = "custom";
        public static int LogLevel
        {
            set
            {
                s_LogLevel = value;

                if (m_Loggers == null)
                    m_Loggers = new Dictionary<ulong, Logger>();

                foreach (Logger logger in m_Loggers.Values)
                    logger.LogLevel = value;
            }
        }
        public static bool Suppressed
        {
            set
            {
                s_Suppressed = value;

                if (m_Loggers == null)
                    m_Loggers = new Dictionary<ulong, Logger>();

                foreach (Logger logger in m_Loggers.Values)
                    logger.Suppressed = value;
            }
        }


        private static int s_LogLevel = 5;
        private static bool s_Suppressed = false;
        private static Dictionary<ulong, Logger> m_Loggers = null;

        public static void DeInit()
        {
            if (m_Loggers == null)
                return;

            foreach (Logger logger in m_Loggers.Values)
            {
                logger.DeInit();
            }
        }

        public static Logger Get(ulong _id)
        {
            if (m_Loggers == null)
                m_Loggers = new Dictionary<ulong, Logger>();

            if (!m_Loggers.ContainsKey(_id))
                m_Loggers[_id] = new Logger(Prefix + "_" + _id.ToString(), _id)
                {
                    LogLevel = s_LogLevel,
                    Suppressed = s_Suppressed
                };

            return m_Loggers[_id];
        }

        public static void Remove(Logger _logger)
        {
            if (m_Loggers == null)
                return;

            foreach (ulong key in m_Loggers.Keys)
            {
                if (m_Loggers[key].Id == _logger.Id)
                {
                    m_Loggers[key].DeInit();
                    m_Loggers.Remove(key);
                    break;
                }
            }
        }
    }

    public class Logger
    {
        public const string c_LogPrefix = "[" + PocketShield.Constants.LOG_PREFIX + "]";
        public const string c_ServerLoggerName = "server";
        public const string c_ClientLoggerName = "client";

        public static uint InstantiatedLoggers { get { return s_InstantiatedLoggers; } }
        public static uint DestroyedLoggers { get { return s_DestroyedLoggers; } }

        private static uint s_InstantiatedLoggers = 0U;
        private static uint s_DestroyedLoggers = 0U;


        public bool Suppressed
        {
            get { return m_Suppressed; }
            set
            {
                if (value == m_Suppressed)
                    return;

                if (value)
                {
                    m_Suppressed = false;
                    Log(">> Log Suppressed <<");
                    m_Suppressed = true;
                }
                else
                {
                    m_Suppressed = false;
                    Log(">> Log Unsuppressed <<");
                    m_Suppressed = true;
                }
            }
        }
        public ulong Id { get; private set; }
        public int LogLevel { get; set; }

        private bool m_Suppressed = false;
        private TextWriter m_TextWriter = null;

        public Logger(string _name, ulong _id = 0U)
        {
            Id = _id;
            LogLevel = 5;

            string filename = "debug_" + _name;
            try
            {
                m_TextWriter = MyAPIGateway.Utilities.WriteFileInWorldStorage(filename + ".log", typeof(Logger));
            }
            catch (Exception _e)
            {
                MyLog.Default.WriteLine(c_LogPrefix + " > Exception < Problem encountered while initializing logger '" + filename + "': " + _e.Message);
            }

            Log(">> Log Begin <<");
            ++s_InstantiatedLoggers;
        }

        /// <summary> Don't use this method. Use ServerLogger.DeInit(), ClientLogger.DeInit() or CustomLogger.DeInit() instead. </summary>
        public void DeInit()
        {
            if (m_TextWriter != null)
            {
                m_Suppressed = false;
                Log(">> Log End <<");
                m_TextWriter.Close();
            }
            ++s_DestroyedLoggers;
        }

        public void Log(string _message, int _level = 0)
        {
            if (m_Suppressed)
                return;
            if (_level > LogLevel)
                return;

            try
            {
                m_TextWriter.WriteLine("[{0:0}][{1:0}]: {2:0}", GetDateTimeAsString(), _level, _message);
                m_TextWriter.Flush();
            }
            catch (Exception _e)
            {
                MyLog.Default.WriteLine(c_LogPrefix + " > Exception < Problem encountered while logging: " + _e.Message);
            }
        }

        public void LogNoBreak(string _message, int _level = 0)
        {
            if (m_Suppressed)
                return;
            if (_level > LogLevel)
                return;

            try
            {
                m_TextWriter.Write("[{0:0}][{1:0}]: {2:0}", GetDateTimeAsString(), _level, _message);
                m_TextWriter.Flush();
            }
            catch (Exception _e)
            {
                MyLog.Default.WriteLine(c_LogPrefix + " > Exception < Problem encountered while logging: " + _e.Message);
            }
        }

        public void Inline(string _message, int _level = 0, bool _breakNow = false)
        {
            if (m_Suppressed)
                return;
            if (_level > LogLevel)
                return;

            try
            {
                if (_breakNow)
                    m_TextWriter.WriteLine(_message);
                else
                    m_TextWriter.Write(_message);
                m_TextWriter.Flush();
            }
            catch (Exception _e)
            {
                MyLog.Default.WriteLine(c_LogPrefix + " > Exception < Problem encountered while logging: " + _e.Message);
            }
        }

        public void BreakLine(int _level = 0)
        {
            if (m_Suppressed)
                return;
            if (_level > LogLevel)
                return;

            try
            {
                m_TextWriter.WriteLine();
                m_TextWriter.Flush();
            }
            catch (Exception _e)
            {
                MyLog.Default.WriteLine(c_LogPrefix + " > Exception < Problem encountered while logging: " + _e.Message);
            }
        }

        private string GetDateTimeAsString()
        {
            DateTime datetime = DateTime.Now;
            //DateTime datetime = DateTime.UtcNow + m_LocalUtcOffset.TimeSpan;
            return datetime.ToString("yy.MM.dd HH:mm:ss.ff");
        }
        
    }
}
