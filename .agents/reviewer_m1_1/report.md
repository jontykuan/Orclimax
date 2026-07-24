# Milestone 1 Code Review & Verification Report

**Reviewer**: `reviewer_m1_1`  
**Date**: 2026-07-24  
**Target**: Milestone 1 (Config & Navigation)  
**Verdict**: **REQUEST_CHANGES**

---

## Executive Summary

The code review and build verification for Milestone 1 (Config & Navigation) has been completed.
- C# build verification (`dotnet build Orclimax.csproj`) **PASSED** with 0 errors and 0 warnings.
- UI layout and state navigation (`TitleScreen`, `VesselUI`, `MapUI`, `BackpackUI`, `GameManager`, `Enums.cs`) are well-structured, function as specified, and comply with UI cleanup requirements (e.g. `HelpArea` removed).
- **CRITICAL ISSUE FOUND**: `GameConfig` configuration management has architectural fragmentation and runtime bug risk:
  1. `src/autoload/GameConfig.cs` is **not registered** in `project.godot` under `[autoload]`.
  2. There are two competing `GameConfig` classes: `Orclimax.Autoload.GameConfig` (`src/autoload/GameConfig.cs`) and `Orclimax.Core.GameConfig` (`src/core/GameConfig.cs`).
  3. `Player.gd` and `EnemyBase.gd` reference different property names across the two files, causing property lookup mismatches and potential runtime errors when `GameConfig.Instance` is `null`.

---

## Detailed Findings by Section

### 1. Centralized Config (`GameConfig.cs` & `project.godot`) — **REQUEST_CHANGES (Major)**
- **Observation**:
  - `project.godot` `[autoload]` section contains `GameManager`, `InventoryManager`, and `CombatManager`, but is missing `GameConfig="*res://src/autoload/GameConfig.cs"`.
  - Two `GameConfig.cs` files exist in the repository:
    - `src/autoload/GameConfig.cs` (`namespace Orclimax.Autoload`, inherits `Node` with exported instance properties).
    - `src/core/GameConfig.cs` (`namespace Orclimax.Core`, `public static class GameConfig` with static fields).
  - In `Player.gd`, properties like `GameConfig.BaseGravity`, `GameConfig.BaseJumpVelocity`, `GameConfig.ParryWindowDuration`, `GameConfig.ParryCooldown`, `GameConfig.ThrustCooldown` are referenced (matching `src/core/GameConfig.cs`), whereas in `EnemyBase.gd`, `GameConfig.DefaultEnemyMaxHp`, `GameConfig.DefaultEnemySpeed`, `GameConfig.ClawSlashCooldown` are referenced (matching `src/autoload/GameConfig.cs`).
- **Why this is a problem**:
  - `src/autoload/GameConfig.cs` cannot serve as an Autoload singleton because it is not listed in `project.godot`. Therefore, `GameConfig.Instance` will evaluate to `null` at runtime.
  - The split between `Orclimax.Core.GameConfig` and `Orclimax.Autoload.GameConfig` causes confusion and inconsistent property names between C# and GDScript entities.
- **Recommendation**:
  - Add `GameConfig="*res://src/autoload/GameConfig.cs"` to `[autoload]` in `project.godot`.
  - Consolidate `GameConfig.cs` into a single single-source-of-truth class, ensuring property names (e.g. `BaseGravity` vs `Gravity`, `ParryWindow` vs `ParryWindowDuration`) match GDScript calls, or update GDScript callers (`Player.gd`, `EnemyBase.gd`, `EnemyShield.gd`) to read consistently from `src/autoload/GameConfig.cs`.

---

### 2. Title Screen Navigation (`TitleScreen.tscn` & `TitleScreen.gd`) — **PASS**
- **Observation**:
  - Includes Start (開始遊戲), Save/Load (存檔紀錄), Gallery (畫廊模式), Settings (系統設定), and Quit (離開遊戲) buttons.
  - Clicking Start executes `GameManager.StartNewGame()`, which initializes stage 1 and gold, then transitions state to `GameState.VesselSelect` (`VesselUI.tscn`).
  - Modal dialogs for Save/Load, Gallery, and Settings display properly with functioning close buttons.
  - Quit button invokes `get_tree().quit()`.

---

