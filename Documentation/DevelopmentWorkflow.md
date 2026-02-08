# Development Workflow Guide

## Overview
This guide outlines the recommended development workflow for contributing to Empire of Glass.

## Daily Development Cycle

### 1. Morning Routine
```bash
# Pull latest changes
git pull origin main

# Check for Unity version updates
# Open Unity Hub → Check for updates

# Run quick smoke test
# Open Boot.unity → Play mode → Test basic navigation
```

### 2. Feature Development
```bash
# Create feature branch
git checkout -b feature/your-feature-name

# Make incremental changes
# Commit frequently with descriptive messages
git add .
git commit -m "Add: descriptive message"

# Push to remote
git push origin feature/your-feature-name
```

### 3. Testing Cycle
```bash
# Run unit tests
Unity → Window → General → Test Runner → Run All (EditMode)

# Run integration tests
Unity → Test Runner → Run All (PlayMode)

# Manual testing
# Play through affected game loop
# Check for visual/audio issues
# Monitor console for errors
```

### 4. Code Review
```bash
# Create pull request
gh pr create --title "Feature: Your Feature" --body "Description"

# Address review feedback
# Make requested changes
git add .
git commit -m "Fix: address review feedback"
git push
```

## Best Practices

### Git Commit Messages
```
Format: <Type>: <Subject>

Types:
- Add: New feature or file
- Fix: Bug fix
- Update: Modification to existing feature
- Remove: Deletion of feature or file
- Refactor: Code restructuring
- Test: Adding or updating tests
- Docs: Documentation changes
- Style: Formatting, no code change
- Perf: Performance improvement

Examples:
Add: swarm LOD system for performance
Fix: hero health not updating in HUD
Update: math gate multiplier from 2x to 3x
Refactor: extract analytics into separate manager
Test: add unit tests for GameManager state transitions
```

### Branch Naming
```
feature/feature-name  - New features
fix/bug-name          - Bug fixes
refactor/component    - Code refactoring
test/test-suite       - Test additions
docs/topic            - Documentation

Examples:
feature/leaderboard-ui
fix/swarm-count-overflow
refactor/hero-controller
test/boss-defeat-mechanics
docs/backend-integration
```

### Code Style

#### C# Naming Conventions
```csharp
// PascalCase for public members
public class GameManager { }
public void StartGame() { }
public int MaxHealth { get; set; }

// camelCase for private members
private int currentHealth;
private float moveSpeed;

// camelCase for parameters
public void TakeDamage(int damageAmount) { }

// UPPER_CASE for constants
private const int MAX_SHARDLINGS = 500;
private const string API_BASE_URL = "https://api.example.com";

// Prefix interfaces with I
public interface IPoolable { }

// Prefix private fields with underscore (optional, if preferred)
private int _health;  // or just: private int health;
```

#### Unity-Specific Conventions
```csharp
// Serialize fields for Inspector
[SerializeField] private float moveSpeed = 5f;

// Header for grouping in Inspector
[Header("Movement")]
[SerializeField] private float walkSpeed = 5f;
[SerializeField] private float runSpeed = 10f;

[Header("Combat")]
[SerializeField] private int maxHealth = 100;

// Tooltip for documentation
[Tooltip("Maximum number of shardlings in swarm")]
[SerializeField] private int maxShardlings = 500;

// Range for slider in Inspector
[Range(0f, 1f)]
[SerializeField] private float volume = 0.8f;
```

### File Organization
```
Assets/
├── Scenes/                    # Unity scene files
│   ├── Boot.unity
│   ├── SwarmPrototype.unity
│   └── README.md
├── Scripts/
│   ├── Core/                  # Core game systems
│   ├── Swarm/                 # Swarm mechanics
│   ├── City/                  # City building
│   ├── Raid/                  # Raid system
│   ├── UI/                    # User interface
│   ├── Data/                  # Data management
│   ├── Analytics/             # Telemetry
│   ├── Monetization/          # IAP/Ads
│   └── Backend/               # API integration
├── Prefabs/                   # Reusable GameObjects
│   ├── Prototype/             # Placeholder prefabs
│   ├── Hero/                  # Hero prefabs
│   ├── Environment/           # Environment prefabs
│   └── City/                  # City building prefabs
├── Tests/                     # Automated tests
│   ├── EditMode/              # Unit tests
│   └── PlayMode/              # Integration tests
├── Materials/                 # Material assets
├── Textures/                  # Texture assets
└── Audio/                     # Audio assets

Documentation/                 # Project documentation
├── PlaytestGuide.md
├── Analytics.md
├── WebGLDeployment.md
├── PerformanceOptimization.md
├── BackendIntegration.md
└── DevelopmentWorkflow.md
```

## Code Review Checklist

### Before Creating PR
- [ ] Code compiles without errors
- [ ] No console warnings in Play mode
- [ ] All tests pass
- [ ] Code follows style guide
- [ ] Added comments for complex logic
- [ ] Updated relevant documentation
- [ ] Tested on target platform
- [ ] No debug code left in
- [ ] Git commit messages are descriptive

### Reviewer Checklist
- [ ] Code is readable and maintainable
- [ ] Logic is correct and efficient
- [ ] No obvious bugs or edge cases
- [ ] Tests cover new functionality
- [ ] Documentation is updated
- [ ] Performance impact is acceptable
- [ ] Security considerations addressed
- [ ] Follows project architecture

## Performance Testing

