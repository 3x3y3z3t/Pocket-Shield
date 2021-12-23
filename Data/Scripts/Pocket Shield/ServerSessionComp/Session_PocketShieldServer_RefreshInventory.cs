// ;
using ExShared;
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

                ServerLogger.Log("    Try creating Emitter..", 4);
                if (FirstEmitterFound == null)
                {
                    // if no emitter is found in inventory, look for emitter;
                    if (subtypeId.String.Contains("Emitter"))
                    {
                        if (oldEmitter == null || oldEmitter.SubtypeId != subtypeId)
                        {
                            ShieldFactory_TryCreateEmitter(subtypeId, character);
                            if (FirstEmitterFound != null)
                            {
                                ReplaceShieldEmitter(character, FirstEmitterFound);
                                oldEmitter = FirstEmitterFound;
                                ServerLogger.Log(">> Emitter count: " + m_PlayerShieldEmitters.Count + " Player's, " + m_NpcShieldEmitters.Count + " Npc's", 1);
                            }
                        }
                        continue;
                    }
                }
                
                ServerLogger.Log("    Try creating Plugin..", 4);
                if (subtypeId.String.Contains("Plugin"))
                {
                    m_Plugins.Add(subtypeId);
                    continue;
                }

                ServerLogger.Log("    Hmm, unknown item..", 4);
                m_UnknownItems.Add(subtypeId);
            }

            if (oldEmitter != null)
            {
                oldEmitter.CleanPluginsList();
                oldEmitter.AddPlugins(m_Plugins);
            }
            
            ServerLogger.Log("  Found " + m_UnknownItems.Count + " unknown items", 4);
            foreach (MyStringHash subtypeid in m_UnknownItems)
            {
                ServerLogger.Log("    " + subtypeid.String, 4);
            }

            FirstEmitterFound = null;
            m_Plugins.Clear();
            m_UnknownItems.Clear();
            return;
        }
    }
}
