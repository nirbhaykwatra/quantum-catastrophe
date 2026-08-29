using System;
using System.Collections.Generic;
using QC.Utilities.EventBusSystem;
using QC.Utilities.ServiceLocation;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UIElements;

namespace QC.Systems.Notifications
{
    public enum NotificationType
    {
        Info,
        Achievement,
        Warning
    }
    
    public class NotificationManager : MonoBehaviour
    {
        [SerializeField] private UIDocument notificationDocument;
        [SerializeField] private VisualTreeAsset notificationTemplate;
        [SerializeField] private int poolSize = 5;
        [SerializeField] private int maxConcurrent = 3;

        [Title("Debug")] 
        [SerializeField] private bool _enableDebugging;
        
        [SerializeField] 
        [ShowInInspector]
        private LootEntry _debugLoot;

        [SerializeField] private NotificationType _debugNotificationType;

        private VisualElement _root;
        private VisualElement _container;

        private readonly Stack<VisualElement> _pool = new();
        private readonly List<VisualElement> _active = new();
        private readonly Queue<OnRequestNotification> _pending = new();

        private UIEventBus _uiEventBus;
        private EventBinding<OnRequestNotification> _onRequestNotification;

        private void Awake()
        {
            _root = notificationDocument.rootVisualElement;
            _container = _root.Q<VisualElement>("notification-container");

            for (int i = 0; i < poolSize; i++)
            {
                TemplateContainer element = notificationTemplate.Instantiate();
                VisualElement item = element.Q<VisualElement>("notification-item");
                item.style.display = DisplayStyle.None;
                _container.Add(element);
                _pool.Push(element);
            }

            _uiEventBus = ServiceLocator.ForSceneOf(this).Get<EventBusRegistry>().Get<UIEventBus>();
        }

        private void OnEnable()
        {
            _onRequestNotification = new EventBinding<OnRequestNotification>(Show);
            _uiEventBus.Register(_onRequestNotification);
        }

        private void OnDisable()
        {
            _uiEventBus.Deregister(_onRequestNotification);
        }

        // Call this from your event bus handler
        private void Show(OnRequestNotification request)
        {
            if (_active.Count >= maxConcurrent)
            {
                _pending.Enqueue(request);
                return;
            }

            DisplayNotification(request);
            if (_enableDebugging) Debug.Log($"Notification requested! Duration: {request.Duration} Message: {request.Message} Type: {request.Type}");
        }

        private void DisplayNotification(OnRequestNotification request)
        {
            if (_pool.Count == 0)
            {
                // Pool exhausted — drop oldest active or just requeue
                _pending.Enqueue(request);
                return;
            }

            VisualElement element = _pool.Pop();
            VisualElement item = element.Q<VisualElement>("notification-item");
            Label label = element.Q<Label>("notification-label");
            VisualElement icon = element.Q<VisualElement>("notification-icon");

            label.text = request.Message;
            if (request.Icon) icon.style.backgroundImage = new StyleBackground(request.Icon);
            item.RemoveFromClassList("notification-warning");
            item.RemoveFromClassList("notification-achievement");
            item.RemoveFromClassList("notification-info");
            item.AddToClassList(GetTypeClass(request.Type));

            item.style.display = DisplayStyle.Flex;
            _active.Add(element);

            // Enter animation
            item.schedule.Execute(() => item.AddToClassList("notification-enter")).StartingIn(0);

            // Schedule exit after duration
            item.schedule.Execute(() => BeginExit(element)).StartingIn((long)(request.Duration * 1000));
        }

        private void BeginExit(VisualElement element)
        {
            VisualElement item = element.Q<VisualElement>("notification-item");
            item.RemoveFromClassList("notification-enter");
            item.AddToClassList("notification-exit");

            // Wait for the CSS transition to actually finish before recycling
            item.RegisterCallback<TransitionEndEvent>(OnExitTransitionEnd);
        }

        private void OnExitTransitionEnd(TransitionEndEvent evt)
        {
            VisualElement item = evt.target as VisualElement;
            item.UnregisterCallback<TransitionEndEvent>(OnExitTransitionEnd);
            item.RemoveFromClassList("notification-exit");
            item.style.display = DisplayStyle.None;

            VisualElement element = item.parent; // adjust if your template root differs
            _active.Remove(element);
            _pool.Push(element);

            if (_pending.Count > 0)
                DisplayNotification(_pending.Dequeue());
        }

        private static string GetTypeClass(NotificationType type) => type switch
        {
            NotificationType.Warning => "notification-warning",
            NotificationType.Achievement => "notification-achievement",
            _ => "notification-info",
        };
        
        [Title("Debug")]
        [Button]
        private void TestNotification()
        {
            Show(new OnRequestNotification { Duration = 5f, Icon = _debugLoot.Item.Icon, Message = $" Test for {_debugLoot.Item.Name}", Type = _debugNotificationType});
        }
    }
}

