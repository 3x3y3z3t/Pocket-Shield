// ;
using ExShared;
using Sandbox.Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;

namespace PocketShield
{
    public partial class Session_PocketShieldServer
    {
        private List<MyStringHash> m_Plugins = null;
        private List<MyStringHash> m_UnknownItems = null;

        private void Inventory_ContentsChanged(MyInventoryBase _inventory)
        {
            IMyCharacter character = _inventory.Container.Entity as IMyCharacter;
            if (character == null)
                return;

            if (character.IsDead)
            {
                ServerLogger.Log("Character [" + Utils.GetCharacterName(character) + "] is dead");
                return;
            }

            ServerLogger.Log("Inventory of character [" + Utils.GetCharacterName(character) + "]'s content has changed", 5);

            RefreshInventory(_inventory);
        }

        private void RefreshInventory(MyInventoryBase _inventory)
        {
            ServerLogger.Log("Starting RefreshInventory()", 4);

            IMyCharacter character = _inventory.Container.Entity as IMyCharacter;
            if (character == null)
                return;
            
            List<MyPhysicalInventoryItem> InventoryItems = _inventory.GetItems();
            ServerLogger.Log("  [" + Utils.GetCharacterName(character) + "]'s inventory now contains " + InventoryItems.Count + " items.", 4);

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
                        ServerLogger.Log("    Old Emitter: " + oldEmitter.SubtypeId.String, 4);
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

                if (GetPlayerSteamUid(character) != 0U)
                    m_ForceSyncPlayers.Add(GetPlayerSteamUid(character));
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

        private void ManipulateDeadCharacterInventory(MyInventory _inventory)
        {
            NpcInventoryOperation flag = ConfigManager.ServerConfig.NpcInventoryOperationOnDeath;
            float ratio = ConfigManager.ServerConfig.NpcShieldItemToCreditRatio;
            float refundAmount = 0.0f;

            List<MyPhysicalInventoryItem> items = _inventory.GetItems();
            for (int i = items.Count - 1; i >= 0; --i)
            {
                if ((items[i].Content.SubtypeId == MyStringHash.GetOrCompute(Constants.SUBTYPEID_EMITTER_BAS) ||
                     items[i].Content.SubtypeId == MyStringHash.GetOrCompute(Constants.SUBTYPEID_EMITTER_ADV)) &&
                    (flag & NpcInventoryOperation.RemoveEmitterOnly) != 0)
                {
                    if (ratio > 0.0f)
                    {
                        refundAmount += m_CachedPrice[items[i].Content.SubtypeId.String] * ratio;
                        ServerLogger.Log("Item " + items[i].Content.SubtypeId.String + ": " + m_CachedPrice[items[i].Content.SubtypeId.String] + " -> " + refundAmount, 4);
                    }
                    _inventory.RemoveItemsAt(i, 1, false);
                }
                else if ((items[i].Content.SubtypeId == MyStringHash.GetOrCompute(Constants.SUBTYPEID_PLUGIN_CAP) ||
                          items[i].Content.SubtypeId == MyStringHash.GetOrCompute(Constants.SUBTYPEID_PLUGIN_DEF_KI) ||
                          items[i].Content.SubtypeId == MyStringHash.GetOrCompute(Constants.SUBTYPEID_PLUGIN_DEF_EX) ||
                          items[i].Content.SubtypeId == MyStringHash.GetOrCompute(Constants.SUBTYPEID_PLUGIN_RES_KI) ||
                          items[i].Content.SubtypeId == MyStringHash.GetOrCompute(Constants.SUBTYPEID_PLUGIN_RES_EX)) &&
                         (flag & NpcInventoryOperation.RemovePluginOnly) != 0)
                {
                    if (ratio > 0.0f)
                    {
                        refundAmount += m_CachedPrice[items[i].Content.SubtypeId.String] * ratio;
                        ServerLogger.Log("Item " + items[i].Content.SubtypeId.String + ": " + m_CachedPrice[items[i].Content.SubtypeId.String] + " -> " + refundAmount, 4);
                    }
                    _inventory.RemoveItemsAt(i, 1, false);
                }
            }

            if (refundAmount > 0.0f)
            {
                _inventory.AddItems((int)refundAmount, MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_PhysicalObject>("SpaceCredit"));
                ServerLogger.Log("Refunded: " + refundAmount, 4);
            }
            
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
