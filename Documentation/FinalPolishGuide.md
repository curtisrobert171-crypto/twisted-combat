# Final Polish and Optimization Guide

## Overview
This guide documents the final polish, optimization, and production-readiness features added to Empire of Glass / Twisted Combat to complete the core launch requirements.

## Table of Contents
1. [Progression System](#progression-system)
2. [Object Pooling](#object-pooling)
3. [LOD System](#lod-system)
4. [Juice & Polish](#juice--polish)
5. [Backend Integration](#backend-integration)
6. [CI/CD Pipeline](#cicd-pipeline)
7. [Performance Targets](#performance-targets)

---

## Progression System

### Overview
Comprehensive progression system with leveling, XP, skill trees, and achievements.

### Components
- **ProgressionManager.cs** - Core progression logic
- **ProgressionPanel.cs** - UI for level/XP display
- **SkillTreePanel.cs** - Interactive skill tree interface

### Features

#### Leveling System
- **Base XP**: 100 XP for level 1
- **Scaling**: 1.5x multiplier per level
- **Max Level**: 100
- **Rewards**: Gold and gems per level up
- **Skill Points**: 1 skill point per level

```csharp
// Add XP to player
ProgressionManager.Instance.AddXP(50);

// Check level progress (0-1)
float progress = ProgressionManager.Instance.GetLevelProgress();
```

#### Skill Tree
10 skills across 4 categories:

**Core Skills:**
- Health Boost I/II (20%/40% health)
- Damage Boost I/II (15%/30% damage)

**Swarm Skills:**
- Swarm Multiplier (+10% math gate bonus)
- Energy Efficiency (-20% energy cost)

**Raid Skills:**
- Raid Power (+25% damage)
- Loot Bonus (+20% rewards)

**City Skills:**
- Construction Speed (+30% build speed)
- Gold Generation (+25% gold)

```csharp
// Unlock a skill
bool success = ProgressionManager.Instance.UnlockSkill("health_boost_1");

// Check if skill is unlocked
bool unlocked = ProgressionManager.Instance.IsSkillUnlocked("health_boost_1");
```

#### Achievements
6 achievements with gold/gem rewards:

| Achievement | Requirement | Gold | Gems |
|-------------|-------------|------|------|
| First Steps | Level 5 | 500 | 10 |
| Experienced | Level 10 | 1,000 | 25 |
| Veteran | Level 25 | 5,000 | 100 |
| Raid Master | 50 raids | 2,000 | 50 |
| Swarm Legend | 10,000 score | 3,000 | 75 |
| City Builder | City level 10 | 2,500 | 60 |

```csharp
// Check achievements
ProgressionManager.Instance.CheckAchievements();

// Listen for unlocks
ProgressionManager.Instance.OnAchievementUnlocked += (id) => {
    Debug.Log($"Achievement unlocked: {id}");
};
```

### Integration
Progression data is saved via SaveManager and synced with backend via APIClient.

---

## Object Pooling

### Overview
Generic object pooling system for performance optimization, critical for 500+ shardling swarm.

### Components
- **ObjectPool.cs** - Pooling manager
- **IPooledObject interface** - For objects that need reset logic

### Usage

#### Setup
```csharp
// Register a pool
ObjectPool.Instance.RegisterPool(
    tag: "shardling",
    prefab: shardlingPrefab,
    size: 500,
    expandIfNeeded: true
);
```

#### Spawning
```csharp
// Spawn from pool
GameObject obj = ObjectPool.Instance.SpawnFromPool(
    "shardling",
    position,
    rotation
);
```

#### Returning
```csharp
// Return to pool
ObjectPool.Instance.ReturnToPool("shardling", obj);
```

#### Monitoring
```csharp
// Get pool statistics
var stats = ObjectPool.Instance.GetPoolStats("shardling");
Debug.Log($"Available: {stats.AvailableCount}, Spawned: {stats.TotalSpawnCount}");

// Get active object count
int active = ObjectPool.Instance.GetActiveObjectCount("shardling");
```

### IPooledObject Interface
Implement for objects that need reset logic:

```csharp
public class MyPooledObject : MonoBehaviour, IPooledObject
{
    public void OnObjectSpawn()
    {
        // Reset state when spawned
        health = maxHealth;
        velocity = Vector3.zero;
    }

    public void OnObjectReturn()
    {
        // Clean up before returning to pool
        StopAllCoroutines();
    }
}
```

### Performance Benefits
- **Eliminates** instantiate/destroy garbage collection
- **Reduces** CPU spikes from object creation
- **Enables** 500+ concurrent objects at 60 FPS

---

## LOD System

### Overview
Level of Detail system for dynamic performance optimization based on distance and FPS.

### Components
- **LODSystem.cs** - LOD manager
- **ShardlingLOD.cs** - Example LOD component
- **ILODObject interface** - For LOD-enabled objects

### LOD Levels

| Level | Distance | Features |
|-------|----------|----------|
| High | < 20m | Full detail, particles, trails |
| Medium | 20-50m | Medium detail, particles only |
| Low | 50-100m | Low detail, no effects |
| VeryLow | > 100m | Minimal detail |

### Dynamic Scaling
Automatically adjusts global LOD based on:
- **FPS**: < 40 FPS → Low, < 55 FPS → Medium
- **Object Count**: > 80% max → Low, > 60% max → Medium
- **Update Interval**: 2 seconds

### Usage

#### Register LOD Object
```csharp
public class MyLODObject : MonoBehaviour, ILODObject
{
    public Transform Transform => transform;
    
    public void SetLODLevel(LODSystem.LODLevel level)
    {
        switch (level)
        {
            case LODSystem.LODLevel.High:
                // Enable high detail
                break;
            case LODSystem.LODLevel.Medium:
                // Enable medium detail
                break;
            case LODSystem.LODLevel.Low:
                // Enable low detail
                break;
        }
    }
}
```

#### Manual Control
```csharp
// Force specific LOD level
LODSystem.Instance.SetGlobalLOD(LODSystem.LODLevel.Medium);

// Re-enable dynamic scaling
LODSystem.Instance.EnableDynamicLOD();

// Get current level
var level = LODSystem.Instance.GetGlobalLOD();
```

### ShardlingLOD Implementation
Automatically switches between:
- High/medium/low detail models
- Particle effects on/off
- Trail renderers on/off

---

## Juice & Polish

### Overview
Visual and tactile feedback system for enhanced game feel.

### Components
- **JuiceManager.cs** - Juice effects manager

### Effects

#### Screen Shake
```csharp
// Custom intensity shake
JuiceManager.Instance.ScreenShake(1.5f);

// Preset intensities
JuiceManager.Instance.LightShake();   // 0.3x
JuiceManager.Instance.MediumShake();  // 0.6x
JuiceManager.Instance.HeavyShake();   // 1.5x
```

#### Hit Pause
Brief slow-motion for impactful moments:
```csharp
JuiceManager.Instance.HitPause();
// Slows time to 0.1x for 0.05 seconds
```

#### Impact Scale
Punch in/out effect for objects:
```csharp
JuiceManager.Instance.ImpactScale(transform);
// Scales to 1.2x and back over 0.15s
```

#### Combined Impact
Full impact feedback:
```csharp
JuiceManager.Instance.ImpactEffect(
    target: bossTransform,
    shakeMultiplier: 2f
);
// Combines shake + hit pause + scale + haptics
```

#### UI Pulse
```csharp
JuiceManager.Instance.PulseUI(buttonTransform);
// Subtle sine-wave pulse for UI feedback
```

### When to Use

| Event | Effect | Intensity |
|-------|--------|-----------|
| Math gate hit | Screen shake + audio | Light |
| Boss hit | Impact effect | Heavy |
| Level up | UI pulse + audio | Medium |
| Hero death | Screen shake + hit pause | Heavy |
| Button press | UI pulse | Light |
| Raid victory | Screen shake + audio | Medium |

---

## Backend Integration

### Overview
Full REST API client for Google Cloud Run backend integration.

### Components
- **APIClient.cs** - Complete backend client

### Features

#### Authentication
```csharp
// Login
APIClient.Instance.Authenticate(email, password, (success) => {
    if (success) {
        Debug.Log("Authenticated!");
    }
});

// Register
APIClient.Instance.Register(email, password, displayName, (success) => {
    // Handle registration result
});

// Logout
APIClient.Instance.Logout();

// Check status
bool authenticated = APIClient.Instance.IsAuthenticated;
```

#### Cloud Save
```csharp
// Save to cloud
string json = SaveManager.Instance.CurrentPlayer.ToJson();
APIClient.Instance.SaveToCloud(json, (success) => {
    Debug.Log($"Cloud save: {success}");
});

// Load from cloud
APIClient.Instance.LoadFromCloud((playerDataJson) => {
    if (playerDataJson != null) {
        var player = PlayerData.FromJson(playerDataJson);
        // Apply loaded data
    }
});
```

#### Leaderboards
```csharp
// Submit score
APIClient.Instance.SubmitScore("swarm_highscore", 12500, (success) => {
    // Handle result
});

// Get leaderboard
APIClient.Instance.GetLeaderboard("swarm_highscore", 50, (entries) => {
    foreach (var entry in entries) {
        Debug.Log($"{entry.rank}. {entry.displayName}: {entry.score}");
    }
});
```

#### PvP Matchmaking
```csharp
// Find opponent
APIClient.Instance.FindOpponent(playerLevel, (opponent) => {
    if (opponent != null) {
        Debug.Log($"Matched with {opponent.displayName}");
        // Load opponent's base layout
    }
});

// Submit raid result
APIClient.Instance.SubmitRaidResult(
    opponentId,
    lootGained: 5000,
    victory: true,
    (success) => { /* Handle result */ }
);
```

### Error Handling
```csharp
APIClient.Instance.OnError += (errorMessage) => {
    Debug.LogError($"API Error: {errorMessage}");
    // Show error UI
};

APIClient.Instance.OnAuthenticationChanged += (isAuth) => {
    Debug.Log($"Auth status: {isAuth}");
    // Update UI
};
```

### Configuration
Set in Unity Inspector:
- **Base URL**: `https://api.empireofglass.com/v1`
- **Request Timeout**: 10 seconds
- **Max Retries**: 3

---

## CI/CD Pipeline

### Overview
Automated testing and build pipeline via GitHub Actions.

### Workflow: `unity-tests.yml`

#### Jobs

**1. EditMode Tests**
- Runs unit tests in edit mode
- ~30 minute timeout
- Uploads test results and coverage

**2. PlayMode Tests**
- Runs integration tests in play mode
- ~45 minute timeout
- Uploads test results

**3. WebGL Build**
- Builds WebGL after tests pass
- Only on main/develop branches
- ~60 minute timeout
- Artifacts retained 7 days

**4. Code Quality Check**
- Runs dotnet-format
- Static analysis preparation
- 15 minute timeout

**5. Test Results Summary**
- Aggregates all test results
- Publishes unified report

### Triggers
- **Push**: main, develop, copilot/* branches
- **Pull Request**: main, develop
- **Manual**: workflow_dispatch

### Required Secrets
Set in GitHub repository settings:
- `UNITY_LICENSE` - Unity license file
- `UNITY_EMAIL` - Unity account email
- `UNITY_PASSWORD` - Unity account password

### Local Testing
```bash
# Run EditMode tests
Unity -runTests -batchmode -projectPath . -testPlatform editmode

# Run PlayMode tests
Unity -runTests -batchmode -projectPath . -testPlatform playmode
```

---

## Performance Targets

### Desktop (GTX 1060+)
- **FPS**: 60 FPS sustained
- **Swarm Count**: 500+ shardlings
- **Memory**: < 2GB
- **Load Time**: < 5 seconds

### Mobile (iPhone 12+)
- **FPS**: 30-60 FPS (adaptive)
- **Swarm Count**: 300+ shardlings
- **Memory**: < 512MB
- **Battery**: < 10% per hour

### WebGL
- **FPS**: 30-60 FPS (browser dependent)
- **Swarm Count**: 200-500 (adaptive)
- **Memory**: < 1GB
- **Load Time**: < 10 seconds

### Optimization Techniques
1. **Object Pooling** - Eliminates GC spikes
2. **LOD System** - Reduces rendering overhead
3. **GPU Instancing** - Batch render similar objects
4. **Occlusion Culling** - Skip rendering hidden objects
5. **Dynamic Scaling** - Adjust quality for performance
6. **Spatial Partitioning** - Optimize collision detection
7. **Shader Optimization** - Minimize pixel operations

### Profiling
Use Unity Profiler to monitor:
- **CPU**: < 16ms per frame (60 FPS)
- **Rendering**: < 8ms per frame
- **Scripts**: < 4ms per frame
- **Physics**: < 2ms per frame
- **GC Alloc**: < 1KB per frame

### Performance Monitoring
```csharp
// Check current FPS
float fps = Performance.PerformanceMonitor.Instance.CurrentFPS;

// Get performance stats
var stats = Performance.PerformanceMonitor.Instance.GetStats();
Debug.Log($"FPS: {stats.averageFPS}, Memory: {stats.memoryUsed}MB");
```

---

## Testing the Systems

### Progression
```csharp
// Test in-game
ProgressionManager.Instance.AddXP(1000);
ProgressionManager.Instance.UnlockSkill("health_boost_1");
ProgressionManager.Instance.CheckAchievements();
```

### Object Pooling
```csharp
// Spawn 500 objects
for (int i = 0; i < 500; i++) {
    ObjectPool.Instance.SpawnFromPool("shardling", 
        Random.insideUnitSphere * 50f, 
        Quaternion.identity);
}
```

### LOD System
```csharp
// Fly camera around to see LOD transitions
// Press F3 to see FPS overlay
// Watch objects switch detail levels
```

### Juice Effects
```csharp
// Test all effects
JuiceManager.Instance.HeavyShake();
JuiceManager.Instance.HitPause();
JuiceManager.Instance.ImpactEffect(transform, 2f);
```

### Backend Integration
```csharp
// Test authentication flow
APIClient.Instance.Authenticate("test@example.com", "password123", 
    (success) => Debug.Log($"Auth: {success}"));
```

---

## Next Steps

### Before Launch
1. ✅ Implement all core systems
2. ⏳ Set up Google Cloud backend
3. ⏳ Add final art assets
4. ⏳ Add particle effects
5. ⏳ Add animation controllers
6. ⏳ Conduct full playtest
7. ⏳ QA pass on all platforms
8. ⏳ Performance profiling
9. ⏳ Security audit
10. ⏳ Deploy to production

### Post-Launch
1. Monitor analytics
2. Track crash reports
3. Gather player feedback
4. Balance game difficulty
5. Plan content updates
6. Optimize based on metrics

---

## Summary

This implementation completes the core launch requirements:

✅ **Polished Systems**: Juice effects, shaders, audio integration  
✅ **Robust Testing**: CI/CD pipeline, 20+ unit tests, integration tests  
✅ **Progression**: Leveling, skill tree, achievements  
✅ **Backend**: Full API client, authentication, cloud save, leaderboards  
✅ **Optimization**: Object pooling, LOD system, dynamic scaling  

**Status**: ~85% launch-ready. Remaining: Backend deployment, final art, QA pass.

---

*Last Updated: 2026-02-10*  
*Version: 0.3.0-alpha*
