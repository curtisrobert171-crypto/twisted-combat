using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EmpireOfGlass.UI
{
    /// <summary>
    /// Simple tooltip system that displays helpful text when hovering over UI elements.
    /// Part of Phase 4: UI Polish.
    /// </summary>
    public class TooltipSystem : MonoBehaviour
    {
        public static TooltipSystem Instance { get; private set; }

        [Header("References")]
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private TextMeshProUGUI tooltipText;
        [SerializeField] private RectTransform tooltipRect;

        [Header("Settings")]
        [SerializeField] private float padding = 10f;
        [SerializeField] private float showDelay = 0.5f;

        private bool isShowing;
        private float showTimer;
        private string pendingText;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(false);
            }
        }

        private void Update()
        {
            if (isShowing && showTimer > 0f)
            {
                showTimer -= Time.deltaTime;
                if (showTimer <= 0f && !string.IsNullOrEmpty(pendingText))
                {
                    ShowTooltipImmediate(pendingText);
                }
            }

            // Update tooltip position to follow mouse
            if (tooltipPanel != null && tooltipPanel.activeSelf)
            {
                UpdateTooltipPosition();
            }
        }

        /// <summary>
        /// Show a tooltip with the given text after a delay.
        /// </summary>
        public void ShowTooltip(string text, float delay = -1f)
        {
            if (delay < 0f)
            {
                delay = showDelay;
            }

            pendingText = text;
            showTimer = delay;
            isShowing = true;
        }

        /// <summary>
        /// Show a tooltip immediately without delay.
        /// </summary>
        public void ShowTooltipImmediate(string text)
        {
            if (tooltipPanel == null || tooltipText == null) return;

            tooltipText.text = text;
            tooltipPanel.SetActive(true);
            UpdateTooltipPosition();
        }

        /// <summary>
        /// Hide the tooltip.
        /// </summary>
        public void HideTooltip()
        {
            isShowing = false;
            showTimer = 0f;
            pendingText = null;

            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(false);
            }
        }

        private void UpdateTooltipPosition()
        {
            if (tooltipRect == null) return;

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                transform as RectTransform,
                Input.mousePosition,
                null,
                out localPoint
            );

            // Add padding to position tooltip away from cursor
            localPoint.x += padding;
            localPoint.y -= padding;

            // Keep tooltip within screen bounds
            Vector2 size = tooltipRect.sizeDelta;
            Vector2 parentSize = (transform as RectTransform).rect.size;

            // Clamp X
            if (localPoint.x + size.x > parentSize.x / 2f)
            {
                localPoint.x = localPoint.x - size.x - (padding * 2);
            }

            // Clamp Y
            if (localPoint.y - size.y < -parentSize.y / 2f)
            {
                localPoint.y = localPoint.y + size.y + (padding * 2);
            }

            tooltipRect.anchoredPosition = localPoint;
        }
    }
}
