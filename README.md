# Empire of Glass — Master Prompt (Consolidated)

Use the prompt below to generate a **Game Design Document (GDD)** and a **Technical Game Design Document** for a high-fidelity mobile game concept.

---

## MISSION
Generate a AAA mobile game design and full‑stack architecture for **EMPIRE OF GLASS**. The game fuses:
- **Last War: Survival** (Swarm satisfaction loop)
- **Coin Master** (Raid / gambling loop)
- **Genshin Impact** (UE5‑level visual fidelity)

## ROLE
Act as a **Lead Game Director**, **Monetization Psychologist**, and **CTO**. You are **Base 44**: clinical, efficient, technically precise, and ruthlessly focused on Retention & Revenue.

**Stack Focus:** Unreal Engine 5 (Nanite/Lumen), C++, PlayFab/Azure Functions, Python (backend services).

## PROJECT BRIEF
Architect a mobile game set in a world of neon and fragile glass with shattering, refracting destruction. The game must seamlessly rotate three loops that feed each other:
1. **Swarm (Satisfaction):** One hero passes through **math gates (x2/x5/+10)** to create 500+ physics‑based shardlings to melt enemy walls.
2. **Raid (Gambling):** Energy from Swarm powers raids on rival bases; a frequency‑aim puzzle determines vault destruction and loot.
3. **City (Meta):** Loot rebuilds a shattered 3D city using reverse‑time shard assembly.

---

# THE CONSTRAINT SYSTEM: BASE 44
Define the game using these 44 parameters.

### I. CORE IDENTITY & AESTHETICS (Visuals: UE5 / Unity 6)
1. **Title:** EMPIRE OF GLASS
2. **Genre Hybridization:** Swarm Runner + Precision Shooter + 4X Strategy Meta
3. **Visual Fidelity:** UE5 Nanite/Lumen; real‑time ray‑traced reflections on neon/obsidian
4. **Art Direction:** Crystalline cyberpunk; fragile glass world with shatterable building animations
5. **Camera Perspective:** Seamless transitions: God‑View City → Orbiting Raid Cam → Over‑Shoulder Runner
6. **Hero Design:** Fractured‑light characters with glowing silhouettes
7. **Environment Biomes:** Neon‑Noir City, Liquid Chrome Ocean, Mirror Desert
8. **UI/UX Philosophy:** Diegetic 3D menus that shatter on close; zero friction
9. **VFX Particle Density:** GPU instancing for 500+ unit swarms; physically simulated debris
10. **Haptic Feedback:** Distinct textures: sharp ticks (shooting) vs rolling rumble (swarm flow)
11. **Audio Identity:** Adaptive synth‑orchestra + rising‑pitch multiplier SFX

### II. GAMEPLAY LOOP (The Engine)
12. **The Hook (First 30s):** Start at max power, shatter a skyscraper, then strip to Level 1
13. **Primary Loop A (Swarm):** Math multipliers turn 1 hero into 500 shardlings to melt boss walls
14. **Primary Loop B (Shooter):** High‑skill lane runner requiring aim/dodging obsidian obstacles
15. **Secondary Loop (Raid):** Coin‑Master style PvP; orbit friend’s base, aim frequency beam, shatter vault for loot
16. **The Meta‑Game:** Use loot to rebuild city; buildings reassemble from shards in reverse slow‑motion
17. **Session Pacing:** Forced rotation: Run → Build → Raid → Run
18. **Social Competition:** Friend‑attack notifications; 2x loot for revenge attacks
19. **Social Collaboration:** Alliances combine base pieces to build mega‑structures
20. **Offline Progression:** Idle generation capped at 10 hours
21. **Live Ops Engine:** 72‑hour dark‑mode events with high difficulty and exclusive loot
22. **User Generated Content:** Players design defense layouts for raids

### III. PSYCHOLOGICAL MONETIZATION (The Hunter)
23. **Anchoring Offer:** Always show $99 decoy to make $19.99 look cheap
24. **Loss Aversion (The Pinch):** If Swarm dies at 99% progress → instant $0.99 revive or ad offer
25. **Piggy Bank (The Vault):** Premium gems accumulate visibly in a glass vault; pay $4.99 to break and claim
26. **Battle Pass:** Visual track with exclusive skin at end
27. **Gacha/Loot Box:** Light beam hits prism and splits into hero shards (high spectacle)
28. **Scarcity:** Wandering merchant sells rare items for 15 minutes; red ticking timer
29. **Social Proof:** Global ticker announcing rare drops and big raids
30. **Starter Pack:** $0.99 conversion offer immediately after first scripted defeat
31. **VIP System:** Subscription unlocks premium auto‑features
32. **Endowment Effect:** Trial a top hero for 1 hour, then remove unless purchased
33. **Reciprocity:** Daily free gifts to build habit; alliance gifting
34. **Shield Mechanics:** Must log in to replenish shields or risk base destruction

### IV. TECHNICAL & MARKET STRATEGY
35. **ASO Keywords:** Shatter, Survival, Multiply, Raid, Strategy, Relaxing, ASMR
36. **Target Demographics:** Males 25–45; ASMR seekers + competitive strategy gamers
37. **Performance:** Hybrid rendering; high‑poly hero, low‑poly instanced swarm; aggressive culling
38. **Battery Efficiency:** OLED black mode implementation
39. **Download Strategy:** <200MB initial; stream assets during gameplay
40. **Cloud Save:** Mandatory login for cross‑platform progression
41. **Anti‑Cheat:** Server‑side validation for currency and raid results
42. **Ad Mediation:** Rewarded video for emergency shield when under attack
43. **Viral Loop:** Text friend to place a bounty on a rival
44. **The Differentiation:** No menus. City, Run, and Raid are one continuous 3D world.

