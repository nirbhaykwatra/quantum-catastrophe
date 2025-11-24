using System;
using SCP.Utilities;
using UnityEngine;
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
    private VerticalLayoutGroup m_notificationPanel;
    private GameObject m_notificationArea;
    private string m_notificationText;
    [SerializeField]
    private GameObject m_notificationPrefab;

    private event Action<Notification> OnNotificationRequested;

    protected override void Awake()
    {
        base.Awake();
        m_notificationArea = m_notificationPanel.gameObject;
    }

    private void OnEnable()
    {
        OnNotificationRequested += PublishNotification;
    }

    private void OnDisable()
    {
        OnNotificationRequested -= PublishNotification;
    }

    public void RequestNotification(Notification notification)
    {
        OnNotificationRequested?.Invoke(notification);
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
        GameObject notificationObject = Instantiate(m_notificationPrefab, m_notificationArea.transform);
        NotificationUI notificationUI = notificationObject.GetComponent<NotificationUI>();
        notificationUI.InstantiateNotification(notification);
        Debug.Log($"Notification!\nMessage: {m_notificationText}\nDuration: {notification.Duration}\nType: {notification.Type}");
    }
    
    
}
