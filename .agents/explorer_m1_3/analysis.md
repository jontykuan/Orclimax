# Technical Analysis Report — Milestone 1 (Config & Navigation)

**Author**: `explorer_m1_3`  
**Date**: 2026-07-24  
**Project**: Orclimax (`Godot 4.6.2-stable Mono / C# .NET 8.0`)  
**Scope**: Build configuration, project autoloads, `GameConfig.cs` parameterization specs, UI multi-page scene structures (`TitleScreen`, `VesselUI`, `MapUI`, `BackpackUI`), and state transition test plan.

---

## 1. Project Configuration & Build Audit

### 1.1 `Orclimax.csproj`
* **Target Framework**: `.NET 8.0` (`net8.0`)
* **SDK**: `Godot.NET.Sdk/4.6.2`
* **Dynamic Loading**: `EnableDynamicLoading = true`
* **Analysis**:
  - Clean C# project configuration.
  - All C# scripts are under `Orclimax.Core` or `Orclimax.Autoload` namespaces.
  - No external NuGet dependencies currently required; test suite can leverage Godot test scripts or custom C# assertion runners.

### 1.2 `project.godot`
* **Display Configuration**:
  - Viewport Width: `1600`, Height: `1200`
  - Window Mode: `canvas_items` stretch, `keep` aspect ratio
* **Current Autoloads**:
  - `GameManager="*res://src/autoload/GameManager.cs"`
  - `InventoryManager="*res://src/autoload/InventoryManager.cs"`
  - `CombatManager="*res://src/autoload/CombatManager.cs"`
