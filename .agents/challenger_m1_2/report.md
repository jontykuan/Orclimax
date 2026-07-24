# Empirical Challenge Report — Milestone 1 UI Navigation & GameConfig Accessibility

**Agent**: `challenger_m1_2`  
**Date**: 2026-07-24  
**Target Workspace**: `d:\cykuan\Godot_v4.6.2-stable_mono_win64\Orclimax`  

---

## Executive Summary

- **Main Scene Setup (`project.godot`)**: **PASSED** — `run/main_scene="res://src/ui/title/TitleScreen.tscn"` is correctly defined under `[application]`.
- **UI Scene File Bindings (`TitleScreen.tscn`, `VesselUI.tscn`, `MapUI.tscn`, `BackpackUI.tscn`)**: **PASSED** — Node paths, script assignments, and button signal connections across all 4 main UI scenes match their underlying GDScript files (`TitleScreen.gd`, `VesselUI.gd`, `MapUI.gd`, `BackpackUI.gd`) with 100% path accuracy.
- **GDScript Accessibility of `GameConfig`**: **PASSED** — `GameConfig` is registered as an Autoload singleton (`*res://src/autoload/GameConfig.cs`) in `project.godot` and accessible in GDScript.
- **Build Execution & Unit Test Verification (`dotnet test`)**: **FAILED (Build Regression)** — `dotnet test` failed with C# compilation error CS0176 in `src/autoload/CombatManager.cs`.

---

## Detailed Findings

### 1. Main Scene Verification (`project.godot`)
- `project.godot` line 14: `run/main_scene="res://src/ui/title/TitleScreen.tscn"`
- Autoload registrations verified:
  ```ini
  [autoload]
  GameConfig="*res://src/autoload/GameConfig.cs"
  GameManager="*res://src/autoload/GameManager.cs"
  InventoryManager="*res://src/autoload/InventoryManager.cs"
  CombatManager="*res://src/autoload/CombatManager.cs"
  ```

### 2. UI Scene & Script Node Path Verification
We inspected each scene file and its associated GDScript:

1. **TitleScreen (`src/ui/title/TitleScreen.tscn` & `TitleScreen.gd`)**:
   - ExtResource script: `res://src/ui/title/TitleScreen.gd`
   - Node paths verified:
     - `start_btn`: `$MainContainer/VBox/MenuButtons/StartButton` (Line 63)
     - `save_load_btn`: `$MainContainer/VBox/MenuButtons/SaveLoadButton` (Line 69)
     - `gallery_btn`: `$MainContainer/VBox/MenuButtons/GalleryButton` (Line 75)
     - `settings_btn`: `$MainContainer/VBox/MenuButtons/SettingsButton` (Line 81)
     - `quit_btn`: `$MainContainer/VBox/MenuButtons/QuitButton` (Line 87)
     - Modals & Close buttons: `$SettingsModal`, `$SaveLoadModal`, `$GalleryModal` and internal close buttons match scene hierarchy.
   - Button handlers connect to `GameManager.StartNewGame()`, modal display, and application exit.

2. **VesselUI (`src/ui/vessel/VesselUI.tscn` & `VesselUI.gd`)**:
   - ExtResource script: `res://src/ui/vessel/VesselUI.gd`
   - Node paths verified:
     - `vessel_list_container`: `$MainLayout/ContentHBox/VesselSelectionList/VesselScroll/VesselList` (Line 73)
     - `vessel_name_label`: `$MainLayout/ContentHBox/DetailsPanel/LeftDetails/VesselNameLabel` (Line 88)
     - Sensitivity labels (`head_sens_label`, `chest_sens_label`, `groin_sens_label`, `limbs_sens_label`): `$MainLayout/ContentHBox/DetailsPanel/LeftDetails/SensitivitiesGroup/*` (Lines 104-119)
     - Stat & Skill labels (`pleasure_stats_label`, `skill_title_label`, `skill_desc_label`, `traits_desc_label`): Lines 124, 135, 141, 148
     - `confirm_button`: `$MainLayout/ContentHBox/DetailsPanel/RightDetails/SelectVesselButton` (Line 155)
     - `back_title_button`: `$MainLayout/TopBar/BackToTitleButton` (Line 49)
   - Scene transitions: Confirm -> `GameManager.GoToMap()`, Back -> `GameManager.GoToTitle()`.

