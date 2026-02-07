using UnityEngine;

namespace EmpireOfGlass.Core
{
    /// <summary>
    /// Manages haptic feedback with distinct textures (Var 10):
    /// - Sharp ticks for shooting
    /// - Rolling rumble for swarm flow
    /// Platform-specific implementation for iOS/Android.
    /// </summary>
    public class HapticManager : MonoBehaviour
    {
        public static HapticManager Instance { get; private set; }

        public enum HapticType
        {
            SharpTick,      // Shooting / gate activation
            RollingRumble,  // Swarm flow
            HeavyImpact,    // Boss shatter
            LightTap         // UI interaction
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Trigger haptic feedback of the specified type.
        /// </summary>
        public void TriggerHaptic(HapticType type)
        {
#if UNITY_IOS
            TriggerIOSHaptic(type);
#elif UNITY_ANDROID
            TriggerAndroidHaptic(type);
#endif
            Debug.Log($"[HapticManager] Haptic: {type}");
        }

#if UNITY_IOS
        private void TriggerIOSHaptic(HapticType type)
        {
            // iOS Taptic Engine integration placeholder
            // Uses UIImpactFeedbackGenerator for different styles
        }
#endif

#if UNITY_ANDROID
        private void TriggerAndroidHaptic(HapticType type)
        {
            long[] pattern = type switch
            {
                HapticType.SharpTick => new long[] { 0, 10 },
                HapticType.RollingRumble => new long[] { 0, 5, 10, 5, 10, 5 },
                HapticType.HeavyImpact => new long[] { 0, 50 },
                HapticType.LightTap => new long[] { 0, 5 },
                _ => new long[] { 0, 10 }
            };

            // Android Vibrator API integration placeholder
            // Uses AndroidJavaObject for platform-specific vibration
        }
#endif
    }
}
