# Empire of Glass Test Suite

## Overview
This directory contains automated tests for the Empire of Glass game, covering core gameplay mechanics, state management, and integration scenarios.

## Test Structure

### EditMode Tests (`EditMode/`)
Unit tests that run in Unity's Edit Mode without entering Play Mode. These tests are fast and ideal for testing individual components in isolation.

**Test Files:**
- `GameManagerTests.cs` - Tests for state transitions and forced rotation loop
- `HeroControllerTests.cs` - Tests for hero health, damage, and revival mechanics
- `SwarmControllerTests.cs` - Tests for swarm math operations and energy calculations

### PlayMode Tests (`PlayMode/`)
Integration tests that run in Unity's Play Mode. These tests validate behavior in a runtime environment.

**Test Files:**
- `GameLoopIntegrationTests.cs` - Tests for cross-scene persistence and state transition events

## Running Tests

### In Unity Editor
1. Open Window → General → Test Runner
2. Select EditMode or PlayMode tab
3. Click "Run All" or select specific tests to run

### From Command Line
```bash
# Run all EditMode tests
Unity -runTests -batchmode -projectPath . -testPlatform editmode -testResults ./TestResults-EditMode.xml

# Run all PlayMode tests
Unity -runTests -batchmode -projectPath . -testPlatform playmode -testResults ./TestResults-PlayMode.xml
```

## Test Coverage

### Core Mechanics (Phase 2 Priority)
- ✅ GameManager state transitions
- ✅ Forced rotation loop (Swarm → City → Raid → Swarm)
- ✅ Hero health and damage system
- ✅ Hero revival mechanics (Loss Aversion - Var 24)
- ✅ Swarm energy calculation for raid system
- 🔄 Swarm multiplication mechanics (pending prefab setup)
- 🔄 Math gate operations (pending prefab setup)
- 🔄 Damage calculations against enemies (pending prefab setup)

### Integration Tests
- ✅ GameManager persistence across scenes
- ✅ State transition event system
- ✅ Hero damage in play mode

## Writing New Tests

### EditMode Test Template
```csharp
using NUnit.Framework;
using UnityEngine;
using EmpireOfGlass.YourNamespace;

namespace EmpireOfGlass.Tests.EditMode
{
    [TestFixture]
    public class YourComponentTests
    {
        private GameObject testObject;
        private YourComponent component;

        [SetUp]
        public void SetUp()
        {
            testObject = new GameObject("Test");
            component = testObject.AddComponent<YourComponent>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(testObject);
        }

        [Test]
        public void YourTest_DoesExpectedThing()
        {
            // Arrange
            // Act
            // Assert
        }
    }
}
```

### PlayMode Test Template
```csharp
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EmpireOfGlass.Tests.PlayMode
{
    public class YourIntegrationTests
    {
        [UnityTest]
        public IEnumerator YourTest_WorksAtRuntime()
        {
            // Arrange
            yield return null;
            
            // Act
            yield return null;
            
            // Assert
        }
    }
}
```

## Best Practices

1. **Isolation**: Each test should be independent and not rely on other tests
2. **Setup/Teardown**: Always clean up created GameObjects in TearDown
3. **Descriptive Names**: Use clear test names that describe what's being tested
4. **Minimal Scope**: Test one thing per test method
5. **Fast Tests**: Keep EditMode tests fast; use PlayMode only when necessary

## CI/CD Integration
Tests are designed to run in automated CI/CD pipelines. See `.github/workflows/` for GitHub Actions configuration.

## Future Test Coverage

### Phase 2 Expansion
- [ ] MathGate trigger interactions
- [ ] ObstacleBarrier collision detection
- [ ] BossController health and defeat mechanics
- [ ] CityBuilder grid system and building placement
- [ ] RaidManager combat loop and loot generation
- [ ] SaveManager data persistence
- [ ] UIManager overlay and tooltip systems

### Phase 3-4 Tests
- [ ] Particle effects validation
- [ ] Animation controller state machines
- [ ] UI responsiveness and juice effects
- [ ] Performance benchmarks for 500+ shardlings

### Phase 5 Tests
- [ ] Backend API integration mocks
- [ ] Leaderboard data validation
- [ ] PvP matchmaking logic
- [ ] Network error handling

### Phase 6 Tests
- [ ] LOD system performance
- [ ] Dynamic scaling validation
- [ ] Frame rate consistency tests
- [ ] Memory usage benchmarks

## Troubleshooting

### Test Runner Not Showing Tests
- Ensure assembly definitions (.asmdef) are properly configured
- Check that test files are in the correct namespaces
- Restart Unity Editor if tests don't appear

### Tests Failing in CI But Passing Locally
- Verify batch mode compatibility
- Check for timing-dependent tests that need yield statements
- Ensure all dependencies are available in CI environment

### Missing References Error
- Add required assemblies to .asmdef files
- Ensure scripts are in correct namespaces
- Check that referenced components exist in the project