### Quick Performance Check
```csharp
// Add to any Update() method temporarily
void Update()
{
    if (Input.GetKeyDown(KeyCode.P))
    {
        Debug.Log($"FPS: {1f / Time.deltaTime}");
        Debug.Log($"Draw Calls: {UnityStats.drawCalls}");
        Debug.Log($"Triangles: {UnityStats.triangles}");
        Debug.Log($"Batches: {UnityStats.batches}");
    }
}
```

### Profiler Snapshots
```bash
# Take profiler snapshot
Unity → Window → Analysis → Profiler
# Deep Profile → Record → Play for 30s → Stop
# Save snapshot: Profiler → Save → MyFeature_Profile.data

# Compare before/after
# Open baseline: Profiler → Load → Baseline.data
# Open current: Profiler → Load → MyFeature_Profile.data
# Compare CPU/GPU/Memory metrics
```

## Debugging Tips

### Common Issues

#### Issue: "Missing Reference" in Inspector
```
Solution:
1. Check if prefab/scene was saved
2. Verify serialized field is public or has [SerializeField]
3. Rebuild prefab connections
```

#### Issue: Script not showing up on GameObject
```
Solution:
1. Ensure class inherits from MonoBehaviour
2. Check for compilation errors
3. Verify script is in Assets/ folder
4. Restart Unity if needed
```

#### Issue: Tests failing unexpectedly
```
Solution:
1. Check test isolation (proper setup/teardown)
2. Verify no global state pollution
3. Check for timing issues (add yield statements)
4. Run tests individually to isolate problem
```

### Debug Tools
```csharp
// Visual debug rays
Debug.DrawRay(transform.position, direction, Color.red, 2f);

// Conditional breakpoints
if (health < 0)
    Debug.Break();  // Pauses editor

// Performance markers
UnityEngine.Profiling.Profiler.BeginSample("MyExpensiveFunction");
MyExpensiveFunction();
UnityEngine.Profiling.Profiler.EndSample();

// Assert conditions
Debug.Assert(health >= 0, "Health cannot be negative!");

// Log with context
Debug.Log($"[{GetType().Name}] Health: {health}", gameObject);
```

## Unity Editor Tips

### Keyboard Shortcuts
```
Ctrl/Cmd + P     - Play mode
Ctrl/Cmd + D     - Duplicate
F                - Frame selected object
Shift + F        - Lock view to selected
Ctrl/Cmd + Shift + F  - Frame selected and lock
W, E, R          - Move, Rotate, Scale tools
V                - Vertex snap
Q                - Hand tool
Z                - Pan view
Alt + LMB        - Orbit camera
Alt + RMB        - Zoom
```

### Scene View Tools
```
2D/3D Toggle     - Switch between 2D and 3D view
Gizmos           - Toggle visibility of icons/gizmos
Shading Mode     - Shaded/Wireframe/Textured
Effects          - Skybox, Fog, Flares, etc.
```

### Productivity Tools
```
Window → Layouts - Save custom layouts
Window → Search  - Universal search (Ctrl/Cmd + K)
Edit → Shortcuts - Customize keybindings
```

## Continuous Integration

### Automated Checks (Future)
```yaml
# .github/workflows/unity-tests.yml
name: Unity Tests
on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - uses: game-ci/unity-test-runner@v2
        with:
          projectPath: .
          testMode: all
      - uses: game-ci/unity-builder@v2
        with:
          targetPlatform: WebGL
```

## Documentation Standards

### Code Comments
```csharp
/// <summary>
/// Brief description of what this class/method does.
/// </summary>
/// <param name="paramName">What this parameter represents</param>
/// <returns>What the method returns</returns>
public int CalculateDamage(int swarmCount)
{
    // Explain WHY, not WHAT (code shows what)
    // Use math gate bonus if player has powerup active
    int bonus = hasPowerup ? 10 : 0;
    return swarmCount + bonus;
}
```

### README Files
```markdown
# Component Name

## Purpose
Brief description of what this component does and why it exists.

## Usage
Code example of how to use this component.

## Dependencies
List of required components or systems.

## Known Issues
Current limitations or bugs.
```

## Getting Help

### Resources
- **Unity Documentation**: https://docs.unity3d.com/
- **Unity Forums**: https://forum.unity.com/
- **Unity Answers**: https://answers.unity.com/
- **Project README**: `/README.md`
- **Architecture Docs**: `/Documentation/`

### Team Communication
- **GitHub Issues**: For bugs and feature requests
- **Pull Request Comments**: For code-specific discussions
- **Documentation**: For how-to guides and references

## Weekly Maintenance

### Monday
- [ ] Review open issues
- [ ] Prioritize week's tasks
- [ ] Update project board

### Wednesday
- [ ] Mid-week sync
- [ ] Address blockers
- [ ] Code review catch-up

### Friday
- [ ] Close completed issues
- [ ] Update documentation
- [ ] Merge ready PRs
- [ ] Plan next week

## Release Process

### Pre-Release Checklist
- [ ] All tests passing
- [ ] No critical bugs
- [ ] Documentation updated
- [ ] Performance meets targets
- [ ] Build succeeds on all platforms
- [ ] Playtest feedback addressed

### Release Steps
1. Create release branch: `release/v1.0.0`
2. Update version in ProjectSettings
3. Build all target platforms
4. Run final QA pass
5. Tag release: `git tag v1.0.0`
6. Push to production
7. Create release notes
8. Announce to community

## Continuous Improvement

### Retrospectives
- Monthly: What went well? What can improve?
- Document learnings
- Update processes
- Share knowledge

### Metrics to Track
- Build times
- Test coverage
- Bug rate
- Iteration speed
- Player feedback themes
