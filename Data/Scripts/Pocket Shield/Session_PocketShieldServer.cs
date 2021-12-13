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
    public class Session_PocketShieldServer : MySessionComponentBase
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
            Logger.Init(LoggerSide.Server);
            Logger.InitCustom("debug_shield.log");
            ConfigManager.ForceInit();
            Logger.SetLogLevel(ConfigManager.ServerConfig.LogLevel);
            
            MyAPIGateway.Entities.OnEntityAdd += Entities_OnEntityAdd;
            MyAPIGateway.Entities.OnEntityRemove += Entities_OnEntityRemove;

            m_Players = new List<IMyPlayer>();
            m_PlayerShieldEmitters = new Dictionary<long, ShieldEmitter>();
            m_NpcShieldEmitters = new Dictionary<long, ShieldEmitter>();

            SaveDataManager.LoadData();


        }

        protected override void UnloadData()
        {
            Shutdown();
            MyAPIGateway.Entities.OnEntityAdd -= Entities_OnEntityAdd;
            MyAPIGateway.Entities.OnEntityRemove -= Entities_OnEntityRemove;

            SaveDataManager.UnloadData();

            m_Players.Clear();
            m_PlayerShieldEmitters.Clear();
            m_NpcShieldEmitters.Clear();

            Logger.DeInit();
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
            Logger.Log("New Entity added", 5);
            IMyCharacter character = _entity as IMyCharacter;
            if (character == null)
                return;

            Logger.Log("  Entity is Character [" + character.DisplayName + "]", 4);
            character.CharacterDied += Character_CharacterDied;

            MyInventory inventory = character.GetInventory() as MyInventory;
            if (inventory == null)
            {
                Logger.Log("  Character [" + character.DisplayName + "] doesn't have inventory", 5);
                return;
            }

            Logger.Log("  Character [" + character.DisplayName + "]: Hooking inventory.InventoryContentChanged...", 5);
            inventory.InventoryContentChanged += Inventory_InventoryContentChanged;
            //RefreshInventory(inventory);
        }

        private void Entities_OnEntityRemove(VRage.ModAPI.IMyEntity _entity)
        {
            IMyCharacter character = _entity as IMyCharacter;
            if (character == null)
                return;
            
            character.CharacterDied -= Character_CharacterDied;

            MyInventory inventory = character.GetInventory() as MyInventory;
            if (inventory == null)
                return;
            
            inventory.InventoryContentChanged -= Inventory_InventoryContentChanged;
        }

        private void Character_CharacterDied(IMyCharacter _character)
        {
            long playerId = _character.ControllerInfo.ControllingIdentityId;
            if (playerId == 0)
            {
                // case: character is NPC;
                if (m_NpcShieldEmitters.Remove(_character.EntityId))
                {
                    Logger.Log("Character [" + _character.DisplayName + "] died and their ShieldEmitter has been removed", 2);
                }
            }
            else
            {
                // case: character is Player;
                if (m_PlayerShieldEmitters.Remove(playerId))
                {
                    Logger.Log("Character [" + _character.DisplayName + "] (Player <" + playerId + ">) died and their ShieldEmitter has been removed", 2);
                }
            }
        }

        private void Inventory_InventoryContentChanged(MyInventoryBase _inventory, MyPhysicalInventoryItem _arg2, VRage.MyFixedPoint _arg3)
        {
            long characterEntityId = _inventory.Container.Entity.EntityId;
            string characterDisplayName = _inventory.Container.Entity.DisplayName;
            Logger.Log("Inventory of character [" + characterDisplayName + "]'s content has changed", 5);

            RefreshInventory(_inventory);
        }
        
        public void Setup()
        {
            IsServer = (MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE || MyAPIGateway.Multiplayer.IsServer);
            IsDedicated = IsServer && MyAPIGateway.Utilities.IsDedicated;
            if (!IsServer)
                return;

            MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(50, BeforeDamageHandler);
            //MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(Constants.MSG_HANDLER_ID_INITIAL_SYNC, HandleInitialSyncRequest);

            UpdateShieldEmittersOnceBeforeSim();

            Logger.Log("  IsServer = " + IsServer);
            Logger.Log("  IsDedicated = " + IsDedicated);

            Logger.Log("  Setup Done.");
            IsSetupDone = true;
        }

        private void Shutdown()
        {
            try
            {
                MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(50, BeforeDamageHandler);
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
            
            ShieldEmitter emitter = GetShieldEmitter(character);
            if (emitter == null)
                return;

            emitter.TakeDamage(ref _damageInfo);
        }

        //public void HandleInitialSyncRequest(ushort _handlerId, byte[] _package, ulong _senderPlayerId, bool _sentMsg)
        //{
        //    Logger.Log("Starting HandleInitialSyncRequest()", 5);

        //    try
        //    {
        //        string decodedPackage = Encoding.Unicode.GetString(_package);
        //        Logger.Log("  _handlerId = " + _handlerId + ", _package = " + decodedPackage + ", _playerId = " + _senderPlayerId + ", _sentMsg = " + _sentMsg, 5);
        //        Logger.Log("  Recieved message from client <" + _senderPlayerId + ">: " + decodedPackage, 1);
        //        if (decodedPackage != Constants.MSG_REQ_INITIAL_VAL)
        //        {
        //            Logger.Log("    Unknown message", 1);
        //            return;
        //        }
        //    }
        //    catch (Exception _e)
        //    {
        //        Logger.Log("  Package string contains invalid characters", 0);
        //        return;
        //    }

        //    // TODO: try return shield data to player;
        //    float value = SaveDataManager.GetPlayerData((long)_senderPlayerId);




        //    Logger.Log("    Data sent back to player <" + _senderPlayerId + ">", 1);
        //}

        private void UpdatePlayerList()
        {
            MyAPIGateway.Players.GetPlayers(m_Players);
        }

        private void UpdateShieldEmittersOnceBeforeSim()
        {
            foreach (long key in m_PlayerShieldEmitters.Keys)
            {
                float value = SaveDataManager.GetPlayerData(key);
                m_PlayerShieldEmitters[key].Energy = value;
            }

            foreach (long key in m_NpcShieldEmitters.Keys)
            {
                float value = SaveDataManager.GetNpcData(key);
                m_NpcShieldEmitters[key].Energy = value;
            }
        }

        private void UpdateShieldEmitters(int _ticks)
        {
            foreach (long key in m_PlayerShieldEmitters.Keys)
            {
                m_PlayerShieldEmitters[key].Update(_ticks);
            }

            foreach (long key in m_NpcShieldEmitters.Keys)
            {
                m_NpcShieldEmitters[key].Update(_ticks);
            }
        }

        private void SyncShieldDataToPlayers()
        {
            foreach (IMyPlayer player in m_Players)
            {
                if (!m_PlayerShieldEmitters.ContainsKey(player.IdentityId))
                    continue;
                
                if (!m_PlayerShieldEmitters[player.IdentityId].RequireSync)
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
        
        private void RefreshInventory(MyInventoryBase _inventory)
        {
            Logger.Log("Starting RefreshInventory()", 4);

            IMyCharacter character = _inventory.Container.Entity as IMyCharacter;
            if (character == null)
            {
                Logger.Log("  This inventory's container is not a Character (this should not happen)", 1);
                return;
            }

            long playerId = character.ControllerInfo.ControllingIdentityId;

            List<MyPhysicalInventoryItem> InventoryItems = _inventory.GetItems();
            Logger.Log("  [" + character.DisplayName + "]'s inventory now contains " + InventoryItems.Count + " items.", 4);

            ShieldEmitter oldEmitter = GetShieldEmitter(character);

            bool foundEmitter = false;
            int foundPlugin = 0;
            foreach (MyPhysicalInventoryItem item in InventoryItems)
            {
                MyStringHash subtypeId = item.Content.SubtypeId;
                
                if (!foundEmitter)
                {
                    if (subtypeId == MyStringHash.GetOrCompute("PocketShield_BasicEmitter"))
                    {
                        if (!(oldEmitter is BasicShieldEmitter))
                        {
                            ShieldEmitter emitter = new BasicShieldEmitter();
                            if (playerId == 0)
                                m_NpcShieldEmitters[character.EntityId] = emitter;
                            else
                                m_PlayerShieldEmitters[playerId] = emitter;

                            Logger.Log("  Emitter changed: " + oldEmitter == null ? "null" : oldEmitter.GetType() + " -> BasicShieldEmitter", 3);
                        }
                        foundEmitter = true;
                        continue;
                    }
                    if (subtypeId == MyStringHash.GetOrCompute("PocketShield_AdvancedEmitter"))
                    {
                        if (!(oldEmitter is AdvancedShieldEmitter))
                        {
                            ShieldEmitter emitter = new AdvancedShieldEmitter();
                            if (playerId == 0)
                                m_NpcShieldEmitters[character.EntityId] = emitter;
                            else
                                m_PlayerShieldEmitters[playerId] = emitter;

                            Logger.Log("  Emitter changed: " + oldEmitter == null ? "null" : oldEmitter.GetType() + " -> AdvancedShieldEmitter", 3);
                        }
                        foundEmitter = true;
                        continue;
                    }
                }

                // TODO: process Plugin items;
                int[] pluginsCount = new int[4] { 0, 0, 0, 0 };
                if (foundPlugin < Constants.MAX_PLUGINS)
                {
                    if (subtypeId == MyStringHash.GetOrCompute("PocketShield_PluginCapacity"))
                    {
                        ++pluginsCount[0];
                        ++foundPlugin;
                        continue;
                    }









                }
            }

            Logger.Log("  Found " + foundPlugin + " plugins", 3);

            // TODO: plug plugins;

            
        }

        private void SendSyncDataToPlayer(IMyPlayer _player)
        {
            ShieldEmitter emitter = m_PlayerShieldEmitters[_player.IdentityId];
            ShieldSyncData data = new ShieldSyncData()
            {
                PlayerId = _player.IdentityId,

                Energy = emitter.Energy,
                MaxEnergy = emitter.MaxEnergy,

                // TODO: more sync data;
            };

            string syncData = MyAPIGateway.Utilities.SerializeToXML(data);
            MyAPIGateway.Multiplayer.SendMessageTo(Constants.MSG_HANDLER_ID_SYNC, Encoding.Unicode.GetBytes(syncData), _player.SteamUserId);
        }

        private ShieldEmitter GetShieldEmitter(IMyCharacter _character)
        {
            long playerId = _character.ControllerInfo.ControllingIdentityId;
            if (playerId == 0)
            {
                if (m_NpcShieldEmitters.ContainsKey(_character.EntityId))
                    return m_NpcShieldEmitters[_character.EntityId];
            }
            else
            {
                if (m_PlayerShieldEmitters.ContainsKey(playerId))
                    return m_PlayerShieldEmitters[playerId];
            }

            return null;
        }
        
    }
}
