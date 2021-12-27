// ;
using Sandbox.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRage.Game;
using System.Text;
using ExShared;


namespace PocketShield
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public partial class Session_PocketShieldServer : MySessionComponentBase
    {
        public bool IsServer { get; private set; }
        public bool IsDedicated { get; private set; }
        public bool IsSetupDone { get; private set; }
        
        private int m_Ticks = 0;

        private List<IMyPlayer> m_Players = null;

        private Dictionary<long, ShieldEmitter> m_PlayerShieldEmitters = null;
        private Dictionary<long, ShieldEmitter> m_NpcShieldEmitters = null;


        public override void LoadData()
        {
            CustomLogger.Prefix = "shield";
            
            ConfigManager.ForceInitServer();
            ServerLogger.Instance.LogLevel = ConfigManager.ServerConfig.LogLevel;
            CustomLogger.SuppressAll(ConfigManager.ServerConfig.SuppressAllShieldLog);
            
            MyAPIGateway.Entities.OnEntityAdd += Entities_OnEntityAdd;
            MyAPIGateway.Entities.OnEntityRemove += Entities_OnEntityRemove;

            ShieldFactory_TryCreateEmitter += CreateEmitterBasic;
            ShieldFactory_TryCreateEmitter += CreateEmitterAdv;
            
            
            m_Players = new List<IMyPlayer>();

            m_PlayerShieldEmitters = new Dictionary<long, ShieldEmitter>();
            m_NpcShieldEmitters = new Dictionary<long, ShieldEmitter>();

            m_Plugins = new List<MyStringHash>();
            m_UnknownItems = new List<MyStringHash>();

            SaveDataManager.LoadData();
            
        }

        protected override void UnloadData()
        {
            Shutdown();
            MyAPIGateway.Entities.OnEntityAdd -= Entities_OnEntityAdd;
            MyAPIGateway.Entities.OnEntityRemove -= Entities_OnEntityRemove;

            ShieldFactory_TryCreateEmitter -= CreateEmitterBasic;
            ShieldFactory_TryCreateEmitter += CreateEmitterAdv;

            SaveDataManager.UnloadData();

            m_Players.Clear();
            m_PlayerShieldEmitters.Clear();
            m_NpcShieldEmitters.Clear();

            CustomLogger.DeInit();
            ServerLogger.DeInit();
        }

        public override void UpdateBeforeSimulation()
        {
            ++m_Ticks;
            // clear ticks count;
            if (m_Ticks >= 2000000000)
                m_Ticks -= 2000000000;

            ServerConfig config = ConfigManager.ServerConfig;
            if (m_Ticks % config.ServerUpdateInterval == 0)
            {
                if (MyAPIGateway.Session == null)
                    return;
                
                if (!IsSetupDone)
                {
                    Setup();
                    return;
                }

                UpdatePlayerList();
                SyncShieldDataToPlayers();

                UpdateSaveData();
            }

            if (m_Ticks % config.ShieldUpdateInterval == 0)
            {
                UpdateShieldEmitters(config.ShieldUpdateInterval);
            }

            // end of method;
            return;
        }

        public override void SaveData()
        {
            SaveDataManager.SaveData();
        }
        
        private void Entities_OnEntityAdd(VRage.ModAPI.IMyEntity _entity)
        {
            ServerLogger.Log("New Entity added..", 5);
            IMyCharacter character = _entity as IMyCharacter;
            if (character == null)
                return;

            ServerLogger.Log("  Entity is Character [" + character.DisplayName + "]", 5);
            ServerLogger.Log("  Character [" + character.DisplayName + "]: Hooking character.CharacterDied..", 5);
            character.CharacterDied += Character_CharacterDied;

            MyInventory inventory = character.GetInventory() as MyInventory;
            if (inventory == null)
            {
                //ServerLogger.Log("  Character [" + character.DisplayName + "] doesn't have inventory", 5);
                return;
            }

            ServerLogger.Log("  Character [" + character.DisplayName + "]: Hooking inventory.InventoryContentChanged..", 5);
            inventory.InventoryContentChanged += Inventory_InventoryContentChanged;
            //RefreshInventory(inventory);
        }

        private void Entities_OnEntityRemove(VRage.ModAPI.IMyEntity _entity)
        {
            ServerLogger.Log("Removing Entity..", 5);
            IMyCharacter character = _entity as IMyCharacter;
            if (character == null)
                return;

            ServerLogger.Log("  Entity is Character [" + character.DisplayName + "]", 5);
            ServerLogger.Log("  Character [" + character.DisplayName + "]: UnHooking character.CharacterDied..", 5);
            character.CharacterDied -= Character_CharacterDied;

            MyInventory inventory = character.GetInventory() as MyInventory;
            if (inventory == null)
                return;

            ServerLogger.Log("  Character [" + character.DisplayName + "]: UnHooking inventory.InventoryContentChanged..", 5);
            inventory.InventoryContentChanged -= Inventory_InventoryContentChanged;
        }

        private void Character_CharacterDied(IMyCharacter _character)
        {
            long playerId = _character.ControllerInfo.ControllingIdentityId;
            if (playerId == 0)
            {
                if (m_NpcShieldEmitters.Remove(_character.EntityId))
                {
                    ServerLogger.Log("Character [" + _character.DisplayName + "] died and their ShieldEmitter has been removed", 2);
                }
            }
            else
            {
                if (m_PlayerShieldEmitters.Remove(playerId))
                {
                    ServerLogger.Log("Character [" + _character.DisplayName + "] (Player <" + playerId + ">) died and their ShieldEmitter has been removed", 2);
                }
            }
        }
        
        public void Setup()
        {
            ServerLogger.Log("Setting up..");

            IsServer = (MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE || MyAPIGateway.Multiplayer.IsServer);
            IsDedicated = IsServer && MyAPIGateway.Utilities.IsDedicated;
            if (!IsServer)
                return;

            MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(50, BeforeDamageHandler);
            //MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(Constants.MSG_HANDLER_ID_INITIAL_SYNC, HandleInitialSyncRequest);

            UpdatePlayerCharacterInventoryOnceBeforeSim();
            UpdateShieldEmittersOnceBeforeSim();

            ServerLogger.Log("  IsServer = " + IsServer);
            ServerLogger.Log("  IsDedicated = " + IsDedicated);

            ServerLogger.Log("Setup Done");
            IsSetupDone = true;
        }

        private void Shutdown()
        {
            try
            {
                //MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(Constants.MSG_HANDLER_ID_INITIAL_SYNC, HandleInitialSyncRequest);
            }
            catch (Exception _e)
            { }
        }

        public void BeforeDamageHandler(object _target, ref MyDamageInformation _damageInfo)
        {
            
            IMyCharacter character = _target as IMyCharacter;
            if (character == null)
                return;
            
            if (_damageInfo.IsDeformation)
                return;
            
            ServerLogger.Log("Damage captured: " + _damageInfo.Amount + " " + _damageInfo.Type.String + " damage", 4);
            ShieldEmitter emitter = GetShieldEmitter(character);
            if (emitter == null)
                return;

            ServerLogger.Log("  Trying passing damage through Shield Emitter..", 4);
            emitter.TakeDamage(ref _damageInfo);
        }
        
        private void UpdatePlayerList()
        {
            m_Players.Clear();
            MyAPIGateway.Players.GetPlayers(m_Players);
        }

        private void UpdatePlayerCharacterInventoryOnceBeforeSim()
        {
            ServerLogger.Log(">> UpdatePlayerCharacterInventoryOnceBeforeSim()..", 4);
            UpdatePlayerList();
            foreach (IMyPlayer player in m_Players)
            {
                ServerLogger.Log(">>   Updating player " + player.SteamUserId, 4);
                if (player.Character == null)
                    continue;
                if (!player.Character.HasInventory)
                    continue;
                var inventory = player.Character.GetInventory() as MyInventory;
                if (inventory == null)
                    continue;

                RefreshInventory(inventory);
                ServerLogger.Log(">>     Player Character Inventory updated", 4);
            }
        }

        private void SyncShieldDataToPlayers()
        {
            foreach (IMyPlayer player in m_Players)
            {
                if (!m_PlayerShieldEmitters.ContainsKey((long)player.SteamUserId))
                    continue;
                
                if (!m_PlayerShieldEmitters[(long)player.SteamUserId].RequireSync)
                    continue;

                SendSyncDataToPlayer(player);
            }
        }

        private void UpdateSaveData()
        {
            foreach (long key in m_PlayerShieldEmitters.Keys)
            {
                SaveDataManager.UpdatePlayerData(key, m_PlayerShieldEmitters[key].Energy);
            }

            foreach (long key in m_NpcShieldEmitters.Keys)
            {
                SaveDataManager.UpdateNpcData(key, m_NpcShieldEmitters[key].Energy);
            }
        }
        
        private void SendSyncDataToPlayer(IMyPlayer _player)
        {
            ShieldEmitter emitter = m_PlayerShieldEmitters[(long)_player.SteamUserId];

            // TODO: let ShieldEmiter generate this struct;
            ShieldSyncData data = new ShieldSyncData()
            {
                PlayerSteamUserId = _player.SteamUserId,

                Energy = emitter.Energy,

                PluginsCount = emitter.PluginsCount,
                MaxEnergy = emitter.MaxEnergy,

                Def = emitter.DefList,
                Res = emitter.ResList,

                SubtypeId = emitter.SubtypeId,

                OverchargeRemainingPercent = emitter.OverchargeRemainingPercent
            };
                // TODO: more sync data;

            string syncData = MyAPIGateway.Utilities.SerializeToXML(data);
            ServerLogger.Log("Sending sync data to player " + _player.SteamUserId, 5);
            MyAPIGateway.Multiplayer.SendMessageTo(Constants.MSG_HANDLER_ID_SYNC, Encoding.Unicode.GetBytes(syncData), _player.SteamUserId);
        }

        private IMyPlayer GetPlayer(IMyCharacter _character)
        {
            if (_character == null)
                return null;

            if (m_Players.Count == 0)
                UpdatePlayerList();
            if (m_Players.Count == 0) // if there is still no player;
                return null;

            foreach (IMyPlayer player in m_Players)
            {
                if (player.Character == null)
                    continue;
                if (player.Character.EntityId == _character.EntityId)
                    return player;
            }

            return null;
        }
        
        private ulong GetPlayerSteamUid(IMyCharacter _character)
        {
            IMyPlayer player = GetPlayer(_character);
            if (player == null)
                return 0U;

            return player.SteamUserId;
        }
    }
}
