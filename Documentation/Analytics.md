# Analytics & Telemetry Framework

## Overview
This document outlines the analytics system for tracking player behavior, performance metrics, and gameplay balance data.

## Analytics Events

### Session Events
```csharp
// Session start
Analytics.SessionStart(sessionId, timestamp, deviceInfo)

// Session end
Analytics.SessionEnd(sessionId, duration, reason)
```

### Gameplay Events
```csharp
// Core loop transitions
Analytics.StateTransition(fromState, toState, timestamp)

// Swarm loop events
Analytics.SwarmStart(initialCount)
Analytics.MathGateHit(gateType, multiplier, beforeCount, afterCount)
Analytics.BossEncounter(bossHP, swarmCount)
Analytics.BossDefeated(finalSwarmCount, timeElapsed)
Analytics.SwarmFailed(reason, swarmCount, progressPercent)

// City loop events
Analytics.BuildingUpgraded(buildingType, level, resourceCost)
Analytics.CityResourceGained(resourceType, amount, source)

// Raid loop events
Analytics.RaidStarted(targetLevel, availableEnergy)
Analytics.RaidCompleted(success, lootGained, accuracyPercent)
```

### Monetization Events
```csharp
// IAP tracking
Analytics.PurchaseAttempted(productId, price)
Analytics.PurchaseCompleted(productId, revenue, currency)
Analytics.PurchaseFailed(productId, reason)

// Ad events
Analytics.AdOffered(adType, context)
Analytics.AdWatched(adType, completion, reward)
Analytics.AdSkipped(adType, reason)

// Revive mechanics (Loss Aversion - Var 24)
Analytics.ReviveOffered(progressPercent, deathCount)
Analytics.ReviveAccepted(method) // "ad", "iap", "free"
Analytics.ReviveDeclined()
```

### Performance Events
```csharp
// Performance metrics
Analytics.FrameRateSnapshot(avgFPS, minFPS, swarmCount)
Analytics.LoadTime(sceneName, duration)
Analytics.MemoryUsage(totalMB, heapMB)
```

### Retention Events
```csharp
// Retention tracking
Analytics.DayNReturn(dayNumber, loginCount)
Analytics.LoopCompletion(loopNumber, duration)
Analytics.SessionsPerDay(count, totalTime)
```

## Key Metrics to Track

### Core Loop Health
- **Swarm Satisfaction Score**: Average multiplier per run
- **Loop Completion Rate**: % of started loops that complete
- **Average Loop Duration**: Time per Swarm → City → Raid cycle
- **Retry Rate**: % of players who retry after failure

### Balance Metrics
- **Death Points**: Where players most commonly fail
- **Math Gate Distribution**: Which gates are hit most/least
- **Resource Economy**: Income vs. spending rates
- **Progression Speed**: Time to reach each milestone

### Monetization Metrics
- **Conversion Rate**: % of players who make first purchase
- **ARPDAU**: Average Revenue Per Daily Active User
- **Ad Engagement**: % of offered ads that are watched
- **Revive Acceptance Rate**: % who pay/watch to continue

### Technical Health
- **Crash Rate**: Crashes per 1000 sessions
- **Frame Rate Distribution**: % of players at 30/60/120+ FPS
- **Load Times**: P50, P95, P99 percentiles
- **Device Breakdown**: Performance by device tier

## Implementation Plan

### Phase 1: Core Events (Week 1)
- [x] Session tracking
- [x] State transition events
- [x] Basic gameplay events
- [ ] Local logging system

### Phase 2: Monetization (Week 2)
- [ ] IAP event tracking
- [ ] Ad event tracking
- [ ] Revive funnel analytics

### Phase 3: Performance (Week 3)
- [ ] FPS monitoring
- [ ] Memory profiling
- [ ] Load time tracking

### Phase 4: Backend Integration (Week 4)
- [ ] Analytics service integration (e.g., Unity Analytics, Firebase)
- [ ] Data pipeline setup
- [ ] Dashboard creation

## Data Privacy

### GDPR Compliance
- All analytics are opt-in on first launch
- User consent required before any data collection
- Clear privacy policy link in settings
- Data deletion available on request

### Data Collected
- **Anonymous**: Device specs, gameplay metrics, performance data
- **Never Collected**: Personal information, contacts, location

## Analytics Dashboard

### Real-Time Metrics
- Current active users
- Live crash reports
- Server status

### Daily Reports
- DAU/MAU/WAU
- Retention curves (D1, D7, D30)
- Revenue and ARPDAU
- Top crash types

### Weekly Deep Dive
- Cohort analysis
- A/B test results
- Balance changes impact
- Feature adoption rates

## Testing Analytics

### Development Mode
```csharp
// Enable verbose logging
Analytics.SetDebugMode(true);

// Test events fire correctly
Analytics.TestEvent("SwarmStart");

// Validate data format
Analytics.ValidateSchema();
```

### QA Checklist
- [ ] All critical events fire
- [ ] Event parameters are correct
- [ ] No PII in event data
- [ ] Events batch correctly
- [ ] Offline queueing works
- [ ] Privacy consent is enforced

## Analytics Tools Integration

### Unity Analytics (Built-in)
- Free tier available
- Easy integration
- Limited customization

### Firebase Analytics (Recommended)
- Free unlimited events
- Cross-platform support
- Integrates with other Firebase services

### Custom Backend
- Full control
- Can correlate with server data
- Requires infrastructure

## Next Steps

1. **Implement AnalyticsManager.cs**: Central analytics controller
2. **Add telemetry hooks**: Insert Analytics calls in existing scripts
3. **Create analytics dashboard**: Set up data visualization
4. **Run playtest with analytics**: Validate data collection
5. **Iterate on events**: Add/remove based on needs

## Related Documentation
- `PlaytestGuide.md` - Manual feedback collection
- `PerformanceOptimization.md` - Performance profiling guide
- `BackendIntegration.md` - Server-side analytics
