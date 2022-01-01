// ;
using Draygo.API;
using ExShared;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Utils;
using VRageMath;

using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace PocketShield
{
    static class Utils
    {
        public static string FormatDistanceAsString(float _distance)
        {
            if (_distance > 1000.0f)
            {
                _distance /= 1000.0f;
                return string.Format("{0:F1}km", _distance);
            }

            return string.Format("{0:F1}m", _distance);
        }

        public static string FormatShieldValue(float _value)
        {
            if ((int)_value > 10000)
                return string.Format("{0:F1}k", _value / 1000.0f);

            return ((int)_value).ToString();
        }

        public static string FormatPercent(float _percent)
        {
            if (_percent == 0.0f)
                return "0%";
            if (_percent == 1.0f)
                return "100%";
            if (_percent < 0.001f)
                return "0.1%";
            if (_percent > 0.999)
                return "99.9%";
            
            return string.Format("{0:F1}%", _percent * 100.0f);
        }

        public static Color CalculateBGColor(Color _color, float _opacity)
        {
            // SK: Stolen stuff
            // https://github.com/THDigi/BuildInfo/blob/master/Data/Scripts/BuildInfo/Utilities/Utils.cs#L256-L263
            //_color *= _opacity * _opacity * 1.075f;
            _color *= _opacity * _opacity * 0.90f;
            _color.A = (byte)(_opacity * 255.0f);

            return _color;
        }
    }

    public partial class ShieldHudPanel
    {
        private const float c_UvSizeX = 1.0f / Constants.TEXTURE_W;
        private const float c_UvSizeY = 1.0f / Constants.TEXTURE_H;
        private const float c_IconSize = 24.0f;
        private const float c_TextScale = 15.0f;

        public static readonly Color PlateColor = new Color(41, 54, 62);
        public static readonly Color BGColor = new Color(80, 92, 103);
        public static readonly Color BGColorDark = Color.FromNonPremultiplied(80, 92, 103, 127);
        public static readonly Color FGColor = new Color(162, 232, 252);

        public static int LastSlot { get; private set; }

        public float Percent { set { m_OverchargeIcon.Percent = value; UpdatePanelConfig(); } }

        public bool Visible { get; set; }
        public float BackgroundOpacity { get; set; }

        //private Vector2D SubstatPanelPos { get { return ConfigManager.ClientConfig.PanelPosition + new Vector2D(0.0, 55.0); } }
        private static Vector2D SubstatPanelOffset { get { return new Vector2D(0.0, 55.0 * ConfigManager.ClientConfig.ItemScale); } }

        private static Vector2 PanelSize
        {
            get
            {
                ClientConfig config = ConfigManager.ClientConfig;
                float bgWidth = Constants.PANEL_WIDTH * config.ItemScale;
                //float bgHeight = config.Padding * 2.0f + config.DisplayItemsCount * ItemCard.ItemCardHeight + config.Margin * 2.0f * (config.DisplayItemsCount - 1);
                float bgHeight = (float)SubstatPanelOffset.Y + ItemCard.Height + Constants.MARGIN * config.ItemScale;
                //if (config.ShowMaxRangeIcon)
                //    bgHeight += config.Padding;

                //float cursorPosY = config.Padding;
                //if (config.ShowMaxRangeIcon)
                //{
                //    bgHeight += RadarRangeIconHeight + config.Padding;
                //    cursorPosY += RadarRangeIconHeight + config.Padding;
                //}

                return new Vector2(bgWidth, bgHeight);
            }
        }

        private StringBuilder m_ShieldLabelSB = null;




        private HudAPIv2.BillBoardHUDMessage m_BackgroundMidPlate = null;

        private HudAPIv2.BillBoardHUDMessage m_ShieldIcon = null;
        private HudAPIv2.BillBoardHUDMessage m_ShieldBarBack = null;
        private HudAPIv2.BillBoardHUDMessage m_ShieldBarFore = null;
        private HudAPIv2.HUDMessage m_ShieldLabel = null;

        private CirclePB m_OverchargeIcon = null;

        private List<ItemCard> m_ItemCards = null;

        private int m_IconTextureSlot = Constants.TEXTURE_BLANK;
        private float m_ShieldBarWidth = 150.0f;



        private HudAPIv2.BillBoardTriHUDMessage m_Part = null;
        private HudAPIv2.BillBoardHUDMessage m_ComparePart = null;

        static ShieldHudPanel()
        {
            LastSlot = 0;

        }

        public ShieldHudPanel()
        {
            ClientConfig config = ConfigManager.ClientConfig;

            Visible = false;

            m_ItemCards = new List<ItemCard>(2);

            m_ShieldLabelSB = new StringBuilder("0");
            
            m_BackgroundMidPlate = new HudAPIv2.BillBoardHUDMessage()
            {
                Material = MyStringId.GetOrCompute("PocketShield_BG"),
                Blend = BlendTypeEnum.PostPP,
                Options = HudAPIv2.Options.Pixel
            };

            m_ShieldIcon = new HudAPIv2.BillBoardHUDMessage()
            {
                Material = MyStringId.GetOrCompute("PocketShield_ShieldIcons"),
                Offset = new Vector2D(15.0, 15.0),
                Width = c_IconSize,
                Height = c_IconSize,
                uvEnabled = true,
                uvSize = new Vector2(1.0f / Constants.TEXTURE_W, 1.0f / Constants.TEXTURE_H),
                //uvOffset = new Vector2((m_IconTextureSlot % Constants.TEXTURE_W) * c_UvSizeX, (m_IconTextureSlot / Constants.TEXTURE_W) * c_UvSizeY),
                BillBoardColor = FGColor,
                Blend = BlendTypeEnum.PostPP,
                Options = HudAPIv2.Options.Pixel
            };
            m_ShieldBarBack = new HudAPIv2.BillBoardHUDMessage()
            {
                Material = MyStringId.GetOrCompute("PocketShield_ShieldBar"),
                Offset = new Vector2D(55.0, 24.0),
                Width = m_ShieldBarWidth,
                Height = 11.0f,
                BillBoardColor = BGColor,
                Blend = BlendTypeEnum.PostPP,
                Options = HudAPIv2.Options.Pixel
            };
            m_ShieldBarFore = new HudAPIv2.BillBoardHUDMessage()
            {
                Material = MyStringId.GetOrCompute("PocketShield_ShieldBar"),
                Offset = new Vector2D(55.0, 24.0),
                Width = m_ShieldBarWidth,
                Height = 11.0f,
                uvEnabled = true,
                BillBoardColor = FGColor,
                Blend = BlendTypeEnum.PostPP,
                Options = HudAPIv2.Options.Pixel
            };
            m_ShieldLabel = new HudAPIv2.HUDMessage
            {
                Message = m_ShieldLabelSB,
                Offset = new Vector2D(210.0, 23.0),
                ShadowColor = Color.Black,
                
                Blend = BlendTypeEnum.PostPP,
                Options = HudAPIv2.Options.Pixel
            };

            AddItemCard(new ItemCard());
            AddItemCard(new ItemCard());








            m_Part = new HudAPIv2.BillBoardTriHUDMessage()
            {
                Material = MyStringId.GetOrCompute("Atlas_E_01"),
                Visible = false,
                Origin = new Vector2D(0.0, 0.0),

                P0 = new Vector2(0.0f, 0.0f),
                P1 = new Vector2(0.0f, 1.0f),
                P2 = new Vector2(1.0f, 1.0f),

                Width = 256,
                Height = 256,
                BillBoardColor = Color.White,

                Blend = BlendTypeEnum.PostPP,
                Options = HudAPIv2.Options.Pixel
            };
            m_ComparePart = new HudAPIv2.BillBoardHUDMessage()
            {
                Material = MyStringId.GetOrCompute("Atlas_E_01"),
                Visible = false,

                Origin = new Vector2D(288.0, 0.0),
                Width = 256,
                Height = 256,
                BillBoardColor = Color.White,

                Blend = BlendTypeEnum.PostPP,
                Options = HudAPIv2.Options.Pixel
            };



            m_OverchargeIcon = new CirclePB()
            {
                //Position = new Vector2D(0.0, 0.0)
            };
            //m_OverchargeIcon.Percent = 1.0f;

#if false
            m_Background = new HudAPIv2.BillBoardHUDMessage()
            {
                Material = MyStringId.GetOrCompute("Pantenna_BG"),
                Origin = config.PanelPosition,
                Width = config.PanelWidth,
                Height = 0.0f,
                Visible = Visible,
                BillBoardColor = CalculateBGColor(),
                Blend = BlendTypeEnum.PostPP,
                Options = HudAPIv2.Options.Pixel
            };
            m_TextureSlot = Constants.TEXTURE_ANTENNA;
            m_RadarRangeIcon = new HudAPIv2.BillBoardHUDMessage()
            {
                Material = MyStringId.GetOrCompute("Pantenna_ShipIcons"),
                Origin = config.PanelPosition + new Vector2D(config.Padding, config.Padding),
                Width = RadarRangeIconHeight,
                Height = RadarRangeIconHeight,
                uvEnabled = true,
                uvSize = new Vector2(0.25f, 0.5f),
                uvOffset = new Vector2((m_TextureSlot % 4) * 0.25f, (m_TextureSlot / 4) * 0.5f),
                TextureSize = 1.0f,
                Visible = Visible,
                Blend = BlendTypeEnum.PostPP,
                Options = HudAPIv2.Options.Pixel
            };
            m_RadarRangeLabel = new HudAPIv2.HUDMessage()
            {
                Message = m_RadarRangeSB,
                //Origin = config.PanelPosition + new Vector2D(s_RadarRangeIconSize + config.Padding + config.SpaceBetweenItems, config.Padding + 8.0 * config.ItemScale),
                //Font = MyFontEnum.Red,
                Scale = RadarPanel.LabelScale,
                //InitialColor = color,
                ShadowColor = Color.Black,
                Visible = Visible,
                Blend = BlendTypeEnum.PostPP,
                Options = HudAPIv2.Options.Pixel | HudAPIv2.Options.Shadowing
            };

            Color color = Color.Darken(Color.FromNonPremultiplied(218, 62, 62, 255), 0.2);
            Vector2D cursorPos = config.PanelPosition + new Vector2D(config.Padding, RadarRangeIconHeight + config.Padding * 2.0f);
            //Color color = new Color(218, 62, 62);
            //Color color = Color.White;

            for (int i = 0; i < Constants.DISPLAY_ITEMS_COUNT; ++i)
            {
                ItemCard item = new ItemCard(cursorPos, color);
                cursorPos = item.NextItemPosition;
                m_ItemCards.Add(item);
            }
#endif

            UpdatePanelConfig();
        }

        ~ShieldHudPanel()
        {
            m_ShieldLabelSB = null;

        }

        public void UpdatePanel(ref MyShieldData _data)
        {
            if (_data.PlayerSteamUserId == 0U)
            {
                if (Visible)
                {
                    Visible = false;
                    UpdatePanelConfig();
                }
                return;
            }
            if (!Visible)
            {
                Visible = true;
                UpdatePanelConfig();
            }
                m_IconTextureSlot = Constants.TEXTURE_BLANK;
            
            if (_data.SubtypeId == MyStringHash.GetOrCompute(Constants.SUBTYPEID_EMITTER_BAS))
            {
                m_IconTextureSlot = Constants.TEXTURE_SHIELD_BAS;
            }
            else if (_data.SubtypeId == MyStringHash.GetOrCompute(Constants.SUBTYPEID_EMITTER_ADV))
            {
                m_IconTextureSlot = Constants.TEXTURE_SHIELD_ADV;
            }

            m_ShieldLabelSB.Clear();
            m_ShieldLabelSB.Append(Utils.FormatShieldValue(_data.Energy));

            m_ShieldIcon.uvOffset = new Vector2((m_IconTextureSlot % Constants.TEXTURE_W) * c_UvSizeX, (m_IconTextureSlot / Constants.TEXTURE_W) * c_UvSizeY);

            m_ShieldBarFore.uvSize = new Vector2(_data.EnergyRemainingPercent, 1.0f);
            m_ShieldBarFore.Width = m_ShieldBarWidth * _data.EnergyRemainingPercent;
               

            //m_TrajectoryIcon.uvOffset = new Vector2((m_TrajectoryTextureSlot % 4) * 0.25f, (m_TrajectoryTextureSlot / 4) * 0.5f);

            //m_OverchargeIcon.Percent = _data.OverchargeRemainingPercent;

            //ClientLogger.Log("_data.EnergyRemainingPercent = " + _data.EnergyRemainingPercent);
            m_OverchargeIcon.Percent = _data.EnergyRemainingPercent;

            m_OverchargeIcon.UpdateItemCard();

            //ClientLogger.Log("count =    " + m_ItemCards.Count);
            //ClientLogger.Log("defcount = " + _data.Def.Count);
            //ClientLogger.Log("rescount = " + _data.Res.Count);
            for (int i = 0; i < m_ItemCards.Count; ++i)
            {
                ItemCard item = m_ItemCards[i];
                if (i < _data.Def.Count)
                {
                    item.Def = _data.Def[i].Item2;
                    item.Res = _data.Res[i].Item2;

                    item.DefTextureSlot = GetDefTextureSlot(_data.Def[i].Item1);
                    item.ResTextureSlot = GetResTextureSlot(_data.Res[i].Item1);
                }
                else
                {
                    item.Def = 0.0f;
                    item.Res = 0.0f;
                }

                item.UpdateItemCard();
            }

        }

        public void UpdatePanelConfig()
        {
            ClientLogger.Log("UpdatePanelConfig()...", 5);
            ClientConfig config = ConfigManager.ClientConfig;

            Vector2 panelSize = PanelSize;

            m_BackgroundMidPlate.Visible = Visible && config.ShowPanel && config.ShowPanelBackground;
            m_BackgroundMidPlate.Origin = config.PanelPosition;
            m_BackgroundMidPlate.Width = PanelSize.X;
            m_BackgroundMidPlate.Height = PanelSize.Y;
            m_BackgroundMidPlate.BillBoardColor = Utils.CalculateBGColor(PlateColor, BackgroundOpacity);

            m_ShieldIcon.Visible = Visible && config.ShowPanel;
            m_ShieldIcon.Origin = config.PanelPosition;
            m_ShieldIcon.Scale = 1.0f;

            m_ShieldBarFore.Visible = Visible && config.ShowPanel;
            m_ShieldBarFore.Origin = config.PanelPosition;
            m_ShieldBarFore.Scale = 1.0f;

            m_ShieldBarBack.Visible = Visible && config.ShowPanel;
            m_ShieldBarBack.Origin = config.PanelPosition;
            m_ShieldBarBack.Scale = 1.0f;

            m_ShieldLabel.Visible = Visible && config.ShowPanel;
            m_ShieldLabel.Origin = config.PanelPosition;
            m_ShieldLabel.Scale = c_TextScale * 1.0f;

            m_OverchargeIcon.Visible = Visible && config.ShowPanel && false;
            //m_OverchargeIcon.Position = shieldIconPos;
            m_OverchargeIcon.Position = config.PanelPosition + m_ShieldIcon.Offset;
            m_OverchargeIcon.Color = FGColor;

            m_OverchargeIcon.UpdateItemCardConfig();

            foreach (ItemCard item in m_ItemCards)
            {
                item.Visible = Visible && config.ShowPanel;
                item.Origin = config.PanelPosition + SubstatPanelOffset;
                item.UpdateItemCardConfig();
            }

#if false
            

            float cursorPosY = (config.ShowMaxRangeIcon ? RadarRangeIconHeight + config.Padding : 0) + config.Padding;

            Logger.Log(">>   ItemCardSize = (" + ItemCard.ItemCardWidth + ", " + ItemCard.ItemCardHeight + ")", 5);
            Logger.Log(">>   bgWidth = " + panelSize.X, 5);
            Logger.Log(string.Format(">>   bgHeight = {0:0} * 2.0f + {1:0} * {2:0} + {3:0} * ({4:0} - 1) = {5:0}",
                config.Padding, config.DisplayItemsCount, ItemCard.ItemCardHeight, config.Margin * 2.0f, config.DisplayItemsCount, panelSize.Y), 5);
            Logger.Log(string.Format(">>   bgHeight = {0:0} + {1:0} + {2:0} = {3:0}",
                config.Padding * 3.0f, config.DisplayItemsCount * ItemCard.ItemCardHeight, config.Margin * 2.0f * (config.DisplayItemsCount - 1), panelSize.Y), 5);

            Logger.Log(">>   Visible = " + Visible + ", ShowPanelBG = " + config.ShowPanelBackground + ", ShowPanel = " + config.ShowPanel, 5);
            m_Background.Visible = Visible && config.ShowPanelBackground && config.ShowPanel;
            Logger.Log(">>     m_Background.Visible = " + m_Background.Visible, 5);
            m_Background.Origin = config.PanelPosition;
            m_Background.Width = panelSize.X;
            m_Background.Height = panelSize.Y;
            m_Background.BillBoardColor = CalculateBGColor();

            m_RadarRangeIcon.Visible = Visible && config.ShowMaxRangeIcon && config.ShowPanel;
            m_RadarRangeIcon.Origin = config.PanelPosition;
            m_RadarRangeIcon.Offset = new Vector2D(config.Padding, config.Padding);
            m_RadarRangeIcon.Width = RadarRangeIconHeight;
            m_RadarRangeIcon.Height = RadarRangeIconHeight;

            float offsY = config.Padding + RadarRangeIconHeight * 0.5f - Constants.MAGIC_LABEL_HEIGHT_16 * 0.5f * config.ItemScale;
            m_RadarRangeLabel.Visible = Visible && config.ShowMaxRangeIcon && config.ShowPanel;
            m_RadarRangeLabel.Origin = config.PanelPosition;
            m_RadarRangeLabel.Offset = new Vector2D(RadarRangeIconHeight + config.Padding + config.Margin * 2.0f, config.Padding + 8.0 * config.ItemScale);
            m_RadarRangeLabel.Scale = RadarPanel.LabelScale;
            Logger.Log(">>   Scale = " + config.ItemScale + ", Label Y = " + m_RadarRangeLabel.GetTextLength().Y, 5);

            Vector2D cursorPos = config.PanelPosition + new Vector2D(config.Padding, cursorPosY);
            for (int i = 0; i < Constants.DISPLAY_ITEMS_COUNT; ++i)
            {
                ItemCard item = m_ItemCards[i];
                item.Visible = Visible && config.ShowPanel;
                item.Position = cursorPos;
                item.UpdateItemCardConfig();
                cursorPos = item.NextItemPosition;
            }
#endif


        }

        private void AddItemCard(ItemCard _itemCard)
        {
            m_ItemCards.Add(_itemCard);
            LastSlot = m_ItemCards.Count;
        }

        private int GetDefTextureSlot(MyStringHash _damageType)
        {
            if (_damageType.String.EndsWith("Bullet"))
                return 2;

            if (_damageType.String.EndsWith("Explosion"))
                return 6;

            return 0;
        }
        
        private int GetResTextureSlot(MyStringHash _damageType)
        {
            if (_damageType.String.EndsWith("Bullet"))
                return 3;

            if (_damageType.String.EndsWith("Explosion"))
                return 7;

            return 0;
        }
    }
}