3. **MapUI (`src/ui/map/MapUI.tscn` & `MapUI.gd`)**:
   - ExtResource script: `res://src/ui/map/MapUI.gd`
   - Node paths verified:
     - `stage_label`: `$MainLayout/Header/StageTitle` (Line 40)
     - `gold_label`: `$MainLayout/Header/GoldLabel` (Line 46)
     - `back_vessel_btn`: `$MainLayout/Header/BackToVesselButton` (Line 56)
     - `prep_btn`: `$MainLayout/DetailPanel/Margin/DetailHBox/EnterPrepButton` (Line 164)
     - `combat_btn`: `$MainLayout/DetailPanel/Margin/DetailHBox/EnterCombatButton` (Line 170)
     - Node selection buttons (`btn_stage1`, `btn_stage2a`, `btn_stage2b`, `btn_stage3`): Lines 68, 84, 100, 116
   - Scene transitions: Back -> `GameManager.GoToVesselSelect()`, Prep -> `GameManager.GoToBackpack()`, Combat -> `GameManager.StartCombatNode()`.

4. **BackpackUI (`src/ui/backpack/BackpackUI.tscn` & `BackpackUI.gd`)**:
   - ExtResource script: `res://src/ui/backpack/BackpackUI.gd`
   - Node paths verified:
     - `grid_container`: `$MainLayout/HBox/GridArea/Panel/GridContainer` (Line 203)
     - `stash_container`: `$MainLayout/HBox/StashArea/ScrollContainer/StashContainer` (Line 227)
     - `shop_container`: `$MainLayout/ShopArea/ShopItems` (Line 250)
     - Header labels and buttons (`gold_label`, `stage_label`, `to_vessel_button`, `to_map_button`): Lines 49, 40, 55, 60
     - Stats panel labels (`hp_label`, `armor_label`, `speed_label`, `atk_spd_label`, `pleasure_rate_label`, `max_pleasure_label`): Lines 122-156
     - Signal connection in `.tscn` (Line 256): `StartCombatButton.pressed -> _on_start_combat_pressed`.
   - Scene transitions: Vessel -> `GameManager.GoToVesselSelect()`, Map -> `GameManager.GoToMap()`, Start Combat -> `GameManager.StartCombatNode()`.

---

## Empirical Verification Failure (Compilation Error)

### Command Executed
```powershell
dotnet test
```

### Compiler Error Log Output
```text
D:\cykuan\Godot_v4.6.2-stable_mono_win64\Orclimax\src\autoload\CombatManager.cs(56,65): error CS0176: 成員 'GameConfig.BaseMoveSpeed' 無法以執行個體參考進行存取; 請改用類型名稱 [D:\cykuan\Godot_v4.6.2-stable_mono_win64\Orclimax\Orclimax.csproj]
D:\cykuan\Godot_v4.6.2-stable_mono_win64\Orclimax\src\autoload\CombatManager.cs(110,65): error CS0176: 成員 'GameConfig.BaseMoveSpeed' 無法以執行個體參考進行存取; 請改用類型名稱 [D:\cykuan\Godot_v4.6.2-stable_mono_win64\Orclimax\Orclimax.csproj]
```

### Root Cause Analysis
1. In `src/autoload/GameConfig.cs` (lines 33-34), `BaseMoveSpeed` is declared as an instance property:
   ```csharp
   [Export] public float BaseMoveSpeed { get; set; } = 250.0f;
   ```
2. In `src/autoload/CombatManager.cs` lines 56 & 110, `BaseMoveSpeed` is accessed statically:
   ```csharp
   // Line 56:
   float baseMoveSpeed = GameConfig.BaseMoveSpeed;
   // Line 110:
   float baseMoveSpeed = GameConfig.BaseMoveSpeed;
   ```
3. Whereas in other lines of `CombatManager.cs` (lines 59, 68, 113, 114, 187), instance properties are properly accessed via `GameConfig.Instance`:
   ```csharp
   float basePleasureRate = GameConfig.Instance != null ? GameConfig.Instance.DefaultPleasureRateMultiplier : 0.5f;
   ```
4. Accessing instance property `BaseMoveSpeed` via class name `GameConfig.BaseMoveSpeed` causes C# compiler error CS0176 ("Member 'GameConfig.BaseMoveSpeed' cannot be accessed with an instance reference; declare it with a type name instead" / static vs instance mismatch).

---

## Suggested Remediation

In `src/autoload/CombatManager.cs`:
Replace lines 56 and 110:
```csharp
float baseMoveSpeed = GameConfig.Instance != null ? GameConfig.Instance.BaseMoveSpeed : 250f;
```
This will allow `dotnet test` to build and execute cleanly.
