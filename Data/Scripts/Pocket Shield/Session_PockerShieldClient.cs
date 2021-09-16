// ;
using Sandbox.ModAPI;
using System;
using System.Text;
using VRage.Game;
using VRage.Game.Components;
using VRage.Utils;
using VRageMath;

using Draygo.API;
using PocketShield.Core;

using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace PocketShield
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class Session_PocketShieldClient : MySessionComponentBase
    {
        //private const ushort MSG_HANDLER_ID = 1351;
        //private const ushort MSG_HANDLER_ID_2 = 1352;
        //private const int WAIT_TICKS = 300;
        //private const string MSG_REQ_INITIAL_VAL = "REQ_SHIELD_INITIAL_VAL";

        private bool IsServer = false;
        private bool IsDedicated = false;
        private bool IsSetupDone = false;
        private bool IsTextHudApiInitDone = false;
        private int Tick = Constants.WAIT_TICKS_CLIENT;

        private HudAPIv2 m_TextHudAPI = null;
        private PlayerShieldDataSync m_MyPlayerShieldData = null;
        private bool m_IsHudDirty = false;        

        private const float s_BonusIconPosY = 777.0f;
        private const float s_BonusStringPosY = 817.0f;
        private const float s_MainStringScale = 13.0f;
        private const float s_BonusStringScale = 13.0f;

        #region HUD Elements
        private readonly MyStringId s_PSIconBGStringId = MyStringId.GetOrCompute("PocketShield_BG");
        private readonly MyStringId s_PSIconShieldBarStringId = MyStringId.GetOrCompute("PocketShield_ShieldBar");
        private readonly MyStringId s_PSIconShieldMainIconStringId = MyStringId.GetOrCompute("PocketShield_ShieldMainIcon");
        private readonly MyStringId s_PSIconShieldBonusIcon0StringId = MyStringId.GetOrCompute("PocketShield_ShieldBonusIcon0");
        private readonly MyStringId s_PSIconShieldBonusIcon1StringId = MyStringId.GetOrCompute("PocketShield_ShieldBonusIcon1");
        private readonly MyStringId s_PSIconShieldBonusIcon2StringId = MyStringId.GetOrCompute("PocketShield_ShieldBonusIcon2");
        private readonly MyStringId s_PSIconShieldBonusIcon3StringId = MyStringId.GetOrCompute("PocketShield_ShieldBonusIcon3");

        private HudAPIv2.BillBoardHUDMessage m_PSIconBG = null;
        private HudAPIv2.BillBoardHUDMessage m_PSIconBar = null;
        private HudAPIv2.BillBoardHUDMessage m_PSIconMain = null;
        private HudAPIv2.BillBoardHUDMessage m_PSIconBonus0 = null;
        private HudAPIv2.BillBoardHUDMessage m_PSIconBonus1 = null;
        private HudAPIv2.BillBoardHUDMessage m_PSIconBonus2 = null;
        private HudAPIv2.BillBoardHUDMessage m_PSIconBonus3 = null;

        private StringBuilder m_PSShieldCapSB = null;
        private StringBuilder m_PSShieldBonus0SB = null;
        private StringBuilder m_PSShieldBonus1SB = null;
        private StringBuilder m_PSShieldBonus2SB = null;
        private StringBuilder m_PSShieldBonus3SB = null;

        private HudAPIv2.HUDMessage m_PSShieldCap = null;
        private HudAPIv2.HUDMessage m_PSShieldBonus0 = null;
        private HudAPIv2.HUDMessage m_PSShieldBonus1 = null;
        private HudAPIv2.HUDMessage m_PSShieldBonus2 = null;
        private HudAPIv2.HUDMessage m_PSShieldBonus3 = null;

        private float m_PsIconBar_Width = 150.0f;
        #endregion

        #region Test
        private float m_GlobalPercentage = 0.0f;
        #endregion

        private Logger m_Logger = null;

        public override void LoadData()
        {
            m_Logger = new Logger(LoggerSide.CLIENT);
            m_Logger.Init();
            //m_Logger.Log("Starting LoadData()");
        }

        //public override void BeforeStart()
        //{
        //    // executed before the world starts updating
        //}

        protected override void UnloadData()
        {
            //m_Logger.Log("Starting UnloadData()");

            Shutdown();

            m_Logger.DeInit();
        }

        //public override void HandleInput()
        //{
        //    // gets called 60 times a second before all other update methods, regardless of framerate, game pause or MyUpdateOrder.
        //}

        public override void UpdateBeforeSimulation()
        {
            if (MyAPIGateway.Session == null)
                return;

            try
            {
                if (MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE || MyAPIGateway.Multiplayer.IsServer)
                {
                    IsServer = true;
                    if (MyAPIGateway.Utilities.IsDedicated)
                        IsDedicated = true;
                    else
                        IsDedicated = false;
                }

                if (IsDedicated)
                    return;
            }
            catch (Exception _e)
            { }

            if (Tick >= Constants.WAIT_TICKS_CLIENT)
            {
                Tick = 0;

                if (!IsSetupDone)
                {
                    Setup();
                    if (!IsServer)
                    {
                        m_Logger.Log("  Sending Initial Shield Value request to server.");
                        MyAPIGateway.Multiplayer.SendMessageToServer(Constants.MSG_HANDLER_ID_2, Encoding.Unicode.GetBytes(Constants.MSG_REQ_INITIAL_VAL));
                    }
                }

                //m_Logger.Log("  IsTextHudApiInitDone = " + IsTextHudApiInitDone + ", Heartbeat = " + m_TextHudAPI.Heartbeat);
                if (!IsTextHudApiInitDone)
                {
                    InitTextHud();
                }


            }
            
            if (Tick % 5 == 0)
            {
                if (m_MyPlayerShieldData != null)
                {
                    // "simulate" shield charging, this value will be overwritten on next sync;
                    if (m_MyPlayerShieldData.Health < m_MyPlayerShieldData.MaxHealth && m_MyPlayerShieldData.ChargeRate > 0.0f)
                    {
                        m_MyPlayerShieldData.Health += m_MyPlayerShieldData.ChargeRate / 60.0f * 5.0f;
                        m_IsHudDirty = true;
                    }
                }

                if (m_IsHudDirty)
                {
                    UpdateTextHud();
                }
            }





            ++Tick;
        }

        //public override void Simulate()
        //{
        //    // executed every tick, 60 times a second, during physics simulation and only if game is not paused.
        //    // NOTE in this example this won't actually be called because of the lack of MyUpdateOrder.Simulation argument in MySessionComponentDescriptor
        //}

        //public override void UpdateAfterSimulation()
        //{
        //    // executed every tick, 60 times a second, after physics simulation and only if game is not paused.

        //    try // example try-catch for catching errors and notifying player, use only for non-critical code!
        //    {
        //        // ...
        //    }
        //    catch (Exception e) // NOTE: never use try-catch for code flow or to ignore errors! catching has a noticeable performance impact.
        //    {
        //        MyLog.Default.WriteLineAndConsole($"{e.Message}\n{e.StackTrace}");

        //        if (MyAPIGateway.Session?.Player != null)
        //            MyAPIGateway.Utilities.ShowNotification($"[ ERROR: {GetType().FullName}: {e.Message} | Send SpaceEngineers.Log to mod author ]", 10000, MyFontEnum.Red);
        //    }
        //}

        //public override void Draw()
        //{
        //    // gets called 60 times a second after all other update methods, regardless of framerate, game pause or MyUpdateOrder.
        //    // NOTE: this is the only place where the camera matrix (MyAPIGateway.Session.Camera.WorldMatrix) is accurate, everywhere else it's 1 frame behind.
        //}

        //public override void SaveData()
        //{
        //    // executed AFTER world was saved
        //}

        //public override MyObjectBuilder_SessionComponent GetObjectBuilder()
        //{
        //    // executed during world save, most likely before entities.

        //    return base.GetObjectBuilder(); // leave as-is.
        //}

        //public override void UpdatingStopped()
        //{
        //    // executed when game is paused
        //}

        public void SyncShieldStatus(ushort _handlerId, byte[] _package, ulong _playerId, bool _sentMsg)
        {
            //m_Logger.Log("Starting SyncShieldStatus()");
            try
            {

                string decodedPackage = Encoding.Unicode.GetString(_package);
                //m_Logger.Log("  _handlerId = " + _handlerId + ", _package = " + decodedPackage + ", _playerId = " + _playerId + ", _sentMsg = " + _sentMsg);
                //m_Logger.Log("  Recieved message from client <" + _playerId + ">: " + decodedPackage);
                m_MyPlayerShieldData = MyAPIGateway.Utilities.SerializeFromXML<PlayerShieldDataSync>(decodedPackage);
                if (m_MyPlayerShieldData == null)
                {
                    return;
                }

                m_IsHudDirty = true;
                //m_Logger.Log("  Recieved PlayerShieldData for player [" + m_MyPlayerShieldData.SteamUserId + "].");
                
                // do update;
                UpdateTextHud();
            }
            catch (Exception _e)
            { }
        }

        public void Setup()
        {
            m_Logger.Log("Starting Setup()");

            //m_Logger.Log("  Initializing Text HUD API v2...");
            m_TextHudAPI = new HudAPIv2();

            //m_Logger.Log("  Registering Message Handler (id " + Constants.MSG_HANDLER_ID + ")...");
            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(Constants.MSG_HANDLER_ID, SyncShieldStatus);
            
            m_Logger.Log("  IsServer = " + IsServer);
            m_Logger.Log("  IsDedicated = " + IsDedicated);

            m_Logger.Log("  Setup Done.");
            IsSetupDone = true;
        }

        private void Shutdown()
        {
            m_Logger.Log("  Starting Shutdown()");

            try
            {
                //m_Logger.Log("    Shutting down Text HUD API v2...");
                m_TextHudAPI.Close();

                //m_Logger.Log("    UnRegistering Message Handler (id " + Constants.MSG_HANDLER_ID + ")...");
                MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(Constants.MSG_HANDLER_ID, SyncShieldStatus);
            }
            catch (Exception _e)
            { }
        }

        private void InitTextHud()
        {
            m_Logger.Log("Starting InitTextHud()");
            if (!m_TextHudAPI.Heartbeat)
            {
                m_Logger.Log("  Text Hud API hasn't recieved heartbeat.");
                if (Tick >= Constants.WAIT_TICKS_CLIENT)
                {
                    MyAPIGateway.Utilities.ShowNotification("Text HUD API mod is missing. HUD will not be displayed.", Constants.WAIT_TICKS_CLIENT / 2, MyFontEnum.Red);
                    m_Logger.Log("  Text Hud API mod is missing.");
                }

                return;
            }

            #region Icons
            m_PSIconBG = new HudAPIv2.BillBoardHUDMessage()
            {
                Material = s_PSIconBGStringId,
                Origin = new Vector2D(20.0, 720.0),
                BillBoardColor = Color.White,
                TextureSize = 1.0f,
                uvEnabled = true,
                uvSize = new Vector2(278.0f / 512.0f, 1.0f),
                uvOffset = new Vector2(117.0f / 512.0f, 0.0f),
                Options = HudAPIv2.Options.Pixel,
                Width = 260.0f,
                Height = 120.0f,
                Visible = true,
                Blend = BlendTypeEnum.PostPP,
            };

            m_PSIconBar = new HudAPIv2.BillBoardHUDMessage()
            {
                Material = s_PSIconShieldBarStringId,
                Origin = new Vector2D(71.0, 742.0),
                BillBoardColor = Color.White,
                TextureSize = 1.0f,
                uvEnabled = true,
                uvSize = new Vector2(230.0f / 256.0f, 1.0f),
                uvOffset = new Vector2(13.0f / 256.0f, 0.0f),
                Options = HudAPIv2.Options.Pixel,
                Width = m_PsIconBar_Width,
                Height = 10.0f,
                Visible = true,
                Blend = BlendTypeEnum.PostPP,
            };

            m_PSIconMain = new HudAPIv2.BillBoardHUDMessage()
            {
                Material = s_PSIconShieldMainIconStringId,
                Origin = new Vector2D(43.0, 735.0),
                BillBoardColor = Color.White,
                //TextureSize = 1.0f,
                //uvEnabled = true,
                //uvSize = new Vector2(1.0f, 1.0f),
                //uvOffset = new Vector2(0.0f, 0.0f),
                Options = HudAPIv2.Options.Pixel,
                Width = 23.0f,
                Height = 23.0f,
                Visible = true,
                Blend = BlendTypeEnum.PostPP,
            };

            m_PSIconBonus0 = new HudAPIv2.BillBoardHUDMessage()
            {
                Material = s_PSIconShieldBonusIcon0StringId,
                Origin = new Vector2D(49.0, s_BonusIconPosY),
                BillBoardColor = Color.White,
                //TextureSize = 1.0f,
                //uvEnabled = true,
                //uvSize = new Vector2(1.0f, 1.0f),
                //uvOffset = new Vector2(0.0f, 0.0f),
                Options = HudAPIv2.Options.Pixel,
                Width = 23.0f,
                Height = 23.0f,
                Visible = true,
                Blend = BlendTypeEnum.PostPP,
            };

            m_PSIconBonus1 = new HudAPIv2.BillBoardHUDMessage()
            {
                Material = s_PSIconShieldBonusIcon1StringId,
                Origin = new Vector2D(109.0, s_BonusIconPosY),
                BillBoardColor = Color.White,
                //TextureSize = 1.0f,
                //uvEnabled = true,
                //uvSize = new Vector2(1.0f, 1.0f),
                //uvOffset = new Vector2(0.0f, 0.0f),
                Options = HudAPIv2.Options.Pixel,
                Width = 23.0f,
                Height = 23.0f,
                Visible = true,
                Blend = BlendTypeEnum.PostPP,
            };

            m_PSIconBonus2 = new HudAPIv2.BillBoardHUDMessage()
            {
                Material = s_PSIconShieldBonusIcon2StringId,
                Origin = new Vector2D(169.0, s_BonusIconPosY),
                BillBoardColor = Color.White,
                //TextureSize = 1.0f,
                //uvEnabled = true,
                //uvSize = new Vector2(1.0f, 1.0f),
                //uvOffset = new Vector2(0.0f, 0.0f),
                Options = HudAPIv2.Options.Pixel,
                Width = 23.0f,
                Height = 23.0f,
                Visible = true,
                Blend = BlendTypeEnum.PostPP,
            };

            m_PSIconBonus3 = new HudAPIv2.BillBoardHUDMessage()
            {
                Material = s_PSIconShieldBonusIcon3StringId,
                Origin = new Vector2D(229.0, s_BonusIconPosY),
                BillBoardColor = Color.White,
                //TextureSize = 1.0f,
                //uvEnabled = true,
                //uvSize = new Vector2(1.0f, 1.0f),
                //uvOffset = new Vector2(0.0f, 0.0f),
                Options = HudAPIv2.Options.Pixel,
                Width = 23.0f,
                Height = 23.0f,
                Visible = true,
                Blend = BlendTypeEnum.PostPP,
            };
            #endregion

            #region Strings
            m_PSShieldCapSB = new StringBuilder("9999.9k+");
            m_PSShieldBonus0SB = new StringBuilder("+999.9%");
            m_PSShieldBonus1SB = new StringBuilder("99.9%");
            m_PSShieldBonus2SB = new StringBuilder("99.9%");
            m_PSShieldBonus3SB = new StringBuilder("99.9%");

            m_PSShieldCap = new HudAPIv2.HUDMessage()
            {
                Message = m_PSShieldCapSB,
                Origin = new Vector2D(227.0f, 741.0f),
                Scale = s_MainStringScale,
                Options = HudAPIv2.Options.Pixel,
                Blend = BlendTypeEnum.PostPP,
            };

            m_PSShieldBonus0 = new HudAPIv2.HUDMessage()
            {
                Message = m_PSShieldBonus0SB,
                Origin = new Vector2D(33.0f, s_BonusStringPosY),
                Scale = s_BonusStringScale,
                Options = HudAPIv2.Options.Pixel,
                Blend = BlendTypeEnum.PostPP,
            };

            m_PSShieldBonus1 = new HudAPIv2.HUDMessage()
            {
                Message = m_PSShieldBonus1SB,
                Origin = new Vector2D(99.0f, s_BonusStringPosY),
                Scale = s_BonusStringScale,
                Options = HudAPIv2.Options.Pixel,
                Blend = BlendTypeEnum.PostPP,
            };

            m_PSShieldBonus2 = new HudAPIv2.HUDMessage()
            {
                Message = m_PSShieldBonus2SB,
                Origin = new Vector2D(159.0f, s_BonusStringPosY),
                Scale = s_BonusStringScale,
                Options = HudAPIv2.Options.Pixel,
                Blend = BlendTypeEnum.PostPP,
            };

            m_PSShieldBonus3 = new HudAPIv2.HUDMessage()
            {
                Message = m_PSShieldBonus3SB,
                Origin = new Vector2D(219.0f, s_BonusStringPosY),
                Scale = s_BonusStringScale,
                Options = HudAPIv2.Options.Pixel,
                Blend = BlendTypeEnum.PostPP,
            };
            #endregion

            IsTextHudApiInitDone = true;
        }

        private void UpdateTextHud()
        {
            //m_Logger.Log("Starting UpdateTextHud()");
            if (!IsTextHudApiInitDone)
                return;

            if (m_MyPlayerShieldData == null)
                return;

            if (!m_IsHudDirty)
                return;

            float hpPercent = m_MyPlayerShieldData.Health / m_MyPlayerShieldData.MaxHealth;
            //m_Logger.Log("  Shield = " + m_MyPlayerShieldData.Health + " (" + hpPercent * 100.0f + "%)");

            m_PSIconBar.uvSize = new Vector2(MathHelper.Lerp(0.0f, 230.0f / 256.0f, hpPercent), 1.0f);
            m_PSIconBar.Width = m_PsIconBar_Width * hpPercent;

            m_PSShieldCapSB.Clear();
            if (m_MyPlayerShieldData.Health <= 1000.0f)
            {
                m_PSShieldCapSB.AppendFormat("{0:F0}", m_MyPlayerShieldData.Health);
            }
            else if (m_MyPlayerShieldData.Health < 1000000.0f)
            {
                m_PSShieldCapSB.AppendFormat("{0:F2}k", m_MyPlayerShieldData.Health / 1000.0f);
            }
            else
            {
                m_PSShieldCapSB.Append("9999.9k+");
            }

            m_PSShieldBonus0SB.Clear();
            //if (m_PlayerShieldData.HealthBonus > 0.0f)
                m_PSShieldBonus0SB.Append('+');
            m_PSShieldBonus0SB.AppendFormat("{0:F1}%", m_MyPlayerShieldData.HealthBonus * 100.0f);
            m_PSShieldBonus1SB.Clear();
            m_PSShieldBonus1SB.AppendFormat("{0:F1}%", m_MyPlayerShieldData.Defense * 100.0f);
            m_PSShieldBonus2SB.Clear();
            m_PSShieldBonus2SB.AppendFormat("{0:F1}%", m_MyPlayerShieldData.BulletDamageResistance * 100.0f);
            m_PSShieldBonus3SB.Clear();
            m_PSShieldBonus3SB.AppendFormat("{0:F1}%", m_MyPlayerShieldData.ExplosiveDamageResistance * 100.0f);
            
            m_IsHudDirty = false;
        }
    }
}
