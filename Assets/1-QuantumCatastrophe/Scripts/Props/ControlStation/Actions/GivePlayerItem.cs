using System;
using System.Collections.Generic;
using QC.Character;
using UnityEngine;

namespace QC.Props.ControlStationActions
{
    [Serializable]
    public class GivePlayerItem : BaseControlStationAction
    {
        [SerializeField]
        private List<Loot> ItemsToGive = new List<Loot>();
        
        public override void Execute(in InteractionContext context)
        {
            CharacterInventory inventory = context.Interactor.GetComponent<CharacterInventory>();
            for (int i = ItemsToGive.Count - 1; i >= 0; i--)
            {
                inventory.AddItem(ItemsToGive[i]);
            }
            // NotificationManager.Instance.RequestNotification("Unlocked " + _ability + " ability!", 5f, NotificationType.Success);
        }
    }
}