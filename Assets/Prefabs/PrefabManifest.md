# Empire of Glass — Prefab Manifest

Vertical Slice prefab list as specified in the GDD. All prefabs are located under `Assets/Prefabs/`.

## 1. Core Unit Prefabs (`Assets/Prefabs/Hero/`)

| Prefab Name        | Script            | Description                                      | Poly Budget |
|--------------------|-------------------|--------------------------------------------------|-------------|
| `Hero.prefab`      | HeroController    | Fractured-light hero with glowing silhouette      | 5,000       |
| `Shardling.prefab` | ShardlingBehavior  | Physics-based swarm unit (GPU instanced)          | 50          |
| `Boss.prefab`      | BossController    | Obsidian boss with destructible HP wall            | 8,000       |

## 2. Environment Module Prefabs (`Assets/Prefabs/Environment/`)

| Prefab Name              | Script           | Interaction          | Poly Budget |
|--------------------------|------------------|----------------------|-------------|
| `GlassFloor_10m.prefab`  | ObstacleBarrier  | Shatter on impact     | 200         |
| `ObsidianWall.prefab`    | ObstacleBarrier  | Block / reduce swarm  | 500         |
| `RefractionGate_x2.prefab` | MathGate       | Multiply swarm x2     | 300         |
| `RefractionGate_x5.prefab` | MathGate       | Multiply swarm x5     | 300         |
| `TrapBarrier.prefab`     | ObstacleBarrier  | Damage + slow          | 400         |

## 3. City State Variant Prefabs (`Assets/Prefabs/City/`)

| Prefab Name                 | Script       | State         | Poly Budget |
|-----------------------------|-------------|---------------|-------------|
| `Building_Ruin.prefab`      | CityBuilder | Ruin           | 1,000       |
| `Building_Construction.prefab` | CityBuilder | Construction | 2,000       |
| `Building_Completed.prefab` | CityBuilder | Completed      | 3,000       |

## Prototype Prefabs (`Assets/Prefabs/Prototype/`)

Placeholder prefabs using Unity primitives for rapid prototyping. These are simple stand-ins for the vertical slice objects.

### Hero Placeholders (`Assets/Prefabs/Prototype/Hero/`)
- **Hero** - Capsule primitive (cyan color)
- **Shardling** - Sphere primitive (blue color, small scale)
- **Boss** - Cube primitive (red color, large scale)

### Environment Placeholders (`Assets/Prefabs/Prototype/Environment/`)
- **MathGate_x2** - Cube primitive (green color, trigger)
- **MathGate_x5** - Cube primitive (yellow color, trigger)
- **ObstacleBarrier** - Cube primitive (gray color)
- **GlassFloor** - Plane primitive (transparent cyan)

### City Building Placeholders (`Assets/Prefabs/Prototype/City/`)
- **Building_Ruin** - Small cube primitive (dark gray, damaged appearance)
- **Building_Construction** - Medium cube primitive (orange, partially complete)
- **Building_Completed** - Tall cube primitive (white, full height)

## Scenes

Prototype scenes are located under `Assets/Scenes/`:
- **Boot.unity** - Initial scene with GameBootstrap and scene navigation
- **SwarmPrototype.unity** - Swarm loop with Hero, SwarmController, MathGates, obstacles, and Boss
- **CityPrototype.unity** - City meta-game with CityBuilder and building placeholders
- **RaidPrototype.unity** - Raid loop with RaidManager and enemy base
