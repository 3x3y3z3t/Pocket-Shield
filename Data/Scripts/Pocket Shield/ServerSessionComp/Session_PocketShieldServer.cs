// ;
using Sandbox.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRage.Game;
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

        private Dictionary<long, ShieldEmitter> m_PlayerShieldEmitters = null; // TODO: maybe use ulong;
        private Dictionary<long, ShieldEmitter> m_NpcShieldEmitters = null;
        private Dictionary<long, OtherCharacterShieldData> m_ShieldDamageEffects = new Dictionary<long, OtherCharacterShieldData>();

        private List<ulong> m_ForceSyncPlayers = new List<ulong>();
        private List<int> m_DamageQueue = new List<int>();
        private List<long> m_IdToRemove = new List<long>();

        public override void LoadData()
        {
            CustomLogger.Prefix = "shield";
            
            ConfigManager.ForceInitServer();
            ServerLogger.Instance.LogLevel = ConfigManager.ServerConfig.LogLevel;
            CustomLogger.Suppressed = ConfigManager.ServerConfig.SuppressAllShieldLog;
            CustomLogger.LogLevel = ConfigManager.ServerConfig.LogLevel;

            UpdateItemsDescription();


            MyAPIGateway.Utilities.MessageEntered += Utilities_MessageEntered;

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
            MyAPIGateway.Utilities.MessageEntered -= Utilities_MessageEntered;

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

            ServerLogger.Log("  Entity is Character [" + Utils.LogCharacterName(character) + "]", 5);
            ServerLogger.Log("  Character [" + Utils.LogCharacterName(character) + "]: Hooking character.CharacterDied..", 5);
            character.CharacterDied += Character_CharacterDied;

            MyInventory inventory = character.GetInventory() as MyInventory;
            if (inventory == null)
            {
                //ServerLogger.Log("  Character [" + character.DisplayName + "] doesn't have inventory", 5);
                return;
            }

            ServerLogger.Log("  Character [" + Utils.LogCharacterName(character) + "]: Hooking inventory.InventoryContentChanged..", 5);
            inventory.InventoryContentChanged += Inventory_InventoryContentChanged;
            inventory.ContentsChanged += Inventory_ContentsChanged;
            //RefreshInventory(inventory);
        }

        private void Entities_OnEntityRemove(VRage.ModAPI.IMyEntity _entity)
        {
            ServerLogger.Log("Removing Entity..", 5);
            IMyCharacter character = _entity as IMyCharacter;
            if (character == null)
                return;

            ServerLogger.Log("  Entity is Character [" + Utils.LogCharacterName(character) + "]", 5);
            ServerLogger.Log("  Character [" + Utils.LogCharacterName(character) + "]: UnHooking character.CharacterDied..", 5);
            character.CharacterDied -= Character_CharacterDied;

            MyInventory inventory = character.GetInventory() as MyInventory;
            if (inventory == null)
                return;

            ServerLogger.Log("  Character [" + Utils.LogCharacterName(character) + "]: UnHooking inventory.InventoryContentChanged..", 5);
            inventory.InventoryContentChanged -= Inventory_InventoryContentChanged;
        }

        private void Character_CharacterDied(IMyCharacter _character)
        {
            ReplaceShieldEmitter(_character, null);
            
            IMyPlayer player = GetPlayer(_character);
            if (player == null || player.SteamUserId == 0)
            {
                ServerLogger.Log("Character [" + Utils.LogCharacterName(_character) + "] died and their ShieldEmitter has been removed", 2);
            }
            else if (player != null)
            {
                m_ForceSyncPlayers.Add(player.SteamUserId);
                ServerLogger.Log("Character [" + Utils.LogCharacterName(_character) + "] (Player <" + player.SteamUserId + ">) died and their ShieldEmitter has been removed", 2);
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

            }
            catch (Exception _e)
            { }
        }

        public void BeforeDamageHandler(object _target, ref MyDamageInformation _damageInfo)
        {
            if (m_DamageQueue.Contains(_damageInfo.GetHashCode()))
                return;

            IMyCharacter character = _target as IMyCharacter;
            if (character == null)
                return;
            
            if (_damageInfo.IsDeformation)
            {
                return;
            }
            
            ServerLogger.Log("Damage captured: " + _damageInfo.Amount + " " + _damageInfo.Type.String + " damage", 4);
            ShieldEmitter emitter = GetShieldEmitter(character);
            if (emitter == null)
                return;

            ServerLogger.Log("  Trying passing damage through Shield Emitter..", 4);
            float beforeDamageHealth = MyVisualScriptLogicProvider.GetPlayersHealth(character.ControllerInfo.ControllingIdentityId);
            if (emitter.TakeDamage(ref _damageInfo))
            {
                if (m_ShieldDamageEffects.ContainsKey(character.EntityId))
                {
                    m_ShieldDamageEffects[character.EntityId].Ticks = m_Ticks;
                    m_ShieldDamageEffects[character.EntityId].ShieldAmountPercent = emitter.ShieldEnergyPercent;
                }
                else
                {
                    m_ShieldDamageEffects[character.EntityId] = new OtherCharacterShieldData()
                    {
                        Entity = character,
                        EntityId = character.EntityId,
                        Ticks = m_Ticks,
                        ShieldAmountPercent = emitter.ShieldEnergyPercent
                    };
                }
            }

            m_DamageQueue.Add(_damageInfo.GetHashCode());
            bool shouldSync = _damageInfo.Amount > 0.0f;
            character.DoDamage(_damageInfo.Amount, _damageInfo.Type, shouldSync, attackerId: _damageInfo.AttackerId);
            _damageInfo.Amount = 0;
            m_DamageQueue.Remove(_damageInfo.GetHashCode());
        }

        private void UpdatePlayerList()
        {
            m_Players.Clear();
            MyAPIGateway.Players.GetPlayers(m_Players);
        }

        private void SyncShieldDataToPlayers()
        {
            foreach (IMyPlayer player in m_Players)
            {
                if (m_ForceSyncPlayers.Contains(player.SteamUserId))
                {
                    m_ForceSyncPlayers.Remove(player.SteamUserId);
                    SendSyncDataToPlayer(player);
                }
                else if (m_PlayerShieldEmitters.ContainsKey((long)player.SteamUserId) && m_PlayerShieldEmitters[(long)player.SteamUserId].RequireSync)
                {
                    SendSyncDataToPlayer(player);
                }
                else if (m_ShieldDamageEffects.Count > 0)
                {
                    SendSyncDataToPlayer(player);
                }
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
