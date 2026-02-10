# Empire of Glass - Architecture & System Relationships

**Visual guide showing how all systems connect**

---

## System Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                         GAME ENTRY POINT                        │
│                                                                 │
│  Unity Startup → Boot.unity → GameBootstrap                    │
│                                    ↓                            │
│                      Initialize All Singletons                  │
└─────────────────────────────────────────────────────────────────┘
                                ↓
        ┌──────────────────────────────────────────────┐
        │          CORE SINGLETON MANAGERS              │
        ├──────────────────────────────────────────────┤
        │  • GameManager          (State machine)      │
        │  • SaveManager          (Data persistence)   │
        │  • MonetizationManager  (IAP & offers)       │
        │  • AudioManager         (Music & SFX)        │
        │  • HapticManager        (Vibration)          │
        │  • UIManager            (Panel switching)    │
        └──────────────────────────────────────────────┘
                                ↓
        ┌──────────────────────────────────────────────┐
        │           GAMEPLAY CONTROLLERS               │
        ├──────────────────────────────────────────────┤
        │  Scene-Specific Controllers:                 │
        │  • SwarmController      (Swarm loop)        │
        │  • CityBuilder          (City loop)         │
        │  • RaidManager          (Raid loop)         │
        │  • HeroController       (Player character)  │
        │  • CameraController     (Camera modes)      │
        └──────────────────────────────────────────────┘
                                ↓
        ┌──────────────────────────────────────────────┐
        │         GAMEPLAY ENTITIES & PREFABS          │
        ├──────────────────────────────────────────────┤
        │  • ShardlingBehavior    (Swarm units)       │
        │  • MathGate            (Multipliers)        │
        │  • ObstacleBarrier     (Hazards)            │
        │  • BossController      (End boss)           │
        │  • CityBuilding        (Building data)      │
        └──────────────────────────────────────────────┘
```

---

## State Machine Flow

```
┌──────────────────────────────────────────────────────────────────┐
│                      GAME STATE MACHINE                          │
│                        (GameManager)                             │
└──────────────────────────────────────────────────────────────────┘

    Splash ──────→ Login ──────→ City
                                  ↓
    ┌─────────────────────────────┘
    │
    │  FORCED ROTATION LOOP (endless):
    │
    ├──→ Swarm ──→ City ──→ Raid ──→ Swarm ──→ ...
    │      ↓          ↓         ↓
    │   Run Game  Build Up   Attack
    │   90 sec    (no limit)  60 sec
    │
    └───→ Shop (accessible from any state)
```

**State Listeners**:
- `AudioManager` → Changes music track
- `UIManager` → Switches HUD panels
- `CameraController` → Changes camera perspective
- Scene loaders → Load appropriate Unity scene

---

## Three Core Gameplay Loops

### Loop 1: SWARM (Satisfaction)
```
┌─────────────────────────────────────────────────────────────────┐
│                         SWARM LOOP                              │
│              One hero → 500+ shardlings                         │
└─────────────────────────────────────────────────────────────────┘

HeroController                SwarmController
(lane runner)                 (manages swarm)
     ↓                              ↓
  StartRunning()            InitializeSwarm()
     │                         (1 shardling)
     │ moves forward                │
     │                              │
     ├─ hits MathGate ──────────────┤
     │  (x2, x5, +10)               │
     │                       ApplyMathGate()
     │                    (spawns more shardlings)
     │                              │
     │                      ┌───────┴───────┐
     │                      │ ShardlingBehavior × N
     │                      │ (flocking AI)
     │                      └───────┬───────┘
     │                              │
     ├─ hits ObstacleBarrier ───────┤
     │                       ApplyMathGate(Subtract)
     │                    (removes shardlings)
     │                              │
     └─ reaches BossController ─────┤
                              CalculateSwarmDamage()
                           (swarm consumed by boss HP)
                                    │
                            remaining shardlings
                                    ↓
                             GetRaidEnergy()
                          (converts to raid fuel)
                                    │
                                    ↓
                          ┌──────────────────┐
                          │  GameManager     │
                          │  AdvanceLoop()   │
                          │  → City State    │
                          └──────────────────┘
