# Backend Integration Guide

## Overview
This document outlines the backend architecture for Empire of Glass, including leaderboards, PvP matchmaking, and cloud save functionality.

## Architecture

### Backend Stack
```
Unity Client ←→ REST API ←→ Cloud Run (Python/Node.js) ←→ Database
                                ↓
                         Cloud Storage (Assets)
                                ↓
                         Analytics Pipeline
```

### Recommended Services
- **Compute**: Google Cloud Run (auto-scaling, serverless)
- **Database**: Cloud Firestore (NoSQL, real-time)
- **Storage**: Cloud Storage (asset bundles, replays)
- **Analytics**: BigQuery (data warehouse)
- **Auth**: Firebase Authentication

## API Design

### Authentication
```
POST /api/v1/auth/login
Request:
{
  "device_id": "uuid",
  "platform": "ios" | "android" | "webgl",
  "version": "1.0.0"
}

Response:
{
  "user_id": "user_12345",
  "session_token": "jwt_token",
  "player_data": { ... }
}
```

### Player Data
```
GET /api/v1/player/{user_id}
Headers:
  Authorization: Bearer {session_token}

Response:
{
  "user_id": "user_12345",
  "display_name": "Player123",
  "level": 15,
  "currencies": {
    "gems": 1000,
    "coins": 5000
  },
  "stats": {
    "loops_completed": 45,
    "bosses_defeated": 120,
    "total_playtime": 3600
  },
  "base_layout": [ ... ],
  "inventory": [ ... ]
}

POST /api/v1/player/{user_id}/save
Request:
{
  "player_data": { ... },
  "checksum": "hash_of_data"
}

Response:
{
  "success": true,
  "saved_at": "2026-02-08T10:00:00Z"
}
```

### Leaderboards
```
GET /api/v1/leaderboard/{category}?limit=100&offset=0
Categories: loops_completed | bosses_defeated | total_score

Response:
{
  "leaderboard": [
    {
      "rank": 1,
      "user_id": "user_12345",
      "display_name": "TopPlayer",
      "score": 99999,
      "updated_at": "2026-02-08T09:00:00Z"
    },
    ...
  ],
  "user_rank": 42,
  "total_entries": 10000
}

POST /api/v1/leaderboard/submit
Request:
{
  "category": "loops_completed",
  "score": 50,
  "proof": "gameplay_hash"
}

Response:
{
  "new_rank": 35,
  "previous_rank": 42,
  "improved": true
}
```

### PvP / Raid System
```
GET /api/v1/raid/find-target?user_id={user_id}&energy={energy}
Response:
{
  "target_user_id": "user_67890",
  "display_name": "Enemy123",
  "base_level": 10,
  "base_layout": [ ... ],
  "potential_loot": {
    "coins": 500,
    "gems": 50
  },
  "defense_power": 75
}

POST /api/v1/raid/complete
Request:
{
  "attacker_id": "user_12345",
  "target_id": "user_67890",
  "result": "victory" | "defeat",
  "damage_dealt": 85,
  "loot_claimed": {
    "coins": 400,
    "gems": 30
  },
  "replay_data": "base64_encoded"
}

Response:
{
  "success": true,
  "revenge_available": true,
  "new_rating": 1250
}
```

### Live Events
```
GET /api/v1/events/active
Response:
{
  "events": [
    {
      "event_id": "dark_mode_weekend",
      "name": "Dark Mode Challenge",
      "description": "72-hour high difficulty event",
      "starts_at": "2026-02-10T00:00:00Z",
      "ends_at": "2026-02-13T00:00:00Z",
      "rewards": [ ... ],
      "difficulty_multiplier": 2.0
    }
  ]
}

POST /api/v1/events/{event_id}/participate
Request:
{
  "user_id": "user_12345"
}

Response:
{
  "enrolled": true,
  "event_token": "token_xyz"
}
```

## Unity Client Implementation

