using System;
using System.Collections.Generic;
using QC.Character;
using QC.Utilities.EventBusSystem;
using UnityEngine;

namespace QC.Props.ControlStationActions
{
    [Serializable]
    public class GivePlayerItem : BaseControlStationAction
    {
        [SerializeField]
        private List<LootEntry> ItemsToGive = new();

        public override void Execute(in InteractionContext context, UIEventBus eventBus)
        {
            CharacterInventory inventory = context.Interactor.GetComponent<CharacterInventory>();
            inventory.AddItems(ItemsToGive);
            
            ;
            foreach (LootEntry entry in ItemsToGive)
            {
                if (entry.Item == null || entry.Quantity <= 0) continue;
            }
        }
    }
}