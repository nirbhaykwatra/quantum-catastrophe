using System;
using QC.Character;
using QC.Systems.Notifications;
using QC.Utilities.EventBusSystem;
using QC.Utilities.ServiceLocation;
using UnityEngine;

namespace QC.Props.ControlStationActions
{
    [Serializable]
    public class GivePlayerAbility : BaseControlStationAction
    {
        [SerializeField]
        public Abilities Ability;

        public override void Execute(in InteractionContext context, UIEventBus eventBus)
        {
            CharacterAbilities abilities = context.Interactor.GetComponent<CharacterAbilities>();
            foreach (Abilities abilityFlag in Enum.GetValues(typeof(Abilities)))
            {
                if (Ability.HasFlag(abilityFlag))
                {
                    abilities.UnlockAbility(abilityFlag);
                    eventBus.Raise(new OnRequestNotification
                    {
                        Message = NotificationText,
                        Icon = NotificationIcon,
                        Type = NotificationType,
                        Duration = NotificationDuration
                    });
                }
            }
        }
    }
}