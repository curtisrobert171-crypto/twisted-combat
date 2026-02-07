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
