using System.Collections.Generic;
using UnityEngine;

namespace EmpireOfGlass.LiveOps
{
    /// <summary>
    /// Manages the Live Ops Engine: 72-hour dark-mode events with high difficulty
    /// and exclusive loot (Var 21).
    /// </summary>
    public class LiveOpsManager : MonoBehaviour
    {
        public static LiveOpsManager Instance { get; private set; }

        [Header("Event Settings (Var 21)")]
        [SerializeField] private float defaultEventDurationHours = 72f;
        [SerializeField] private float difficultyMultiplier = 2f;
        [SerializeField] private float exclusiveLootDropRate = 0.05f;

        private LiveEvent activeEvent;
        private readonly List<LiveEvent> eventHistory = new List<LiveEvent>();

        public LiveEvent ActiveEvent => activeEvent;
        public bool IsEventActive => activeEvent != null && !activeEvent.HasExpired;
        public IReadOnlyList<LiveEvent> EventHistory => eventHistory;

        public event System.Action<LiveEvent> OnEventStarted;
        public event System.Action<LiveEvent> OnEventEnded;
        public event System.Action<string> OnExclusiveRewardEarned;

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
            if (activeEvent != null && activeEvent.HasExpired)
            {
                EndEvent();
            }
        }

        /// <summary>
        /// Start a new 72-hour dark-mode event (Var 21).
        /// </summary>
        public void StartEvent(string eventId, string eventName, EventType type)
        {
            if (activeEvent != null && !activeEvent.HasExpired)
            {
                Debug.Log("[LiveOpsManager] An event is already active");
                return;
            }

            long now = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            activeEvent = new LiveEvent
            {
                EventId = eventId,
                EventName = eventName,
                Type = type,
                StartTimestamp = now,
                EndTimestamp = now + (long)(defaultEventDurationHours * 3600),
                DifficultyMultiplier = difficultyMultiplier,
                ExclusiveLootDropRate = exclusiveLootDropRate,
                PlayerScore = 0
            };

            Debug.Log($"[LiveOpsManager] Event started: {eventName} ({type}), ends in {defaultEventDurationHours}h");
            OnEventStarted?.Invoke(activeEvent);
        }

        /// <summary>
        /// Add score to the current event from gameplay actions.
        /// </summary>
        public void AddEventScore(int score)
        {
            if (activeEvent == null || activeEvent.HasExpired) return;

            activeEvent.PlayerScore += score;
            Debug.Log($"[LiveOpsManager] Event score: {activeEvent.PlayerScore} (+{score})");
        }

        /// <summary>
        /// Roll for exclusive event loot based on drop rate.
        /// </summary>
        public bool RollExclusiveLoot()
        {
            if (activeEvent == null || activeEvent.HasExpired) return false;

            bool dropped = Random.value <= activeEvent.ExclusiveLootDropRate;
            if (dropped)
            {
                Debug.Log($"[LiveOpsManager] Exclusive loot dropped from event: {activeEvent.EventName}!");
                OnExclusiveRewardEarned?.Invoke(activeEvent.EventId);
            }
            return dropped;
        }

        /// <summary>
        /// Get remaining time for the active event in seconds.
        /// </summary>
        public float GetEventTimeRemaining()
        {
            if (activeEvent == null) return 0f;
            long now = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return Mathf.Max(0f, activeEvent.EndTimestamp - now);
        }

        private void EndEvent()
        {
            if (activeEvent == null) return;

            Debug.Log($"[LiveOpsManager] Event ended: {activeEvent.EventName}, final score: {activeEvent.PlayerScore}");
            eventHistory.Add(activeEvent);
            OnEventEnded?.Invoke(activeEvent);
            activeEvent = null;
        }
    }

    /// <summary>
    /// Data for a live ops event.
    /// </summary>
    [System.Serializable]
    public class LiveEvent
    {
        public string EventId;
        public string EventName;
        public EventType Type;
        public long StartTimestamp;
        public long EndTimestamp;
        public float DifficultyMultiplier;
        public float ExclusiveLootDropRate;
        public int PlayerScore;

        public bool HasExpired =>
            System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= EndTimestamp;
    }

    /// <summary>
    /// Types of live ops events.
    /// </summary>
    public enum EventType
    {
        DarkMode,
        SwarmChallenge,
        RaidTournament,
        CityBuildRace,
        BossRush
    }
}
