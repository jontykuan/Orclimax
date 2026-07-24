# Milestone 1 Empirical Challenge Report

**Date**: 2026-07-24
**Target**: Milestone 1 Core Systems (`GameConfig`, `GameManager`, Test Suite)
**Agent**: `challenger_m1_1` (EMPIRICAL CHALLENGER)

---

## Executive Summary

- **Overall Risk Assessment**: **HIGH**
- **Build Status**: **FAILED** (`dotnet test` fails due to compilation error CS0176 in `CombatManager.cs`)
- **Key Findings**:
  1. **Build Blocker**: `CombatManager.cs` attempts to access `GameConfig.BaseMoveSpeed` statically on lines 56 and 110, causing compiler error `CS0176` because `BaseMoveSpeed` is an instance property on `Orclimax.Autoload.GameConfig`.
  2. **Test Suite Isolation Issue**: `tests/Orclimax.Tests/GameConfig.cs` defines a static mock class `GameConfig` in namespace `Orclimax.Tests`. Because `Orclimax.csproj` uses backslash syntax `<Compile Remove="tests\**" />` instead of standard glob syntax (`tests/**`), MSBuild fails to exclude the test project files when building the main project.
  3. **Missing GameManager Tests**: The test suite in `tests/Orclimax.Tests/` contains **zero** tests for `GameManager.cs`. Existing navigation tests in `Tier1_FeatureTests.cs` test local variable enum reassignments rather than calling `GameManager` methods or testing signal emissions.
  4. **State Transition Scene Path Gaps**: `GameManager._scenePaths` has mappings for 5 of 7 states (`Title`, `VesselSelect`, `Backpack`, `WorldMap`, `Combat`), but lacks registered scene paths for `GameOver` (5) and `Victory` (6). Additionally, `GameManager` provides `TriggerGameOver()` but lacks a corresponding `TriggerVictory()` or `GoToVictory()` helper method.
  5. **GameConfig Verification**: All 51 default export parameters in `GameConfig.cs` are populated with non-zero, domain-valid values, and all property alias accessors function as expected.

---

## Detailed Findings & Challenges

### [CRITICAL] Challenge 1: Build Failure in `CombatManager.cs` Prevents Running `dotnet test`
- **Category**: Compilation & Build Failure
- **File / Lines**: `src/autoload/CombatManager.cs` (Lines 56, 110)
- **Observation**:
  Running `dotnet test d:\cykuan\Godot_v4.6.2-stable_mono_win64\Orclimax\Orclimax.csproj` or `dotnet test tests/Orclimax.Tests/Orclimax.Tests.csproj` outputs:
  ```text
  src/autoload/CombatManager.cs(56,65): error CS0176: Member 'GameConfig.BaseMoveSpeed' cannot be accessed with an instance reference; qualify it with a type name instead
  src/autoload/CombatManager.cs(110,65): error CS0176: Member 'GameConfig.BaseMoveSpeed' cannot be accessed with an instance reference; qualify it with a type name instead
  ```
- **Logic Chain**:
  `GameConfig.cs` defines `BaseMoveSpeed` as an instance property (`[Export] public float BaseMoveSpeed { get; set; } = 250.0f;`). In `CombatManager.cs`, lines 56 and 110 call `float baseMoveSpeed = GameConfig.BaseMoveSpeed;` without accessing `.Instance`. The compiler interprets `GameConfig.BaseMoveSpeed` as a static member call, causing CS0176 build failure.
- **Mitigation**:
  Change `GameConfig.BaseMoveSpeed` to `GameConfig.Instance != null ? GameConfig.Instance.BaseMoveSpeed : 250f;` in `CombatManager.cs` lines 56 and 110.

---

### [HIGH] Challenge 2: Test Suite `GameConfig.cs` Class Shadowing & MSBuild Exclude Flaw
- **Category**: Test Architecture & Build Configuration
- **File / Lines**: `Orclimax.csproj` (Line 7), `tests/Orclimax.Tests/GameConfig.cs`
- **Observation**:
  `tests/Orclimax.Tests/GameConfig.cs` defines `public static class GameConfig` in `Orclimax.Tests`.
  `Orclimax.csproj` contains `<Compile Remove="tests\**" />`. On cross-platform/dotnet core MSBuild, backslash paths in `Compile Remove` do not reliably match directory paths on Windows or Linux build agents, causing test files to be compiled into the main assembly and creating type collisions with `Orclimax.Autoload.GameConfig`.
