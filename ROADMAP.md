# Empire of Glass - Implementation Roadmap

## Completed Tasks ✓

### Phase 1: Playtest & Feedback Foundation
- [x] Playtest documentation with feedback forms
- [x] Analytics framework for telemetry
- [x] Session tracking system
- [x] Performance metrics foundation

### Phase 2: Test Infrastructure
- [x] Unity Test Framework setup (EditMode + PlayMode)
- [x] Unit tests for GameManager (state transitions)
- [x] Unit tests for HeroController (health, damage, revival)
- [x] Unit tests for SwarmController (swarm management)
- [x] Integration tests for game loop
- [x] Test documentation and guidelines

### Phase 4: UI Framework
- [x] GameHUD component with health bar
- [x] Swarm count display
- [x] Energy meter
- [x] Game state display
- [x] Color-coded health indicators

### Phase 8: WebGL Foundation
- [x] WebGL deployment documentation
- [x] Build optimization guide
- [x] Itch.io deployment guide
- [x] Performance target definitions

### Documentation
- [x] Playtest Guide
- [x] Analytics Framework
- [x] Performance Optimization Guide
- [x] WebGL Deployment Guide
- [x] Backend Integration Guide
- [x] Development Workflow Guide

## In Progress 🔄

### Phase 1: Core Loop Polish
- [ ] Integrate analytics hooks into existing scripts
- [ ] Balance math gate multipliers
- [ ] Implement difficulty tuning system
- [ ] Add satisfaction curve metrics

### Phase 2: CI/CD Setup
- [ ] GitHub Actions workflow for tests
- [ ] Automated build pipeline
- [ ] Test result reporting

## Planned 📋

### Phase 3: Art & Visual Upgrades
**Priority: Medium** | **Effort: High** | **Dependency: Art Team**

Tasks:
- [ ] Define art requirements document
- [ ] Create art asset pipeline
- [ ] Implement glass/neon shader system
- [ ] Add particle effects for gate interactions
- [ ] Create animation controllers
- [ ] Replace primitive placeholders with final art

Deliverables:
- Art specification document
- Shader library (glass, neon, crystalline)
- Particle effect prefabs (gate hit, shatter, multiplier)
- Animation state machines for hero/enemies
- Final art assets integrated

### Phase 4: UI Polish (Complete)
**Priority: High** | **Effort: Medium** | **Dependency: Phase 1**

Remaining Tasks:
- [ ] Implement tooltip system
- [ ] Add juice effects (screen shake, chromatic aberration)
- [ ] Polish scene transitions with animations
- [ ] Add audio-visual feedback for actions
- [ ] Create pause menu
- [ ] Add settings UI

### Phase 5: Backend Integration
**Priority: High** | **Effort: High** | **Dependency: Infrastructure Team**

Tasks:
- [ ] Set up Google Cloud project
- [ ] Deploy Cloud Run API backend
- [ ] Implement Unity API client (see BackendIntegration.md)
- [ ] Add authentication flow
- [ ] Implement cloud save system
- [ ] Create leaderboard UI
- [ ] Add PvP matchmaking
- [ ] Implement raid system backend
- [ ] Add live events system

Deliverables:
- Backend API deployed and documented
- Unity APIClient fully implemented
- Cloud save/load working
- Leaderboards functional
- PvP matchmaking operational

### Phase 6: Performance Optimization
**Priority: Critical** | **Effort: High** | **Dependency: Phase 1-4**

Tasks:
- [ ] Implement spatial partitioning for swarm
- [ ] Add GPU instancing for shardlings
- [ ] Create LOD system (high/medium/low detail)
- [ ] Implement dynamic performance scaling
- [ ] Add object pooling system
- [ ] Optimize shaders for mobile
- [ ] Implement occlusion culling
- [ ] Create performance benchmark suite
- [ ] Stress test with 500+ shardlings
- [ ] Profile and optimize hotspots

Deliverables:
- 60 FPS on desktop with 500 shardlings
- 30 FPS on mobile with 300 shardlings
- Memory under 512MB on mobile
- Optimized shader library
- Performance benchmark results

### Phase 7: Monetization MVP
**Priority: Low** | **Effort: Medium** | **Dependency: Phase 5, Legal Review**

Note: Only if launching as paid product. Implement after core is stable.

Tasks:
- [ ] Create monetization strategy document
- [ ] Implement IAP framework
- [ ] Add ad integration (Unity Ads or AdMob)
- [ ] Create reward video system
- [ ] Implement revive offers (Loss Aversion)
- [ ] Add piggy bank system
- [ ] Create battle pass UI
- [ ] Implement gacha/loot box system
- [ ] Add store UI
- [ ] Analytics for monetization events
- [ ] A/B testing framework

Deliverables:
- IAP store functional
- Ad integration working
- Monetization analytics dashboard
- Conversion funnel tracking
- ARPDAU monitoring

### Phase 8: Launch Preparation
**Priority: Critical** | **Effort: Medium** | **Dependency: Phases 1-6**

Tasks:
- [ ] Configure WebGL build settings
- [ ] Optimize for web performance
- [ ] Set up crash reporting (Sentry/Backtrace)
- [ ] Add in-game bug report button
- [ ] Create itch.io page
- [ ] Deploy WebGL build to itch.io
- [ ] Set up analytics dashboard
- [ ] Create launch announcement
- [ ] Prepare press kit
- [ ] Set up community Discord
- [ ] Plan marketing campaign

Deliverables:
- WebGL build on itch.io (public or private)
- Crash reporting operational
- Analytics dashboard live
- Community channels established
- Launch materials ready

