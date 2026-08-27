using System;
using System.Collections.Generic;
using QC.Character;
using QC.Utilities.EventBusSystem;
using QC.Utilities.ServiceLocation;
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

                string message = entry.Quantity > 1
                    ? $"Received {entry.Quantity}× {entry.Item.Name}!"
                    : $"Received {entry.Item.Name}!";

                eventBus.Raise(new OnRequestNotification
                {
                    Message = message,
                    Icon = entry.Item.Icon,
                    Type = NotificationType,
                    Duration = 3f
                });
            }
        }
    }
}