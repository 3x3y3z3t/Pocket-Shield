// ;
using Sandbox.ModAPI;
using System;
using System.IO;

namespace PocketShield.Core
{
    public enum LoggerSide
    {
        SERVER,
        CLIENT,
        SHIELD
    }

    public class Logger
    {
        private LoggerSide m_LoggerSide = LoggerSide.SERVER;
        private TextWriter m_TextWriter = null;
        private string bufferMsg = null;
        private string prefixLogMsg = null;

        public Logger(LoggerSide _loggerSide)
        {
            m_LoggerSide = _loggerSide;
            bufferMsg = "";

            if (m_LoggerSide == LoggerSide.SERVER)
                prefixLogMsg = "[PocketShield][Server]: ";
            else if (m_LoggerSide == LoggerSide.CLIENT)
                prefixLogMsg = "[PocketShield][Client]: ";
            else if (m_LoggerSide == LoggerSide.SHIELD)
                prefixLogMsg = "[PocketShield][Shield]: ";

        }

        public void Init()
        {
            try
            {
                string filename = "";
                if (m_LoggerSide == LoggerSide.SERVER)
                {
                    filename = "debug_server.log";
                    bufferMsg = "PocketShield Logger (Server-side) init done.";
                    m_TextWriter = MyAPIGateway.Utilities.WriteFileInLocalStorage(filename, typeof(PocketShield.Core.Logger));
                }
                else if (m_LoggerSide == LoggerSide.CLIENT)
                {
                    filename = "debug_client.log";
                    bufferMsg = "PocketShield Logger (Client-side) init done.";
                    m_TextWriter = MyAPIGateway.Utilities.WriteFileInLocalStorage(filename, typeof(PocketShield.Core.Logger));
                }
                else if (m_LoggerSide == LoggerSide.SHIELD)
                {
                    filename = "debug_shield.log";
                    bufferMsg = "PocketShield Logger (Shield damage) init done.";
                    m_TextWriter = MyAPIGateway.Utilities.WriteFileInWorldStorage(filename, typeof(PocketShield.Core.Logger));
                }

                m_TextWriter.WriteLine(bufferMsg);
                m_TextWriter.Flush();
                bufferMsg = "";
            }
            catch (Exception _e)
            { }
        }

        public void DeInit()
        {
            try
            {
                if (m_LoggerSide == LoggerSide.SERVER)
                    bufferMsg = "PocketShield Logger (Server-side) deinit done.";
                else if (m_LoggerSide == LoggerSide.CLIENT)
                    bufferMsg = "PocketShield Logger (Client-side) deinit done.";
                else if (m_LoggerSide == LoggerSide.SHIELD)
                    bufferMsg = "PocketShield Logger (Shield damage) deinit done.";

                m_TextWriter.WriteLine(bufferMsg);
                m_TextWriter.Flush();
                m_TextWriter.Close();
                m_TextWriter = null;
            }
            catch (Exception _e)
            { }
        }

        public void Log(string _message)
        {
            try
            {
                m_TextWriter.WriteLine(prefixLogMsg + _message);
                m_TextWriter.Flush();
            }
            catch (Exception _e)
            { }
        }
    }

    public static class ShieldLogger
    {
        public static Logger Logger { get; private set; }

        static ShieldLogger() { Logger = new Logger(LoggerSide.SHIELD); }

        public static void Init() { Logger.Init(); }
        public static void DeInit() { Logger.DeInit(); }

        public static void Log(string _msg) { Logger.Log(_msg); }
    }
}
