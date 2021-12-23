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
                m_Loggers[_id] = new Logger(Prefix + "_" + _id.ToString());

            m_Loggers[_id].Suppressed = s_Suppressed;
            return m_Loggers[_id];
        }

        public static void SuppressAll(bool _suppress)
        {
            s_Suppressed = _suppress;

            if (m_Loggers == null)
                m_Loggers = new Dictionary<ulong, Logger>();

            foreach (Logger logger in m_Loggers.Values)
                logger.Suppressed = _suppress;
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
        public int LogLevel { get; set; }

        private bool m_Suppressed = false;
        private TextWriter m_TextWriter = null;

        public Logger(string _name)
        {
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



#if false
namespace Ref { 

    public class ServerLogger : Logger
    {
        public new static void Init()
        {
            InitInternal("debug_server.log");
        }
    }

    public class ClientLogger : Logger
    {
        public new static void Init()
        {
            InitInternal("debug_server.log");
        }
    }

    public class ShieldLogger
    {
        private static Dictionary<ulong, Logger> s_Loggers = null;

        /// <summary> Don't use this method, use Init(string) instead. </summary>
        public static void Init()
        {
            if (s_Loggers == null)
                s_Loggers = new Dictionary<ulong, Logger>();
        }

        public static void DeInit()
        {
            if (s_Loggers == null)
                return;

            foreach (Logger logger in s_Loggers.Values)
            {
                logger.DeInit();
            }
            s_Loggers.Clear();
            s_Loggers = null;
        }

        private static void Init(ulong _playerId, string _characterName)
        {
            string filename = "";
        }
    }





    public enum LoggerSide
    {
        Custom = 0,
        Server,
        Client,
    }

    /// <summary>
    /// 
    /// </summary>
    public class Logger
    {
        private const string c_LogPrefix = "[" + PocketShield.Constants.LOG_PREFIX + "]";
        
        ~Logger()
        {

        }

        public static void Init()
        {
            InitInternal("debug.log");
        }

        public static void DeInit()
        {
            DeInitInternal();
        }

        protected static void InitInternal(string _filename)
        {
            if (s_Instance != null)
                return;

            try
            {
                s_Instance = new Logger()
                {
                    m_TextWriter = MyAPIGateway.Utilities.WriteFileInWorldStorage(_filename, typeof(ExShared.Logger)),
                };

            }
            catch (Exception _e)
            {

            }


        }

        protected static void DeInitInternal()
        {

        }



















        private const uint c_LOLThreshold = uint.MaxValue;
        
        private static Dictionary<MyStringHash, Logger> s_Loggers = null;

        internal static void Log(MyStringHash _key, string _message, uint _level)
        {
            if (s_Loggers == null)
                s_Loggers = new Dictionary<MyStringHash, Logger>(MyStringHash.Comparer);

            if (!s_Loggers.ContainsKey(_key))
                s_Loggers[_key] = new Logger()
                {
                    m_TextWriter = MyAPIGateway.Utilities.WriteFileInWorldStorage(ConstructLogFileName(_key.String, 0U), typeof(ExShared.Logger)),
                };



        }

        private TextWriter m_TextWriter;



        //private MyTimeSpan m_LocalUtcOffset;

        private LoggerSide m_LoggerSide = LoggerSide.Custom;

        private uint m_LOL = 0U; // Lines Of Log, lol;
        private uint m_CustomLOL = 0U;
        private uint m_FileIndex = 0U; // current index of log file, for this session;
        private uint m_CustomFileIndex = 0U;

        private TextWriter m_CustomTextWriter;
        private int m_LogLevel = 5;
        private bool m_IsSuppressed = false;

        public static string CustomLogFilename { get; set; }
        private static Logger s_Instance = null;

        /// <summary> Initializes Logger instance. Unless spacified, Logger Side is Common by default. </summary>
        /// <param name="_loggerSide">indicates which side, client or server, this Logger instance is working on</param>
        /// <returns>true if the initialization success, otherwise false</returns>
        public static bool Init(LoggerSide _loggerSide)
        {
            if (s_Instance != null)
                return false;

            MyLog.Default.WriteLine(c_LogPrefix + " Logger.Init() called (" + _loggerSide + ")");
            TimeSpan offs;
            TimeSpan.TryParse(DateTime.Now.ToString("zzz"), out offs);

            try
            {
                switch (_loggerSide)
                {
                    case LoggerSide.Server:
                        CleanupOldLogFiles("debug_server.log");
                        s_Instance = new Logger
                        {
                            m_LoggerSide = _loggerSide,
                            m_TextWriter = MyAPIGateway.Utilities.WriteFileInWorldStorage(ConstructLogFileName("debug_server.log", 0U), typeof(ExShared.Logger))
                        };
                        break;
                    case LoggerSide.Client:
                        CleanupOldLogFiles("debug_client.log");
                        s_Instance = new Logger
                        {
                            m_LoggerSide = _loggerSide,
                            m_TextWriter = MyAPIGateway.Utilities.WriteFileInWorldStorage(ConstructLogFileName("debug_client.log", 0U), typeof(ExShared.Logger))
                        };
                        break;
                    default:
                        CleanupOldLogFiles(CustomLogFilename);
                        s_Instance = new Logger
                        {
                            m_LoggerSide = LoggerSide.Custom,
                            m_CustomTextWriter = MyAPIGateway.Utilities.WriteFileInWorldStorage(ConstructLogFileName(CustomLogFilename, 0U), typeof(ExShared.Logger))
                        };
                        break;
                }
            }
            catch (Exception _e)
            {
                MyLog.Default.WriteLine(c_LogPrefix + "   Problem encountered while initializing logger: " + _e.Message);
                //throw _e;
                return false;
            }
            //s_Instance.m_LocalUtcOffset = new MyTimeSpan(offs.Ticks);

            MyLog.Default.WriteLine(c_LogPrefix + "   Logger init done");
            Log(">> Log Begin <<");

            return true;
        }

        public static bool InitCustom(string _customLogFilename = "debug_custom.log")
        {
            CustomLogFilename = _customLogFilename;

            MyLog.Default.WriteLine("Is Instance null: " + (s_Instance == null));
            if (s_Instance == null)
            {
                return Init(LoggerSide.Custom);
            }
            if (s_Instance.m_CustomTextWriter != null)
                return false;

            Log(">> Initializing Custom Logger...");
            CleanupOldLogFiles(CustomLogFilename);
            try
            {
                s_Instance.m_CustomTextWriter = MyAPIGateway.Utilities.WriteFileInWorldStorage(ConstructLogFileName(CustomLogFilename, 0U), typeof(ExShared.Logger));
            }
            catch (Exception _e)
            {
                Log(">>   Problem encountered while initializing logger: " + _e.Message);
                return false;
            }
            Log(">>   Custom Logger init done");
            Log(">> Log Begin <<", 0, true);

            return true;
        }

        public static bool DeInit()
        {
            MyLog.Default.WriteLine(c_LogPrefix + " Logger.DeInit() called");
            if (s_Instance == null)
            {
                MyLog.Default.WriteLine(c_LogPrefix + "   Logger instance is null, there is no need to deinit");
                return true;
            }

            if (s_Instance.m_CustomTextWriter != null)
            {
                Log(">> Log End <<", 0, true);
                s_Instance.m_CustomTextWriter.Close();
            }

            Log(">> Custom Logger deinit done");
            Log(">> Log End <<");

            if (s_Instance.m_TextWriter != null)
                s_Instance.m_TextWriter.Close();
            s_Instance = null;
            MyLog.Default.WriteLine(c_LogPrefix + "   Logger deinit done");

            return true;
        }

        public static void SetLogLevel(int _level)
        {
            if (s_Instance == null)
                InitCustom();

            s_Instance.m_LogLevel = _level;
        }

        public static void SuppressLogger(bool _suppress = true)
        {
            if (s_Instance == null)
                InitCustom();

            s_Instance.m_IsSuppressed = _suppress;
        }

        public static void Log(string _message, int _level = 0, bool _writeToCustomStream = false)
        {
            if (s_Instance == null)
                InitCustom();

            if (s_Instance.m_IsSuppressed)
                return;

            // custom logging bypass all log level settings;
            if (_writeToCustomStream)
            {
                if (s_Instance.m_CustomTextWriter == null)
                    InitCustom();

                try
                {
                    if (s_Instance.m_CustomLOL > c_LOLThreshold)
                        AdvanceToNextLogFile(true);

                    s_Instance.m_CustomTextWriter.WriteLine("[{0:0}][1:0]: {2:0}", s_Instance.GetDateTimeAsString(), _level, _message);
                    s_Instance.m_CustomTextWriter.Flush();
                    ++s_Instance.m_CustomLOL;
                }
                catch (Exception _e)
                { }

                return;
            }

            if (_level > s_Instance.m_LogLevel)
                return;

            try
            {
                if (s_Instance.m_LOL > c_LOLThreshold)
                    AdvanceToNextLogFile();

                s_Instance.m_TextWriter.WriteLine("[{0:0}][1:0]: {2:0}", s_Instance.GetDateTimeAsString(), _level, _message);
                s_Instance.m_TextWriter.Flush();
                ++s_Instance.m_LOL;
            }
            catch (Exception _e)
            { }
        }

        internal static void AdvanceToNextLogFile(bool _isCustomStream = false)
        {
            if (s_Instance == null)
                return; // this should not happens;


            if (_isCustomStream || s_Instance.m_LoggerSide == LoggerSide.Custom)
            {
                s_Instance.m_CustomTextWriter.Close();
                ++s_Instance.m_CustomFileIndex;
                s_Instance.m_CustomTextWriter = MyAPIGateway.Utilities.WriteFileInWorldStorage(ConstructLogFileName(CustomLogFilename, s_Instance.m_CustomFileIndex), typeof(ExShared.Logger));
                s_Instance.m_CustomLOL = 0U;
            }
            else
            {
                s_Instance.m_TextWriter.Close();
                ++s_Instance.m_FileIndex;
                if (s_Instance.m_LoggerSide == LoggerSide.Server)
                {
                    s_Instance.m_TextWriter = MyAPIGateway.Utilities.WriteFileInWorldStorage(ConstructLogFileName("debug_server.log", s_Instance.m_FileIndex), typeof(ExShared.Logger));
                }
                else if (s_Instance.m_LoggerSide == LoggerSide.Client)
                {
                    s_Instance.m_TextWriter = MyAPIGateway.Utilities.WriteFileInWorldStorage(ConstructLogFileName("debug_server.log", s_Instance.m_FileIndex), typeof(ExShared.Logger));
                }
                s_Instance.m_LOL = 0U;
            }
        }

        internal static void CleanupOldLogFiles(string _baseName)
        {
            MyLog.Default.WriteLine(c_LogPrefix + " Starting CleanupOldLogFiles(" + _baseName + ")..");
            try
            {
                uint index = 0;
                while (true)
                {
                    string filename = ConstructLogFileName(_baseName, index++);
                    if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(filename, typeof(ExShared.Logger)))
                        break;
                    MyLog.Default.WriteLine(c_LogPrefix + "   Cleaning up file " + filename + "..");
                    MyAPIGateway.Utilities.DeleteFileInWorldStorage(filename, typeof(ExShared.Logger));
                }
            }
            catch (Exception _e)
            {
                MyLog.Default.WriteLine(c_LogPrefix + " Error occured while cleaning up old log files: " + _e.Message);
            }
            MyLog.Default.WriteLine(c_LogPrefix + " CleanupOldLogFiles() done");
        }

        internal static string ConstructLogFileName(string _baseName, uint _index)
        {
            MyLog.Default.WriteLine("Request for filename " + _baseName + ", index " + _index);
            string namePart = _baseName;
            string extPart = ".log";
            int pos = _baseName.LastIndexOf('.');
            if (pos >= 0)
            {
                namePart = _baseName.Substring(0, pos);
                extPart = _baseName.Substring(pos);
            }

            string fullname = namePart + "_" + _index + extPart;
            return fullname;
        }

        internal string GetDateTimeAsString()
        {
            DateTime datetime = DateTime.Now;
            //DateTime datetime = DateTime.UtcNow + m_LocalUtcOffset.TimeSpan;
            return datetime.ToString("yy.MM.dd HH:mm:ss.ff");
        }
    }
}
#endif