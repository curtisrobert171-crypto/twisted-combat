# Empire of Glass - Quick Code Reference

**Fast lookup guide for developers** - Shows key functions and file locations

---

## File Structure

```
Assets/
├── Scenes/                          # Unity scenes (4 total)
│   ├── Boot.unity                   # Entry point with navigation
│   ├── SwarmPrototype.unity         # Swarm runner gameplay
│   ├── CityPrototype.unity          # City building meta-game
│   └── RaidPrototype.unity          # Raid/PvP loop
│
├── Scripts/                         # C# game code (18 files)
│   ├── Core/                        # Core game systems
│   │   ├── GameManager.cs           # ⭐ State machine & loop rotation
│   │   ├── GameBootstrap.cs         # System initialization
│   │   ├── BootSceneController.cs   # Scene navigation
│   │   ├── HeroController.cs        # Player character (lane runner)
│   │   ├── CameraController.cs      # 3 camera perspectives
│   │   ├── AudioManager.cs          # Adaptive music & SFX
│   │   ├── HapticManager.cs         # Vibration feedback
│   │   └── LevelGenerator.cs        # Procedural track generation
│   │
│   ├── Swarm/                       # Swarm gameplay
│   │   ├── SwarmController.cs       # ⭐ Core swarm manager (500+ units)
│   │   ├── MathGate.cs              # x2, x5, +10 gates
│   │   ├── ShardlingBehavior.cs     # Individual unit AI (flocking)
│   │   ├── BossController.cs        # End boss with HP wall
│   │   └── ObstacleBarrier.cs       # Obstacles that damage swarm
│   │
│   ├── City/                        # City building
│   │   └── CityBuilder.cs           # ⭐ Grid system with building states
│   │
│   ├── Raid/                        # Raid/PvP
│   │   └── RaidManager.cs           # ⭐ Raid loop with loot calculation
│   │
│   ├── Data/                        # Save/Load system
│   │   ├── PlayerData.cs            # ⭐ JSON data model (all player state)
│   │   └── SaveManager.cs           # Local save with offline rewards
│   │
│   ├── Monetization/                # IAP & monetization
│   │   └── MonetizationManager.cs   # ⭐ 12 psychological systems
│   │
│   └── UI/                          # User interface
│       └── UIManager.cs             # Panel management & HUD updates
│
└── Prefabs/                         # Game objects (9 placeholders)
    └── Prototype/                   # Unity primitive placeholders
        ├── Hero/
        │   ├── Hero.prefab          # Capsule (player)
        │   ├── Shardling.prefab     # Sphere (swarm unit)
        │   └── Boss.prefab          # Cube (end boss)
        ├── Environment/
        │   ├── MathGate_x2.prefab   # Green trigger cube
        │   ├── MathGate_x5.prefab   # Yellow trigger cube
        │   └── ObstacleBarrier.prefab # Gray solid cube
        └── City/
            ├── Building_Ruin.prefab        # Short cube (damaged)
            ├── Building_Construction.prefab # Medium cube (building)
            └── Building_Completed.prefab    # Tall cube (finished)
```

---

## Core Game Loop

### GameManager State Flow
```csharp
// Location: Assets/Scripts/Core/GameManager.cs

public enum GameState 
{ 
    Splash, Login, City, Swarm, Raid, Shop, Paused 
}

// Forced rotation: Swarm → City → Raid → Swarm (endless)
public void AdvanceLoop()
{
    switch (currentState)
    {
        case GameState.Swarm: TransitionTo(GameState.City); break;
        case GameState.City:  TransitionTo(GameState.Raid); break;
        case GameState.Raid:  TransitionTo(GameState.Swarm); break;
    }
}
```

**Usage**:
```csharp
GameManager.Instance.TransitionTo(GameManager.GameState.Swarm);
GameManager.Instance.AdvanceLoop();  // Move to next state in rotation
```

---

## Swarm System (Core Mechanic)

### SwarmController - Main API
```csharp
// Location: Assets/Scripts/Swarm/SwarmController.cs

// ⭐ CORE MATH FORMULA: (CurrentUnits * GateValue) - EnemyHP

// Initialize swarm at start of run
public void InitializeSwarm()

// Apply math gate (x2, x5, +10)
public void ApplyMathGate(MathGate.GateOperation operation, int value)
// Examples:
//   ApplyMathGate(Multiply, 2)  → doubles swarm size
//   ApplyMathGate(Multiply, 5)  → 5x swarm size
//   ApplyMathGate(Add, 10)      → adds 10 shardlings
//   ApplyMathGate(Subtract, 10) → removes 10 (for obstacles)

// Calculate damage against boss/wall
public int CalculateSwarmDamage(int enemyHP)
// Returns: total damage dealt
// Side effect: removes shardlings equal to enemyHP

// Get raid energy (for next loop)
public int GetRaidEnergy() => activeShardlings.Count * 10

// Current swarm size
public int ShardlingCount { get; }

// Event when size changes
public event System.Action<int> OnSwarmSizeChanged
```