## Success Criteria

### Phase 1: Playtest Success
- ✓ Playtest documentation complete
- Collect feedback from 10+ playtesters
- Average fun rating > 7/10
- Clear understanding of core mechanics
- No critical bugs reported

### Phase 2: Test Success
- ✓ Test framework operational
- Test coverage > 60% for core systems
- All tests passing in CI/CD
- No regressions in test suite

### Phase 3: Art Success
- All placeholder art replaced
- Visual style consistent and appealing
- Frame rate maintained with final art
- Positive feedback on aesthetics

### Phase 4: UI Success
- ✓ HUD displays all critical information
- All UI responsive and readable
- Tooltips explain systems clearly
- Positive feedback on clarity

### Phase 5: Backend Success
- 99.9% API uptime
- < 100ms API response times
- Leaderboards update in real-time
- Cloud saves working reliably
- PvP matchmaking < 10s wait

### Phase 6: Performance Success
- 60 FPS on mid-tier desktop (GTX 1060+)
- 30 FPS on mid-tier mobile (iPhone 12+)
- Memory under targets
- No stuttering or hitching
- Smooth scene transitions

### Phase 7: Monetization Success (If Applicable)
- 5% conversion rate on first purchase
- ARPDAU > $0.50
- 80%+ ad completion rate
- Positive sentiment on pricing
- No complaints about "pay to win"

### Phase 8: Launch Success
- < 1% crash rate
- DAU > 100 players (first week)
- Average session > 5 minutes
- 30% D1 retention
- Positive community feedback

## Timeline Estimates

**Conservative Estimate:**
- Phase 1 completion: 2 weeks
- Phase 2 completion: 1 week  
- Phase 3 completion: 4 weeks (art dependent)
- Phase 4 completion: 2 weeks
- Phase 5 completion: 3 weeks
- Phase 6 completion: 3 weeks
- Phase 7 completion: 2 weeks (if needed)
- Phase 8 completion: 1 week

**Total: 16-18 weeks to public launch**

**Aggressive Estimate:**
- MVP (Phases 1-2, 4, 8): 5 weeks
- Full Launch (All phases): 12 weeks

## Risk Assessment

### High Risk
- **Performance**: 500+ shardlings may not hit 60 FPS
  - Mitigation: Early profiling, LOD system, dynamic scaling
- **Backend**: Infrastructure costs and complexity
  - Mitigation: Start with Firebase free tier, scale gradually

### Medium Risk
- **Art Pipeline**: Waiting on art assets
  - Mitigation: Proceed with placeholders, parallel workstreams
- **WebGL**: Browser compatibility issues
  - Mitigation: Test early and often, fallback strategies

### Low Risk
- **Tests**: Test framework well-established
- **Documentation**: Clear templates and processes

## Dependencies

```
Phase 1 (Playtest) → Phase 6 (Optimization)
Phase 2 (Tests) → All Phases (regression prevention)
Phase 3 (Art) → Phase 6 (final optimization with real assets)
Phase 4 (UI) → Phase 5 (backend requires UI)
Phase 5 (Backend) → Phase 7 (monetization needs backend)
Phase 6 (Performance) → Phase 8 (launch readiness)
Phases 1-6 → Phase 8 (launch)
```

## Current Sprint Focus

**Sprint Goal: Complete Phase 2 Foundation**

This Sprint:
1. ✓ Create test framework
2. ✓ Add core unit tests
3. ✓ Document test guidelines
4. Set up CI/CD pipeline
5. Add analytics hooks to GameManager
6. Create performance profiler script

Next Sprint:
1. Integrate analytics throughout codebase
2. Begin UI polish (tooltips, juice)
3. Start performance optimization research
4. Plan backend architecture

## Notes for Team

- **Prioritize quality over features**: Better to have a polished core loop than many half-finished features
- **Test early and often**: Every new feature should have accompanying tests
- **Document as you go**: Future you will thank present you
- **Performance is a feature**: Optimize continuously, not at the end
- **Player feedback is gold**: Playtesting should inform all design decisions
- **Iterate quickly**: Short cycles, frequent deployments, rapid feedback

## Quick Wins (Easy wins for immediate impact)

1. ✓ Add analytics to GameManager state transitions
2. Hook analytics into MathGate triggers
3. Add FPS counter debug overlay
4. Create performance test scene
5. Add tooltip component template
6. Create transition animation template
7. Document current known issues
8. Set up GitHub Issues templates

## Blockers & Dependencies

**Current Blockers:**
- None currently blocking progress

**External Dependencies:**
- Art assets (Phase 3) - can proceed with placeholders
- Backend infrastructure (Phase 5) - can develop locally first
- Legal review for monetization (Phase 7) - not blocking

## Questions to Answer

1. Target platform priority? (Mobile-first? Desktop? WebGL?)
   - **Recommendation**: WebGL first for rapid iteration
2. Monetization model? (Free-to-play? Premium? Ads?)
   - **Recommendation**: F2P with optional ads/IAP
3. Art style final decision? (High poly? Low poly? Pixel art?)
   - **Recommendation**: Low poly with stylized glass shaders
4. Backend hosting preference? (Firebase? AWS? Google Cloud?)
   - **Recommendation**: Google Cloud (Cloud Run + Firestore)

## Contact & Resources

- **Documentation**: `/Documentation/` folder
- **Architecture**: See README.md and PR_SUMMARY.md
- **Tests**: Run via Unity Test Runner (Window → General → Test Runner)
- **Issues**: GitHub Issues tab
- **Discussions**: GitHub Discussions tab
