using System;
using System.Collections.Generic;
using QC.Character;
using QC.Props.ControlStationActions;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public enum ControlStationAction
{
    GivePlayerItem,
    GivePlayerAbility,
    WorldInteraction
}

public enum ConditionType
{
    None,
    RequireInventoryItems,
    RequireAbility,
    Custom
}

namespace QC.Props
{
    public class ControlStation : MonoBehaviour, IInteractable
    {
        [Title("Settings")]
        
        [SerializeReference]
        private List<BaseControlStationAction> ControlStationActions = new();
        
        private int m_openTrigger = Animator.StringToHash("Open");
        private int m_closeTrigger = Animator.StringToHash("Close");
        private Animator m_animator;
        private TextMeshProUGUI m_interactionText;
        private bool m_isOpen = false;
        
        [Title("Already Used")]
        [SerializeField]
        private bool hasBeenUsed = false;
        
        [ShowIf("hasBeenUsed")]
        [SerializeField]
        private string usedMessage = "Already activated.";

        private void Awake()
        {
            m_animator = GetComponent<Animator>();
            m_interactionText = GetComponentInChildren<TextMeshProUGUI>();
        }

        public void Interact(in InteractionContext context)
        {
            if (hasBeenUsed)
            {
                ShowMessage(usedMessage);
                return;
            }

            foreach (BaseControlStationAction action in ControlStationActions)
            {
                if (!action.CheckConditions(context)) return;
            }

            foreach (BaseControlStationAction action in ControlStationActions)
            {
                action.Execute(context);
            }
            
            Close();
            hasBeenUsed = true;
        }
        
        private void ShowMessage(string message)
        {
            NotificationManager.Instance.RequestModal(message);
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasBeenUsed) return;
            if (other.GetComponent<PlayerController>())
            {
                Open();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (hasBeenUsed) return;
            if (other.GetComponent<PlayerController>())
            {
                Close();
            }
        }

        private void Open()
        {
            m_animator.SetTrigger(m_openTrigger);
            m_isOpen = true;
            m_interactionText.text = "Press E to activate";
        }
        
        private void Close()
        {
            m_animator.SetTrigger(m_closeTrigger);
            m_isOpen = false;
            m_interactionText.text = "";
        }
    }
}


