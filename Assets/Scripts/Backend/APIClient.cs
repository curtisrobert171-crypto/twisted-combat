using UnityEngine;

namespace EmpireOfGlass.Backend
{
    /// <summary>
    /// Placeholder API client for backend integration.
    /// To be implemented with actual REST API calls in Phase 5.
    /// See Documentation/BackendIntegration.md for full implementation.
    /// </summary>
    public class APIClient : MonoBehaviour
    {
        private const string BASE_URL = "https://api.empireofglass.com/v1";
        private string sessionToken;

        public void SetSessionToken(string token)
        {
            sessionToken = token;
        }

        public string GetSessionToken()
        {
            return sessionToken;
        }
    }
}
