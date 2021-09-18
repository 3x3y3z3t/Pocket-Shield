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

using PocketShield.Core;
using System.Text;

namespace PocketShield
{
    // This object is always present, from the world load to world unload.
    // NOTE: all clients and server run mod scripts, keep that in mind.
    // NOTE: this and gamelogic comp's update methods run on the main game thread, don't do too much in a tick or you'll lower sim speed.
    // NOTE: also mind allocations, avoid realtime allocations, re-use collections/ref-objects (except value types like structs, integers, etc).
    //
    // The MyUpdateOrder arg determines what update overrides are actually called.
    // Also remove all comments you've read to avoid the overload of comments that is this file.
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]

    public class Session_PocketShieldServer : MySessionComponentBase
    {
        //private const ushort MSG_HANDLER_ID = 1351;
        //private const ushort MSG_HANDLER_ID_2 = 1353;
        //private const int WAIT_TICKS = 300;
        //private const string MSG_REQ_INITIAL_VAL = "REQ_SHIELD_INITIAL_VAL";

        int Tick = Constants.WAIT_TICKS_SERVER;

        private List<IMyPlayer> m_Players;
        private Dictionary<long, ShieldEmitter> m_Shields;

        PlayerShieldDataManager m_PlayerShieldDataManager;
        private Logger m_Logger = null;

        public override void LoadData()
        {
            // amongst the earliest execution points, but not everything is available at this point.

            // These can be used anywhere, not just in this method/class:
            // MyAPIGateway. - main entry point for the API
            // MyDefinitionManager.Static. - reading/editing definitions
            // MyGamePruningStructure. - fast way of finding entities in an area
            // MyTransparentGeometry. and MySimpleObjectDraw. - to draw sprites (from TransparentMaterials.sbc) in world (they usually live a single tick)
            // MyVisualScriptLogicProvider. - mainly designed for VST but has its uses, use as a last resort.
            // System.Diagnostics.Stopwatch - for measuring code execution time.
            // ...and many more things, ask in #programming-modding in keen's discord for what you want to do to be pointed at the available things to use.

            //Instance = this;
            //MyAPIGateway.Utilities.WriteFileInLocalStorage("debug.log", GetType());

            m_Logger = new Logger(LoggerSide.SERVER);
            m_Logger.Init();
            ShieldLogger.Init();
            //m_Logger.Log("Starting LoadData()");

            m_Players = new List<IMyPlayer>();
            m_Shields = new Dictionary<long, ShieldEmitter>();
            m_PlayerShieldDataManager = new PlayerShieldDataManager();

            MyAPIGateway.Entities.OnEntityAdd += Entities_OnEntityAdd;




        }

        //public override void BeforeStart()
        //{
        //    // executed before the world starts updating
        //}

        protected override void UnloadData()
        {
            // executed when world is exited to unregister events and stuff

            //Instance = null; // important for avoiding this object to remain allocated in memory

            //m_Logger.Log("Starting UnloadData()");
            ShieldLogger.DeInit();

            Shutdown();

            MyAPIGateway.Entities.OnEntityAdd -= Entities_OnEntityAdd;

            m_Logger.DeInit();
        }

        //public override void HandleInput()
        //{
        //    // gets called 60 times a second before all other update methods, regardless of framerate, game pause or MyUpdateOrder.
        //}

        public override void UpdateBeforeSimulation()
        {
            // executed every tick, 60 times a second, before physics simulation and only if game is not paused.
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
                else
                {
                    return;
                }

            }
            catch (Exception _e)
            { }

            if (Tick > Constants.WAIT_TICKS_SERVER)
            {
                Tick = 0;

                if (!IsSetupDone)
                {
                    Setup();
                }

                // shield will be sync to EVERY players each WAIT_TICKS_SERVER ticks;
                foreach (IMyPlayer player in m_Players)
                {
                    if (m_Shields.ContainsKey(player.Character.EntityId))
                    {
                        SendSyncDataToPlayer(player);
                    }
                    else
                    {
                        m_Logger.Log("    Player <" + player.Character.EntityId + "> doesn't have shield.");
                    }
                    //break;
                    return;
                }

            }

            if (Tick % 2 == 0)
            {
                // shield will be update 30 times per second;
                foreach (ShieldEmitter shield in m_Shields.Values)
                {
                    shield.Update(2);
                }
            }

            if (Tick % 30 == 0)
            {
                m_Players.Clear();
                MyAPIGateway.Multiplayer.Players.GetPlayers(m_Players);
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

        public override void SaveData()
        {
            // executed AFTER world was saved
            m_PlayerShieldDataManager.SaveData();
        }

        //public override MyObjectBuilder_SessionComponent GetObjectBuilder()
        //{
        //    // executed during world save, most likely before entities.

        //    return base.GetObjectBuilder(); // leave as-is.
        //}

        //public override void UpdatingStopped()
        //{
        //    // executed when game is paused
        //}



        private void Entities_OnEntityAdd(VRage.ModAPI.IMyEntity _entity)
        {
            IMyCharacter character = _entity as IMyCharacter;

            if ((character == null) || string.IsNullOrEmpty(character.DisplayName))
            {
                return;
            }

            m_Logger.Log("Found character '" + character.DisplayName + "'.");
            character.CharacterDied += Character_CharacterDied;

            MyInventory inventory = character.GetInventory() as MyInventory;
            if (inventory == null)
            {
                m_Logger.Log("Character '" + character.DisplayName + "' doesn't have inventory.");
                return;
            }

            m_Logger.Log("  Character '" + character.DisplayName + "' has inventory.");
            m_Logger.Log("  Hooking inventory.InventoryContentChanged...");
            inventory.InventoryContentChanged += Inventory_InventoryContentChanged;
            //RefreshInventory(inventory);
        }

        private void Character_CharacterDied(IMyCharacter _character)
        {
            if (m_Shields.Remove(_character.EntityId))
            {
                foreach (IMyPlayer player in m_Players)
                {
                    if (player.Character != null && player.Character == _character)
                    {
                        PlayerShieldDataSync data = m_PlayerShieldDataManager.GetData(player);
                        data.Health = 0.0f;

                        break;
                    }
                }
            }
        }

        private void Inventory_InventoryContentChanged(MyInventoryBase _inventory, MyPhysicalInventoryItem _arg2, VRage.MyFixedPoint _arg3)
        {
            long characterEntityId = _inventory.Container.Entity.EntityId;
            string characterDisplayName = _inventory.Container.Entity.DisplayName;
            //m_Logger.Log("Inventory of character '" + characterDisplayName + "''s content has changed.");

            RefreshInventory(_inventory);

            foreach (IMyPlayer player in m_Players)
            {
                if (player.Character.EntityId == characterEntityId)
                {
                    SendSyncDataToPlayer(player);

                    //PlayerShieldData data = m_PlayerShieldDataManager.GetData(player);
                    //string syncData = MyAPIGateway.Utilities.SerializeToXML(data);
                    //m_Logger.Log("    Sending PlayerShieldData to player <" + player.SteamUserId + ">.");
                    //MyAPIGateway.Multiplayer.SendMessageTo(Constants.MSG_HANDLER_ID, Encoding.Unicode.GetBytes(syncData), player.SteamUserId);
                }
                break;
            }

        }

        public void DamageHandler(object _target, ref MyDamageInformation _damageInfo)
        {
            IMyCharacter character = _target as IMyCharacter;
            if (character == null)
            {
                return;
            }

            if (_damageInfo.IsDeformation)
            {
                return;
            }

            if (!m_Shields.ContainsKey(character.EntityId))
            {
                return;
            }

            ShieldEmitter emitter = m_Shields[character.EntityId];
            emitter.TakeDamage(ref _damageInfo);

            foreach (IMyPlayer player in m_Players)
            {
                if (player.Character.EntityId == character.EntityId)
                {
                    SendSyncDataToPlayer(player);
                }
                break;
            }
        }

        //
        // Summary:
        //     Allows you do reliable checks WHO have sent message to you.
        //
        // Parameters:
        //   id:
        //     Uniq handler id
        //
        //   messageHandler:
        //     Call function
        //     ushort:
        //       HandlerId
        //     byte[][]:
        //       Package
        //     ulong:
        //       Player SteamID or 0 for Dedicated server
        //     bool:
        //       Sent message comes from server
        //void RegisterSecureMessageHandler(ushort id, Action<ushort, byte[], ulong, bool> messageHandler);
        public void HandleInitialSyncRequest(ushort _handlerId, byte[] _package, ulong _playerId, bool _sentMsg)
        {
            m_Logger.Log("Starting HandleInitialSyncRequest()");

            string decodedPackage = Encoding.Unicode.GetString(_package);
            m_Logger.Log("  _handlerId = " + _handlerId + ", _package = " + decodedPackage + ", _playerId = " + _playerId + ", _sentMsg = " + _sentMsg);
            m_Logger.Log("  Recieved message from client <" + _playerId + ">: " + decodedPackage);

            if (decodedPackage == Constants.MSG_REQ_INITIAL_VAL)
            {
                m_Logger.Log("    Request acknowledged.");
                foreach (IMyPlayer player in m_Players)
                {
                    if (player.SteamUserId == _playerId)
                    {
                        if (m_Shields.ContainsKey(player.Character.EntityId))
                        {
                            SendSyncDataToPlayer(player);
                            //PlayerShieldData data = m_PlayerShieldDataManager.GetData(player);
                            //string syncData = MyAPIGateway.Utilities.SerializeToXML(data);
                            //m_Logger.Log("    Sending PlayerShieldData to player <" + player.SteamUserId + ">.");
                            //MyAPIGateway.Multiplayer.SendMessageTo(Constants.MSG_HANDLER_ID, Encoding.Unicode.GetBytes(syncData), player.SteamUserId);
                        }
                        else
                        {
                            m_Logger.Log("    Player <" + _playerId + "> doesn't have shield.");
                        }
                        //break;
                        return;
                    }
                }

                m_Logger.Log("    Player <" + _playerId + "> not found.");

            }
            else
            {
                m_Logger.Log("    Unknown message.");
            }
        }

        private bool IsSetupDone = false;
        private bool IsServer = false;
        private bool IsDedicated = false;

        public void Setup()
        {
            m_Logger.Log("Starting Setup()");

            MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(50, DamageHandler);

            //m_Logger.Log("  Registering Message Handler (id " + Constants.MSG_HANDLER_ID_2 + ")...");
            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(Constants.MSG_HANDLER_ID_2, HandleInitialSyncRequest);

            m_PlayerShieldDataManager.LoadData();

            MyAPIGateway.Players.GetPlayers(m_Players);
            m_Logger.Log("  Player count: " + m_Players.Count);
            foreach (IMyPlayer player in m_Players)
            {
                m_Logger.Log("    Player <" + player.SteamUserId + "> '" + player.DisplayName + "'");
                if (player.Character == null || (player.Character.GetInventory() as MyInventory) == null)
                    continue;

                RefreshInventory(player.Character.GetInventory() as MyInventory);

                if (!m_Shields.ContainsKey(player.Character.EntityId))
                    continue;

                PlayerShieldDataSync data = m_PlayerShieldDataManager.GetData(player);
                ShieldEmitter emitter = m_Shields[player.Character.EntityId];
                emitter.Health = data.Health;
                emitter.Character = player.Character;

                if (IsServer && !IsDedicated)
                {
                    // single player, update directly;
                }
                else
                {
                    // send message to player;
                }

                SendSyncDataToPlayer(player, data);
            }

            m_Logger.Log("  IsServer = " + IsServer);
            m_Logger.Log("  IsDedicated = " + IsDedicated);

            m_Logger.Log("  Setup Done.");
            IsSetupDone = true;
        }

        private void Shutdown()
        {
            m_Logger.Log("  Starting Shutdown()");

            m_Players.Clear();
            m_Shields.Clear();
            m_PlayerShieldDataManager.Clear();

            //m_Logger.Log("    UnRegistering Message Handler (id " + Constants.MSG_HANDLER_ID_2 + ")...");
            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(Constants.MSG_HANDLER_ID_2, HandleInitialSyncRequest);
        }

        private void RefreshInventory(MyInventoryBase _inventory)
        {
            //m_Logger.Log("Starting RefreshInventory()");
            long characterEntityId = _inventory.Container.Entity.EntityId;
            string characterDisplayName = _inventory.Container.Entity.DisplayName;

            List<MyPhysicalInventoryItem> InventoryItems = _inventory.GetItems();
            //m_Logger.Log("  " + characterDisplayName + "'s inventory now contains " + InventoryItems.Count + " items.");

            uint pluginsCount = 0;
            uint capPluginsCount = 0;
            uint defPluginsCount = 0;
            uint kiPluginsCount = 0;
            uint exPluginsCount = 0;
            //ShieldItemType shieldFound = ShieldItemType.None;
            Type foundType = null;
            int pos = -1;
            foreach (MyPhysicalInventoryItem item in InventoryItems)
            {
                ++pos;
                MyStringHash subtypeId = item.Content.SubtypeId;
                //m_Logger.Log("  Found " + subtypeId + " at pos " + pos);

                if (foundType == null)
                {
                    if (subtypeId == MyStringHash.GetOrCompute("PocketShieldBasicEmitter"))
                    {
                        //m_Logger.Log("  Found basic emitter at pos " + pos);
                        foundType = typeof(BasicShieldEmitter);
                    }
                    else if (subtypeId == MyStringHash.GetOrCompute("PocketShieldAdvancedEmitter"))
                    {
                        //m_Logger.Log("  Found advanced emitter at pos " + pos);
                        foundType = typeof(AdvancedShieldEmitter);
                    }
                }
                else if (pluginsCount < 8)
                {
                    if (subtypeId == MyStringHash.GetOrCompute("PocketShieldCapacityPlugin"))
                    {
                        //m_Logger.Log("  Found Plugin 0");
                        ++capPluginsCount;
                        ++pluginsCount;
                    }
                    else if (subtypeId == MyStringHash.GetOrCompute("PocketShieldDefensePlugin"))
                    {
                        //m_Logger.Log("  Found Plugin 1");
                        ++defPluginsCount;
                        ++pluginsCount;
                    }
                    else if (subtypeId == MyStringHash.GetOrCompute("PocketShieldBulletResistancePlugin"))
                    {
                        //m_Logger.Log("  Found Plugin 2");
                        ++kiPluginsCount;
                        ++pluginsCount;
                    }
                    else if (subtypeId == MyStringHash.GetOrCompute("PocketShieldExplosiveResistancePlugin"))
                    {
                        //m_Logger.Log("  Found Plugin 3");
                        ++exPluginsCount;
                        ++pluginsCount;
                    }
                }
                else
                {
                    break;
                }
            }

            //m_Logger.Log("  Shield Found: " + foundType);
            //m_Logger.Log("  Plugins Found: " + capPluginsCount + " " + defPluginsCount + " " + kiPluginsCount+ " " + exPluginsCount);

            if (foundType == null)
            {
                //m_Logger.Log("  Shield item lost.");
                if (m_Shields.ContainsKey(characterEntityId))
                {
                    //m_Logger.Log("    Removed existing shield.");
                    m_Shields.Remove(characterEntityId);
                }
            }
            else
            {
                IMyCharacter character = _inventory.Container.Entity as IMyCharacter;
                if (m_Shields.ContainsKey(characterEntityId))
                {
                    ShieldEmitter emitter = m_Shields[characterEntityId];
                    //m_Logger.Log("  Found existing shield of type " + emitter.GetType());
                    if (emitter.GetType() != foundType)
                    {
                        //m_Logger.Log("    Found existing conflicting shield.");

                        emitter = ConstructEmitter(GetPlayerOfThisCharacter(character), character, foundType);
                    }
                    else
                    {
                        //m_Logger.Log("    Found same shield type.");
                    }
                    emitter.UpdatePlugins(capPluginsCount, defPluginsCount, kiPluginsCount, exPluginsCount);
                    m_Shields[characterEntityId] = emitter;
                }
                else
                {
                    //m_Logger.Log("  Creating new shield...");
                    ShieldEmitter emitter = ConstructEmitter(GetPlayerOfThisCharacter(character), character, foundType);
                    emitter.UpdatePlugins(capPluginsCount, defPluginsCount, kiPluginsCount, exPluginsCount);
                    m_Shields.Add(characterEntityId, emitter);
                }

            }

            //m_Logger.Log("  Shields list now has " + m_Shields.Count + " shields.");
        }

        private void SendSyncDataToPlayer(IMyPlayer _player, PlayerShieldDataSync _data = null)
        {
            if (_data == null)
            {
                _data = m_PlayerShieldDataManager.GetData(_player);
            }

            if (m_Shields.ContainsKey(_player.Character.EntityId))
            {
                //m_Logger.Log("  Have Shield");
                ShieldEmitter emitter = m_Shields[_player.Character.EntityId];
                _data.MaxHealth = emitter.MaxHealth;
                _data.Health = emitter.Health;
                _data.HealthBonus = emitter.HealthBonus;
                _data.ChargeRate = emitter.ChargeRate;
                _data.Defense = emitter.Defense;
                _data.BulletDamageResistance = emitter.BulletDamageResistance;
                _data.ExplosiveDamageResistance = emitter.ExplosiveDamageResistance;
            }
            else
            {
                //m_Logger.Log("  No have Shield");
                _data.MaxHealth = 0.0f;
                _data.Health = 0.0f;
                _data.HealthBonus = 0.0f;
                _data.ChargeRate = 0.0f;
                _data.Defense = 0.0f;
                _data.BulletDamageResistance = 0.0f;
                _data.ExplosiveDamageResistance = 0.0f;
            }
            m_PlayerShieldDataManager.ModifyData(_player, _data);

            string syncData = MyAPIGateway.Utilities.SerializeToXML(_data);
            //m_Logger.Log("    Sending PlayerShieldData to player <" + _player.SteamUserId + ">.");
            MyAPIGateway.Multiplayer.SendMessageTo(Constants.MSG_HANDLER_ID, Encoding.Unicode.GetBytes(syncData), _player.SteamUserId);
        }

        private IMyPlayer GetPlayerOfThisCharacter(IMyCharacter _character)
        {
            if (m_Players == null)
                return null;

            foreach (IMyPlayer player in m_Players)
            {
                if (player.Character != null && player.Character == _character)
                {
                    return player;
                }
            }

            return null;
        }

        private ShieldEmitter ConstructEmitter(IMyPlayer _player, IMyCharacter _character, Type _type)
        {
            if (_type == typeof(BasicShieldEmitter))
            {
                //m_Logger.Log("    Created Basic Shield.");
                return new BasicShieldEmitter()
                {
                    Character = _character,
                    Player = _player,
                };
            }
            if (_type == typeof(AdvancedShieldEmitter))
            {
                //m_Logger.Log("    Created Advanced Shield.");
                return new AdvancedShieldEmitter()
                {
                    Character = _character,
                    Player = _player,
                };
            }
            return null;
        }
    }
}
