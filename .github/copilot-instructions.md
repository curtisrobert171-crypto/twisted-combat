# Copilot Instructions — Empire of Glass

## Project Overview

Empire of Glass is a mobile game built in **Unity 6** (6000.0.23f1) using C# and the **Universal Render Pipeline (URP)**. The game combines three interconnected gameplay loops:

1. **Swarm** — A runner/multiplier loop where math gates multiply hero units into 500+ shardlings.
2. **Raid** — A PvP frequency-aim puzzle that shatters rival vaults for loot.
3. **City** — A meta-game where loot rebuilds a shattered 3D city using reverse-time shard assembly.

## Technology Stack

| Layer | Technology |
|-------|-----------|
| Engine | Unity 6 (6000.0.23f1) |
| Language | C# |
| Render Pipeline | Universal Render Pipeline (URP) 17.x |
| Input | Unity Input System 1.11.x |
| UI | TextMeshPro + Unity UI (uGUI) |
| Assembly Definition | `EmpireOfGlass.asmdef` |

## Coding Conventions

- **Namespaces**: Use `EmpireOfGlass.<Module>` (e.g., `EmpireOfGlass.Core`, `EmpireOfGlass.Swarm`, `EmpireOfGlass.City`, `EmpireOfGlass.Raid`, `EmpireOfGlass.Data`, `EmpireOfGlass.UI`, `EmpireOfGlass.Monetization`).
- **Class names**: PascalCase (e.g., `GameManager`, `HeroController`).
- **Private fields**: Use `[SerializeField]` for inspector-exposed privates; group with `[Header("Section")]`.
- **Properties**: Public getters with private setters (`public Type Prop { get; private set; }`).
- **Singletons**: Managers use `public static Instance { get; private set; }` with `DontDestroyOnLoad`.
- **Events**: Use `event System.Action` for decoupled communication.
- **Documentation**: XML summary comments (`/// <summary>`) on all public members.
- **No underscores** in field names; use camelCase for private fields.

## Architecture

```
Assets/Scripts/
├── Core/           # Game managers, bootstrap, camera, audio, haptics
├── Swarm/          # Swarm loop: math gates, shardling flocking, boss encounters
├── City/           # City building meta-game
├── Raid/           # Raid PvP loop
├── Data/           # Player data model and save/load (JSON)
├── UI/             # UI management
├── Monetization/   # In-app purchase and ad logic
└── EmpireOfGlass.asmdef
```

### Key Patterns

- **GameBootstrap** in the Boot scene initializes all managers via `DontDestroyOnLoad`.
- Scene navigation uses `UnityEngine.SceneManagement.SceneManager.LoadSceneAsync`.
- Player data is serialized as JSON via `PlayerData` / `SaveManager` in the `Data` module.

## Scenes

| Scene | Purpose |
|-------|---------|
| `Boot` | Splash screen, manager initialization |
| `SwarmPrototype` | Swarm runner gameplay |
| `CityPrototype` | City building meta-game |
| `RaidPrototype` | Raid PvP gameplay |

## Prefabs

See `Assets/Prefabs/PrefabManifest.md` for the full list of prototype prefabs, their scripts, interaction types, and polygon budgets.

## Adding New Code

1. Place scripts under the appropriate `Assets/Scripts/<Module>/` folder.
2. Use the matching `EmpireOfGlass.<Module>` namespace.
3. Follow the singleton pattern for new managers; register them in `GameBootstrap`.
4. Add XML summary documentation to all public classes and members.
5. Keep MonoBehaviour logic thin — prefer pure C# helper classes for testable logic.

## Testing

There is currently no automated test infrastructure. When adding tests, use Unity Test Framework (NUnit) and place them under `Assets/Tests/`.

## Build & Run

Open the project in **Unity 6000.0.23f1**. The entry scene is `Assets/Scenes/Boot.unity`. Use keys **1**, **2**, **3** in the editor to switch between Swarm, City, and Raid prototype scenes.
