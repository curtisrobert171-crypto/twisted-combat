using UnityEngine;
using System.Collections;

namespace EmpireOfGlass.Core
{
    /// <summary>
    /// Provides "juice" effects for visual polish: screen shake, hit pause, chromatic aberration, impact feedback.
    /// Enhances game feel with minimal performance overhead.
    /// </summary>
    public class JuiceManager : MonoBehaviour
    {
        public static JuiceManager Instance { get; private set; }

        [Header("Screen Shake")]
        [SerializeField] private float shakeIntensity = 0.3f;
        [SerializeField] private float shakeDuration = 0.2f;
        [SerializeField] private AnimationCurve shakeDecay = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [Header("Hit Pause")]
        [SerializeField] private float hitPauseDuration = 0.05f;
        [SerializeField] private float hitPauseTimeScale = 0.1f;

        [Header("Impact Settings")]
        [SerializeField] private float impactScaleDuration = 0.15f;
        [SerializeField] private float impactScaleAmount = 1.2f;

        private Camera mainCamera;
        private Vector3 originalCameraPosition;
        private Coroutine shakeCoroutine;
        private Coroutine hitPauseCoroutine;

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

        private void Start()
        {
            mainCamera = Camera.main;
            if (mainCamera != null)
            {
                originalCameraPosition = mainCamera.transform.localPosition;
            }
        }

        /// <summary>
        /// Trigger screen shake effect with specified intensity multiplier.
        /// </summary>
        public void ScreenShake(float intensityMultiplier = 1f)
        {
            if (mainCamera == null) return;

            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
            }

            shakeCoroutine = StartCoroutine(ScreenShakeCoroutine(intensityMultiplier));
        }

        private IEnumerator ScreenShakeCoroutine(float intensityMultiplier)
        {
            float elapsed = 0f;
            Vector3 startPos = mainCamera.transform.localPosition;

            while (elapsed < shakeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / shakeDuration;
                float strength = shakeDecay.Evaluate(progress) * shakeIntensity * intensityMultiplier;

                Vector3 offset = Random.insideUnitSphere * strength;
                mainCamera.transform.localPosition = originalCameraPosition + offset;

                yield return null;
            }

            mainCamera.transform.localPosition = originalCameraPosition;
            shakeCoroutine = null;
        }

        /// <summary>
        /// Trigger hit pause (brief slow-motion) for impact feedback.
        /// </summary>
        public void HitPause()
        {
            if (hitPauseCoroutine != null)
            {
                StopCoroutine(hitPauseCoroutine);
            }

            hitPauseCoroutine = StartCoroutine(HitPauseCoroutine());
        }

        private IEnumerator HitPauseCoroutine()
        {
            float originalTimeScale = Time.timeScale;
            Time.timeScale = hitPauseTimeScale;

            yield return new WaitForSecondsRealtime(hitPauseDuration);

            Time.timeScale = originalTimeScale;
            hitPauseCoroutine = null;
        }

        /// <summary>
        /// Apply impact scale effect to a transform (punch in/out).
        /// </summary>
        public void ImpactScale(Transform target)
        {
            if (target == null) return;
            StartCoroutine(ImpactScaleCoroutine(target));
        }

        private IEnumerator ImpactScaleCoroutine(Transform target)
        {
            if (target == null) yield break;

            Vector3 originalScale = target.localScale;
            Vector3 targetScale = originalScale * impactScaleAmount;

            float elapsed = 0f;
            float halfDuration = impactScaleDuration / 2f;

            // Scale up
            while (elapsed < halfDuration)
            {
                if (target == null) yield break;
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                target.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }

            // Scale down
            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                if (target == null) yield break;
                elapsed += Time.deltaTime;
                float t = elapsed / halfDuration;
                target.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }

            if (target != null)
            {
                target.localScale = originalScale;
            }
        }

        /// <summary>
        /// Play full impact effect: shake + hit pause + optional scale.
        /// </summary>
        public void ImpactEffect(Transform target = null, float shakeMultiplier = 1f)
        {
            ScreenShake(shakeMultiplier);
            HitPause();
            if (target != null)
            {
                ImpactScale(target);
            }

            // Play haptic if available
            if (HapticManager.Instance != null)
            {
                HapticManager.Instance.TriggerImpact();
            }

            // Track analytics
            if (Analytics.AnalyticsManager.Instance != null)
            {
                Analytics.AnalyticsManager.Instance.TrackEvent("Impact_Effect", new System.Collections.Generic.Dictionary<string, object>
                {
                    { "shake_multiplier", shakeMultiplier },
                    { "has_target", target != null }
                });
            }
        }

        /// <summary>
        /// Light shake for UI feedback.
        /// </summary>
        public void LightShake()
        {
            ScreenShake(0.3f);
        }

        /// <summary>
        /// Medium shake for gate activations.
        /// </summary>
        public void MediumShake()
        {
            ScreenShake(0.6f);
        }

        /// <summary>
        /// Heavy shake for boss hits.
        /// </summary>
        public void HeavyShake()
        {
            ScreenShake(1.5f);
        }

        /// <summary>
        /// Pulse effect for UI elements.
        /// </summary>
        public void PulseUI(Transform uiElement)
        {
            if (uiElement == null) return;
            StartCoroutine(PulseUICoroutine(uiElement));
        }

        private IEnumerator PulseUICoroutine(Transform uiElement)
        {
            if (uiElement == null) yield break;

            Vector3 originalScale = uiElement.localScale;
            float pulseDuration = 0.3f;
            float elapsed = 0f;

            while (elapsed < pulseDuration)
            {
                if (uiElement == null) yield break;
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / pulseDuration;
                float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.1f;
                uiElement.localScale = originalScale * scale;
                yield return null;
            }

            if (uiElement != null)
            {
                uiElement.localScale = originalScale;
            }
        }
    }
}