```

**Key Formula**: `(CurrentUnits * GateValue) - EnemyHP`

---

### Loop 2: CITY (Meta-Game)
```
┌─────────────────────────────────────────────────────────────────┐
│                          CITY LOOP                              │
│               Rebuild shattered 3D city                         │
└─────────────────────────────────────────────────────────────────┘

        RaidManager.OnRaidComplete (loot earned)
                    ↓
        ┌───────────────────────┐
        │   PlayerData.Gold     │ (currency from raid)
        └───────────────────────┘
                    ↓
        ┌───────────────────────────────────────────┐
        │         CityBuilder                       │
        ├───────────────────────────────────────────┤
        │  10x10 Grid System                        │
        │                                           │
        │  PlaceBuilding(x, y, type, goldCost)     │
        │    ↓                                      │
        │  BuildingState: Empty → Construction      │
        │    ↓                                      │
        │  [Time passes / resources accumulated]    │
        │    ↓                                      │
        │  CompleteBuilding(x, y)                   │
        │    ↓                                      │
        │  BuildingState: Construction → Completed  │
        │    ↓                                      │
        │  [Reverse-time shard assembly animation]  │
        │    ↓                                      │
        │  Building generates resources             │
        └───────────────────────────────────────────┘
                    ↓
        ┌───────────────────────┐
        │   GameManager         │
        │   AdvanceLoop()       │
        │   → Raid State        │
        └───────────────────────┘
```

**Building Types**:
- Residential → Generates gold over time
- Defense → Protects against raids
- Resource → Produces special materials
- Vault → Stores excess currency
- MegaStructure → Alliance collaborative projects

---

### Loop 3: RAID (PvP)
```
┌─────────────────────────────────────────────────────────────────┐
│                          RAID LOOP                              │
│            Coin Master-style frequency puzzle                   │
└─────────────────────────────────────────────────────────────────┘

    SwarmController.GetRaidEnergy()
           (fuel from swarm run)
                    ↓
        ┌───────────────────────┐
        │   RaidManager         │
        │   StartRaid(energy)   │
        │    • 60-sec timer     │
        │    • Generate target  │
        │      frequency (0-1)  │
        └───────────────────────┘
                    ↓
        ┌───────────────────────────────────┐
        │  Player Input                     │
        │  (adjust frequency slider)        │
        │  FireFrequencyBeam(playerFreq)    │
        └───────────────────────────────────┘
                    ↓
        ┌───────────────────────────────────────────┐
        │  Calculate Precision                      │
        │  precision = 1.0 - |player - target|      │
        │                                           │
        │  CalculateLoot(precision, isRevenge)      │
        │    lootTier = floor(precision * 5)        │
        │    gold = baseLoot * (1 + tier) * mult    │
        │    shards = baseShards * tier * mult      │
        │                                           │
        │  RaidResult { Tier, Gold, Shards }        │
        └───────────────────────────────────────────┘
                    ↓
        ┌───────────────────────┐
        │  Update PlayerData    │
        │  • Add gold/shards    │
        │  • Increment raids    │
        │  • Save progress      │
        └───────────────────────┘
                    ↓
        ┌───────────────────────┐
        │   GameManager         │
        │   AdvanceLoop()       │
        │   → Swarm State       │
        └───────────────────────┘
```

**Revenge Mechanic**: If raided by friend → revenge raid gives 2x loot

---

## Data Flow

```
┌────────────────────────────────────────────────────────────────┐
│                    DATA PERSISTENCE FLOW                       │
└────────────────────────────────────────────────────────────────┘

App Launch
    ↓
GameBootstrap.Awake()
    ↓