### API Client
```csharp
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text;

namespace EmpireOfGlass.Backend
{
    public class APIClient : MonoBehaviour
    {
        private const string BASE_URL = "https://api.empireofglass.com/v1";
        private string sessionToken;

        public IEnumerator Login(string deviceId, Action<PlayerData> onSuccess, Action<string> onError)
        {
            var url = $"{BASE_URL}/auth/login";
            var request = new UnityWebRequest(url, "POST");
            
            var jsonData = JsonUtility.ToJson(new LoginRequest
            {
                device_id = deviceId,
                platform = Application.platform.ToString(),
                version = Application.version
            });
            
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
                sessionToken = response.session_token;
                onSuccess?.Invoke(response.player_data);
            }
            else
            {
                onError?.Invoke(request.error);
            }
        }

        public IEnumerator GetLeaderboard(string category, Action<LeaderboardData> onSuccess, Action<string> onError)
        {
            var url = $"{BASE_URL}/leaderboard/{category}?limit=100";
            var request = UnityWebRequest.Get(url);
            request.SetRequestHeader("Authorization", $"Bearer {sessionToken}");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                var data = JsonUtility.FromJson<LeaderboardData>(request.downloadHandler.text);
                onSuccess?.Invoke(data);
            }
            else
            {
                onError?.Invoke(request.error);
            }
        }

        public IEnumerator SubmitScore(string category, int score, Action<RankUpdate> onSuccess, Action<string> onError)
        {
            var url = $"{BASE_URL}/leaderboard/submit";
            var request = new UnityWebRequest(url, "POST");
            
            var jsonData = JsonUtility.ToJson(new ScoreSubmission
            {
                category = category,
                score = score,
                proof = GenerateProof(score)
            });
            
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {sessionToken}");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<RankUpdate>(request.downloadHandler.text);
                onSuccess?.Invoke(response);
            }
            else
            {
                onError?.Invoke(request.error);
            }
        }

        private string GenerateProof(int score)
        {
            // TODO: Implement anti-cheat proof generation
            return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{score}_{sessionToken}"));
        }
    }

    [Serializable]
    public class LoginRequest
    {
        public string device_id;
        public string platform;
        public string version;
    }

    [Serializable]
    public class LoginResponse
    {
        public string user_id;
        public string session_token;
        public PlayerData player_data;
    }

    [Serializable]
    public class LeaderboardData
    {
        public LeaderboardEntry[] leaderboard;
        public int user_rank;
        public int total_entries;
    }

    [Serializable]
    public class LeaderboardEntry
    {
        public int rank;
        public string user_id;
        public string display_name;
        public int score;
        public string updated_at;
    }

    [Serializable]
    public class ScoreSubmission
    {
        public string category;
        public int score;
        public string proof;
    }

    [Serializable]
    public class RankUpdate
    {
        public int new_rank;
        public int previous_rank;
        public bool improved;
    }
}
```

### Backend Manager
```csharp
using UnityEngine;
using System;

namespace EmpireOfGlass.Backend
{
    public class BackendManager : MonoBehaviour
    {
        public static BackendManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool useBackend = true;
        [SerializeField] private float autoSaveInterval = 300f; // 5 minutes

        private APIClient apiClient;
        private string userId;
        private float nextAutoSave;

        public bool IsConnected { get; private set; }
        public string UserId => userId;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            apiClient = gameObject.AddComponent<APIClient>();
        }

        private void Start()
        {
            if (useBackend)
            {
                InitializeBackend();
            }
        }

        private void Update()
        {
            if (useBackend && IsConnected && Time.time >= nextAutoSave)
            {
                SavePlayerData();
                nextAutoSave = Time.time + autoSaveInterval;
            }
        }

        private void InitializeBackend()
        {
            string deviceId = SystemInfo.deviceUniqueIdentifier;
            StartCoroutine(apiClient.Login(
                deviceId,
                OnLoginSuccess,
                OnLoginError
            ));
        }

        private void OnLoginSuccess(Data.PlayerData playerData)
        {
            userId = playerData.userId;
            IsConnected = true;
            nextAutoSave = Time.time + autoSaveInterval;
            
            Debug.Log($"[BackendManager] Connected as {userId}");
            
            // Load player data
            Data.SaveManager.Instance.LoadPlayerData(playerData);
        }

        private void OnLoginError(string error)
        {
            IsConnected = false;
            Debug.LogError($"[BackendManager] Login failed: {error}");
            
            // Fallback to local save
            Data.SaveManager.Instance.LoadLocal();
        }

        public void SavePlayerData()
        {
            if (!IsConnected) return;

            var playerData = Data.SaveManager.Instance.GetCurrentPlayerData();
            // TODO: Implement save API call
            Debug.Log("[BackendManager] Auto-saving player data...");
        }

        public void LoadLeaderboard(string category, Action<LeaderboardData> callback)
        {
            if (!IsConnected)
            {
                callback?.Invoke(null);
                return;
            }

            StartCoroutine(apiClient.GetLeaderboard(
                category,
                callback,
                error => Debug.LogError($"Leaderboard error: {error}")
            ));
        }

        public void SubmitScore(string category, int score, Action<RankUpdate> callback)
        {
            if (!IsConnected)
            {
                callback?.Invoke(null);
                return;
            }

            StartCoroutine(apiClient.SubmitScore(
                category,
                score,
                callback,
                error => Debug.LogError($"Score submission error: {error}")
            ));
        }
    }
}
```

## Anti-Cheat Measures

### Server-Side Validation
```python
# Cloud Run endpoint example
from flask import Flask, request, jsonify
import hashlib
import time

app = Flask(__name__)

@app.route('/api/v1/leaderboard/submit', methods=['POST'])
def submit_score():
    data = request.json
    user_id = get_user_id_from_token(request.headers['Authorization'])
    
    # Validate score is possible
    if not is_score_valid(data['score'], user_id):
        return jsonify({'error': 'Invalid score'}), 400
    
    # Validate proof
    if not verify_proof(data['proof'], data['score'], user_id):
        return jsonify({'error': 'Invalid proof'}), 400
    
    # Update leaderboard
    new_rank = update_leaderboard(user_id, data['category'], data['score'])
    
    return jsonify({
        'new_rank': new_rank,
        'previous_rank': get_previous_rank(user_id, data['category']),
        'improved': True
    })

def is_score_valid(score, user_id):
    # Check if score is within reasonable bounds
    player_data = get_player_data(user_id)
    max_possible_score = calculate_max_score(player_data)
    return score <= max_possible_score

def verify_proof(proof, score, user_id):
    # Verify cryptographic proof
    expected_proof = generate_proof_server_side(score, user_id)
    return proof == expected_proof
```