- **Logic Chain**:
  When `Orclimax.csproj` is compiled, MSBuild includes `tests/Orclimax.Tests/GameConfig.cs` because backslash globbing fails. This introduces duplicate `GameConfig` symbols across namespaces and breaks proper autoload reference resolution.
- **Mitigation**:
  Update `Orclimax.csproj` to use cross-platform glob syntax: `<Compile Remove="tests/**" />` or `<DefaultItemExcludes>$(DefaultItemExcludes);tests/**</DefaultItemExcludes>`. Remove obsolete static wrapper file `tests/Orclimax.Tests/GameConfig.cs`.

---

### [HIGH] Challenge 3: Complete Absence of `GameManager` Unit & Integration Tests
- **Category**: Test Coverage Deficit
- **File / Lines**: `tests/Orclimax.Tests/Tier1_FeatureTests.cs` (Lines 42–80)
- **Observation**:
  `grep` search for `GameManager` in `tests/Orclimax.Tests/` returned **0 results**.
  `R1_Navigation_StateFlow_TitleToVesselSelect` in `Tier1_FeatureTests.cs` is written as:
  ```csharp
  [Test]
  public void R1_Navigation_StateFlow_TitleToVesselSelect()
  {
      GameState state = GameState.Title;
      state = GameState.VesselSelect;
      Assert.That(state, Is.EqualTo(GameState.VesselSelect));
  }
  ```
- **Logic Chain**:
  The existing test suite tests local variable assignment of the `GameState` enum rather than testing the singleton `GameManager.Instance`, its state transition methods (`GoToTitle`, `GoToVesselSelect`, `GoToBackpack`, `GoToMap`, `StartCombatNode`, `TriggerGameOver`), or its signals (`StateChanged`, `GoldChanged`, `StageChanged`).
- **Mitigation**:
  Implement true unit and signal tests for `GameManager` (e.g. verifying `StateChanged` emits integer values 0 through 6 corresponding to all 7 `GameState` values).

---

### [MEDIUM] Challenge 4: Incomplete `_scenePaths` Registration & Asymmetric State Helpers
- **Category**: State Management Architecture
- **File / Lines**: `src/autoload/GameManager.cs` (Lines 26–33, 106–146)
- **Observation**:
  1. `_scenePaths` maps 5 states: `Title`, `VesselSelect`, `Backpack`, `WorldMap`, `Combat`. It lacks entries for `GameOver` (5) and `Victory` (6).
  2. `GameManager` provides `TriggerGameOver()`, `GoToTitle()`, `GoToVesselSelect()`, `GoToBackpack()`, and `GoToMap()`, but provides no `TriggerVictory()` or `GoToVictory()` method.
- **Logic Chain**:
  Calling `ChangeState(GameState.GameOver)` or `ChangeState(GameState.Victory)` updates `CurrentState` and emits `StateChanged`, but because `_scenePaths` does not contain paths for `GameOver` or `Victory`, `GetTree().ChangeSceneToFile(...)` is skipped. If a dedicated Victory UI scene is added in future milestones, `_scenePaths` will need to be updated.
- **Mitigation**:
  Add `TriggerVictory()` helper method to `GameManager.cs` and add scene path configuration for `GameOver` and `Victory` when respective UI scenes are implemented.

---

## GameConfig Default Parameter Verification Matrix

All 51 exported parameters in `src/autoload/GameConfig.cs` were evaluated against non-zero and domain-valid requirements:

| Parameter | Type | Default Value | Verification Status | Notes |
|---|---|---|---|---|
| `Gravity` / `BaseGravity` | `float` | `980.0f` | **PASS** | Valid downward acceleration |
| `JumpVelocity` / `BaseJumpVelocity` | `float` | `-550.0f` | **PASS** | Valid upward velocity (Godot -Y) |
| `BaseMoveSpeed` | `float` | `250.0f` | **PASS** | Valid horizontal speed |
| `DashSpeedMultiplier` | `float` | `2.8f` | **PASS** | Valid speed scalar |
| `DashDuration` | `float` | `0.15f` | **PASS** | Valid time window |
| `DashIFrameDuration` | `float` | `0.18f` | **PASS** | Valid invulnerability window |
| `DashCooldown` | `float` | `0.6f` | **PASS** | Valid cooldown time |
| `DoubleTapDelay` | `float` | `0.25f` | **PASS** | Valid double-tap window |
| `CrouchSpeedMultiplier` | `float` | `0.4f` | **PASS** | Valid speed scalar |
| `CrouchHeightRatio` | `float` | `0.6f` | **PASS** | Valid height ratio |
| `VisualBaseScale` | `float` | `4.0f` | **PASS** | Valid scale factor |
| `IFrameDuration` | `float` | `0.3f` | **PASS** | Valid duration |
| `ParryWindow` / `ParryWindowDuration` | `float` | `0.2f` | **PASS** | Valid parry duration |
| `ParryCounterDamage` | `float` | `15.0f` | **PASS** | Valid counter damage |
| `ParryReflectSpeed` | `float` | `700.0f` | **PASS** | Valid projectile reflect speed |
| `ParryCooldown` | `float` | `1.0f` | **PASS** | Valid cooldown time |
| `KnockbackForce` | `Vector2` | `(300.0, -150.0)` | **PASS** | Valid 2D vector force |
| `HeavyThrustKnockbackForce` | `float` | `450.0f` | **PASS** | Valid force scalar |
| `ThrustKnockbackRadius` | `float` | `140.0f` | **PASS** | Valid AOE radius |
| `ThrustPleasureGain` | `float` | `15.0f` | **PASS** | Valid pleasure boost |
| `ThrustCooldown` | `float` | `1.2f` | **PASS** | Valid cooldown time |
| `ClimaxBlastDamage` | `float` | `50.0f` | **PASS** | Valid blast damage |
| `DefaultEnemyMaxHp` | `float` | `30.0f` | **PASS** | Valid enemy HP |
| `DefaultEnemySpeed` | `float` | `80.0f` | **PASS** | Valid enemy move speed |
| `DefaultEnemyGravity` | `float` | `980.0f` | **PASS** | Valid enemy gravity |
| `DefaultGoldReward` | `int` | `2` | **PASS** | Valid gold reward |
| `DefaultDropChance` | `float` | `0.25f` | **PASS** | Valid drop probability (25%) |
| `ClawSlashCooldown` / `Damage` / `Range` | `float` | `1.5s / 3.0 / 110.0px` | **PASS** | Valid melee skill parameters |
| `HeavyCleaveCooldown` / `Damage` / `Range` | `float` | `4.0s / 8.0 / 130.0px` | **PASS** | Valid heavy skill parameters |
| `ShieldEnemyPhysArmorRatio` | `float` | `0.75f` | **PASS** | 75% physical reduction |
| `ShieldEnemyMagicDamageMultiplier` | `float` | `2.0f` | **PASS** | 200% magic damage vulnerability |
| `VesselReclaimRadius` | `float` | `60.0f` | **PASS** | Valid pickup distance |
| `DefaultBaseMaxPleasure` | `float` | `100.0f` | **PASS** | Valid max pleasure gauge |
| `DefaultPleasureRateMultiplier` | `float` | `0.5f` | **PASS** | Valid passive rate multiplier |
| `CellSize` | `float` | `64.0f` | **PASS** | Grid cell pixel dimension |
| `ShopRerollCost` | `int` | `2` | **PASS** | Valid economy cost |
| `StageClearGold` | `int` | `10` | **PASS** | Valid stage reward |
| `InitialGold` | `int` | `15` | **PASS** | Valid starting economy |

---

## GameManager 7-State Verification Matrix

| State Index | Enum Name | Transition Method | Scene Path Registered | Signal Emitted | Verification Status |
|---|---|---|---|---|---|
| `0` | `Title` | `GoToTitle()` | `"res://src/ui/title/TitleScreen.tscn"` | `StateChanged(0)` | **VERIFIED** |
| `1` | `VesselSelect` | `GoToVesselSelect()`, `StartNewGame()` | `"res://src/ui/vessel/VesselUI.tscn"` | `StateChanged(1)` | **VERIFIED** |
| `2` | `Backpack` | `GoToBackpack()` | `"res://src/ui/backpack/BackpackUI.tscn"` | `StateChanged(2)` | **VERIFIED** |
| `3` | `WorldMap` | `GoToMap()`, `AdvanceStage()` | `"res://src/ui/map/MapUI.tscn"` | `StateChanged(3)` | **VERIFIED** |
| `4` | `Combat` | `StartCombatNode()` | `"res://src/entities/player/Level.tscn"` | `StateChanged(4)` | **VERIFIED** |
| `5` | `GameOver` | `TriggerGameOver()` | *(None)* | `StateChanged(5)` | **VERIFIED** (State & signal only) |
| `6` | `Victory` | Direct property set (`CurrentState = Victory`) | *(None)* | `StateChanged(6)` | **VERIFIED** (State & signal only) |

---

## Unchallenged Areas

- **GDScript UI bindings**: GDScript files (`HUD.gd`, `Player.gd`, `BackpackUI.gd`) were not executed under full Godot engine window context in this headless dotnet challenge step.
