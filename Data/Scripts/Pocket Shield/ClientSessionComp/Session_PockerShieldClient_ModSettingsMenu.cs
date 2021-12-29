// ;
using Sandbox.ModAPI;

namespace PocketShield
{
    public partial class Session_PocketShieldClient
    {

        public void RefreshSettingsMenu()
        {
            // TODO: refresh settings menu;

#if false
            RefreshSettingsMenuRootItem();

            m_ConfigVersionItem.Text = ConfigVersionString;

            m_LogLevelItem.Text = LogLevelString;
            m_LogLevelItem.InitialPercent = LogLevelInitialPercent;

            m_ClientUpdateIntervalItem.Text = ClientUpdateIntervalString;

            m_ItemsCountItem.Text = ItemsCountString;
            m_ItemsCountItem.InitialPercent = ItemsCountInitialPercent;

            m_MaxRangeItem.Text = MaxRangeString;

            m_SensitivityItem.Text = SensitivityString;
            m_SensitivityItem.InitialPercent = SensitivityInitialPercent;

            // HUD settings;
            m_ShowPanelItem.Text = ShowPanelString;
            RefreshHudSettingsInteractability();

            m_ShowPanelBGItem.Text = ShowPanelBGString;

            m_ShowMaxRangeItem.Text = ShowMaxRangeString;

            m_ShowDisplayNameItem.Text = ShowDisplayNameString;

            m_PanelPositionItem.Text = PanelPositionString;

            m_PanelWidthItem.Text = PanelWidthString;
            m_PanelWidthItem.InitialPercent = PanelWidthInitialPercent;

            m_PaddingItem.Text = PaddingString;
            m_PaddingItem.InitialPercent = PaddingInitialPercent;

            m_MarginItem.Text = MarginString;
            m_MarginItem.InitialPercent = MarginInitialPercent;

            m_ScaleItem.Text = ScaleString;
            m_ScaleItem.InitialPercent = ScaleInitialPercent;
#endif
        }

        public void LoadConfig()
        {
            if (ConfigManager.ClientConfig.LoadConfigFile())
            {
                MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] Config reloaded", 2000);
                UpdatePanelConfig();
                RefreshSettingsMenu();
            }
            else
            {
                MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] Config reload failed", 2000);
            }
        }

        public void SaveConfig()
        {
            if (ConfigManager.ClientConfig.SaveConfigFile())
            {
                MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] Config saved", 2000);
            }
            else
            {
                MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] Config saving failed", 2000);
            }
        }


    }
}
