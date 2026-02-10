# Performance Optimization Guide

## Overview
This guide covers optimization strategies for Empire of Glass, focusing on maintaining 60 FPS with 500+ shardlings and large-scale swarm mechanics.

## Performance Targets

### Frame Rate Goals
- **Desktop**: 60 FPS with 500 shardlings
- **Mobile**: 30 FPS with 300 shardlings
- **WebGL**: 30 FPS with 200 shardlings

### Memory Goals
- **Desktop**: < 2GB RAM
- **Mobile**: < 512MB RAM
- **WebGL**: < 512MB heap

## Profiling Tools

### Unity Profiler
```
Window → Analysis → Profiler
- CPU Usage
- GPU Usage
- Memory
- Rendering
- Physics
```

### Frame Debugger
```
Window → Analysis → Frame Debugger
- Draw call analysis
- Batch analysis
- Shader performance
```

### Memory Profiler Package
```
Window → Package Manager → Memory Profiler
- Heap snapshots
- Memory leaks
- Allocation tracking
```

## CPU Optimization

### 1. Swarm Management (Critical Path)

#### Current Implementation Issues
```csharp
// BEFORE: O(n²) neighbor search
foreach (var shardling in activeShardlings)
{
    foreach (var other in activeShardlings)
    {
        // Distance check for flocking
    }
}
```

#### Optimized Implementation
```csharp
// AFTER: Spatial partitioning with grid
private SpatialGrid spatialGrid;

void Update()
{
    // O(n) grid update
    spatialGrid.Clear();
    foreach (var shardling in activeShardlings)
    {
        spatialGrid.Add(shardling);
    }
    
    // O(n * k) where k = neighbors per cell
    foreach (var shardling in activeShardlings)
    {
        var neighbors = spatialGrid.GetNeighbors(shardling.Position, flockingRadius);
        shardling.UpdateFlocking(neighbors);
    }
}
```

### 2. Object Pooling

```csharp
public class ShardlingPool : MonoBehaviour
{
    [SerializeField] private GameObject shardlingPrefab;
    [SerializeField] private int poolSize = 500;
    
    private Queue<GameObject> pool = new Queue<GameObject>();
    
    void Start()
    {
        // Pre-instantiate pool
        for (int i = 0; i < poolSize; i++)
        {
            var obj = Instantiate(shardlingPrefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }
    
    public GameObject Spawn(Vector3 position)
    {
        if (pool.Count > 0)
        {
            var obj = pool.Dequeue();
            obj.transform.position = position;
            obj.SetActive(true);
            return obj;
        }
        return Instantiate(shardlingPrefab, position, Quaternion.identity);
    }
    
    public void Despawn(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
```

### 3. Update Loop Optimization

```csharp
// BEFORE: Every frame updates
void Update()
{
    UpdateFlocking();
    UpdatePhysics();
    UpdateAnimation();
}

// AFTER: Staggered updates
void Update()
{
    // Update 1/3 of shardlings per frame
    int startIdx = (Time.frameCount % 3) * (activeShardlings.Count / 3);
    int endIdx = startIdx + (activeShardlings.Count / 3);
    
    for (int i = startIdx; i < endIdx && i < activeShardlings.Count; i++)
    {
        activeShardlings[i].UpdateBehavior();
    }
}
```

## GPU Optimization

### 1. GPU Instancing

```csharp
// Enable GPU instancing in material
[Header("Rendering")]
[SerializeField] private Material instancedMaterial;

void Start()
{
    instancedMaterial.enableInstancing = true;
}

// Batch draw calls
void LateUpdate()
{
    Matrix4x4[] matrices = new Matrix4x4[activeShardlings.Count];
    for (int i = 0; i < activeShardlings.Count; i++)
    {
        matrices[i] = activeShardlings[i].transform.localToWorldMatrix;
    }
    
    Graphics.DrawMeshInstanced(
        shardlingMesh,
        0,
        instancedMaterial,
        matrices
    );
}
```

### 2. LOD System

