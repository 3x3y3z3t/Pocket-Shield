// ;
using Draygo.API;
using ExShared;
using Sandbox.ModAPI;
using System;
using System.Text;
using VRage.Game;
using VRage.Game.Components;
using VRageMath;

namespace PocketShield
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public partial class Session_PocketShieldClient : MySessionComponentBase
    {
        private static Vector2 s_ViewportSize = Vector2.Zero;

        public bool IsServer { get; private set; }
        public bool IsDedicated { get; private set; }
        public bool IsSetupDone { get; private set; }

        private bool m_IsHudDirty = false;
        private bool m_IsTextHudModMissingConfirmed = false;

        private bool m_IsHudVisible = false;
        private float m_HudBGOpacity = 1.0f;

        private int m_Ticks = 0;

        private ShieldHudPanel m_ShieldHudPanel = null;
        private HudAPIv2 m_TextHudAPI = null;

        private MyShieldData m_ShieldData = new MyShieldData();
        
        public override void LoadData()
        {
            ConfigManager.ForceInitClient();
            ClientLogger.Instance.LogLevel = ConfigManager.ClientConfig.LogLevel;

            MyAPIGateway.Utilities.MessageEntered += Utilities_MessageEntered;
            MyAPIGateway.Gui.GuiControlRemoved += Gui_GuiControlRemoved;
        }

        protected override void UnloadData()
        {
            Shutdown();
            MyAPIGateway.Gui.GuiControlRemoved -= Gui_GuiControlRemoved;
            MyAPIGateway.Utilities.MessageEntered -= Utilities_MessageEntered;

            ClientLogger.DeInit();
        }
        
        public override void UpdateAfterSimulation()
        {
            ++m_Ticks;
            // clear ticks count;
            if (m_Ticks >= 2000000000)
                m_Ticks -= 2000000000;

            ClientConfig config = ConfigManager.ClientConfig;
            if (m_Ticks % config.ClientUpdateInterval == 0)
            {
                if (MyAPIGateway.Session == null)
                    return;

                if (!IsSetupDone)
                {
                    Setup();
                    return;
                }

                if (!m_IsTextHudModMissingConfirmed && !m_TextHudAPI.Heartbeat && m_Ticks >= 300)
                {
                    ClientLogger.Log("Text Hud API still hasn't recieved heartbeat.", 3);
                    //MyAPIGateway.Utilities.ShowNotification("Text HUD API mod is missing. HUD will not be displayed.", (config.ClientUpdateInterval * (int)(100.0f / 6.0f)), MyFontEnum.Red);
                    MyAPIGateway.Utilities.ShowNotification("Text HUD API mod is missing. HUD will not be displayed.", 3000, MyFontEnum.Red);
                    ClientLogger.Log("  Text Hud API mod is missing.", 3);
                    m_IsTextHudModMissingConfirmed = true;
                }

                // TODO: doing main job?;
                UpdateFakeShieldStat();

            }

                UpdateHitEffectDuration(1);

            //if (m_Ticks % 6 == 0)
            //{
            //    percent -= 0.01f;
            //    if (percent <= 0)
            //        percent += 1;
            //}

            //timer -= 1;
            //if (timer < 0)
            //    timer = Constants.HIT_EFFECT_TICKS;

            // Attempt to steal from https://github.com/THDigi/BuildInfo/blob/master/Data/Scripts/BuildInfo/Systems/GameConfig.cs#L52...
            // Stealing In Progress...
            if (MyAPIGateway.Input.IsNewGameControlPressed(Sandbox.Game.MyControlsSpace.TOGGLE_HUD) ||
               MyAPIGateway.Input.IsNewGameControlPressed(Sandbox.Game.MyControlsSpace.PAUSE_GAME))
            {
                UpdateHudConfigs();
            }

            if (m_IsHudDirty)
            {
                UpdateTextHud();
            }

            // end of method;
            return;
        }

        private void Gui_GuiControlRemoved(object _obj)
        {
            // Attempt to steal from https://github.com/THDigi/BuildInfo/blob/master/Data/Scripts/BuildInfo/Systems/GameConfig.cs#L58...
            // Stealing In Progress...
            try
            {
                if (_obj.ToString().EndsWith("ScreenOptionsSpace")) // closing options menu just assumes you changed something so it'll re-check config settings
                {
                    UpdateHudConfigs();
                }
            }
            catch (Exception _e)
            {
                ClientLogger.Log(_e.Message);
            }
        }

        private void InitTextHudCallback()
        {
            ClientLogger.Log("Starting InitTextHudCallback()", 5);
            ClientConfig config = ConfigManager.ClientConfig;

            m_ShieldHudPanel = new ShieldHudPanel()
            {
                Visible = m_IsHudVisible,
                BackgroundOpacity = m_HudBGOpacity
            };
            //m_ShieldHudPanel.UpdatePanelConfig();
            UpdateHudConfigs();
            
            InitModSettingsMenu();

            ClientLogger.Log("InitTextHudCallback() done", 5);
        }

        public void Setup()
        {
            IsServer = (MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE || MyAPIGateway.Multiplayer.IsServer);
            IsDedicated = IsServer && MyAPIGateway.Utilities.IsDedicated;
            if (IsDedicated)
                return;

            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(Constants.MSG_HANDLER_ID_SYNC, HandleSyncShieldData);

            //m_Logger.Log("  Initializing Text HUD API v2...");
            m_TextHudAPI = new HudAPIv2(InitTextHudCallback);

            //UpdateHudConfigs();

            //MyAPIGateway.Utilities.SendMessage("[PocketShield v2.7] Client-side Chat Command is deprecated and will be removed soon. Use Mod Settings menu instead.");

            ClientLogger.Log("  IsServer = " + IsServer);
            ClientLogger.Log("  IsDedicated = " + IsDedicated);

            ClientLogger.Log("  Setup Done.");
            IsSetupDone = true;
        }

        private void Shutdown()
        {
            try
            {
                ClientLogger.Log("Shutting down Text HUD API v2...");
                m_TextHudAPI.Close();

                MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(Constants.MSG_HANDLER_ID_SYNC, HandleSyncShieldData);
            }
            catch (Exception _e)
            { }
        }
        
        private void UpdateHudConfigs()
        {
            if (MyAPIGateway.Session.Config != null)
            {
                m_HudBGOpacity = MyAPIGateway.Session.Config.HUDBkOpacity;
                m_IsHudVisible = MyAPIGateway.Session.Config.HudState != 0; // 0 = Off, 1 = Hints, 2 = Basic;
            }
            if (MyAPIGateway.Session.Camera != null)
            {
                s_ViewportSize = MyAPIGateway.Session.Camera.ViewportSize;
            }

            
            //m_HudBGColor = Color.FromNonPremultiplied(41, 54, 62, 255) * m_HudBGOpacity * m_HudBGOpacity * 1.075f;
            //m_HudBGColor = Color.White * m_HudBGOpacity * m_HudBGOpacity * magicNum;
            //m_HudBGColor.A = (byte)(m_HudBGOpacity * 255.0f);

            UpdatePanelConfig();
            m_IsHudDirty = true;
        }

        private void UpdatePanelConfig()
        {
            if (m_ShieldHudPanel != null)
            {
                m_ShieldHudPanel.Visible = m_IsHudVisible;
                m_ShieldHudPanel.BackgroundOpacity = m_HudBGOpacity;
                m_ShieldHudPanel.UpdatePanelConfig();
                m_IsHudDirty = true;
            }
        }

        private void UpdateTextHud()
        {
            ClientConfig config = ConfigManager.ClientConfig;
            if (!m_TextHudAPI.Heartbeat)
                return;
            if (!config.ShowPanel)
                return;

            ClientLogger.Log("Starting UpdateTextHud()", 5);

            if (m_ShieldData != null)
            {
                m_ShieldHudPanel.UpdatePanel(ref m_ShieldData);
            }

            m_IsHudDirty = false;
            ClientLogger.Log("UpdateTextHud() done", 5);
        }

        private void UpdateHitEffectDuration(int _ticks)
        {
            foreach (var data in m_DrawList)
            {
                data.Ticks -= _ticks;
            }

            int removed = m_DrawList.RemoveAll((OtherCharacterShieldData _data) => { return _data.Ticks <= 0; });
        }

        private void UpdateFakeShieldStat()
        {
            if (m_ShieldData == null)
                return;
            if (m_ShieldData.PlayerSteamUserId == 0U)
                return;





            m_IsHudDirty = true;


        }

    }
}