SaveManager.Instance.LoadPlayerData()
    ↓
    ├─ Check PlayerPrefs for save key
    │
    ├─ IF FOUND:
    │     ├─ PlayerData.FromJson(json)
    │     └─ Calculate offline rewards
    │         └─ ClaimOfflineRewards(goldPerSecond)
    │
    └─ IF NOT FOUND:
          └─ PlayerData.CreateNew(guid)
                ↓
        ┌──────────────────────────┐
        │     PlayerData           │
        │  (in-memory state)       │
        ├──────────────────────────┤
        │  • UserID                │
        │  • DisplayName           │
        │  • Level                 │
        │  • Gold                  │
        │  • PremiumGems           │
        │  • RaidEnergy            │
        │  • ShieldCount           │
        │  • SwarmHighScore        │
        │  • RaidsCompleted        │
        │  • CitySizeLevel         │
        │  • PiggyBankGems         │
        │  • BattlePassTier        │
        │  • VIPLevel              │
        │  • BaseLayout[][]        │
        │  • Inventory[]           │
        │  • Timestamps            │
        └──────────────────────────┘
                ↓
    [Gameplay modifies PlayerData]
        • Earn gold from raids
        • Spend gold on buildings
        • Accumulate gems in piggy bank
        • Progress battle pass tiers
        • Complete swarm runs
                ↓
SaveManager.SavePlayerData()
    ↓
PlayerData.ToJson()
    ↓
PlayerPrefs.SetString(key, json)
    ↓
PlayerPrefs.Save()
```

**Save Triggers**:
- Raid completion
- Building placed/upgraded
- Currency earned/spent
- App pause/quit
- Manual save button

---

## Event-Driven Communication

```
┌────────────────────────────────────────────────────────────────┐
│              EVENT SYSTEM (Decoupled Communication)            │
└────────────────────────────────────────────────────────────────┘

GameManager.OnStateChanged
    ├─→ AudioManager.HandleStateChanged()  (switch music)
    ├─→ UIManager.HandleStateChanged()     (switch panels)
    └─→ CameraController.HandleGameStateChanged() (camera mode)

HeroController.OnHeroDeath
    └─→ [Check progress] → MonetizationManager.ShouldOfferRevive()
        └─→ UIManager.ShowReviveOffer()

HeroController.OnHealthChanged
    └─→ UIManager.UpdateHealthBar()

SwarmController.OnSwarmSizeChanged
    ├─→ UIManager.UpdateSwarmCount()
    └─→ AudioManager.SetSwarmIntensity()

RaidManager.OnRaidComplete
    ├─→ SaveManager.UpdatePlayerData()  (add loot)
    ├─→ UIManager.ShowLootPopup()
    └─→ AudioManager.PlayLoot()

CityBuilder.OnBuildingPlaced
    ├─→ AudioManager.PlayBuildComplete()
    └─→ [Trigger reverse-time animation]

CityBuilder.OnBuildingUpgraded
    └─→ SaveManager.UpdateCityState()

MonetizationManager.OnPiggyBankBroken
    ├─→ SaveManager.UpdatePlayerData()  (add gems)
    └─→ UIManager.ShowClaimAnimation()

MonetizationManager.OnBattlePassTierUp
    └─→ UIManager.ShowTierReward()

MonetizationManager.OnMerchantTimerUpdate
    └─→ UIManager.UpdateMerchantTimer()
```

**Pattern**: Publisher-Subscriber (loose coupling)

---

## Camera Perspective System

```
┌────────────────────────────────────────────────────────────────┐
│                   CAMERA MODE TRANSITIONS                      │
└────────────────────────────────────────────────────────────────┘

GameState.City
    ↓
CameraController.TransitionToMode(GodView)
    ↓
┌─────────────────────────────┐
│       GOD-VIEW CAMERA       │
│  • Position: (0, 30, -10)   │
│  • Rotation: 60° pitch      │
│  • Target: City center      │
│  • Use: Top-down strategy   │
└─────────────────────────────┘

GameState.Swarm
    ↓
CameraController.TransitionToMode(OverShoulderRunner)
    ↓
┌─────────────────────────────┐
│  OVER-SHOULDER RUNNER CAM   │
│  • Position: hero + (0,5,-8)│
│  • Rotation: 15° pitch      │
│  • Target: Hero back        │
│  • Use: Lane runner view    │
└─────────────────────────────┘

GameState.Raid
    ↓
CameraController.TransitionToMode(OrbitRaid)
    ↓
┌─────────────────────────────┐
│      ORBIT RAID CAMERA      │
│  • Radius: 15 units         │
│  • Height: 10 units         │
│  • Speed: 30°/sec rotation  │
│  • Target: Enemy base       │
│  • Use: 360° raid view      │
└─────────────────────────────┘