```csharp
public class SwarmLODManager : MonoBehaviour
{
    [SerializeField] private int highDetailCount = 50;
    [SerializeField] private int mediumDetailCount = 200;
    
    void Update()
    {
        // Sort by distance to camera
        activeShardlings.Sort((a, b) => 
            Vector3.Distance(a.transform.position, Camera.main.transform.position)
            .CompareTo(Vector3.Distance(b.transform.position, Camera.main.transform.position))
        );
        
        // Apply LOD levels
        for (int i = 0; i < activeShardlings.Count; i++)
        {
            if (i < highDetailCount)
                activeShardlings[i].SetLOD(LODLevel.High);
            else if (i < mediumDetailCount)
                activeShardlings[i].SetLOD(LODLevel.Medium);
            else
                activeShardlings[i].SetLOD(LODLevel.Low);
        }
    }
}
```

### 3. Shader Optimization

```hlsl
// BEFORE: Complex per-pixel lighting
Shader "Custom/ShardlingExpensive"
{
    Pass
    {
        CGPROGRAM
        fixed4 frag(v2f i) : SV_Target
        {
            // Expensive per-pixel calculations
            float3 normal = normalize(i.normal);
            float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
            float diffuse = dot(normal, lightDir);
            
            // Specular, fresnel, etc.
            return CalculateComplexLighting();
        }
        ENDCG
    }
}

// AFTER: Simplified mobile-friendly shader
Shader "Custom/ShardlingOptimized"
{
    Pass
    {
        CGPROGRAM
        fixed4 frag(v2f i) : SV_Target
        {
            // Simple vertex color + ambient
            return i.color * _AmbientColor;
        }
        ENDCG
    }
}
```

### 4. Occlusion Culling

```
Window → Rendering → Occlusion Culling
- Bake occlusion data for static geometry
- Enable in Camera settings
- Set culling distance appropriately
```

## Physics Optimization

### 1. Reduce Physics Updates

```csharp
// Project Settings → Physics
Fixed Timestep: 0.02 (50 Hz instead of 60 Hz)
Maximum Allowed Timestep: 0.1

// Use kinematic bodies for simple movement
rigidbody.isKinematic = true;
rigidbody.MovePosition(newPosition);
```

### 2. Simplified Colliders

```csharp
// BEFORE: Mesh colliders (expensive)
gameObject.AddComponent<MeshCollider>();

// AFTER: Sphere colliders (cheap)
gameObject.AddComponent<SphereCollider>();
```

### 3. Layer-Based Collision

```
Edit → Project Settings → Physics → Layer Collision Matrix
- Disable unnecessary collision pairs
- Shardlings only collide with obstacles, not each other
```

## Memory Optimization

### 1. Asset Memory

```csharp
// Texture compression
- Use ASTC for mobile
- Use BC7 for desktop
- Maximum texture size: 2048x2048

// Audio compression
- Use Vorbis for music (quality: 70)
- Use ADPCM for SFX
- Stream long audio clips

// Mesh optimization
- Optimize mesh in import settings
- Reduce vertex count
- Remove unnecessary UV channels
```

### 2. Runtime Memory

```csharp
public class MemoryManager : MonoBehaviour
{
    [SerializeField] private float cleanupInterval = 60f;
    private float nextCleanup;
    
    void Update()
    {
        if (Time.time >= nextCleanup)
        {
            CleanupMemory();
            nextCleanup = Time.time + cleanupInterval;
        }
    }
    
    void CleanupMemory()
    {
        // Unload unused assets
        Resources.UnloadUnusedAssets();
        
        // Force garbage collection
        System.GC.Collect();
        
        Debug.Log($"Memory cleanup: {System.GC.GetTotalMemory(false) / 1024 / 1024} MB");
    }
}
```

### 3. Asset Bundles

```csharp
// Load assets on demand
IEnumerator LoadLevel(string levelName)
{
    var bundle = AssetBundle.LoadFromFileAsync($"Levels/{levelName}");
    yield return bundle;
    
    var levelAsset = bundle.assetBundle.LoadAssetAsync<GameObject>("Level");
    yield return levelAsset;
    
    Instantiate(levelAsset.asset);
    
    // Unload when done
    bundle.assetBundle.Unload(false);
}
```