**Example Flow**:
```csharp
SwarmController swarm = FindObjectOfType<SwarmController>();

// Start run with 1 shardling
swarm.InitializeSwarm();  // Count = 1

// Pass through x5 gate
swarm.ApplyMathGate(MathGate.GateOperation.Multiply, 5);  // Count = 5

// Pass through x2 gate
swarm.ApplyMathGate(MathGate.GateOperation.Multiply, 2);  // Count = 10

// Hit obstacle (loses 3)
swarm.ApplyMathGate(MathGate.GateOperation.Subtract, 3);  // Count = 7

// Attack boss with 50 HP
int damage = swarm.CalculateSwarmDamage(50);  // damage = 7, Count = 0 (all consumed)

// Convert remaining swarm to raid energy
int energy = swarm.GetRaidEnergy();  // energy = 0 (swarm depleted)
```

---

## City System

### CityBuilder - Grid Management
```csharp
// Location: Assets/Scripts/City/CityBuilder.cs

// Grid size (default 10x10)
[SerializeField] private int gridWidth = 10;
[SerializeField] private int gridHeight = 10;

// Place a new building
public bool PlaceBuilding(int gridX, int gridY, BuildingType type, int goldCost)
// Returns: true if placed successfully
// Types: Residential, Defense, Resource, Vault, MegaStructure

// Complete construction (triggers reverse-time animation)
public void CompleteBuilding(int gridX, int gridY)

// Damage building during raid
public void DamageBuilding(int gridX, int gridY)

// Check grid cell state
public BuildingState GetCellState(int gridX, int gridY)
// States: Empty, Ruin, Construction, Completed

// Events
public event System.Action<CityBuilding> OnBuildingPlaced
public event System.Action<CityBuilding> OnBuildingUpgraded
```

**Example Usage**:
```csharp
CityBuilder city = FindObjectOfType<CityBuilder>();

// Place a defense tower at grid position (5, 5)
bool placed = city.PlaceBuilding(5, 5, BuildingType.Defense, goldCost: 100);

if (placed)
{
    // Complete construction after timer
    city.CompleteBuilding(5, 5);
}

// During raid, damage buildings
city.DamageBuilding(5, 5);  // Reduces to Ruin state
```

---

## Raid System

### RaidManager - Frequency Puzzle
```csharp
// Location: Assets/Scripts/Raid/RaidManager.cs

// Start raid with energy from swarm
public void StartRaid(int energy)
// Starts 60-second timer
// Generates random target frequency (0.2 - 0.8)

// Player fires frequency beam
public void FireFrequencyBeam(float playerFrequency)
// precision = 1.0 - |playerFrequency - targetFrequency|
// Higher precision = better loot tier (0-5)

// Calculate loot rewards
public RaidResult CalculateLoot(float precision, bool isRevenge = false)
// Returns: { LootTier, Gold, Shards, Precision, WasRevenge }
// Formula: 
//   lootTier = floor(precision * 5)  // 0-5
//   gold = baseLootGold * (1 + lootTier) * revengeMultiplier
//   shards = baseLootShards * lootTier * revengeMultiplier
//   revengeMultiplier = 2.0 if revenge, else 1.0

// Properties
public bool IsRaidActive { get; }
public int CurrentEnergy { get; }

// Event
public event System.Action<RaidResult> OnRaidComplete
```

**Example Raid Flow**:
```csharp
RaidManager raid = FindObjectOfType<RaidManager>();

// 1. Get energy from swarm run
SwarmController swarm = FindObjectOfType<SwarmController>();
int energy = swarm.GetRaidEnergy();  // e.g., 500 shardlings * 10 = 5000 energy

// 2. Start raid
raid.StartRaid(energy);

// 3. Player adjusts frequency (0.0 - 1.0) and fires
float playerFrequency = 0.65f;  // Player input
raid.FireFrequencyBeam(playerFrequency);

// 4. Raid ends, loot calculated
// If target was 0.62:
//   precision = 1.0 - |0.65 - 0.62| = 0.97
//   lootTier = floor(0.97 * 5) = 4
//   gold = 50 * (1 + 4) * 1.0 = 250
//   shards = 5 * 4 * 1.0 = 20
```

---

## Data & Save System

