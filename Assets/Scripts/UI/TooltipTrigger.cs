using UnityEngine;
using UnityEngine.EventSystems;

namespace EmpireOfGlass.UI
{
    /// <summary>
    /// Component to attach to UI elements that should show tooltips on hover.
    /// Part of Phase 4: UI Polish.
    /// </summary>
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Tooltip Settings")]
        [SerializeField] [TextArea(2, 5)] private string tooltipText;
        [SerializeField] private float showDelay = 0.5f;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (TooltipSystem.Instance != null && !string.IsNullOrEmpty(tooltipText))
            {
                TooltipSystem.Instance.ShowTooltip(tooltipText, showDelay);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (TooltipSystem.Instance != null)
            {
                TooltipSystem.Instance.HideTooltip();
            }
        }

        /// <summary>
        /// Update tooltip text dynamically at runtime.
        /// </summary>
        public void SetTooltipText(string text)
        {
            tooltipText = text;
        }
    }
}
