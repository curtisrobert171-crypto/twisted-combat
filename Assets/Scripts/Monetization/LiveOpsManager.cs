using System;
using System.Collections.Generic;
using UnityEngine;

namespace EmpireOfGlass.Monetization
{
    /// <summary>
    /// Live Ops engine for timed events with exclusive loot (Var 21).
    /// Supports 72-hour dark-mode events with high difficulty and exclusive rewards.
    /// </summary>
    public class LiveOpsManager : MonoBehaviour
    {
        public static LiveOpsManager Instance { get; private set; }

        [Header("Event Settings (Var 21)")]
        [SerializeField] private float defaultEventDurationHours = 72f;
        [SerializeField] private float darkModeMultiplier = 2.5f;

        private readonly List<LiveEvent> activeEvents = new List<LiveEvent>();

        public event Action<LiveEvent> OnEventStarted;
        public event Action<LiveEvent> OnEventEnded;
        public event Action<LiveEvent, string> OnEventRewardClaimed;

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

        private void Update()
        {
            CheckExpiredEvents();
        }

        /// <summary>
        /// Start a new timed event (72-hour dark-mode event — Var 21).
        /// </summary>
        public LiveEvent StartEvent(string eventId, string eventName, EventType type, float durationHours = 0f)
        {
            if (durationHours <= 0f)
                durationHours = defaultEventDurationHours;

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var liveEvent = new LiveEvent
            {
                EventID = eventId,
                EventName = eventName,
                Type = type,
                StartTimestamp = now,
                EndTimestamp = now + (long)(durationHours * 3600),
                DifficultyMultiplier = type == EventType.DarkMode ? darkModeMultiplier : 1f,
                ExclusiveRewards = new List<string>(),
                Leaderboard = new List<EventLeaderboardEntry>()
            };

            activeEvents.Add(liveEvent);
            Debug.Log($"[LiveOpsManager] Event started: '{eventName}' ({type}) for {durationHours}h");
            OnEventStarted?.Invoke(liveEvent);
            return liveEvent;
        }

        /// <summary>
        /// Add an exclusive reward to an active event.
        /// </summary>
        public void AddEventReward(string eventId, string rewardId)
        {
            var evt = GetEvent(eventId);
            if (evt == null) return;

            evt.ExclusiveRewards.Add(rewardId);
            Debug.Log($"[LiveOpsManager] Reward '{rewardId}' added to event '{evt.EventName}'");
        }

        /// <summary>
        /// Submit a player's score to the event leaderboard.
        /// </summary>
        public void SubmitScore(string eventId, string userId, string displayName, int score)
        {
            var evt = GetEvent(eventId);
            if (evt == null) return;

            var entry = evt.Leaderboard.Find(e => e.UserID == userId);
            if (entry != null)
            {
                if (score > entry.Score)
                    entry.Score = score;
            }
            else
            {
                evt.Leaderboard.Add(new EventLeaderboardEntry
                {
                    UserID = userId,
                    DisplayName = displayName,
                    Score = score
                });
            }

            Debug.Log($"[LiveOpsManager] Score submitted: {displayName} = {score} in '{evt.EventName}'");
        }

        /// <summary>
        /// Claim an exclusive reward from an event (Var 21).
        /// </summary>
        public bool ClaimEventReward(string eventId, string rewardId)
        {
            var evt = GetEvent(eventId);
            if (evt == null || !evt.ExclusiveRewards.Contains(rewardId)) return false;

            var player = Data.SaveManager.Instance?.CurrentPlayer;
            if (player == null) return false;

            if (player.Inventory == null)
                player.Inventory = new List<Data.InventoryItem>();

            player.Inventory.Add(new Data.InventoryItem
            {
                ItemID = rewardId,
                ItemName = $"Event Reward ({evt.EventName})",
                Type = Data.ItemType.Consumable,
                Quantity = 1,
                Rarity = evt.Type == EventType.DarkMode ? 5 : 3
            });

            Debug.Log($"[LiveOpsManager] Reward '{rewardId}' claimed from '{evt.EventName}'");
            OnEventRewardClaimed?.Invoke(evt, rewardId);
            return true;
        }

        /// <summary>
        /// Get an active event by ID.
        /// </summary>
        public LiveEvent GetEvent(string eventId)
        {
            return activeEvents.Find(e => e.EventID == eventId);
        }

        /// <summary>
        /// Get all currently active events.
        /// </summary>
        public List<LiveEvent> GetActiveEvents()
        {
            return new List<LiveEvent>(activeEvents);
        }

        /// <summary>
        /// Get time remaining for an event in seconds.
        /// </summary>
        public float GetEventTimeRemaining(string eventId)
        {
            var evt = GetEvent(eventId);
            if (evt == null) return 0f;
            long remaining = evt.EndTimestamp - DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return remaining > 0 ? remaining : 0f;
        }

        private void CheckExpiredEvents()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            for (int i = activeEvents.Count - 1; i >= 0; i--)
            {
                if (now >= activeEvents[i].EndTimestamp)
                {
                    var expired = activeEvents[i];
                    activeEvents.RemoveAt(i);
                    Debug.Log($"[LiveOpsManager] Event ended: '{expired.EventName}'");
                    OnEventEnded?.Invoke(expired);
                }
            }
        }
    }

    /// <summary>
    /// Live event data model.
    /// </summary>
    [Serializable]
    public class LiveEvent
    {
        public string EventID;
        public string EventName;
        public EventType Type;
        public long StartTimestamp;
        public long EndTimestamp;
        public float DifficultyMultiplier;
        public List<string> ExclusiveRewards;
        public List<EventLeaderboardEntry> Leaderboard;
    }

    /// <summary>
    /// Leaderboard entry for live events.
    /// </summary>
    [Serializable]
    public class EventLeaderboardEntry
    {
        public string UserID;
        public string DisplayName;
        public int Score;
    }

    /// <summary>
    /// Types of live ops events (Var 21).
    /// </summary>
    public enum EventType
    {
        Standard,
        DarkMode,    // 72-hour high-difficulty with exclusive loot
        Weekend,
        Seasonal
    }
}