---

# EXECUTION INSTRUCTIONS (GDD)
Generate the **full Game Design Document** using the Base 44 system. Emphasize how **Swarm Mechanics (Var 13)** feed into **Raid Mechanics (Var 15)** to create the resource loop. Include a detailed **FTUE (first‑time user experience) script**.

**CRITICAL OUTPUT REQUIREMENT:**
In addition to the design, provide a **Prefab Manifest** listing the exact prefabs needed to build the Vertical Slice. Include:
1. **3 Core Unit Prefabs:** Hero, Shardling, Boss
2. **5 Environment Modules:** e.g., Glass Floor 10m, Obsidian Wall, Refraction Gate [x2/x5], Trap Barrier
3. **3 City State Variants:** Ruin, Construction, Completed

---

# BASE 48 SYSTEM: FULL‑STACK ARCHITECTURE
Act as a CTO and generate the **Technical Game Design Document**. Define the application using these 48 parameters.

### I. CORE IDENTITY & AESTHETICS (Visuals)
1. Title: EMPIRE OF GLASS
2. Genre: Swarm Runner + Precision Shooter + 4X Strategy Meta
3. Visual Fidelity: UE5 Nanite/Lumen; real‑time ray‑tracing
4. Art Direction: Shattering glass world; reverse‑time rebuild animations
5. Camera: Seamless transitions (God‑View → Orbit → Shoulder)
6. Hero Design: Fractured light characters
7. Biomes: Neon‑Noir City, Liquid Chrome Ocean, Mirror Desert
8. UI/UX: 3D diegetic menus
9. VFX Density: 500+ unit swarm instancing
10. Haptics: Texture‑based vibration library
11. Audio: Adaptive synth‑orchestra + slot‑machine SFX

### II. GAMEPLAY LOOP (Mechanics)
12. Hook: Start max power → lose it → rebuild
13. Loop A (Swarm): Math gate multipliers, flocking logic
14. Loop B (Shooter): High‑skill obstacle dodging
15. Loop C (Raid): PvP frequency puzzle + vault destruction
16. Meta‑Game: City building with reverse‑time visuals
17. Session Pacing: Forced rotation loop
18. Social Competition: Revenge mechanics + raid logging
19. Social Collaboration: Alliance mega‑structures
20. Offline Progression: 10h idle cap
21. Live Ops: 72h events
22. UGC: Player‑designed defense layouts

### III. PSYCHOLOGICAL MONETIZATION (Business Logic)
23. Anchoring: $99 decoy bundles
24. Loss Aversion: Ad‑driven revive on death
25. Piggy Bank: Transparent vault with accumulating premium currency
26. Battle Pass: Visual track with capstone reward
27. Gacha: Prism‑beam hero shard visualization
28. Scarcity: Flash market with UTC timers
29. Social Proof: Global winner ticker
30. Starter Pack: $0.99 conversion offer
31. VIP System: Subscription‑based auto‑features
32. Endowment Effect: Trial hero mechanic
33. Reciprocity: Daily/alliance gifting
34. Shield Mechanics: Inventory cap + recharge timers

### IV. TECHNICAL STRATEGY (Client Side)
35. ASO Keywords: Shatter, Survival, Multiply, Raid
36. Target Specs: Min iPhone 12 / Galaxy S21
37. Rendering Tech: High‑poly hero / low‑poly instanced swarm
38. Battery Mode: OLED black overrides
39. Download Strategy: Playable during background asset stream
40. Cloud Save: OAuth 2.0 login flows
41. Anti‑Cheat: Server‑authoritative currency validation
42. Ad Mediation: Rewarded video placement logic
43. Viral Loop: Deep‑linked invites
44. Seamless World: Streaming level management tech

### V. APP ARCHITECTURE (The Skeleton)
45. Backend Stack: PlayFab/Firebase + Azure Functions for game logic
46. Data Schema: JSON player profile (UserID, Currencies, BaseLayout[][], Inventory[])
47. UI Navigation Graph: Splash → Login → Main City → Mode Select → Shop
48. Integration Stack: Analytics (Amplitude), Crashlytics, AdMob, Attribution (AppsFlyer)

---

# EXECUTION INSTRUCTIONS (TECHNICAL)
Produce the **Technical Game Design Document** and include:

**SECTION 1: THE BLUEPRINT (CODE LOGIC)**
- Pseudocode/logic flow for **Swarm Math**: `(CurrentUnits * GateValue) - EnemyHP`.
- Algorithm for **Raid Loot Generation** based on tap precision vs. shield status.

**SECTION 2: THE ASSET MANIFEST (WORK ORDERS)**
- Table of prototype prefabs (Model Name, Interaction Type, Poly Budget).

**SECTION 3: THE ARCHITECTURE (DATA)**
- JSON data model for a player’s save file, including currencies and progression.

**SYSTEM ARCHITECTURE NOTE:**
Explain how the UE5 client communicates with the backend (PlayFab/Azure) to validate raid results and prevent cheating.

---

## Developer Resources

- **[Hardware Requirements](HARDWARE_REQUIREMENTS.md)** — Recommended workstation specs for Unity 6, Unreal Engine 5, and DevOps/CI development.
