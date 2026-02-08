using UnityEngine;

namespace EmpireOfGlass.Viral
{
    /// <summary>
    /// Manages viral loop mechanics: deep-linked invites and friend bounties (Var 43).
    /// Players can text friends to place bounties on rivals, generating re-engagement.
    /// </summary>
    public class ViralManager : MonoBehaviour
    {
        public static ViralManager Instance { get; private set; }

        [Header("Viral Settings (Var 43)")]
        [SerializeField] private int referralBonusGems = 50;
        [SerializeField] private int bountyRewardGold = 200;
        [SerializeField] private int maxActiveBounties = 5;

        private int activeBountyCount;

        public int ActiveBountyCount => activeBountyCount;

        public event System.Action<string> OnReferralCompleted;
        public event System.Action<string, string> OnBountyPlaced;
        public event System.Action<string> OnBountyCompleted;

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
        /// Generate a deep-linked invite URL for sharing with friends (Var 43).
        /// </summary>
        public string GenerateInviteLink(string referrerId)
        {
            string link = $"empireofglass://invite?ref={referrerId}";
            Debug.Log($"[ViralManager] Invite link generated: {link}");
            return link;
        }

        /// <summary>
        /// Process a referral when a new player joins via deep link.
        /// Grants bonus gems to the referrer (Var 43).
        /// </summary>
        public void ProcessReferral(string referrerId, string newPlayerId)
        {
            var player = Data.SaveManager.Instance?.CurrentPlayer;
            if (player == null) return;

            player.PremiumGems += referralBonusGems;
            Debug.Log($"[ViralManager] Referral completed! {referrerId} earned {referralBonusGems} gems for inviting {newPlayerId}");
            OnReferralCompleted?.Invoke(newPlayerId);
        }

        /// <summary>
        /// Place a bounty on a rival player by texting a friend (Var 43).
        /// </summary>
        public bool PlaceBounty(string targetPlayerId, string friendId)
        {
            if (activeBountyCount >= maxActiveBounties)
            {
                Debug.Log("[ViralManager] Maximum active bounties reached");
                return false;
            }

            activeBountyCount++;
            Debug.Log($"[ViralManager] Bounty placed on {targetPlayerId} via friend {friendId}. Reward: {bountyRewardGold} gold");
            OnBountyPlaced?.Invoke(targetPlayerId, friendId);
            return true;
        }

        /// <summary>
        /// Complete a bounty when a friend successfully raids the target.
        /// </summary>
        public void CompleteBounty(string targetPlayerId)
        {
            if (activeBountyCount <= 0) return;

            var player = Data.SaveManager.Instance?.CurrentPlayer;
            if (player != null)
            {
                player.Gold += bountyRewardGold;
            }

            activeBountyCount--;
            Debug.Log($"[ViralManager] Bounty completed on {targetPlayerId}! +{bountyRewardGold} gold");
            OnBountyCompleted?.Invoke(targetPlayerId);
        }

        /// <summary>
        /// Handle an incoming deep link when the app is opened via a shared URL.
        /// </summary>
        public void HandleDeepLink(string url)
        {
            if (string.IsNullOrEmpty(url)) return;

            Debug.Log($"[ViralManager] Deep link received: {url}");

            if (url.Contains("invite?ref="))
            {
                int refIndex = url.IndexOf("ref=") + 4;
                string referrerId = url.Substring(refIndex);
                Debug.Log($"[ViralManager] Processing invite from referrer: {referrerId}");
            }
        }
    }
}