### 3. Vessel UI & Vessel Data (`VesselUI.tscn`, `VesselUI.gd`, `VesselData.cs`) — **PASS**
- **Observation**:
  - `VesselData.cs` contains exported attributes for sensitivity (`HeadSensitivity`, `ChestSensitivity`, `GroinSensitivity`, `LimbsSensitivity`), dev levels (`HeadDevLevel`, `ChestDevLevel`, `GroinDevLevel`, `LimbsDevLevel`), pleasure multipliers (`BaseMaxPleasure`, `PleasureBuildRateMultiplier`), climax skill properties (`ClimaxSkillName`, `ClimaxSkillDescription`), traits text (`Traits`, `TraitsDescription`), and grid zone vectors.
  - `VesselUI.tscn` and `VesselUI.gd` render all sensitivity and dev levels, skills, pleasure multipliers, traits descriptions, and a scrollable selection list of vessels (Lydia and Cynthia).
  - Selecting a vessel updates `InventoryManager.SetVessel(...)`. Confirm button correctly calls `GameManager.GoToMap()`.

---

### 4. World Map Navigation (`MapUI.tscn` & `MapUI.gd`) — **PASS**
- **Observation**:
  - Stage node graph contains Node1 (1-1 Borderlands [Cleared]), Node2A (1-2A Black Market Outpost [Available]), Node2B (1-2B Elite Vanguard [Available]), and Node3 (1-3 Sanctuary Keep [Boss]).
  - Header updates cleared/current stage text ("WORLD MAP — STAGE 1") and gold via signals.
  - Node selection updates node detail description labels and visual highlights.
  - Navigation buttons: "Backpack Prep" -> `GameManager.GoToBackpack()`, "Enter Combat" -> `GameManager.StartCombatNode()`, "Vessel Select" -> `GameManager.GoToVesselSelect()`.

---

### 5. Backpack UI Container Cleanup (`BackpackUI.tscn`) — **PASS**
- **Observation**:
  - Bottom `HelpArea` container is confirmed to be **completely removed** from `BackpackUI.tscn`.
  - Header navigation buttons (`ToVesselButton`, `ToMapButton`, `StartCombatButton`) are connected and trigger state machine scene changes.

---

### 6. GameManager & GameState Enum (`GameManager.cs` & `Enums.cs`) — **PASS**
- **Observation**:
  - `GameState` enum in `Enums.cs` includes `Title` (0), `VesselSelect` (1), `Backpack` (2), `WorldMap` (3), `Combat` (4), `GameOver` (5), `Victory` (6).
  - `GameManager.cs` maintains `_scenePaths` mapping and `ChangeState(GameState newState)` transitions scenes via `GetTree().ChangeSceneToFile(path)`.

---

### 7. Build Verification (`dotnet build`) — **PASS**
- **Command**: `dotnet build d:\cykuan\Godot_v4.6.2-stable_mono_win64\Orclimax\Orclimax.csproj`
- **Output**:
  - `Orclimax -> D:\cykuan\Godot_v4.6.2-stable_mono_win64\Orclimax\.godot\mono\temp\bin\Debug\Orclimax.dll`
  - `Build Succeeded. 0 Warning(s), 0 Error(s).`

---

## Summary Table of Verified Claims

| Requirement | Target Files | Verification Method | Status |
| --- | --- | --- | --- |
| Centralized Config & Export Parameters | `GameConfig.cs`, `project.godot` | File Inspection & Cross-Reference | ❌ **FAIL** (Missing Autoload entry & dual class conflict) |
| Title Screen & Button Transitions | `TitleScreen.tscn`, `TitleScreen.gd` | File Inspection & Signal Binding Trace | ✅ **PASS** |
| Vessel Data & Management UI | `VesselUI.tscn`, `VesselUI.gd`, `VesselData.cs` | File Inspection & Property Verification | ✅ **PASS** |
| World Map & Stage Node Graph | `MapUI.tscn`, `MapUI.gd` | File Inspection & State Trigger Verification | ✅ **PASS** |
| Backpack UI Clean Layout | `BackpackUI.tscn`, `BackpackUI.gd` | Scene Hierarchy Check (`HelpArea` removal) | ✅ **PASS** |
| GameManager State Machine | `GameManager.cs`, `Enums.cs` | Code Inspection & Enum Trace | ✅ **PASS** |
| C# Project Build | `Orclimax.csproj` | `dotnet build` execution | ✅ **PASS** |

---

## Action Items for Fixes
1. Edit `project.godot` to register `GameConfig="*res://src/autoload/GameConfig.cs"` under `[autoload]`.
2. Remove or merge `src/core/GameConfig.cs` into `src/autoload/GameConfig.cs` to eliminate duplicate/conflicting class definitions.
3. Synchronize property names in `Player.gd`, `EnemyBase.gd`, `EnemyShield.gd`, and `HUD.gd` to point directly to `GameConfig.<PropertyName>`.