### PlayerData - JSON Schema
```csharp
// Location: Assets/Scripts/Data/PlayerData.cs

[Serializable]
public class PlayerData
{
    // Identity
    public string UserID;
    public string DisplayName;
    public int Level;
    
    // Currencies
    public int Gold;              // Main currency
    public int PremiumGems;       // IAP currency
    public int RaidEnergy;        // Raid attempts
    public int ShieldCount;       // Defense shields
    
    // Progression
    public int SwarmHighScore;    // Max shardlings achieved
    public int RaidsCompleted;    // Total raids done
    public int CitySizeLevel;     // City expansion tier
    
    // Monetization
    public int PiggyBankGems;     // Accumulated (visible)
    public bool PiggyBankBroken;  // Already purchased?
    public int BattlePassTier;    // 0-50
    public bool BattlePassPremium;
    public int VIPLevel;          // 0-5
    
    // City Layout
    public int[][] BaseLayout;    // 2D grid for defense
    
    // Inventory
    public List<InventoryItem> Inventory;
    
    // Offline
    public long LastOfflineTimestamp;
    public float OfflineAccumulatedGold;
}

// Create new player
public static PlayerData CreateNew(string userId)

// Serialize/deserialize
public string ToJson()
public static PlayerData FromJson(string json)

// Calculate offline rewards (capped at 10 hours)
public float CalculateOfflineRewards(float goldPerSecond, float maxOfflineHours = 10f)
```

### SaveManager - Save/Load
```csharp
// Location: Assets/Scripts/Data/SaveManager.cs

// Singleton access
SaveManager.Instance

// Load player data (or create new)
public PlayerData LoadPlayerData()

// Save current state
public void SavePlayerData()

// Claim offline rewards on return
public float ClaimOfflineRewards(float goldPerSecond)

// Current player
public PlayerData CurrentPlayer { get; }
```

**Usage**:
```csharp
// Load on startup (done in GameBootstrap)
SaveManager save = SaveManager.Instance;
PlayerData player = save.LoadPlayerData();

// Modify player data
player.Gold += 100;
player.Level++;

// Save
save.SavePlayerData();

// Claim offline rewards
float offlineGold = save.ClaimOfflineRewards(goldPerSecond: 0.5f);
// If player was away for 5 hours: 5 * 3600 * 0.5 = 9000 gold
```

---

## Monetization System

### MonetizationManager - IAP Hooks
```csharp
// Location: Assets/Scripts/Monetization/MonetizationManager.cs

// Piggy Bank (accumulate → pay $4.99 to claim)
public void AddToPiggyBank(int gems)
public int BreakPiggyBank()  // Returns gems claimed

// Starter Pack ($0.99 after first defeat)
public void PurchaseStarterPack()  // Adds 500 gold + 50 gems

// Scarcity Timer (wandering merchant for 15 min)
public void ActivateWanderingMerchant()
public float GetMerchantTimeRemaining()
public bool IsMerchantActive()

// Battle Pass (0-50 tiers)
public void AwardBattlePassXP(int xp)

// Loss Aversion (revive offer)
public bool ShouldOfferRevive(float progressPercent)
// Returns true if progress >= 80%

// Events
public event System.Action OnPiggyBankBroken
public event System.Action OnStarterPackPurchased
public event System.Action<float> OnMerchantTimerUpdate
public event System.Action<int> OnBattlePassTierUp
```

**Monetization Flow Examples**:

```csharp
MonetizationManager monetization = MonetizationManager.Instance;

// 1. Piggy Bank
// Player earns gems through gameplay
monetization.AddToPiggyBank(5);  // Shows visibly in glass vault UI
// ... after accumulating 100+ gems
int claimed = monetization.BreakPiggyBank();  // Triggers $4.99 IAP

// 2. Loss Aversion (Revive Offer)
HeroController hero = FindObjectOfType<HeroController>();
hero.OnHeroDeath += () =>
{
    float progress = hero.transform.position.z / 200f;  // % of track completed
    if (monetization.ShouldOfferRevive(progress))
    {
        UIManager.Instance.ShowReviveOffer();  // Show $0.99 revive or ad
    }
};

// 3. Scarcity Timer
monetization.ActivateWanderingMerchant();
// UI shows 15:00 countdown
// ... player makes purchase within time limit

// 4. Battle Pass
// Award XP after completing swarm run
monetization.AwardBattlePassXP(50);
```

---

## UI System

