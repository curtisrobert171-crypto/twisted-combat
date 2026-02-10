using System.Collections.Generic;
using UnityEngine;

namespace EmpireOfGlass.Core
{
    /// <summary>
    /// Manages in-game notifications for social competition (Var 18).
    /// Handles friend-attack alerts, revenge prompts, 2x loot notifications,
    /// and alliance activity updates.
    /// </summary>
    public class NotificationManager : MonoBehaviour
    {
        public static NotificationManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private int maxNotifications = 50;
        [SerializeField] private float notificationDisplayDuration = 5f;

        private readonly List<GameNotification> notifications = new List<GameNotification>();
        private readonly List<GameNotification> unreadNotifications = new List<GameNotification>();

        public int UnreadCount => unreadNotifications.Count;

        public event System.Action<GameNotification> OnNotificationReceived;
        public event System.Action OnNotificationsCleared;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Notify the player that their base was attacked (Var 18 — Social Competition).
        /// </summary>
        public void NotifyBaseAttacked(string attackerName, int goldStolen)
        {
            AddNotification(new GameNotification
            {
                Type = NotificationType.BaseAttacked,
                Title = "Base Raided!",
                Message = $"{attackerName} raided your base and stole {goldStolen} gold!",
                AttackerID = attackerName,
                GoldAmount = goldStolen,
                CanRevenge = true
            });
        }

        /// <summary>
        /// Notify the player about a revenge opportunity with 2x loot (Var 18).
        /// </summary>
        public void NotifyRevengeAvailable(string targetName)
        {
            AddNotification(new GameNotification
            {
                Type = NotificationType.RevengeAvailable,
                Title = "Revenge Ready!",
                Message = $"Attack {targetName} back for 2x loot!",
                AttackerID = targetName,
                CanRevenge = true
            });
        }

        /// <summary>
        /// Notify about alliance activity (Var 19 — Social Collaboration).
        /// </summary>
        public void NotifyAllianceActivity(string memberName, string activity)
        {
            AddNotification(new GameNotification
            {
                Type = NotificationType.AllianceActivity,
                Title = "Alliance Update",
                Message = $"{memberName} {activity}"
            });
        }

        /// <summary>
        /// Notify about a bounty placed on the player (Var 43 — Viral Loop).
        /// </summary>
        public void NotifyBountyPlaced(string placerName, int bountyGold)
        {
            AddNotification(new GameNotification
            {
                Type = NotificationType.BountyPlaced,
                Title = "Bounty Alert!",
                Message = $"{placerName} placed a {bountyGold} gold bounty on you!",
                GoldAmount = bountyGold
            });
        }

        /// <summary>
        /// Notify about a live ops event starting (Var 21).
        /// </summary>
        public void NotifyEventStarted(string eventName, float durationHours)
        {
            AddNotification(new GameNotification
            {
                Type = NotificationType.EventStarted,
                Title = "New Event!",
                Message = $"{eventName} has begun! {durationHours}h remaining."
            });
        }

        /// <summary>
        /// Notify about shield expiry (Var 34 — Shield Mechanics).
        /// </summary>
        public void NotifyShieldExpiring()
        {
            AddNotification(new GameNotification
            {
                Type = NotificationType.ShieldExpiring,
                Title = "Shield Warning",
                Message = "Your base shield is about to expire! Log in to replenish."
            });
        }

        /// <summary>
        /// Get all notifications.
        /// </summary>
        public List<GameNotification> GetAllNotifications()
        {
            return new List<GameNotification>(notifications);
        }

        /// <summary>
        /// Get unread notifications.
        /// </summary>
        public List<GameNotification> GetUnreadNotifications()
        {
            return new List<GameNotification>(unreadNotifications);
        }

        /// <summary>
        /// Mark all notifications as read.
        /// </summary>
        public void MarkAllAsRead()
        {
            unreadNotifications.Clear();
            OnNotificationsCleared?.Invoke();
            Debug.Log("[NotificationManager] All notifications marked as read");
        }

        private void AddNotification(GameNotification notification)
        {
            notification.Timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            notifications.Add(notification);
            unreadNotifications.Add(notification);

            if (notifications.Count > maxNotifications)
                notifications.RemoveAt(0);

            Debug.Log($"[NotificationManager] {notification.Type}: {notification.Message}");
            OnNotificationReceived?.Invoke(notification);
        }
    }

    /// <summary>
    /// In-game notification data.
    /// </summary>
    [System.Serializable]
    public class GameNotification
    {
        public NotificationType Type;
        public string Title;
        public string Message;
        public long Timestamp;
        public string AttackerID;
        public int GoldAmount;
        public bool CanRevenge;
    }

    /// <summary>
    /// Notification categories.
    /// </summary>
    public enum NotificationType
    {
        BaseAttacked,
        RevengeAvailable,
        AllianceActivity,
        BountyPlaced,
        EventStarted,
        ShieldExpiring,
        DailyGiftReminder,
        TrialHeroExpiring
    }
}
