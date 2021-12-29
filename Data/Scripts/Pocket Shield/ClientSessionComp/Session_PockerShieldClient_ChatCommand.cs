// ;
using ExShared;
using Sandbox.ModAPI;
using System;

namespace PocketShield
{
    public partial class Session_PocketShieldClient
    {
        internal const string c_ChatCmdPrefix = "/PShield";
        
        private void Utilities_MessageEntered(string _messageText, ref bool _sendToOthers)
        {
            ClientLogger.Log(">> Ultilities_MessageEntered triggered <<", 5);
            if (MyAPIGateway.Session.Player == null)
                return;

            if (!_messageText.StartsWith(c_ChatCommandPrefix))
                return;

            ClientLogger.Log("  Chat Command captured: " + _messageText, 1);
            ProcessCommands(_messageText);

            _sendToOthers = false;
        }

        private bool ProcessCommands(string _commands)
        {
            string[] commands = _commands.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (commands.Length <= 1)
            {
                MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] You didn't specify any command", 2000);
                return false;
            }

            for (int i = 1; i < commands.Length; ++i)
            {
                string cmd = commands[i].Trim();
                ClientLogger.Log("    Processing command " + i + ": " + cmd, 1);
                if (ProcessSingleCommand(cmd))
                {
                    MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] Command executed.", 2000);
                }
                else
                {
                    MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] Command execution failed. See log for more info.", 2000);
                }
            }

            return true;
        }

        private bool ProcessSingleCommand(string _command)
        {
            ClientConfig config = ConfigManager.ClientConfig;
            
            if (_command.StartsWith("ShowPanelBG"))
            {
                if (!_command.Contains("="))
                {
                    ClientLogger.Log("      No argument", 1);
                    return false;
                }

                bool flag = false;
                if (!bool.TryParse(_command.Substring(12), out flag))
                {
                    ClientLogger.Log("      Bad argument", 1);
                    //Logger.Log("        expect true/false, got " + _command.Substring(12), 1);
                    return false;
                }

                ClientLogger.Log("      Execute ShowPanelBG command with argument '" + flag + "'", 1);
                config.ShowPanelBackground = flag;
                UpdatePanelConfig();
                return true;
            }

            if (_command.StartsWith("ShowPanel"))
            {
                if (!_command.Contains("="))
                {
                    ClientLogger.Log("      No argument", 1);
                    return false;
                }

                bool flag = false;
                if (!bool.TryParse(_command.Substring(10), out flag))
                {
                    ClientLogger.Log("      Bad argument", 1);
                    return false;
                }

                ClientLogger.Log("      Execute ShowPanel command with argument '" + flag + "'", 1);
                config.ShowPanel = flag;
                UpdatePanelConfig();
                return true;
            }
            
            if (_command.StartsWith("Scale"))
            {
                if (!_command.Contains("="))
                {
                    ClientLogger.Log("      No argument", 1);
                    return false;
                }

                float scale = 1.0f;
                if (!float.TryParse(_command.Substring(6), out scale) || scale < 0.0f)
                {
                    ClientLogger.Log("      Bad argument", 1);
                    return false;
                }

                ClientLogger.Log("      Execute Scale command with argument '" + scale + "'", 1);
                // TODO: add this;
                //config.ItemScale = scale;
                UpdatePanelConfig();
                return true;
            }

            if (_command.StartsWith("PanelPos"))
            {
                if (!_command.Contains("="))
                {
                    ClientLogger.Log("      No argument", 1);
                    return false;
                }
                if (!_command.Contains(","))
                {
                    ClientLogger.Log("      Bad argument", 1);
                    return false;
                }
                
                int index1 = _command.IndexOf(',');

                float x = 0.0f;
                float y = 0.0f;
                if (!float.TryParse(_command.Substring(9, index1 - 9), out x) || !float.TryParse(_command.Substring(index1 + 1), out y)
                    || (x < 0.0f || y < 0.0f))
                {
                    ClientLogger.Log("      Bad argument", 1);
                    return false;
                }

                ClientLogger.Log("      Execute PanelPos command with arguments (" + x + ", " + y + ")", 1);
                config.PanelPosition = new VRageMath.Vector2D(x, y);
                UpdatePanelConfig();
                return true;
            }

            if (_command == "ReloadCfg")
            {
                ClientLogger.Log("      Executing reload command", 1);
                LoadConfig();
                return true;
            }

            if (_command == "SaveCfg")
            {
                ClientLogger.Log("      Executing save command", 1);
                SaveConfig();
                return true;
            }

            #region Debug
            if (_command == "LoadedCfg")
            {
                ClientLogger.Log("  Executing LoadedCfg command");
                string configs = MyAPIGateway.Utilities.SerializeToXML(ConfigManager.ClientConfig);
                //configs += "\nViewport Size = " + s_ViewportSize.ToString();
                MyAPIGateway.Utilities.ShowMissionScreen(
                    screenTitle: "Loaded Configs",
                    currentObjectivePrefix: "",
                    currentObjective: "ClientConfig.xml",
                    screenDescription: configs,
                    okButtonCaption: "Close"
                );
                return true;
            }

            if (_command == "PeekCfg")
            {
                ClientLogger.Log("  Executing PeekCfg command");
                //MyAPIGateway.Utilities.ShowNotification("[Pantenna] PeekCfg Command", 3000);
                string configs = ConfigManager.ClientConfig.PeekConfigFile();
                MyAPIGateway.Utilities.ShowMissionScreen(
                    screenTitle: "Raw Config File",
                    currentObjectivePrefix: "",
                    currentObjective: "ClientConfig.xml",
                    screenDescription: configs,
                    okButtonCaption: "Close"
                );
                return true;
            }
            #endregion

            MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] Unknown Command [" + _command + "]", 2000);
            ClientLogger.Log("      Unknown command [" + _command + "]", 1);
            return false;
            
        }


    }

}
