# WebGL Build & Deployment Guide

## Overview
This guide covers building and deploying Empire of Glass for WebGL distribution on itch.io and other platforms.

## Prerequisites

### Unity WebGL Support
1. Open Unity Hub
2. Install WebGL Build Support module for your Unity version
3. File → Build Settings → WebGL

### System Requirements
- Unity 2021.3 or later
- Modern web browser with WebAssembly support
- 4GB RAM minimum for building
- Stable internet connection for initial asset streaming

## Build Configuration

### Player Settings (Edit → Project Settings → Player → WebGL)

#### Resolution and Presentation
```
Default Screen Width: 1920
Default Screen Height: 1080
Run In Background: Enabled
WebGL Template: Default (or custom template)
```

#### Publishing Settings
```
Compression Format: Brotli (best compression)
Decompression Fallback: Enabled
Data Caching: Enabled
```

#### Optimization
```
Optimization Level: Fast (for testing) / Smallest (for production)
Code Optimization: IL2CPP
Managed Stripping Level: Medium (or High for production)
Enable Exceptions: None (for best performance)
```

#### Memory Settings
```
Memory Size: 512 MB (adjust based on testing)
Enable Memory Profiler: Only for debug builds
```

## Build Process

### Development Build
```bash
# Quick iteration build
1. File → Build Settings
2. Select WebGL platform
3. Check "Development Build"
4. Build
```

### Production Build
```bash
# Optimized release build
1. File → Build Settings
2. Select WebGL platform
3. Uncheck "Development Build"
4. Check "Autoconnect Profiler" OFF
5. Player Settings:
   - Strip Engine Code: ON
   - Managed Stripping Level: High
   - Compression Format: Brotli
6. Build
```

## Testing WebGL Build

### Local Testing
```bash
# Using Python (simplest method)
cd Build/WebGL
python -m http.server 8000

# Open browser to http://localhost:8000

# Using Node.js
npx http-server ./Build/WebGL -p 8000
```

### Browser Compatibility
Test on:
- [ ] Chrome 90+
- [ ] Firefox 88+
- [ ] Safari 14+
- [ ] Edge 90+
- [ ] Mobile browsers (iOS Safari, Chrome Mobile)

## Optimization for WebGL

### Asset Optimization
- **Textures**: Use compression (DXT/BC7 for desktop, ASTC for mobile)
- **Audio**: Use compressed formats (Vorbis for music, ADPCM for SFX)
- **Meshes**: Keep poly counts low (<5k per mesh)
- **Animations**: Compress animation clips

### Code Optimization
```csharp
// Avoid in WebGL builds
- Multithreading (use main thread only)
- Reflection (use IL2CPP-compatible code)
- Large allocations (causes garbage collection pauses)
- Synchronous network calls

// Prefer for WebGL
- Coroutines instead of async/await
- Object pooling for frequent allocations
- Asynchronous asset loading
- Progressive enhancement
```

### Memory Management
```csharp
// Aggressive cleanup for WebGL
Resources.UnloadUnusedAssets();
System.GC.Collect();
```

### Loading Strategy
```csharp
// Split loading into chunks
IEnumerator LoadGameAssets()
{
    yield return LoadEssentialAssets();
    ShowMainMenu();
    yield return LoadGameplayAssets();
    EnableGameplayButton();
}
```

## Itch.io Deployment

### Preparing for Upload
1. Build project to `Builds/WebGL/`
2. Compress build folder to ZIP
3. Ensure `index.html` is in root of ZIP

### Upload Steps
1. Go to itch.io → Dashboard → Create New Project
2. Project Settings:
   - **Kind of project**: HTML
   - **Upload**: Your WebGL.zip
   - **This file will be played in the browser**: ✓
   - **Embed options**: 
     - Width: 1920 (or 1280 for mobile-friendly)
     - Height: 1080 (or 720)
     - Fullscreen button: ✓
     - Mobile friendly: ✓ (if optimized)