### UIManager - Panel Management
```csharp
// Location: Assets/Scripts/UI/UIManager.cs

// Singleton access
UIManager.Instance

// Update HUD displays (optimized - only updates if changed)
public void UpdateSwarmCount(int count)
public void UpdateCurrencyDisplay(int gold, int gems)

// Show monetization offers
public void ShowReviveOffer()
public void ShowMerchantOffer()

// Open shop
public void OpenShop()
```

**Usage**:
```csharp
UIManager ui = UIManager.Instance;

// Update swarm counter during gameplay
SwarmController swarm = FindObjectOfType<SwarmController>();
swarm.OnSwarmSizeChanged += (count) =>
{
    ui.UpdateSwarmCount(count);
};

// Update currencies
SaveManager save = SaveManager.Instance;
ui.UpdateCurrencyDisplay(save.CurrentPlayer.Gold, save.CurrentPlayer.PremiumGems);

// Show offers
ui.ShowReviveOffer();  // Loss aversion popup
ui.ShowMerchantOffer();  // Scarcity timer popup
```

---

## Camera System

### CameraController - Perspectives
```csharp
// Location: Assets/Scripts/Core/CameraController.cs

public enum CameraMode
{
    GodView,              // Top-down (y=30, angle=60°) - City
    OrbitRaid,            // Orbit around base (r=15, h=10) - Raid
    OverShoulderRunner    // Behind hero (y=5, z=-8, angle=15°) - Swarm
}

// Set follow target
public void SetFollowTarget(Transform target)

// Set orbit center (for raid)
public void SetOrbitCenter(Transform center)

// Manual mode change
public void TransitionToMode(CameraMode mode)

// Current mode
public CameraMode CurrentMode { get; }
```

**Auto-switching** (wired to GameManager):
```csharp
// Camera automatically switches based on game state:
// GameState.City → CameraMode.GodView
// GameState.Swarm → CameraMode.OverShoulderRunner
// GameState.Raid → CameraMode.OrbitRaid
```

---

## Hero Controller

### HeroController - Player Character
```csharp
// Location: Assets/Scripts/Core/HeroController.cs

// Movement
[SerializeField] private float forwardSpeed = 10f;    // Auto-run
[SerializeField] private float laneWidth = 3f;        // 3-lane system
[SerializeField] private int laneCount = 3;

// Control
public void StartRunning()  // Begin auto-run
public void StopRunning()   // Stop movement

// Combat
public int CurrentHealth { get; }
public bool IsAlive { get; }
public void TakeDamage(int damage)
public void Revive(int healthPercent = 50)  // Ad-driven revive

// Events
public event System.Action OnHeroDeath
public event System.Action<int> OnHealthChanged

// Input: Arrow keys or A/D for lane switching
```

**Usage**:
```csharp
HeroController hero = FindObjectOfType<HeroController>();

// Start run
hero.StartRunning();

// Listen for death
hero.OnHeroDeath += () =>
{
    Debug.Log("Hero died!");
    UIManager.Instance.ShowReviveOffer();
};

// Revive after ad watch
hero.Revive(healthPercent: 50);  // Restores 50% HP
hero.StartRunning();
```

---

## Audio & Haptics

### AudioManager
```csharp
// Location: Assets/Scripts/Core/AudioManager.cs

AudioManager.Instance

// Adaptive pitch based on swarm size
public void SetSwarmIntensity(int swarmCount)
// Pitch scales from 1.0 → 1.5 as swarm grows to 500

// Play SFX
public void PlayGateActivate()
public void PlayShatter()
public void PlayLoot()
public void PlayBuildComplete()

// Generic SFX
public void PlaySFX(AudioClip clip)
```

### HapticManager
```csharp
// Location: Assets/Scripts/Core/HapticManager.cs

HapticManager.Instance

public enum HapticType
{
    SharpTick,      // Gate hit (10ms)
    RollingRumble,  // Swarm flow (pulsing)
    HeavyImpact,    // Boss shatter (50ms)
    LightTap        // UI touch (5ms)
}

public void TriggerHaptic(HapticType type)
```

**Usage**:
```csharp
// In MathGate.OnTriggerEnter:
AudioManager.Instance.PlayGateActivate();
HapticManager.Instance.TriggerHaptic(HapticType.SharpTick);

// In SwarmController.ApplyMathGate:
AudioManager.Instance.SetSwarmIntensity(activeShardlings.Count);
```

---

## Level Generation

### LevelGenerator - Procedural Tracks
```csharp
// Location: Assets/Scripts/Core/LevelGenerator.cs

[SerializeField] private float trackLength = 200f;
[SerializeField] private float gateSpacing = 15f;
[SerializeField] private float obstacleDensity = 0.4f;

// Generate full track
public void GenerateTrack()
// Creates:
//   - Math gate pairs every 15m
//   - Random obstacles (40% density)
//   - Boss at end (z = trackLength)
```