Transitions: 1.5s smooth Lerp
```

---

## Monetization Decision Tree

```
┌────────────────────────────────────────────────────────────────┐
│          MONETIZATION TRIGGER DECISION TREE                    │
└────────────────────────────────────────────────────────────────┘

Player Event → Check Monetization Opportunity

1. HERO DEATH during swarm run
    ↓
    Check progress: progress >= 80%?
    ├─ YES: Show Revive Offer ($0.99 or ad)
    └─ NO:  End run normally

2. FIRST DEFEAT (tutorial)
    ↓
    Flag: isFirstDefeat = true
    ↓
    Show Starter Pack Offer ($0.99)
    • 500 gold + 50 gems

3. GEMS EARNED during gameplay
    ↓
    MonetizationManager.AddToPiggyBank(gems)
    ↓
    IF PiggyBankGems >= 100:
        Show visual prompt: "Break bank for $4.99?"
        • Glass vault UI shows gems visibly

4. SWARM RUN COMPLETED
    ↓
    Award Battle Pass XP
    ↓
    IF tier advanced:
        Show tier reward popup
        IF tier 50 reached:
            Show exclusive skin unlock

5. RANDOM TRIGGER (1% chance per raid)
    ↓
    MonetizationManager.ActivateWanderingMerchant()
    ↓
    Show merchant popup with 15-min timer
    • Scarcity-based offer (rare items)

6. LOGIN (daily)
    ↓
    Check VIP status: VIPLevel > 0 && VIPExpiryTimestamp > now
    ├─ Active: Grant VIP daily bonus
    └─ Expired: Show VIP renewal offer ($9.99/month)

7. BASE ATTACKED (raid on player)
    ↓
    IF ShieldCount == 0:
        Show shield purchase offer
        • Emergency shield $1.99
        • Or watch ad for 1 shield
```

---

## Performance Optimization Points

```
┌────────────────────────────────────────────────────────────────┐
│                   PERFORMANCE PATTERNS                         │
└────────────────────────────────────────────────────────────────┘

1. SwarmController - Flocking Optimization
    ┌───────────────────────────────────────┐
    │  GetNeighbors() returns shared buffer │
    │  • No per-call allocations            │
    │  • Caller must not cache result       │
    │  • Single-pass neighbor iteration     │
    └───────────────────────────────────────┘

2. ShardlingBehavior - Flocking Calculation
    ┌───────────────────────────────────────┐
    │  Single-pass over neighbors:          │
    │  • Separation force                   │
    │  • Cohesion center                    │
    │  • Alignment velocity                 │
    │  → Combined in one loop               │
    └───────────────────────────────────────┘

3. UIManager - Text Updates
    ┌───────────────────────────────────────┐
    │  Cache last value, only update if     │
    │  changed:                             │
    │    if (value != lastValue)            │
    │        text = value.ToString()        │
    │  → Avoids ToString() spam             │
    └───────────────────────────────────────┘

4. Rendering - GPU Instancing
    ┌───────────────────────────────────────┐
    │  ShardlingBehavior prefab:            │
    │  • Enable GPU Instancing on material  │
    │  • Low-poly mesh (50 tris)            │
    │  • Shared material                    │
    │  → 500+ units at 60 FPS               │
    └───────────────────────────────────────┘

5. Physics - Aggressive Culling
    ┌───────────────────────────────────────┐
    │  • Neighbor radius: 5 units           │
    │  • Squared distance checks            │
    │  • Skip self in neighbor list         │
    │  → O(n²) but with tight radius        │
    └───────────────────────────────────────┘
```

---

## Dependency Graph

```
┌────────────────────────────────────────────────────────────────┐
│                    SCRIPT DEPENDENCIES                         │
└────────────────────────────────────────────────────────────────┘

