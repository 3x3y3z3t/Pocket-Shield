// ;
using ExShared;
using Sandbox.ModAPI;
using System;

namespace PocketShield
{
    public partial class Session_PocketShieldServer
    {
        internal const string c_ChatCmdPrefix = "/PShield";

        private void Utilities_MessageEntered(string _messageText, ref bool _sendToOthers)
        {
            ServerLogger.Log(">> Ultilities_MessageEntered triggered <<", 5);
            if (MyAPIGateway.Session.Player == null)
                return;

            if (!_messageText.StartsWith(c_ChatCmdPrefix))
                return;
            if (!_messageText.Contains("Server"))
                return;

            ServerLogger.Log("  Chat Command captured: " + _messageText, 1);
            ProcessCommands(_messageText);

            _sendToOthers = false;
        }

        private bool ProcessCommands(string _commands)
        {
            _commands = _commands.Remove(_commands.IndexOf("Server"), 6);
            string[] commands = _commands.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (commands.Length <= 1)
            {
                MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] [Server] You didn't specify any command", 2000);
                return false;
            }

            for (int i = 1; i < commands.Length; ++i)
            {
                string cmd = commands[i].Trim();
                if (cmd == "Server")
                    continue;

                ServerLogger.Log("    Processing command " + i + ": " + cmd, 1);
                if (ProcessSingleCommand(cmd))
                {
                    MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] [Server] Command executed.", 2000);
                }
                else
                {
                    MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] [Server] Command execution failed. See log for more info.", 2000);
                }
            }

            return true;
        }

        private bool ProcessSingleCommand(string _command)
        {
            ServerConfig config = ConfigManager.ServerConfig;

            if (_command == "ReloadCfg")
            {
                ServerLogger.Log("      Executing reload command", 1);
                if (ConfigManager.ServerConfig.LoadConfigFile())
                {
                    MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] [Server] Config reloaded", 2000);
                    ServerLogger.Instance.LogLevel = ConfigManager.ServerConfig.LogLevel;
                    CustomLogger.Suppressed = ConfigManager.ServerConfig.SuppressAllShieldLog;
                    UpdateBlueprintData();
                }
                else
                {
                    MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] [Server] Config reload failed", 2000);
                }
                return true;
            }

            if (_command == "SaveCfg")
            {
                ServerLogger.Log("      Executing save command", 1);
                if (ConfigManager.ServerConfig.SaveConfigFile())
                {
                    MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] [Server] Config saved", 2000);
                }
                else
                {
                    MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] [Server] Config saving failed", 2000);
                }
                return true;
            }

            #region Debug
            if (_command == "EmitterCount")
            {
                ServerLogger.Log("      Executing EmitterCount command", 1);

                MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] [Server] Emitter Cunt: "
                    + m_PlayerShieldEmitters.Count + " Player's, " + m_NpcShieldEmitters.Count + " Npc's", 5000);
                ServerLogger.Log("    Emitter count: " + m_PlayerShieldEmitters.Count + " Player's, " + m_NpcShieldEmitters.Count + " Npc's", 1);

                return true;
            }

            if (_command == "LoadedCfg")
            {
                ServerLogger.Log("  Executing LoadedCfg command");
                string configs = MyAPIGateway.Utilities.SerializeToXML(ConfigManager.ServerConfig);
                MyAPIGateway.Utilities.ShowMissionScreen(
                    screenTitle: "Loaded Configs",
                    currentObjectivePrefix: "",
                    currentObjective: "ServerConfig.xml",
                    screenDescription: configs,
                    okButtonCaption: "Close"
                );
                return true;
            }

            if (_command == "PeekCfg")
            {
                ServerLogger.Log("  Executing PeekCfg command");
                string configs = ConfigManager.ServerConfig.PeekConfigFile();
                MyAPIGateway.Utilities.ShowMissionScreen(
                    screenTitle: "Raw Config File",
                    currentObjectivePrefix: "",
                    currentObjective: "ServerConfig.xml",
                    screenDescription: configs,
                    okButtonCaption: "Close"
                );
                return true;
            }
            #endregion

            MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] [Server] Unknown Command [" + _command + "]", 2000);
            ServerLogger.Log("      Unknown command [" + _command + "]", 1);
            return false;

        }


    }

}
