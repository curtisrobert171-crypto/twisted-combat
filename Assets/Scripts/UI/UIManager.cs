using UnityEngine;

namespace EmpireOfGlass.UI
{
    /// <summary>
    /// Manages UI navigation following the graph: Splash → Login → Main City → Mode Select → Shop (Var 47).
    /// Implements diegetic 3D menus that shatter on close (Var 8).
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI Panels")]
        [SerializeField] private GameObject splashPanel;
        [SerializeField] private GameObject loginPanel;
        [SerializeField] private GameObject cityHUD;
        [SerializeField] private GameObject swarmHUD;
        [SerializeField] private GameObject raidHUD;
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private GameObject reviveOfferPanel;
        [SerializeField] private GameObject merchantPanel;

        [Header("Swarm HUD")]
        [SerializeField] private UnityEngine.UI.Text swarmCountText;
        [SerializeField] private UnityEngine.UI.Text goldText;
        [SerializeField] private UnityEngine.UI.Text gemsText;

        private GameObject activePanel;

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
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.OnStateChanged += HandleStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.OnStateChanged -= HandleStateChanged;
            }
        }

        private void HandleStateChanged(Core.GameManager.GameState from, Core.GameManager.GameState to)
        {
            HideAllPanels();

            switch (to)
            {
                case Core.GameManager.GameState.Splash:
                    ShowPanel(splashPanel);
                    break;
                case Core.GameManager.GameState.Login:
                    ShowPanel(loginPanel);
                    break;
                case Core.GameManager.GameState.City:
                    ShowPanel(cityHUD);
                    break;
                case Core.GameManager.GameState.Swarm:
                    ShowPanel(swarmHUD);
                    break;
                case Core.GameManager.GameState.Raid:
                    ShowPanel(raidHUD);
                    break;
                case Core.GameManager.GameState.Shop:
                    ShowPanel(shopPanel);
                    break;
            }
        }

        /// <summary>
        /// Update the swarm counter display during gameplay.
        /// </summary>
        public void UpdateSwarmCount(int count)
        {
            if (swarmCountText != null)
            {
                swarmCountText.text = count.ToString();
            }
        }

        /// <summary>
        /// Update currency displays.
        /// </summary>
        public void UpdateCurrencyDisplay(int gold, int gems)
        {
            if (goldText != null) goldText.text = gold.ToString();
            if (gemsText != null) gemsText.text = gems.ToString();
        }

        /// <summary>
        /// Show the $0.99 revive offer panel (Loss Aversion — Var 24).
        /// </summary>
        public void ShowReviveOffer()
        {
            if (reviveOfferPanel != null)
            {
                reviveOfferPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Show the wandering merchant panel with scarcity timer (Var 28).
        /// </summary>
        public void ShowMerchantOffer()
        {
            if (merchantPanel != null)
            {
                merchantPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Open the shop panel.
        /// </summary>
        public void OpenShop()
        {
            Core.GameManager.Instance?.TransitionTo(Core.GameManager.GameState.Shop);
        }

        private void ShowPanel(GameObject panel)
        {
            if (panel != null)
            {
                panel.SetActive(true);
                activePanel = panel;
            }
        }

        private void HideAllPanels()
        {
            if (splashPanel != null) splashPanel.SetActive(false);
            if (loginPanel != null) loginPanel.SetActive(false);
            if (cityHUD != null) cityHUD.SetActive(false);
            if (swarmHUD != null) swarmHUD.SetActive(false);
            if (raidHUD != null) raidHUD.SetActive(false);
            if (shopPanel != null) shopPanel.SetActive(false);
            if (reviveOfferPanel != null) reviveOfferPanel.SetActive(false);
            if (merchantPanel != null) merchantPanel.SetActive(false);
            activePanel = null;
        }
    }
}