GameBootstrap (entry point)
    ↓
    ├─→ GameManager ────────┐
    ├─→ SaveManager         │
    ├─→ MonetizationManager │  (Core Singletons)
    ├─→ AudioManager        │
    ├─→ HapticManager       │
    └─→ UIManager ──────────┘
            ↓
            ↓ (listens to GameManager.OnStateChanged)
            ↓
    ┌───────────────────────────────┐
    │   Scene-Specific Controllers  │
    ├───────────────────────────────┤
    │  SwarmController              │
    │    ├─→ ShardlingBehavior      │
    │    └─→ MathGate               │
    │                               │
    │  HeroController               │
    │    └─→ SwarmController        │
    │                               │
    │  BossController               │
    │    └─→ SwarmController        │
    │                               │
    │  ObstacleBarrier              │
    │    └─→ SwarmController        │
    │                               │
    │  CityBuilder                  │
    │    └─→ CityBuilding (data)    │
    │                               │
    │  RaidManager                  │
    │    └─→ RaidResult (data)      │
    │                               │
    │  CameraController             │
    │    └─→ GameManager            │
    │                               │
    │  LevelGenerator               │
    │    └─→ Prefabs                │
    └───────────────────────────────┘

PlayerData (data model)
    ↑
    └─ SaveManager
    └─ MonetizationManager
```

**Legend**:
- `→` : Direct reference / dependency
- `↑` : Used by / consumed by
- Singletons accessible via `.Instance`

---

## Testing Strategy

```
┌────────────────────────────────────────────────────────────────┐
│                    RECOMMENDED TESTS                           │
└────────────────────────────────────────────────────────────────┘

UNIT TESTS (Priority: HIGH)
├─ SwarmController
│  ├─ ApplyMathGate_Multiply_DoublesCount()
│  ├─ ApplyMathGate_Add_IncreasesCount()
│  ├─ ApplyMathGate_Subtract_DecreasesCount()
│  ├─ ApplyMathGate_MaxCap_EnforcesLimit()
│  ├─ CalculateSwarmDamage_ConsumesShardlings()
│  └─ GetRaidEnergy_ReturnsCorrectValue()
│
├─ RaidManager
│  ├─ CalculateLoot_PerfectPrecision_ReturnsTier5()
│  ├─ CalculateLoot_ZeroPrecision_ReturnsTier0()
│  ├─ CalculateLoot_Revenge_Doubles()
│  └─ StartRaid_SetsTimer()
│
├─ PlayerData
│  ├─ CalculateOfflineRewards_CapsAt10Hours()
│  ├─ CalculateOfflineRewards_Zero_WhenNoTimeElapsed()
│  └─ CreateNew_SetsDefaults()
│
└─ CityBuilder
   ├─ PlaceBuilding_ValidPosition_ReturnsTrue()
   ├─ PlaceBuilding_OccupiedCell_ReturnsFalse()
   ├─ CompleteBuilding_UpdatesState()
   └─ DamageBuilding_SetsRuinState()

INTEGRATION TESTS (Priority: MEDIUM)
├─ Swarm → Raid Energy Conversion
│  └─ CompleteSwarmRun_ProvideRaidEnergy()
│
├─ Raid → City Currency
│  └─ CompleteRaid_AddsGoldToPlayer()
│
├─ Save/Load Persistence
│  ├─ SaveAndLoad_PreservesPlayerData()
│  └─ LoadAfterDelay_CalculatesOfflineRewards()
│
└─ State Machine Transitions
   └─ AdvanceLoop_FollowsCorrectOrder()

PERFORMANCE TESTS (Priority: HIGH)
├─ Swarm500Shardlings_Maintains60FPS()
├─ FlockingCalculation_UnderXms()
├─ UIUpdate_NoGCAllocation()
└─ SaveLoad_UnderYms()

PLAYTESTS (Priority: CRITICAL)
├─ Full Loop: Boot → Swarm → City → Raid → Swarm
├─ Math Gates: x2, x5, +10 multiply correctly
├─ Boss Defeat: Various swarm sizes
├─ Offline Rewards: 1 hour, 5 hours, 15 hours (should cap at 10)
├─ Piggy Bank: Accumulate and break
├─ Revive Offer: At 80%+ progress
└─ Merchant Timer: 15-minute countdown
```

---

## Code Quality Checklist

```
┌────────────────────────────────────────────────────────────────┐
│                   CODE REVIEW CHECKLIST                        │
└────────────────────────────────────────────────────────────────┘