## Rendering Optimization

### 1. Draw Call Batching

```
Edit → Project Settings → Player → Other Settings
- Static Batching: ✓
- Dynamic Batching: ✓ (for small meshes)
- GPU Instancing: ✓
```

### 2. Texture Atlasing

```csharp
// Combine multiple textures into atlas
- Use Sprite Atlas for UI
- Use Texture Packer for game sprites
- Share materials where possible
```

### 3. Shadow Optimization

```
Quality Settings → Shadows
- Shadow Distance: 50 (reduce for mobile)
- Shadow Cascades: 2 (reduce for mobile)
- Shadow Resolution: Medium
- Soft Shadows: Disabled (mobile)
```

## Dynamic Scaling System

```csharp
public class PerformanceScaler : MonoBehaviour
{
    [Header("Thresholds")]
    [SerializeField] private float targetFPS = 60f;
    [SerializeField] private float minAcceptableFPS = 30f;
    
    [Header("Scaling")]
    [SerializeField] private int maxShardlings = 500;
    [SerializeField] private int minShardlings = 100;
    
    private float[] fpsHistory = new float[60];
    private int historyIndex;
    
    void Update()
    {
        // Track FPS
        fpsHistory[historyIndex] = 1f / Time.deltaTime;
        historyIndex = (historyIndex + 1) % fpsHistory.Length;
        
        float avgFPS = CalculateAverageFPS();
        
        // Scale down if performance is poor
        if (avgFPS < minAcceptableFPS)
        {
            ScaleDown();
        }
        // Scale up if performance is good
        else if (avgFPS > targetFPS + 10f)
        {
            ScaleUp();
        }
    }
    
    void ScaleDown()
    {
        // Reduce max shardlings
        maxShardlings = Mathf.Max(minShardlings, maxShardlings - 50);
        
        // Reduce quality settings
        QualitySettings.DecreaseLevel();
        
        Debug.Log($"[PerformanceScaler] Scaling down: max shardlings = {maxShardlings}");
    }
    
    void ScaleUp()
    {
        maxShardlings = Mathf.Min(500, maxShardlings + 50);
        QualitySettings.IncreaseLevel();
        
        Debug.Log($"[PerformanceScaler] Scaling up: max shardlings = {maxShardlings}");
    }
    
    float CalculateAverageFPS()
    {
        float sum = 0f;
        foreach (float fps in fpsHistory)
        {
            sum += fps;
        }
        return sum / fpsHistory.Length;
    }
}
```

## Profiling Checklist

### Before Optimization
- [ ] Run Profiler for 5 minutes
- [ ] Record average FPS
- [ ] Note CPU bottlenecks
- [ ] Note GPU bottlenecks
- [ ] Check memory usage
- [ ] Count draw calls

### After Optimization
- [ ] Run same profiling session
- [ ] Compare FPS improvement
- [ ] Verify no new bottlenecks
- [ ] Check memory reduction
- [ ] Count draw call reduction
- [ ] Test on target devices

## Platform-Specific Tips

### Desktop (Windows/Mac/Linux)
- Target 60 FPS
- Can use higher quality settings
- More aggressive with effects

### Mobile (iOS/Android)
- Target 30 FPS
- Simplified shaders
- Reduced particle counts
- Lower texture resolutions

### WebGL
- Target 30 FPS
- No multithreading
- Aggressive asset compression
- Progressive loading

## Stress Testing

### Test Scenarios
1. **Max Swarm Test**: Spawn 500 shardlings instantly
2. **Sustained Play**: Play for 30 minutes continuously
3. **Rapid Transitions**: Switch scenes quickly
4. **Memory Stress**: Play multiple loops without restart

### Metrics to Capture
- FPS distribution (min, avg, max)
- Memory growth over time
- Load times per scene
- Draw call counts
- Physics update time

## Next Steps

1. Implement spatial partitioning for swarm
2. Add GPU instancing for shardlings
3. Create LOD system
4. Add performance scaler
5. Run stress tests
6. Optimize based on profiling
7. Test on target devices
8. Document final performance characteristics
