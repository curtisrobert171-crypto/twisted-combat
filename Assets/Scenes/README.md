# Prototype Scenes and Prefabs

This directory contains prototype-ready Unity scenes and placeholder prefabs for the Empire of Glass game loops.

## Scenes (Assets/Scenes/)

### Boot.unity
- Entry point scene with GameBootstrap and BootSceneController
- Press 1 to load SwarmPrototype
- Press 2 to load CityPrototype  
- Press 3 to load RaidPrototype
- Initializes GameManager and core systems

### SwarmPrototype.unity
- Complete swarm gameplay loop setup
- Contains: Hero (Capsule), SwarmController, MathGate_x2, ObstacleBarrier, Boss (large Cube)
- Ground plane for navigation
- Camera positioned for runner view

### CityPrototype.unity
- City building meta-game loop
- Contains: CityBuilder with grid system
- Placeholder buildings showing three states:
  - Building_Ruin (low height, damaged)
  - Building_Construction (medium height, in progress)
  - Building_Completed (tall, finished)
- Camera positioned for god-view

### RaidPrototype.unity
- Raid gameplay loop setup
- Contains: RaidManager with enemy base cube
- Camera positioned for raid view
- Debug output for raid testing

## Prefabs (Assets/Prefabs/Prototype/)

All prefabs use Unity primitive meshes (Capsule, Sphere, Cube, Plane) as placeholders.

### Hero/ 
- **Hero.prefab** - Capsule primitive with HeroController
- **Shardling.prefab** - Small sphere primitive with ShardlingBehavior
- **Boss.prefab** - Large cube primitive with BossController

### Environment/
- **MathGate_x2.prefab** - Thin cube with trigger collider, MathGate (multiply x2)
- **MathGate_x5.prefab** - Thin cube with trigger collider, MathGate (multiply x5)
- **ObstacleBarrier.prefab** - Wide cube with ObstacleBarrier component

### City/
- **Building_Ruin.prefab** - Short cube (height 1)
- **Building_Construction.prefab** - Medium cube (height 3)
- **Building_Completed.prefab** - Tall cube (height 5)

## Usage

1. Open Boot.unity scene in Unity
2. Enter Play mode
3. Use keyboard (1, 2, or 3) to navigate between prototype scenes
4. Each scene demonstrates its respective game loop with placeholder visuals

## Next Steps

- Replace primitive meshes with actual 3D models
- Add materials and shaders for glass/neon effects
- Implement VFX for gate interactions, building assembly, raid effects
- Add UI overlays for each scene
- Connect scenes to save/load progression system
