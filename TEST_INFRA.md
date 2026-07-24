# TEST_INFRA.md — E2E & Integrated Multi-Tier Test Suite Specification

## 1. Test Suite Invocation Command

To execute the entire unit and E2E test suite across Tiers 1 through 4:

```bash
dotnet test tests/Orclimax.Tests/Orclimax.Tests.csproj
```

---

## 2. Testing Methodology

The test infrastructure employs formal testing methodologies across 4 distinct testing tiers:

1. **Tier 1: Feature Functionality & Category-Partitioning**
   - Systematically partitions feature domains into equivalence classes based on inputs, states, and system requirements.
   - Tests happy path operations, multi-page state flow, combat mechanics, disarming/reclaiming, double-tap input parsing, and parameterization.
   - **Target Coverage**: $\ge 20$ tests.

2. **Tier 2: Boundary Value Analysis (BVA) & Limit Testing**
   - Tests system boundary conditions (minimum, maximum, exact threshold, just-outside threshold, zero values, extreme values).
   - Validates threshold limits for double-tap timings (e.g. 0.250s vs 0.251s), reclaim radius boundaries (60.000 vs 60.001), armor ratios (0.0 vs 1.0), and speed/scale extremes.
   - **Target Coverage**: $\ge 20$ tests.

3. **Tier 3: Pairwise / Combinatorial Testing**
   - Evaluates multi-feature interactions and state combinations (e.g., Disarmed player state combined with Parry input; Custom GameConfig settings combined with Shield enemy physical/magic damage reduction).
   - Reduces multi-dimensional interaction space while assuring high defect coverage for feature synergies.
   - **Target Coverage**: $\ge 4$ tests.

4. **Tier 4: Real-World Workload & Application Scenarios**
   - Simulates realistic end-to-end user journeys and complex game campaign flows spanning title navigation, vessel selection, backpack preparation, stage progression, snatcher combat, boss encounters, and dynamic parameter tuning.
   - **Target Coverage**: $\ge 5$ tests.

---

## 3. Feature Inventory

The test suite validates four major feature requirements introduced across recent development iterations:

### R1: Multi-Page Navigation & Scene State Flow
- **Components**: `TitleScreen.tscn`, `VesselUI.tscn`, `MapUI.tscn`, `BackpackUI.tscn`
- **State Machine**: Transitions between `Title`, `VesselSelect`, `Backpack`, `WorldMap`, and `Combat`.
- **Key Logic**: Screen resolution toggles (F1-F3), vessel development levels & skills, branching node navigation, item clearing on vessel swap.

### R2: Advanced Enemies & Vessel Disarming Mechanics
- **Archetypes**:
  - `RangedEnemy`: High/low flat shots and parabolic mortar lobs with dodge/crouch height checks.
  - `ShieldedEnemy`: High physical armor (75% reduction), vulnerable to magic/toy weapons (2x multiplier).
  - `FlyingEnemy`: Airborne sine-wave hover tracking.
  - `VesselSnatcherEnemy`: Snatch attack that detaches Vessel, disarming Orc (0x weapon damage multiplier) until player moves within `VesselReclaimRadius` (60.0 units) to reclaim.

### R3: Directional Double-Tap Action Controls
- **Inputs & Actions**:
  - **Left/Right Double-Tap**: Dodge/Dash burst with speed multiplier ($2.8\times$), i-frames ($0.18\text{s}$), and cooldown ($0.6\text{s}$).
  - **Up Double-Tap**: Precise Guard/Parry physical counter & projectile reflection with invulnerability window ($0.22\text{s}$).
  - **Down Double-Tap**: Heavy Thrust AOE knockback ($140.0\text{px}$ radius, $450.0\text{ force}$) and instant pleasure boost ($15.0$).

### R4: Centralized GameConfig Parameterization
- **Component**: Centralized configuration (`GameConfig`) encapsulating physics (`BaseGravity`, `BaseJumpVelocity`, `BaseMoveSpeed`), double-tap input timing (`DoubleTapDelay`), combat timings/i-frames, knockback forces, and enemy defaults.

---

## 4. Real-World Application Scenarios (Tier 4)

1. **Scenario 1: Full Game Loop Flow (`Scenario1_FullGameLoopFlow`)**
   - Validates transition from Title Screen $\rightarrow$ Vessel Select $\rightarrow$ Backpack Equipment Setup $\rightarrow$ World Map Node Selection $\rightarrow$ Combat Wave Clear $\rightarrow$ Stage Advancement & Gold Economy Reward.
2. **Scenario 2: VesselSnatcher Disarm & Reclaim Flow (`Scenario2_VesselSnatcherDisarmAndReclaimFlow`)**
   - Simulates player disarming upon Snatcher hit (0x damage penalty), using Double-Tap Dash to close distance to the thrown vessel, and entering the 60u reclaim radius to restore full combat capability.
3. **Scenario 3: Boss Wave Multi-Archetype Combat (`Scenario3_BossWave_RangedShieldedFlyerCombo`)**
   - Tests simultaneous combat against Ranged (crouching & parrying projectiles), Shielded (switching to magic/toy weapon damage), and Flying enemies.
4. **Scenario 4: Dynamic Live GameConfig Parameter Tuning (`Scenario4_LiveGameConfigParameterTuning`)**
   - Validates live tuning of move speeds, double-tap thresholds, parry windows, and armor ratios by game designers without invalidating runtime calculations.
5. **Scenario 5: Multi-Vessel Switching & Item Fusion Campaign (`Scenario5_MultiVesselSwitchingAndItemFusionCampaign`)**
   - Simulates switching between Vessel A (Lydia) and Vessel B (Cynthia) while retaining equipped items when "Keep Equipped" is toggled, performing adjacent item fusion, and preserving independent grid state.

---

## 5. Coverage Thresholds & Test Summary

| Test Tier | Methodology / Focus | Required Threshold | Actual Count | Pass Status |
| :--- | :--- | :--- | :--- | :--- |
| **Sanity** | Project Setup & Assembly Load | N/A | 1 | PASSED |
| **Tier 1** | Feature Functionality (R1-R4) | $\ge 20$ | 20 | PASSED |
| **Tier 2** | Boundary Value Analysis (BVA) | $\ge 20$ | 20 | PASSED |
| **Tier 3** | Pairwise Combinatorial Interactions | $\ge 4$ | 5 | PASSED |
| **Tier 4** | Real-World Application Scenarios | $\ge 5$ | 5 | PASSED |
| **TOTAL** | **Full E2E Suite** | **$\ge 50$** | **51** | **PASSED (51/51)** |

---

*Generated by E2E Testing Subagent — All 51 tests executed with 0 failures.*
