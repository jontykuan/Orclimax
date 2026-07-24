# Scope: E2E Testing Track (Orclimax)

## Architecture Overview
The E2E Testing Track creates a requirement-driven, opaque-box test suite for the Orclimax game project. The test suite exercises game features independently of internal implementation details using standard C# test runners (`dotnet test` via NUnit / xUnit in `tests/Orclimax.Tests/`) and/or Godot CLI test scripts.

## Target Requirements & Features
1. **R1: Multi-Page Navigation**
   - TitleScreen (`TitleScreen.tscn` / `TitleScreen.gd`) buttons: Start, Save/Load, Gallery, Settings, Quit.
   - VesselUI (`VesselUI.tscn` / `VesselUI.gd`) vessel sensitivity levels, skills, pleasure multipliers, traits.
   - MapUI (`MapUI.tscn` / `MapUI.gd`) cleared nodes, current stage, branching uncleared paths.
   - BackpackUI (`BackpackUI.tscn` / `BackpackUI.gd`) hint text cleanup.
   - Game state transitions (Title -> VesselSelect -> Backpack -> WorldMap -> Combat).

2. **R2: Advanced Enemies & Vessel Disarming**
   - RangedEnemy: parabolic projectiles (dodged left/right) and high/low straight projectiles (dodged crouch/jump).
   - ShieldedEnemy: reduced physical damage, normal magic/toy damage.
   - FlyingEnemy: airborne sine-wave hovering and swooping patrol.
   - VesselSnatcherEnemy: snatch attack detaches Vessel; Orc items disabled until reclaimed.

3. **R3: Directional Double-Tap Actions**
   - Left/Right x2: Dodge/Dash burst movement with i-frames and cooldown.
   - Up x2: Precise Guard/Parry counters physical attacks and reflects ranged projectiles with cooldown.
   - Down x2: Heavy Thrust short-range AOE knockback, Climax/Pleasure gauge boost with cooldown.

4. **R4: Parameterized Game Constants**
   - Centralized `GameConfig.cs` parameterization (speeds, gravity, double-tap window, i-frame duration, parry duration, knockback force, enemy stats).

## Milestones

| # | Name | Scope | Dependencies | Status |
|---|------|-------|-------------|--------|
| E2E-M1 | `test_infra_setup` | Create `TEST_INFRA.md` & C# test project `tests/Orclimax.Tests/` | none | IN_PROGRESS |
| E2E-M2 | `tier1_feature_coverage` | Tier 1 tests (≥5 per feature, 20 total) | E2E-M1 | PLANNED |
| E2E-M3 | `tier2_boundary_corner` | Tier 2 tests (≥5 per feature, 20 total) | E2E-M1 | PLANNED |
| E2E-M4 | `tier3_cross_feature` | Tier 3 tests (pairwise feature interactions, ≥4 total) | E2E-M1 | PLANNED |
| E2E-M5 | `tier4_real_world` | Tier 4 tests (real-world application scenarios, ≥5 total) | E2E-M1 | PLANNED |
| E2E-M6 | `verification_and_publish` | Verify build & test run, publish `TEST_READY.md` | E2E-M2..M5 | PLANNED |