---

## Common Code Patterns

### Singleton Setup
```csharp
public static MyManager Instance { get; private set; }

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
```

### Event-Driven Updates
```csharp
// Listener pattern for decoupling
public event System.Action<int> OnValueChanged;

// Trigger event
OnValueChanged?.Invoke(newValue);

// Subscribe
manager.OnValueChanged += HandleValueChanged;

// Unsubscribe (in OnDestroy)
manager.OnValueChanged -= HandleValueChanged;
```

### Performance-Optimized UI Updates
```csharp
private int lastValue = -1;

public void UpdateDisplay(int value)
{
    // Only update if changed (avoid ToString() allocations)
    if (value != lastValue)
    {
        lastValue = value;
        textField.text = value.ToString();
    }
}
```

---

## Scene Navigation

### Boot Scene Flow
```
Boot.unity
  ↓ (Press 1, 2, or 3)
  ├─→ SwarmPrototype.unity  (Swarm loop)
  ├─→ CityPrototype.unity   (City loop)
  └─→ RaidPrototype.unity   (Raid loop)
```

**Keyboard Controls**:
- `1` - Load Swarm scene
- `2` - Load City scene
- `3` - Load Raid scene
- `Arrow Keys` or `A/D` - Lane switching (in Swarm)

---

## Testing Checklist

**Quick Playtest Steps**:

1. **Boot Scene**
   - [ ] Open `Boot.unity` in Unity
   - [ ] Enter Play mode
   - [ ] See log: "Boot scene loaded. Press 1 for Swarm..."

2. **Swarm Scene** (Press 1)
   - [ ] Hero auto-runs forward
   - [ ] Press A/D to switch lanes
   - [ ] Pass through green gate (x2) - see swarm multiply
   - [ ] Hit gray obstacle - see swarm shrink
   - [ ] Reach boss - swarm attacks

3. **City Scene** (Press 2)
   - [ ] See 3 buildings (Ruin, Construction, Completed)
   - [ ] Camera in god-view (top-down)

4. **Raid Scene** (Press 3)
   - [ ] See enemy base cube
   - [ ] Check console for raid logs

5. **Save/Load**
   - [ ] Exit and restart Unity
   - [ ] Player data persists (check gold/level)
   - [ ] Offline rewards calculated (if > 10 sec elapsed)

---

## Debug Console Commands

**Useful Debug.Log filters**:
```
[GameManager] - State transitions
[SwarmController] - Gate applications, swarm size changes
[RaidManager] - Raid start/end, loot calculation
[SaveManager] - Save/load operations, offline rewards
[MonetizationManager] - IAP events, timer updates
[Bootstrap] - System initialization
```

**Check logs for**:
- State transition: `[GameManager] State transition: Splash → Swarm`
- Gate activation: `[SwarmController] Gate applied: Multiply 2. Swarm: 1 → 2`
- Boss damage: `[BossController] Obsidian Sentinel took 50 damage. HP: 150/200`
- Raid loot: `[RaidManager] Raid complete — Tier 4, Gold: 250, Shards: 20`

---

## Key Formulas

**Swarm Damage**:
```
damage = min(swarmCount, enemyHP)
remainingShardlings = swarmCount - damage
```

**Raid Loot**:
```
precision = 1.0 - |playerFrequency - targetFrequency|
lootTier = floor(precision * 5)  // 0 to 5
gold = baseLoot * (1 + lootTier) * revengeMultiplier
revengeMultiplier = isRevenge ? 2.0 : 1.0
```

**Offline Rewards**:
```
elapsedSeconds = min(now - lastLogin, 10 hours * 3600)
gold = elapsedSeconds * goldPerSecond
```

**Raid Energy**:
```
raidEnergy = shardlingCount * 10
```

---

## File Sizes (Approximate)

```
Core Systems:        ~1,200 LOC
Gameplay Systems:    ~900 LOC
Data/Monetization:   ~400 LOC
Total:              ~2,500 LOC
```

---

## Next Steps for Developers

1. **Art Pipeline**: Replace all prefabs in `Assets/Prefabs/Prototype/` with actual 3D models
2. **Audio**: Assign audio clips to AudioManager SerializeFields
3. **UI**: Create Canvas prefabs and assign to UIManager SerializeFields
4. **Backend**: Implement PlayFab/Firebase cloud save
5. **Testing**: Add unit tests for core formulas (SwarmController, RaidManager)

---

*Quick reference last updated: February 8, 2026*  
*For full documentation, see: CORE_GAME_CODE_DOCUMENTATION.md*