* **Required Autoload Additions**:
  - `GameConfig="*res://src/autoload/GameConfig.cs"` (Registered as Autoload so C# and GDScript can access global constants seamlessly).
* **Main Scene Entry Point Modification**:
  - **Current**: `run/main_scene="res://src/ui/backpack/BackpackUI.tscn"`
  - **Required**: `run/main_scene="res://src/ui/title/TitleScreen.tscn"`

---

## 2. Parameterized Game Constants Specification (`GameConfig.cs`)

Centralizing all physical movement constants, combat parameters, double-tap window thresholds, i-frame durations, parry windows, knockback forces, enemy stats, and UI multipliers into `src/autoload/GameConfig.cs`.

### 2.1 Concrete Parameter Values

| Parameter Name | Type | Default Value | Description / Usage |
|---|---|---|---|
| **Physics & Player Base Movement** | | | |
| `DefaultGravity` | `float` | `980.0f` | Downward acceleration applied to CharacterBody2D |
| `DefaultJumpVelocity` | `float` | `-550.0f` | Initial upward velocity when jumping |
| `BaseMoveSpeed` | `float` | `250.0f` | Base horizontal movement speed (px/sec) |
| `VisualBaseScale` | `float` | `4.0f` | Base scale for player sprite root node |
| `CrouchSpeedMultiplier` | `float` | `0.4f` | Speed factor applied when crouching |
| `CrouchHeightRatio` | `float` | `0.6f` | Height reduction ratio applied to collision shape |
| **Input & Double-Tap Timings** | | | |
| `DoubleTapWindow` | `float` | `0.25f` | Max delay (sec) between taps to register directional action |
| **Action 1: Left/Right x2 (Dodge/Dash)** | | | |
| `DashSpeedMultiplier` | `float` | `2.8f` | Speed multiplier applied during burst dash |
| `DashDuration` | `float` | `0.15f` | Active dash duration (sec) |
| `DashCooldown` | `float` | `0.6f` | Cooldown period before dash can be reused |
| `DashIFrameDuration` | `float` | `0.2f` | Duration of invulnerability during dash |
| **Action 2: Up x2 (Precise Guard / Parry)** | | | |
| `ParryWindow` | `float` | `0.2f` | Active parry frame window (sec) |
| `ParryCooldown` | `float` | `1.0f` | Cooldown period before parry can be reused |
| `ParryCounterDamage` | `float` | `15.0f` | Damage dealt to enemy on successful parry |
| `ParryReflectSpeed` | `float` | `700.0f` | Speed of reflected projectile |
| **Action 3: Down x2 (Heavy Thrust)** | | | |
| `HeavyThrustCooldown` | `float` | `1.5f` | Cooldown period before heavy thrust can be reused |
| `HeavyThrustKnockbackForce` | `float` | `400.0f` | Horizontal knockback impulse applied to enemies |
| `HeavyThrustRange` | `float` | `120.0f` | AOE radius around player for thrust knockback |
| `HeavyThrustDamage` | `float` | `10.0f` | Direct physical damage dealt by heavy thrust |
| `HeavyThrustPleasureBoost` | `float` | `15.0f` | Flat bonus added to Vessel pleasure gauge on hit |
| **Base Player Combat Stats** | | | |
| `BaseMaxHp` | `float` | `100.0f` | Base max hit points |
| `BaseAttackSpeed` | `float` | `1.5f` | Base attack speed multiplier |
| `BaseArmor` | `float` | `0.0f` | Base physical damage absorption |
| `BasePleasureRate` | `float` | `0.5f` | Base pleasure generation multiplier |
| `BaseMaxPleasure` | `float` | `100.0f` | Base max pleasure threshold |
| **Enemy Stats (4 Types)** | | | |
| *Melee Basic Enemy* | | | |
| `MeleeHp` | `float` | `30.0f` | Basic melee health |
| `MeleeSpeed` | `float` | `80.0f` | Basic melee move speed |
| `MeleeGoldReward` | `int` | `2` | Gold dropped upon kill |
| `MeleeDropChance` | `float` | `0.25f` | Item drop probability |
| `MeleeClawDamage` | `float` | `3.0f` | Quick attack damage |
| `MeleeCleaveDamage` | `float` | `8.0f` | Heavy cleave damage |
| *Ranged Enemy* | | | |
| `RangedHp` | `float` | `25.0f` | Ranged enemy health |
| `RangedSpeed` | `float` | `60.0f` | Ranged enemy move speed |
| `RangedGoldReward` | `int` | `3` | Gold dropped upon kill |
| `RangedDropChance` | `float` | `0.30f` | Item drop probability |
| `RangedStraightSpeed` | `float` | `400.0f` | Straight projectile speed |
| `RangedParabolicSpeedX` | `float` | `300.0f` | Parabolic projectile horizontal speed |
| `RangedParabolicSpeedY` | `float` | `-400.0f` | Parabolic projectile initial arc velocity |
| `RangedAttackCooldown` | `float` | `2.5f` | Attack interval |
| *Shielded Enemy* | | | |
| `ShieldedHp` | `float` | `50.0f` | Shielded enemy health |
| `ShieldedSpeed` | `float` | `45.0f` | Shielded enemy move speed |
| `ShieldedGoldReward` | `int` | `4` | Gold dropped upon kill |
| `ShieldedDropChance` | `float` | `0.35f` | Item drop probability |
| `ShieldedPhysicalDamageMultiplier` | `float` | `0.3f` | Physical damage multiplier (70% armor reduction) |
| `ShieldedMagicDamageMultiplier` | `float` | `1.0f` | Magic/Toy damage multiplier (100% full damage) |
| *Flying Enemy* | | | |
| `FlyingHp` | `float` | `20.0f` | Flying enemy health |
| `FlyingSpeed` | `float` | `100.0f` | Flying enemy move speed |
| `FlyingGoldReward` | `int` | `3` | Gold dropped upon kill |
| `FlyingSineAmplitude` | `float` | `50.0f` | Vertical hover sine wave height (px) |
| `FlyingSineFrequency` | `float` | `2.0f` | Hover oscillation speed |
| `FlyingSwoopSpeed` | `float` | `250.0f` | Diagonal swoop attack velocity |
| *Vessel-Snatcher Enemy* | | | |
| `SnatcherHp` | `float` | `35.0f` | Snatcher enemy health |
| `SnatcherSpeed` | `float` | `120.0f` | Snatcher pursuit move speed |
| `SnatcherGoldReward` | `int` | `5` | Gold dropped upon kill |
| `SnatcherGrabRange` | `float` | `50.0f` | Range threshold to trigger disarm grab |
| `SnatcherCarrySpeed` | `float` | `90.0f` | Speed while carrying detached Vessel away |
| **UI & System Multipliers** | | | |
| `CellSizeDefault` | `float` | `64.0f` | Backpack grid cell dimension in pixels |
| `RerollCost` | `int` | `2` | Gold cost to reroll shop items |
| `StageClearGoldBonus` | `int` | `10` | Gold awarded on stage clear |
| `SellPriceRatio` | `float` | `0.5f` | Fraction of base price returned when selling |

---

## 3. UI Scene Node Structures & Navigation Flow

### 3.1 `GameState` Enum Update (`src/core/Enums.cs`)
```csharp
namespace Orclimax.Core
{
    public enum GameState
    {
        Title = 0,         // Main Title Screen
        VesselSelect = 1,  // Vessel Management Page
        Map = 2,           // World Map Page
        Shop = 3,          // Backpack Preparation & Shop Phase
        Combat = 4,        // Side-scrolling Combat Action Phase
        GameOver = 5,      // Orc Defeated
        Victory = 6        // Stage / Run Cleared
    }
}
```

### 3.2 Page Scene Specifications

#### 1. Title Screen (`res://src/ui/title/TitleScreen.tscn`)
* **Node Hierarchy**:
  ```
  TitleScreen (Control, Full Rect)
  ├── Background (ColorRect, color=#0d0e15)
  ├── MainLayout (VBoxContainer, centered)
  │   ├── Header (VBoxContainer)
  │   │   ├── GameTitleLabel ("ORCLIMAX", font_size=54, color=#f59e0b)
  │   │   └── SubtitleLabel ("Dark Fantasy Tactical RPG", font_size=20, color=#94a3b8)
  │   ├── Spacer (Control, custom_minimum_size y=40)
  │   ├── MenuButtons (VBoxContainer, separation=15, custom_minimum_size x=260)
  │   │   ├── StartButton (Button, "Start Game (開始遊戲)")
  │   │   ├── SaveLoadButton (Button, "Save / Load (存檔 / 讀檔)")
  │   │   ├── GalleryButton (Button, "Gallery (畫廊)")
  │   │   ├── SettingsButton (Button, "Settings (設定)")
  │   │   └── QuitButton (Button, "Quit (結束遊戲)")
  │   └── VersionLabel (Label, "v0.2.0 - Milestone 1", bottom right)
  ```
* **Behaviors**:
  - `StartButton`: `GameManager.CurrentState = GameState.VesselSelect`; change scene to `res://src/ui/vessel/VesselUI.tscn`.
  - `QuitButton`: `get_tree().quit()`.

#### 2. Vessel Management Page (`res://src/ui/vessel/VesselUI.tscn`)
* **Node Hierarchy**:
  ```
  VesselUI (Control, Full Rect)
  ├── Background (ColorRect, color=#0f172a)
  ├── MainLayout (VBoxContainer, margin 20)
  │   ├── Header (HBoxContainer)
  │   │   ├── PageTitle (Label, "VESSEL MANAGEMENT (魔導器管理)", font_size=24)
  │   │   ├── Spacer (Control, size_flags_horizontal=Expand)
  │   │   └── BackToTitleButton (Button, "Title Screen")
  │   ├── ContentSplit (HBoxContainer, separation=20, size_flags_vertical=Expand)
  │   │   ├── LeftPanel (VBoxContainer, min_width=300)
  │   │   │   ├── Title (Label, "Select Vessel")
  │   │   │   ├── VesselScroll (ScrollContainer)
  │   │   │   │   └── VesselList (VBoxContainer)
  │   │   │   └── EquipButton (Button, "Equip Active Vessel")
  │   │   ├── CenterPanel (PanelContainer, min_width=350)
  │   │   │   ├── BustImage (TextureRect)
  │   │   │   └── VesselName (Label, font_size=22)
  │   │   └── RightPanel (VBoxContainer, min_width=400)
  │   │       ├── SpecsTitle (Label, "Sensitivity & Stats")
  │   │       ├── SensitivityGrid (GridContainer, columns=2)
  │   │       │   ├── HeadSens ("Head Sensitivity: Level 1")
  │   │       │   ├── ChestSens ("Chest Sensitivity: Level 2")
  │   │       │   ├── GroinSens ("Groin Sensitivity: Level 3")
  │   │       │   └── LimbsSens ("Limbs Sensitivity: Level 1")
  │   │       ├── SkillPanel (PanelContainer)
  │   │       │   └── SkillText ("Climax Skill: Lightning Cascade")
  │   │       └── TraitText (RichTextLabel, "Traits & Pleasure Multipliers")
  │   └── Footer (HBoxContainer)
  │       └── ConfirmButton (Button, "Confirm & Proceed to World Map ->")
  ```
* **Behaviors**:
  - Switching vessel updates details and calls `InventoryManager.SetVessel(vessel)`.
  - `ConfirmButton`: `GameManager.CurrentState = GameState.Map`; change scene to `res://src/ui/map/MapUI.tscn`.

#### 3. World Map Page (`res://src/ui/map/MapUI.tscn`)
* **Node Hierarchy**:
  ```
  MapUI (Control, Full Rect)
  ├── Background (ColorRect, color=#090d16)
  ├── MainLayout (VBoxContainer, margin 20)
  │   ├── Header (HBoxContainer)
  │   │   ├── StageTitle (Label, "WORLD MAP - STAGE 1", font_size=24)
  │   │   ├── Spacer (Control, size_flags_horizontal=Expand)
  │   │   └── GoldLabel (Label, "GOLD: 15", color=#f59e0b)
  │   ├── MapViewport (PanelContainer, size_flags_vertical=Expand)
  │   │   ├── GraphControl (Control - custom line rendering)
  │   │   └── NodesContainer (Control)
  │   │       ├── StageNode1 (Button, Cleared)
  │   │       ├── StageNode2A (Button, Available)
  │   │       └── StageNode2B (Button, Available)
  │   ├── DetailPanel (PanelContainer, min_height=100)
  │   │   └── DetailHBox (HBoxContainer)
  │   │       ├── NodeDesc (Label, "Selected Node Details")
  │   │       ├── Spacer (Control, size_flags_horizontal=Expand)
  │   │       └── EnterPrepButton (Button, "Enter Preparation (整備)")
  │   └── Footer (HBoxContainer)
  │       └── BackToVesselButton (Button, "<- Back to Vessel Management")
  ```
* **Behaviors**:
  - `EnterPrepButton`: `GameManager.CurrentState = GameState.Shop`; change scene to `res://src/ui/backpack/BackpackUI.tscn`.

#### 4. Backpack Preparation Page (`res://src/ui/backpack/BackpackUI.tscn`)
* **Modifications**:
  - **Removal of Hint Text Box**: Delete `MainLayout/HelpArea` and `HelpLabel` node.
  - **Header Navigation**: Add `MapButton` ("<- World Map") allowing clean navigation back to MapUI.
  - `StartCombatButton`: `GameManager.CurrentState = GameState.Combat`; change scene to `res://src/entities/player/Level.tscn`.

---

## 4. C# Test Plan & Validation Strategy

### 4.1 Unit Testing Strategy for C# Core & Autoloads
Godot 4.6 C# scripts run within the .NET runtime. Unit tests can be executed either by running a C# test suite runner scene (`res://src/tests/TestRunner.tscn` / `TestRunner.cs`) or running `godot --headless --script res://src/tests/RunTests.gd`.

### 4.2 Concrete Test Cases

#### `GameConfigTests`
1. `Test_GameConfig_DefaultValues`: Assert `Gravity == 980.0f`, `DoubleTapWindow == 0.25f`, `DashCooldown == 0.6f`, `ParryWindow == 0.2f`, `HeavyThrustKnockbackForce == 400.0f`.
2. `Test_ShieldedEnemy_DamageReduction`: Assert physical damage is scaled by `0.3f` while magic damage is scaled by `1.0f`.

#### `GameManagerTests`
1. `Test_InitialState`: Assert `CurrentState == GameState.Title`, `Gold == 15`, `CurrentStage == 1`.
2. `Test_StateTransitions`: Assert setting `CurrentState` emits `StateChanged` signal and stores value correctly across full flow (`Title -> VesselSelect -> Map -> Shop -> Combat -> Victory -> Map`).
3. `Test_GoldAndStage`: Assert `AddGold`, `SpendGold`, and `AdvanceStage` update properties and emit appropriate signals.

---

## 5. Summary & Action Plan for Implementation
1. Register `GameConfig.cs` in `project.godot` Autoload section.
2. Update `run/main_scene` to `res://src/ui/title/TitleScreen.tscn`.
3. Create `TitleScreen.tscn` & `TitleScreen.gd`, `VesselUI.tscn` & `VesselUI.gd`, `MapUI.tscn` & `MapUI.gd`.
4. Modify `BackpackUI.tscn` & `BackpackUI.gd` to remove `HelpArea` hint box and add `MapButton`.
5. Implement unit tests in `src/tests/`.
