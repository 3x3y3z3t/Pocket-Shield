// ;
using ExShared;
using Sandbox.Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace PocketShield
{
    public partial class Session_PocketShieldServer
    {
        private List<MyStringHash> m_Plugins = null;
        private List<MyStringHash> m_UnknownItems = null;
        
        private void Inventory_ContentsChanged(MyInventoryBase _inventory)
        {
            long characterEntityId = _inventory.Container.Entity.EntityId;
            string characterDisplayName = _inventory.Container.Entity.DisplayName;
            ServerLogger.Log("Inventory of character [" + characterDisplayName + "]'s content has changed", 5);

            RefreshInventory(_inventory);
        }
        
        private void Inventory_InventoryContentChanged(MyInventoryBase _inventory, MyPhysicalInventoryItem _arg2, VRage.MyFixedPoint _arg3)
        {
        }

        private void RefreshInventory(MyInventoryBase _inventory)
        {
            ServerLogger.Log("Starting RefreshInventory()", 4);

            IMyCharacter character = _inventory.Container.Entity as IMyCharacter;
            if (character == null)
                return;
            
            List<MyPhysicalInventoryItem> InventoryItems = _inventory.GetItems();
            ServerLogger.Log("  [" + character.DisplayName + "]'s inventory now contains " + InventoryItems.Count + " items.", 4);

            ShieldEmitter oldEmitter = GetShieldEmitter(character);

            foreach (MyPhysicalInventoryItem item in InventoryItems)
            {
                MyStringHash subtypeId = item.Content.SubtypeId;
                ServerLogger.Log("  Processing item " + subtypeId, 4);

                if (!subtypeId.String.Contains("PocketShield_"))
                    continue;

                if (subtypeId.String.Contains("Emitter"))
                {
                    // if another emitter item has been processed before, ignore this emitter item;
                    if (FirstEmitterFound != null)
                        continue;

                    if (oldEmitter == null)
                    {
                        ServerLogger.Log("    Old Emitter is null, try creating Emitter..", 4);
                        ShieldFactory_TryCreateEmitter(subtypeId, character);
                        if (FirstEmitterFound != null)
                        {
                            ReplaceShieldEmitter(character, FirstEmitterFound);
                            oldEmitter = FirstEmitterFound;
                            ServerLogger.Log("    Emitter Created: " + FirstEmitterFound.SubtypeId.String, 4);
                            ServerLogger.Log("    Old Emitter:     " + oldEmitter.SubtypeId.String, 4);
                            ServerLogger.Log("    Emitter count: " + m_PlayerShieldEmitters.Count + " Player's, " + m_NpcShieldEmitters.Count + " Npc's", 1);
                        }
                    }
                    else
                    {
                        ServerLogger.Log("    Old Emitter: " + oldEmitter.SubtypeId.String);
                        if (oldEmitter.SubtypeId != subtypeId)
                        {
                            ServerLogger.Log("    Try creating new Emitter..", 4);
                            ShieldFactory_TryCreateEmitter(subtypeId, character);
                            if (FirstEmitterFound != null)
                            {
                                ReplaceShieldEmitter(character, FirstEmitterFound);
                                oldEmitter = FirstEmitterFound;
                                ServerLogger.Log("    Emitter Created: " + FirstEmitterFound.SubtypeId.String, 4);
                                ServerLogger.Log("    Old Emitter:     " + oldEmitter.SubtypeId.String, 4);
                                ServerLogger.Log("    Emitter count: " + m_PlayerShieldEmitters.Count + " Player's, " + m_NpcShieldEmitters.Count + " Npc's", 1);
                            }
                        }
                        else
                        {
                            FirstEmitterFound = oldEmitter;
                        }
                    }
                    
                    continue;
                }

                if (subtypeId.String.Contains("Plugin"))
                {
                    ServerLogger.Log("    Adding Plugin..", 4);
                    m_Plugins.Add(subtypeId);
                    continue;
                }

                ServerLogger.Log("    Hmm, unknown PocketShield item..", 4);
                m_UnknownItems.Add(subtypeId);
            }

            ServerLogger.Log("  Found " + m_UnknownItems.Count + " unknown PocketShield items", 4);
            foreach (MyStringHash subtypeid in m_UnknownItems)
            {
                ServerLogger.Log("    " + subtypeid.String, 4);
            }

            if (oldEmitter != null && FirstEmitterFound == null)
            {
                ServerLogger.Log("  Emitter dropped: " + oldEmitter.SubtypeId);
                ReplaceShieldEmitter(character, null);

                IMyPlayer player = GetPlayer(character);
                if (player != null)
                    m_ForceSyncPlayers.Add(player.SteamUserId);
            }

            if (oldEmitter != null)
            {
                oldEmitter.CleanPluginsList();
                oldEmitter.AddPlugins(m_Plugins);
            }
            

            FirstEmitterFound = null;
            m_Plugins.Clear();
            m_UnknownItems.Clear();
            return;
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



    }
}