✅ ARCHITECTURE
  ✅ Proper namespace organization (Core, Swarm, City, Raid, Data)
  ✅ Singleton pattern correctly implemented
  ✅ DontDestroyOnLoad for persistent managers
  ✅ Event-driven communication (loose coupling)
  ✅ SerializeField for Inspector-editable values

✅ PERFORMANCE
  ✅ Cached component references
  ✅ Minimal allocations in Update/FixedUpdate
  ✅ Shared buffers for frequently-called methods
  ✅ Squared distance checks (avoid sqrt)
  ✅ Early exit conditions

✅ DOCUMENTATION
  ✅ XML comments on public methods
  ✅ Clear variable names
  ✅ Inline comments for complex logic
  ✅ GDD variable references (Var X)

🔧 NEEDS IMPROVEMENT
  🔧 Add null checks for SerializeFields
  🔧 Error handling (try/catch for JSON)
  🔧 Logging framework (replace Debug.Log)
  🔧 Magic numbers → constants or ScriptableObjects
  🔧 Platform-specific implementations (iOS/Android)
  🔧 Unit tests (currently 0% coverage)
```

---

## Build Pipeline

```
┌────────────────────────────────────────────────────────────────┐
│                    BUILD & DEPLOY FLOW                         │
└────────────────────────────────────────────────────────────────┘

Development (Current)
    ↓
    ├─ Unity Editor testing
    ├─ Prototype scenes with primitives
    └─ No external assets

Pre-Alpha
    ↓
    ├─ Replace placeholders with 3D models
    ├─ Add audio/VFX
    └─ Local device testing

Alpha (Backend Integration)
    ↓
    ├─ PlayFab/Firebase setup
    ├─ Cloud save working
    ├─ Anti-cheat validation
    └─ Internal alpha testers

Beta (Polish)
    ↓
    ├─ All 12 monetization systems live
    ├─ Performance optimized (60 FPS)
    ├─ Social features active
    └─ External beta testers (TestFlight/Play Console)

Release Candidate
    ↓
    ├─ ASO keywords optimized
    ├─ App store assets ready
    ├─ Analytics/crashlytics enabled
    └─ Submission to App Store & Play Store

Post-Launch
    ↓
    ├─ Live ops events (72-hour)
    ├─ Seasonal content updates
    ├─ A/B testing monetization
    └─ UE5 migration (future)
```

---

## Quick Lookup: Key Constants

```csharp
// Session Durations
const float SWARM_DURATION = 90f;   // seconds
const float RAID_DURATION = 60f;    // seconds

// Swarm Limits
const int MAX_SHARDLINGS = 500;
const int INITIAL_SHARDLINGS = 1;

// Flocking
const float NEIGHBOR_RADIUS = 5f;
const float SEPARATION_WEIGHT = 1.5f;
const float COHESION_WEIGHT = 1.0f;
const float ALIGNMENT_WEIGHT = 1.0f;

// City Grid
const int GRID_WIDTH = 10;
const int GRID_HEIGHT = 10;
const float CELL_SIZE = 5f;

// Raid Loot
const int BASE_LOOT_GOLD = 50;
const int BASE_LOOT_SHARDS = 5;
const float REVENGE_MULTIPLIER = 2f;

// Monetization
const float PIGGY_BANK_PRICE = 4.99f;
const int PIGGY_BANK_CAPACITY = 500;
const float STARTER_PACK_PRICE = 0.99f;
const float MERCHANT_DURATION = 900f;  // 15 minutes
const int BATTLE_PASS_MAX_TIER = 50;

// Offline
const float MAX_OFFLINE_HOURS = 10f;
const float OFFLINE_GOLD_PER_SECOND = 0.5f;

// Camera
const float GOD_VIEW_HEIGHT = 30f;
const float GOD_VIEW_ANGLE = 60f;
const float SHOULDER_HEIGHT = 5f;
const float SHOULDER_DISTANCE = -8f;
const float SHOULDER_ANGLE = 15f;
const float ORBIT_RADIUS = 15f;
const float ORBIT_HEIGHT = 10f;
```

---

*Architecture guide last updated: February 8, 2026*  
*For code examples, see: CODE_QUICK_REFERENCE.md*  
*For full feature list, see: CORE_GAME_CODE_DOCUMENTATION.md*
