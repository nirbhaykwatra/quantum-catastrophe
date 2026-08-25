using System;
using QC.Character;
using UnityEngine;

namespace QC.Props.ControlStationActions
{
    [Serializable]
    public class GivePlayerAbility : BaseControlStationAction
    {
        [SerializeField]
        public Abilities Ability;

        public override void Execute(in InteractionContext context)
        {
            CharacterAbilities abilities = context.Interactor.GetComponent<CharacterAbilities>();
            foreach (Abilities abilityFlag in Enum.GetValues(typeof(Abilities)))
            {
                if (Ability.HasFlag(abilityFlag))
                {
                    abilities.UnlockAbility(abilityFlag);
                }
            }
            // NotificationManager.Instance.RequestNotification("Unlocked " + Ability + " ability!", 5f, NotificationType.Success);
        }
    }
}