### Rate Limiting
```python
from functools import wraps
from flask import request
import redis

redis_client = redis.Redis()

def rate_limit(max_requests=10, window=60):
    def decorator(f):
        @wraps(f)
        def wrapped(*args, **kwargs):
            user_id = get_user_id_from_request()
            key = f"rate_limit:{user_id}:{f.__name__}"
            
            current = redis_client.get(key)
            if current and int(current) >= max_requests:
                return jsonify({'error': 'Rate limit exceeded'}), 429
            
            redis_client.incr(key)
            redis_client.expire(key, window)
            
            return f(*args, **kwargs)
        return wrapped
    return decorator

@app.route('/api/v1/raid/complete', methods=['POST'])
@rate_limit(max_requests=5, window=60)  # 5 raids per minute max
def complete_raid():
    # ...
    pass
```

## Data Schema

### Player Document (Firestore)
```javascript
{
  "user_id": "user_12345",
  "display_name": "Player123",
  "created_at": "2026-02-01T00:00:00Z",
  "last_login": "2026-02-08T10:00:00Z",
  "level": 15,
  "xp": 5000,
  "currencies": {
    "gems": 1000,
    "coins": 5000,
    "energy": 100
  },
  "stats": {
    "loops_completed": 45,
    "bosses_defeated": 120,
    "math_gates_hit": 500,
    "total_playtime": 3600,
    "deaths": 25
  },
  "base_layout": [
    { "x": 0, "y": 0, "building_type": "completed", "level": 3 },
    { "x": 1, "y": 0, "building_type": "construction", "level": 1 }
  ],
  "inventory": [
    { "item_id": "hero_skin_01", "quantity": 1 },
    { "item_id": "boost_x2", "quantity": 5 }
  ],
  "settings": {
    "music_volume": 0.8,
    "sfx_volume": 1.0,
    "analytics_enabled": true
  }
}
```

### Leaderboard Document
```javascript
{
  "leaderboard_id": "loops_completed_weekly",
  "category": "loops_completed",
  "period": "weekly",
  "start_date": "2026-02-08",
  "end_date": "2026-02-15",
  "entries": [
    {
      "user_id": "user_12345",
      "display_name": "Player123",
      "score": 50,
      "rank": 1,
      "updated_at": "2026-02-08T10:00:00Z"
    }
  ]
}
```

## Deployment

### Cloud Run Deployment
```bash
# Build Docker container
docker build -t gcr.io/empire-of-glass/api:v1 .

# Push to Container Registry
docker push gcr.io/empire-of-glass/api:v1

# Deploy to Cloud Run
gcloud run deploy empire-api \
  --image gcr.io/empire-of-glass/api:v1 \
  --platform managed \
  --region us-central1 \
  --allow-unauthenticated \
  --set-env-vars="DATABASE_URL=firestore://..."
```

### Environment Variables
```
DATABASE_URL=firestore://project-id
REDIS_URL=redis://cache.example.com:6379
JWT_SECRET=your_secret_key
API_VERSION=v1
LOG_LEVEL=info
```

## Testing

### Local Development
```bash
# Run local API server
python api_server.py

# Test endpoints
curl -X POST http://localhost:8080/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"device_id": "test_device"}'
```

### Integration Tests
```csharp
[UnityTest]
public IEnumerator BackendManager_CanConnect()
{
    var manager = new GameObject().AddComponent<BackendManager>();
    yield return new WaitForSeconds(2f);
    Assert.IsTrue(manager.IsConnected);
}
```

## Security Checklist

- [ ] Use HTTPS for all API calls
- [ ] Implement JWT authentication
- [ ] Validate all client inputs server-side
- [ ] Use rate limiting on all endpoints
- [ ] Encrypt sensitive player data
- [ ] Implement anti-cheat validation
- [ ] Log security events
- [ ] Regular security audits
- [ ] GDPR compliance (data export/deletion)
- [ ] Regular backup of player data

## Monitoring

### Metrics to Track
- API response times (P50, P95, P99)
- Error rates per endpoint
- Active users (DAU, MAU)
- Database query performance
- Cloud Run instance scaling
- Cost per user

### Alerts
- High error rate (>5%)
- Slow API responses (>500ms)
- Database connection issues
- High costs
- Security events

## Next Steps

1. Set up Google Cloud project
2. Deploy backend API
3. Implement Unity API client
4. Test authentication flow
5. Implement save/load
6. Add leaderboard UI
7. Test PvP matchmaking
8. Launch beta with backend
