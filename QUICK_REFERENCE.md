# Empire of Glass - Quick Reference

## Development Setup

### Unity Version
- Unity 2021.3 LTS or later
- Required modules: WebGL Build Support

### Running the Game
1. Open project in Unity
2. Open `Assets/Scenes/Boot.unity`
3. Press Play
4. Use number keys 1-3 to navigate scenes:
   - 1 = SwarmPrototype
   - 2 = CityPrototype
   - 3 = RaidPrototype

## Debug Controls

### In-Game Shortcuts
- **F3** - Toggle performance overlay (FPS, memory)
- **1, 2, 3** - Scene navigation (in Boot scene)
- **Arrow Keys / A/D** - Hero lane switching
- **ESC** - Pause menu (when implemented)

### Performance Monitoring
Press F3 to show/hide performance overlay showing:
- Current FPS
- Average FPS (60-frame rolling average)
- Min/Max FPS
- Memory usage

Color coding:
- 🟢 Green: FPS above target (60+)
- 🟡 Yellow: FPS between min acceptable and target (30-60)
- 🔴 Red: FPS below acceptable (<30)

## Testing

### Run Unit Tests
1. Window → General → Test Runner
2. Select "EditMode" tab
3. Click "Run All"

### Run Integration Tests
1. Window → General → Test Runner
2. Select "PlayMode" tab
3. Click "Run All"

## Analytics

### Event Tracking
Analytics are automatically tracked for:
- State transitions
- Math gate hits
- Boss encounters/defeats
- Performance metrics

### View Analytics
Check console for analytics events (when debug mode enabled):
```
[Analytics] math_gate_hit: x2, before=50, after=100
[Analytics] boss_defeated: final_count=75, time=12.5s
```

## UI Systems

### HUD Components
Located in Swarm/City/Raid scenes:
- **Swarm Count** - Current number of shardlings
- **Health Bar** - Hero HP (color-coded)
- **Energy** - Available energy for raids
- **Game State** - Current mode (SWARM/CITY/RAID)

### Tooltips
Attach `TooltipTrigger` component to any UI element:
1. Add component to GameObject
2. Set tooltip text
3. Adjust show delay if needed
4. Tooltip displays on hover

## Common Tasks

### Add New Analytics Event
```csharp
if (Analytics.AnalyticsManager.Instance != null)
{
    Analytics.AnalyticsManager.Instance.TrackEvent("event_name", parameters);
}
```

### Add Performance Marker
```csharp
UnityEngine.Profiling.Profiler.BeginSample("MyFunction");
MyExpensiveFunction();
UnityEngine.Profiling.Profiler.EndSample();
```

### Create New Test
```csharp
[Test]
public void MyTest_DoesExpectedThing()
{
    // Arrange
    var obj = CreateTestObject();
    
    // Act
    var result = obj.DoSomething();
    
    // Assert
    Assert.AreEqual(expected, result);
}
```

## Project Structure Quick Reference

```
Assets/
├── Scenes/             # Game scenes
│   ├── Boot.unity      # Entry point
│   ├── SwarmPrototype  # Swarm runner
│   ├── CityPrototype   # City building
│   └── RaidPrototype   # PvP raids
│
├── Scripts/
│   ├── Core/           # GameManager, HeroController
│   ├── Swarm/          # Swarm mechanics
│   ├── City/           # City building
│   ├── Raid/           # Raid system
│   ├── UI/             # User interface
│   ├── Analytics/      # Telemetry
│   ├── Performance/    # Performance monitoring
│   ├── Backend/        # API integration
│   └── Tests/          # Unit & integration tests
│
├── Prefabs/
│   └── Prototype/      # Placeholder prefabs
│
└── Documentation/      # Guides & documentation
    ├── PlaytestGuide.md
    ├── Analytics.md
    ├── WebGLDeployment.md
    ├── PerformanceOptimization.md
    ├── BackendIntegration.md
    └── DevelopmentWorkflow.md
```

## Key Scripts Reference

### Core Systems
- **GameManager** - State machine, session control
- **HeroController** - Hero movement, health
- **SwarmController** - Swarm management, math gates
- **AnalyticsManager** - Event tracking
- **PerformanceMonitor** - FPS and memory monitoring

### UI Components
- **GameHUD** - In-game overlay
- **TooltipSystem** - Tooltip display
- **TooltipTrigger** - Attach to UI elements

### Swarm Mechanics
- **MathGate** - Multiplier gates (x2, x5, +10)
- **ShardlingBehavior** - Individual shardling AI
- **BossController** - Boss enemy logic

## Troubleshooting

### Tests Not Appearing
- Check .asmdef files are configured
- Ensure test files are in Tests/ folder
- Restart Unity Editor

### Performance Issues
- Press F3 to check FPS
- Use Unity Profiler (Window → Analysis → Profiler)
- Check swarm count (reduce if >500)
- Lower quality settings for testing

### Analytics Not Working
- Ensure AnalyticsManager is in scene
- Check debug mode is enabled
- Verify null checks around analytics calls

### Missing References in Inspector
- Save scene after assigning references
- Verify prefabs are properly saved
- Check for compilation errors

## Quick Links

### Documentation
- `/README.md` - Project overview
- `/ROADMAP.md` - Development roadmap
- `/Documentation/` - Detailed guides

### External Resources
- Unity Docs: https://docs.unity3d.com/
- Unity Test Framework: https://docs.unity3d.com/Packages/com.unity.test-framework@latest

## Support

### Internal
- Check `/Documentation/` folder first
- Review ROADMAP.md for task status
- Create GitHub Issue for bugs

### External
- Unity Forums: https://forum.unity.com/
- Unity Answers: https://answers.unity.com/

## Quick Commands

### Git
```bash
# Create feature branch
git checkout -b feature/your-feature

# Commit changes
git add .
git commit -m "Add: your changes"

# Push to remote
git push origin feature/your-feature
```

### Unity (via Terminal)
```bash
# Run EditMode tests
Unity -runTests -batchmode -projectPath . -testPlatform editmode

# Build WebGL
Unity -quit -batchmode -projectPath . -buildTarget WebGL -buildPath ./Build/WebGL
```

## Development Phases Status

- ✅ Phase 1: Playtest Foundation - **In Progress**
- ✅ Phase 2: Test Infrastructure - **Complete**
- ⏳ Phase 3: Art & Visuals - **Planned**
- ✅ Phase 4: UI Polish - **Partially Complete**
- ⏳ Phase 5: Backend - **Planned**
- ⏳ Phase 6: Performance - **In Progress**
- ⏳ Phase 7: Monetization - **Planned**
- ✅ Phase 8: WebGL Launch - **Documentation Complete**

## Next Steps

1. Set up CI/CD pipeline
2. Complete tooltip system integration
3. Add juice effects (screen shake, particles)
4. Begin performance profiling
5. Plan first playtest session

---

**Last Updated**: 2026-02-08
**Version**: 0.2.0-alpha
