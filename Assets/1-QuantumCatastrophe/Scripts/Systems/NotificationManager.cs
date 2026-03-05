using System;
using SCP.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public struct Notification
{
    public string Message;
    public float Duration;
    public NotificationType Type;
    
}

public enum NotificationType
{
    Info,
    Warning,
    Error,
    Success,
}

public class NotificationManager : Singleton<NotificationManager>
{
    // TODO: For each requested notification, create a UI notification box prefab, instantiate it and change the data in that instance to
    //  enable notification to be queued on the screen.
    [SerializeField]
    private GameObject m_notificationPanel;
    [SerializeField] 
    private GameObject m_HUDCanvas;
    [SerializeField]
    private GameObject m_notificationPrefab;
    [SerializeField]
    private GameObject m_modalPrefab;

    public UnityEvent OnPublishModal;
    
    private string m_notificationText;
    private event Action<Notification> OnNotificationRequested;
    private event Action<string> OnModalRequested;

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        OnNotificationRequested += PublishNotification;
        OnModalRequested += PublishModal;
    }

    private void OnDisable()
    {
        OnNotificationRequested -= PublishNotification;
        OnModalRequested -= PublishModal;
    }

    public void RequestNotification(Notification notification)
    {
        OnNotificationRequested?.Invoke(notification);
    }
    
    public void RequestModal(string message)
    {
        OnModalRequested?.Invoke(message);
    }
    
    public void RequestNotification(string message, float duration = 3f, NotificationType type = NotificationType.Info)
    {
        RequestNotification(new Notification
        {
            Message = message,
            Duration = duration,
            Type = type
        });
    }

    private void PublishNotification(Notification notification)
    {
        GameObject notificationObject = Instantiate(m_notificationPrefab, m_notificationPanel.transform);
        NotificationUI notificationUI = notificationObject.GetComponent<NotificationUI>();
        notificationUI.InstantiateNotification(notification);
    }

    private void PublishModal(string message)
    {
        GameObject modalObject = Instantiate(m_modalPrefab, m_HUDCanvas.transform);
        Modal modal = modalObject.GetComponent<Modal>();
        modal.SetText(message);
        OnPublishModal?.Invoke();
    }
    
}
