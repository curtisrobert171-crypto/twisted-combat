using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace EmpireOfGlass.Core
{
    /// <summary>
    /// Manages smooth scene transitions with fade effects and loading screens.
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        [Header("Transition Settings")]
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private Color fadeColor = Color.black;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Loading Screen")]
        [SerializeField] private GameObject loadingScreenPrefab;
        [SerializeField] private bool showLoadingScreen = true;
        [SerializeField] private float minLoadingTime = 1f;

        private CanvasGroup fadeCanvasGroup;
        private GameObject loadingScreenInstance;
        private bool isTransitioning = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            CreateFadeCanvas();
        }

        /// <summary>
        /// Create the fade overlay canvas.
        /// </summary>
        private void CreateFadeCanvas()
        {
            // Create canvas
            GameObject canvasObj = new GameObject("FadeCanvas");
            canvasObj.transform.SetParent(transform);

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Create fade image
            GameObject imageObj = new GameObject("FadeImage");
            imageObj.transform.SetParent(canvasObj.transform);

            var rectTransform = imageObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            var image = imageObj.AddComponent<UnityEngine.UI.Image>();
            image.color = fadeColor;

            fadeCanvasGroup = imageObj.AddComponent<CanvasGroup>();
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }

        /// <summary>
        /// Load a scene with transition effects.
        /// </summary>
        public void LoadScene(string sceneName, bool useTransition = true)
        {
            if (isTransitioning)
            {
                Debug.LogWarning("[SceneTransitionManager] Already transitioning");
                return;
            }

            if (useTransition)
            {
                StartCoroutine(LoadSceneWithTransition(sceneName));
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }
        }

        /// <summary>
        /// Load a scene asynchronously with transition.
        /// </summary>
        private IEnumerator LoadSceneWithTransition(string sceneName)
        {
            isTransitioning = true;

            // Fade out
            yield return FadeOut();

            // Show loading screen
            if (showLoadingScreen && loadingScreenPrefab != null)
            {
                ShowLoadingScreen();
            }

            // Track loading time
            float startTime = Time.realtimeSinceStartup;

            // Start loading scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;

            // Wait for scene to load
            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                UpdateLoadingProgress(progress);

                // Scene is ready
                if (asyncLoad.progress >= 0.9f)
                {
                    // Ensure minimum loading time for smooth experience
                    float elapsedTime = Time.realtimeSinceStartup - startTime;
                    if (elapsedTime < minLoadingTime)
                    {
                        yield return new WaitForSeconds(minLoadingTime - elapsedTime);
                    }

                    asyncLoad.allowSceneActivation = true;
                }

                yield return null;
            }

            // Hide loading screen
            if (loadingScreenInstance != null)
            {
                HideLoadingScreen();
            }

            // Fade in
            yield return FadeIn();

            isTransitioning = false;

            // Track analytics
            if (Analytics.AnalyticsManager.Instance != null)
            {
                Analytics.AnalyticsManager.Instance.TrackEvent("Scene_Loaded", new System.Collections.Generic.Dictionary<string, object>
                {
                    { "scene_name", sceneName },
                    { "load_time", Time.realtimeSinceStartup - startTime }
                });
            }
        }

        /// <summary>
        /// Fade out to black.
        /// </summary>
        private IEnumerator FadeOut()
        {
            if (fadeCanvasGroup == null) yield break;

            fadeCanvasGroup.blocksRaycasts = true;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = fadeCurve.Evaluate(elapsed / fadeDuration);
                fadeCanvasGroup.alpha = t;
                yield return null;
            }

            fadeCanvasGroup.alpha = 1f;
        }

        /// <summary>
        /// Fade in from black.
        /// </summary>
        private IEnumerator FadeIn()
        {
            if (fadeCanvasGroup == null) yield break;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = 1f - fadeCurve.Evaluate(elapsed / fadeDuration);
                fadeCanvasGroup.alpha = t;
                yield return null;
            }

            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }

        /// <summary>
        /// Show loading screen.
        /// </summary>
        private void ShowLoadingScreen()
        {
            if (loadingScreenPrefab != null && loadingScreenInstance == null)
            {
                loadingScreenInstance = Instantiate(loadingScreenPrefab, transform);
            }

            if (loadingScreenInstance != null)
            {
                loadingScreenInstance.SetActive(true);
            }
        }

        /// <summary>
        /// Hide loading screen.
        /// </summary>
        private void HideLoadingScreen()
        {
            if (loadingScreenInstance != null)
            {
                loadingScreenInstance.SetActive(false);
                Destroy(loadingScreenInstance, 0.5f);
                loadingScreenInstance = null;
            }
        }

        /// <summary>
        /// Update loading progress indicator.
        /// </summary>
        private void UpdateLoadingProgress(float progress)
        {
            if (loadingScreenInstance == null) return;

            // Find progress bar/text in loading screen
            var progressBar = loadingScreenInstance.GetComponentInChildren<UnityEngine.UI.Slider>();
            if (progressBar != null)
            {
                progressBar.value = progress;
            }

            var progressText = loadingScreenInstance.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (progressText != null)
            {
                progressText.text = $"Loading... {Mathf.RoundToInt(progress * 100)}%";
            }
        }

        /// <summary>
        /// Restart current scene with transition.
        /// </summary>
        public void RestartScene()
        {
            string currentScene = SceneManager.GetActiveScene().name;
            LoadScene(currentScene);
        }

        /// <summary>
        /// Quit game with fade transition.
        /// </summary>
        public void QuitGame()
        {
            StartCoroutine(QuitGameWithTransition());
        }

        private IEnumerator QuitGameWithTransition()
        {
            yield return FadeOut();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// Get if currently transitioning.
        /// </summary>
        public bool IsTransitioning => isTransitioning;
    }
}