### Itch.io Page Configuration
```
Title: Empire of Glass - WebGL Demo
Short description: Shatter glass, multiply swarms, raid rivals in this satisfying hybrid runner!

Tags:
- action
- runner
- strategy
- mobile
- experimental

Embed settings:
- Viewport dimensions: 1920x1080
- Enable fullscreen: Yes
- Mobile optimized: Yes
- Automatically start on page load: No (let user click to start)
```

## Performance Targets

### FPS Targets
- Desktop: 60 FPS minimum
- Mobile: 30 FPS minimum
- Swarm at 100 units: Maintain target FPS
- Swarm at 500 units: May drop to 30 FPS (acceptable)

### Load Time Targets
- Initial load: < 10 seconds
- Scene transitions: < 2 seconds
- Asset streaming: Background only

### Build Size Targets
- Initial download: < 50 MB (compressed)
- Total assets: < 200 MB
- Memory footprint: < 512 MB

## Common Issues & Solutions

### Issue: Build Size Too Large
**Solutions:**
- Enable Code Stripping (High)
- Use texture compression
- Remove unused assets
- Split scenes for progressive loading

### Issue: Long Load Times
**Solutions:**
- Enable asset bundle streaming
- Reduce initial asset count
- Use Addressables for on-demand loading
- Optimize texture sizes

### Issue: Low Frame Rate
**Solutions:**
- Reduce draw calls (use batching)
- Lower particle density
- Use simpler shaders
- Implement LOD system
- Reduce shadow quality

### Issue: Memory Errors
**Solutions:**
- Increase memory size in Player Settings
- Unload unused assets more aggressively
- Use object pooling
- Optimize texture memory

### Issue: Broken Input on Mobile
**Solutions:**
- Add touch input support
- Test with mobile simulators
- Ensure UI scales properly
- Add on-screen controls if needed

## Post-Launch Checklist

### Analytics Setup
- [ ] Integrate analytics (see Analytics.md)
- [ ] Track load times
- [ ] Monitor crash reports
- [ ] Track session duration
- [ ] Monitor FPS on various devices

### Bug Reporting
- [ ] Add in-game bug report button
- [ ] Include session info in reports
- [ ] Log console errors
- [ ] Capture system information

### Monitoring
- [ ] Check itch.io analytics daily
- [ ] Monitor player feedback
- [ ] Track crash rate
- [ ] Review performance reports

## Continuous Improvement

### Weekly Tasks
- Review player feedback
- Check crash logs
- Analyze load time distribution
- Monitor FPS across devices
- Update documentation

### Iteration Cycle
1. Collect feedback and metrics
2. Identify top 3 issues
3. Fix and test
4. Deploy new build
5. Announce update
6. Repeat

## WebGL Limitations

### What Works
✓ Basic gameplay
✓ 2D/3D graphics
✓ Audio playback
✓ Input (keyboard, mouse, touch)
✓ LocalStorage for saves
✓ REST API calls

### What Doesn't Work
✗ Multithreading
✗ Sockets (use WebSockets instead)
✗ File system access
✗ Native plugins
✗ Some .NET features

## Resources

### Tools
- **WebGL Build Size Analyzer**: Window → Analysis → Build Report
- **Chrome DevTools**: F12 → Network/Performance tabs
- **WebGL Profiler**: Enable in development builds

### Documentation
- Unity WebGL Documentation: https://docs.unity3d.com/Manual/webgl.html
- Itch.io Creator Guide: https://itch.io/docs/creators/html5
- WebAssembly Optimization: https://emscripten.org/docs/optimizing/

### Support
- Unity Forums: https://forum.unity.com/forums/web.60/
- Itch.io Forums: https://itch.io/community
- Discord: [Your game's Discord server]

## Next Steps

1. Complete Phase 6 performance optimizations
2. Build and test WebGL locally
3. Deploy to itch.io private page
4. Conduct closed beta testing
5. Address critical bugs
6. Launch public demo
7. Gather feedback and iterate
