// ;
using ExShared;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game;
using VRage.Game.Components;
using VRageMath;

namespace PocketShield
{
    public partial class Session_PocketShieldClient
    {
        public void HandleSyncShieldData(ushort _handlerId, byte[] _package, ulong _senderPlayerId, bool _sentMsg)
        {
            if (MyAPIGateway.Session == null)
                return;

            ClientLogger.Log("Starting HandleSyncShieldData()", 5);

            try
            {
                string decodedPackage = Encoding.Unicode.GetString(_package);
                //ClientLogger.Log("  _handlerId = " + _handlerId + ", _package = " + decodedPackage + ", _senderPlayerId = " + _senderPlayerId + ", _sentMsg = " + _sentMsg, 5);
                ClientLogger.Log("  Recieved message from <" + _senderPlayerId + ">: " + decodedPackage, 5);

                SyncObject obj = MyAPIGateway.Utilities.SerializeFromXML<SyncObject>(decodedPackage);

                if (obj.m_OthersShieldData != null && obj.m_OthersShieldData.Count > 0)
                {
                    foreach (var data in obj.m_OthersShieldData)
                    {
                        data.ShouldPlaySound = true;
                        AddOrUpdateData(data);
                    }
                }

                if (obj.m_MyShieldData != null)
                {
                    if (obj.m_MyShieldData.PlayerSteamUserId == 0U || obj.m_MyShieldData.PlayerSteamUserId == MyAPIGateway.Session.Player.SteamUserId)
                    {
                        ClientLogger.Log("  Shield Data updated", 5);
                        m_ShieldData = obj.m_MyShieldData;
                        m_IsHudDirty = true;
                    }
                    else
                    {
                        ClientLogger.Log("  Data is for player <" + m_ShieldData.PlayerSteamUserId + ">, not me", 4);
                    }
                }
            }
            catch (Exception _e)
            {
                ClientLogger.Log("  > Exception < Error during parsing sync data from <" + _senderPlayerId + ">: " + _e.Message, 0);
            }
        }
        
        public void AddOrUpdateData(OtherCharacterShieldData _data)
        {
            foreach (var data in m_DrawList)
            {
                if (data.EntityId == _data.EntityId)
                {
                    data.Ticks = _data.Ticks;
                    data.ShieldAmountPercent = _data.ShieldAmountPercent;
                    data.ShouldPlaySound = _data.ShouldPlaySound;
                    return;
                }
            }
            
            m_DrawList.Add(_data);
        }
        

    }
}