#if false
namespace Pantenna
{ 


    
    public partial class RadarPanel
    {
        public bool Visible { get; set; }
        public float BackgroundOpacity { get; set; }

        private Color m_HudBGColor = Color.White;
        private List<ItemCard> m_ItemCards = null; /* This keeps track of all 5 displayed item cards. */

        private int m_TextureSlot = 3;

        private static float RadarRangeIconHeight
        {
            get { return 30.0f * ConfigManager.ClientConfig.ItemScale; }
        }

        public static float LabelScale
        {
            get { return 16.0f * ConfigManager.ClientConfig.ItemScale; }
        }

        public static float PanelMinWidth
        {
            get { return ItemCard.ItemCardMinWidth + ConfigManager.ClientConfig.Padding * 2.0f; }
        }

        private StringBuilder m_RadarRangeSB = null;
        //private StringBuilder m_DummySB = null;

        private HudAPIv2.BillBoardHUDMessage m_Background = null;
        private HudAPIv2.BillBoardHUDMessage m_RadarRangeIcon = null;
        private HudAPIv2.HUDMessage m_RadarRangeLabel = null;

        public RadarPanel()
        {
        }

        ~RadarPanel()
        {
            m_ItemCards.Clear();
            m_ItemCards = null;

            //s_CharacterSize.Clear();
            //s_CharacterSize = null;

            m_RadarRangeSB = null;
            //m_DummySB = null;
        }

        public void UpdatePanel(List<SignalData> _signals)
        {
            ClientConfig config = ConfigManager.ClientConfig;

            m_RadarRangeSB.Clear();
            m_RadarRangeSB.Append(Utils.FormatDistanceAsString(config.RadarMaxRange));

            Logger.Log("  signal count: " + _signals.Count, 5);
            for (int i = 0; i < Constants.DISPLAY_ITEMS_COUNT; ++i)
            {
                ItemCard item = m_ItemCards[i];
                if (i >= _signals.Count || i >= config.DisplayItemsCount)
                {
                    Logger.Log("  updating item card " + i + ": i > _signal.Count, hide item", 5);
                    item.Visible = false;
                    item.UpdateItemCard();
                }
                else
                {
                    Logger.Log("  updating item card " + i + ": updating item", 5);
                    SignalData signal = _signals[i];

                    item.Visible = Visible && config.ShowPanel;
                    item.SignalType = signal.SignalType;
                    item.RelativeVelocity = signal.Velocity;
                    item.Distance = signal.Distance;

                    item.DisplayNameRawString = signal.DisplayName;

                    item.UpdateItemCard();
                }
            }
        }

        public void UpdatePanelConfig()
        {
        }


    }
}
#endif
