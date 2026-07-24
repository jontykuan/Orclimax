# Milestone 1: Config & Navigation — Codebase Analysis & Technical Design

## 1. Executive Summary & Codebase Architecture Analysis

### 1.1 Project Overview
**Orclimax** is a hybrid Godot 4.6 (Mono / C# + GDScript) side-scrolling action RPG with backpack grid management and erotic vessel mechanics. The architecture leverages C# singletons (`GameManager`, `InventoryManager`, `CombatManager`) for core state management, grid logic, and combat data aggregation, while using GDScript for entity physics (`Player.gd`, `EnemyBase.gd`) and UI controllers (`BackpackUI.gd`, `HUD.gd`).

### 1.2 Existing Codebase Inspection Findings

| Component | File Path | Current State & Findings |
|---|---|---|
| **Main Entry** | `project.godot` | Currently sets `run/main_scene="res://src/ui/backpack/BackpackUI.tscn"`. Needs to be updated to `res://src/ui/title/TitleScreen.tscn`. |
| **Game State Manager** | `src/autoload/GameManager.cs` | Manages `GameState` enum (`Shop`, `Combat`, `GameOver`, `Victory`), Gold, Stage, and F1-F3 resolution shortcuts. Needs extension to handle full state machine and scene navigation. |
| **Combat Logic** | `src/autoload/CombatManager.cs` | Aggregates stats from `InventoryManager` backpack grid, processes passive pleasure accumulation, weapon firing timers, damage, and climax triggers. |
| **Grid & Vessel Engine** | `src/core/VesselData.cs`<br>`src/autoload/InventoryManager.cs` | Defines body placement zones (`Head`, `Chest`, `Groin`, `Limbs`, `General`, `Inactive`) and vessel grid structures. Needs extension for sensitivity stats and traits. |
| **Player Controller** | `src/entities/player/Player.gd` | Hardcodes constants like `gravity = 980.0`, `jump_velocity = -550.0`, `DOUBLE_TAP_DELAY = 0.25`, `dash_duration = 0.15`, `dash_cooldown = 0.6`. Needs refactoring to use `GameConfig`. |
| **Enemy Controller** | `src/entities/enemy/EnemyBase.gd` | Hardcodes `max_hp = 30.0`, `speed = 80.0`, `gold_reward = 2`, `drop_chance = 0.25`, action cooldowns and ranges. Needs refactoring to use `GameConfig`. |
| **Backpack UI** | `src/ui/backpack/BackpackUI.tscn`<br>`src/ui/backpack/BackpackUI.gd` | Contains bottom `HelpArea` (hint text box) on lines 251-263 of `.tscn`. Needs removal and layout adjustment. |

---

## 2. Technical Design Specifications for Milestone 1

### 2.1 Feature 1: `GameConfig.cs` — Centralized Config Engine

#### Objective
Centralize all gameplay parameters (physics, speeds, gravity, double-tap window thresholds, i-frame durations, parry windows, knockback forces, enemy stats, UI multipliers) into a single, clean C# AutoLoad singleton accessible natively by both C# and GDScript.

#### File Location
`src/core/GameConfig.cs` (AutoLoad: `GameConfig="*res://src/core/GameConfig.cs"` in `project.godot`).

#### C# Class Structure (`GameConfig.cs`)
```csharp
using Godot;
using System;

namespace Orclimax.Core
{
    public partial class GameConfig : Node
    {
        public static GameConfig Instance { get; private set; }

        // --- Physics & Dynamics ---
        [Export] public float Gravity { get; set; } = 980.0f;
        [Export] public float JumpVelocity { get; set; } = -550.0f;
        [Export] public float BaseMoveSpeed { get; set; } = 250.0f;
        [Export] public float DashSpeedMultiplier { get; set; } = 2.8f;
        [Export] public float DashDuration { get; set; } = 0.15f;
        [Export] public float DashCooldown { get; set; } = 0.6f;
        [Export] public float DoubleTapDelay { get; set; } = 0.25f;
        [Export] public float CrouchSpeedMultiplier { get; set; } = 0.4f;
        [Export] public float CrouchHeightRatio { get; set; } = 0.6f;
        [Export] public float VisualBaseScale { get; set; } = 4.0f;

        // --- Combat Action Windows & Timings ---
        [Export] public float IFrameDuration { get; set; } = 0.3f;
        [Export] public float ParryWindow { get; set; } = 0.2f;
        [Export] public Vector2 KnockbackForce { get; set; } = new Vector2(300.0f, -150.0f);
        [Export] public float ClimaxBlastDamage { get; set; } = 50.0f;

        // --- Enemy Base Defaults ---
        [Export] public float DefaultEnemyMaxHp { get; set; } = 30.0f;
        [Export] public float DefaultEnemySpeed { get; set; } = 80.0f;
        [Export] public int DefaultGoldReward { get; set; } = 2;
        [Export] public float DefaultDropChance { get; set; } = 0.25f;
        [Export] public float ClawSlashCooldown { get; set; } = 1.5f;
        [Export] public float ClawSlashDamage { get; set; } = 3.0f;
        [Export] public float ClawSlashRange { get; set; } = 110.0f;
        [Export] public float HeavyCleaveCooldown { get; set; } = 4.0f;
        [Export] public float HeavyCleaveDamage { get; set; } = 8.0f;
        [Export] public float HeavyCleaveRange { get; set; } = 130.0f;

        // --- Vessel & Pleasure Defaults ---
        [Export] public float DefaultBaseMaxPleasure { get; set; } = 100.0f;
        [Export] public float DefaultPleasureRateMultiplier { get; set; } = 0.5f;
        [Export] public float ThrustPleasureGain { get; set; } = 2.0f;

        // --- UI & Economy ---
        [Export] public float CellSize { get; set; } = 64.0f;
        [Export] public int ShopRerollCost { get; set; } = 2;
        [Export] public int StageClearGold { get; set; } = 10;
        [Export] public int InitialGold { get; set; } = 15;

        public override void _EnterTree()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                QueueFree();
            }
        }
    }
}
```

#### Dual-Language Access Pattern
- **In C#**:
  `GameConfig.Instance.Gravity` or `GameConfig.Instance.DoubleTapDelay`
- **In GDScript**:
  `GameConfig.Gravity`, `GameConfig.DoubleTapDelay`, `GameConfig.IFrameDuration`, `GameConfig.ParryWindow`, `GameConfig.KnockbackForce`

---

### 2.2 Feature 2: `TitleScreen.tscn` / `TitleScreen.gd` — Main Title & Submenus

#### Objective
Create an aesthetic dark-fantasy RPG main menu scene with Start (開始), Save/Load (存檔), Gallery (畫廊), Settings (設定), and Quit (結束) buttons, accompanied by modal overlay panels.

#### File Locations
- Scene: `src/ui/title/TitleScreen.tscn`
- Script: `src/ui/title/TitleScreen.gd`

#### Scene Node Tree Structure
```
TitleScreen (Control)
├── Background (ColorRect - Dark Indigo #0D0F17)
├── MainContainer (MarginContainer)
│   └── VBox (VBoxContainer)
│       ├── Header (VBoxContainer)
│       │   ├── TitleLabel ("ORCLIMAX", font size 48, #F59E0B)
│       │   └── SubtitleLabel ("The Orc & Vessel Conquest", font size 20, #94A3B8)
│       ├── MenuButtons (VBoxContainer, separation 15)
│       │   ├── StartButton ("Start Adventure / 開始冒險")
│       │   ├── SaveLoadButton ("Save & Load / 存檔紀錄")
│       │   ├── GalleryButton ("Erotic Gallery / 畫廊模式")
│       │   ├── SettingsButton ("System Settings / 系統設定")
│       │   └── QuitButton ("Quit Game / 離開遊戲")
│       └── FooterLabel ("v1.0.0 — Orclimax Studio")
├── SettingsModal (PanelContainer, initial visible = false)
│   └── Margin -> VBox (Title, Display Mode Buttons, Resolution Dropdown, Volume Sliders, Close Button)
├── SaveLoadModal (PanelContainer, initial visible = false)
│   └── Margin -> VBox (Title, Save Slots List 1-3, Close Button)
└── GalleryModal (PanelContainer, initial visible = false)
    └── Margin -> VBox (Title, Unlocked Vessels CG Grid, Close Button)
```

#### Script Mechanics (`TitleScreen.gd`)
```gdscript
extends Control

@onready var start_btn: Button = $MainContainer/VBox/MenuButtons/StartButton
@onready var save_load_btn: Button = $MainContainer/VBox/MenuButtons/SaveLoadButton
@onready var gallery_btn: Button = $MainContainer/VBox/MenuButtons/GalleryButton
@onready var settings_btn: Button = $MainContainer/VBox/MenuButtons/SettingsButton
@onready var quit_btn: Button = $MainContainer/VBox/MenuButtons/QuitButton

@onready var settings_modal: PanelContainer = $SettingsModal
@onready var save_load_modal: PanelContainer = $SaveLoadModal
@onready var gallery_modal: PanelContainer = $GalleryModal

func _ready() -> void:
	settings_modal.visible = false
	save_load_modal.visible = false
	gallery_modal.visible = false
	
	start_btn.pressed.connect(_on_start_pressed)
	save_load_btn.pressed.connect(func(): save_load_modal.visible = true)
	gallery_btn.pressed.connect(func(): gallery_modal.visible = true)
	settings_btn.pressed.connect(func(): settings_modal.visible = true)
	quit_btn.pressed.connect(func(): get_tree().quit())

func _on_start_pressed() -> void:
	GameManager.StartNewGame()
```

---

### 2.3 Feature 3: `VesselUI.tscn` / `VesselUI.gd` — Vessel Inspection & Selection

#### Objective
Display comprehensive vessel statistics, body zone sensitivities (Head, Chest, Groin, Limbs), skills, pleasure multipliers, traits, and grid layouts, allowing players to select and inspect vessels prior to prep.

#### Enhancements to `VesselData.cs`
Add sensitivity attributes and traits to `src/core/VesselData.cs`:
```csharp
[Export] public float HeadSensitivity { get; set; } = 1.0f;
[Export] public float ChestSensitivity { get; set; } = 1.2f;
[Export] public float GroinSensitivity { get; set; } = 1.5f;
[Export] public float LimbsSensitivity { get; set; } = 0.8f;
[Export] public Godot.Collections.Array<string> Traits { get; set; } = new Godot.Collections.Array<string>();
[Export] public string LoreDescription { get; set; } = "";
```

#### File Locations
- Scene: `src/ui/vessel/VesselUI.tscn`
- Script: `src/ui/vessel/VesselUI.gd`

#### Scene Node Tree Structure
```
VesselUI (Control)
├── Background (ColorRect)
├── MainLayout (VBoxContainer)
│   ├── TopBar (HBoxContainer)
│   │   ├── Title ("Vessel Inspection & Selection / 女騎士整備與選擇")
│   │   ├── Spacer
│   │   └── BackToTitleButton ("Back to Title")
│   └── ContentHBox (HBoxContainer)
│       ├── VesselSelectionList (VBoxContainer, left column)
│       │   ├── Label ("Available Vessels")
│       │   └── ScrollContainer / VesselCardsContainer
│       └── DetailsPanel (HBoxContainer, right column)
│           ├── LeftDetails (VBoxContainer)
│           │   ├── CharacterNameLabel
│           │   ├── BustPreview (TextureRect)
│           │   ├── SensitivitiesGroup (VBoxContainer: Head, Chest, Groin, Limbs bars/labels)
│           │   └── PleasureMultipliersLabel (Base Max Pleasure, Build Rate)
│           └── RightDetails (VBoxContainer)
│               ├── SkillTitle & Description (Climax Skill Name & Blast Damage)
│               ├── TraitsList (VBoxContainer / RichTextLabel)
│               ├── GridPreviewControl (Visual diagram of body zone grid)
│               └── SelectVesselButton ("Confirm Vessel / 選擇裝載此女騎士")
```

---

### 2.4 Feature 4: `MapUI.tscn` / `MapUI.gd` — Overworld Stage Node Navigation

#### Objective
Display cleared nodes, current stage, and branching combat paths in a multi-stage node tree map layout.

#### Node Map Graph Data Architecture
Each stage consists of 4 node depths:
- **Layer 0**: Entry Node (`Combat` / `Shop`)
- **Layer 1**: Branching Path A (`Combat`), Path B (`EliteCombat`)
- **Layer 2**: Branching Path A (`Shop`), Path B (`Combat`)
- **Layer 3**: Chapter Boss Node (`Boss`)

Node Types: `Combat`, `EliteCombat`, `Shop`, `Boss`
Node States: `Locked`, `Available`, `Current`, `Cleared`

#### File Locations
- Scene: `src/ui/map/MapUI.tscn`
- Script: `src/ui/map/MapUI.gd`

#### Scene Layout Hierarchy
```
MapUI (Control)
├── Background (ColorRect - Dark Map Texture/Color)
├── MainLayout (VBoxContainer)
│   ├── Header (HBoxContainer)
│   │   ├── StageTitle ("WORLD MAP — STAGE 1")
│   │   ├── GoldLabel ("GOLD: 15")
│   │   ├── Spacer
│   │   └── BackpackPrepButton ("Backpack / 背包整備")
│   └── MapArea (ScrollContainer / Control)
│       ├── PathLineRenderer (Control - draws connection lines between nodes)
│       └── NodesContainer (Control - holds node buttons positioned across layers)
```

#### Node Navigation Behavior
When player clicks an `Available` node:
- Mark previous current node as `Cleared`.
- Set new node as `Current`.
- Update accessible next layer nodes to `Available`.
- Call `GameManager.EnterNode(node_type)`:
  - If `Shop`: Transition to `BackpackUI.tscn`.
  - If `Combat` / `EliteCombat` / `Boss`: Transition to `Level.tscn`.

---

### 2.5 Feature 5: `BackpackUI.tscn` Cleanup

#### Objective
Remove the bottom hint text box (`HelpArea`) and integrate navigation buttons in the top header.

#### Modifications
1. In `src/ui/backpack/BackpackUI.tscn`:
   - Delete node `MainLayout/HelpArea` (lines 251-263).
   - In `MainLayout/Header`:
     - Add `MapButton` ("World Map / 世界地圖").
     - Add `TitleMenuButton` ("Title / 主選單").
2. In `src/ui/backpack/BackpackUI.gd`:
   - Connect `MapButton.pressed` -> `GameManager.GoToMap()`.
   - Connect `TitleMenuButton.pressed` -> `GameManager.GoToTitle()`.

---

### 2.6 Feature 6: `GameManager.cs` — State Machine & Seamless Navigation

#### Objective
Centralize game state management and seamless scene transitions: `Title` -> `Vessel Select` -> `Backpack` -> `World Map` -> `Combat`.

#### Enum Update (`src/core/Enums.cs`)
```csharp
namespace Orclimax.Core
{
    public enum GameState
    {
        Title = 0,
        VesselSelect = 1,
        Backpack = 2,
        WorldMap = 3,
        Combat = 4,
        GameOver = 5,
        Victory = 6
    }
}
```

#### Updated `GameManager.cs` Implementation
```csharp
using Godot;
using System;
using Orclimax.Core;

namespace Orclimax.Autoload
{
    public partial class GameManager : Node
    {
        public static GameManager Instance { get; private set; }

        [Signal] public delegate void StateChangedEventHandler(int newState);
        [Signal] public delegate void GoldChangedEventHandler(int newGold);
        [Signal] public delegate void StageChangedEventHandler(int newStage);

        private GameState _currentState = GameState.Title;
        private int _gold = 15;
        private int _currentStage = 1;

        public const string TitleScenePath = "res://src/ui/title/TitleScreen.tscn";
        public const string VesselSelectScenePath = "res://src/ui/vessel/VesselUI.tscn";
        public const string BackpackScenePath = "res://src/ui/backpack/BackpackUI.tscn";
        public const string MapScenePath = "res://src/ui/map/MapUI.tscn";
        public const string CombatScenePath = "res://src/entities/player/Level.tscn";

        public GameState CurrentState
        {
            get => _currentState;
            set
            {
                if (_currentState != value)
                {
                    _currentState = value;
                    EmitSignal(SignalName.StateChanged, (int)_currentState);
                }
            }
        }

        public int Gold
        {
            get => _gold;
            set
            {
                _gold = value;
                EmitSignal(SignalName.GoldChanged, _gold);
            }
        }

        public int CurrentStage
        {
            get => _currentStage;
            set
            {
                _currentStage = value;
                EmitSignal(SignalName.StageChanged, _currentStage);
            }
        }

        public override void _EnterTree()
        {
            if (Instance == null) Instance = this;
            else QueueFree();
        }

        public override void _Ready()
        {
            Gold = GameConfig.Instance != null ? GameConfig.Instance.InitialGold : 15;
        }

        public void StartNewGame()
        {
            Gold = GameConfig.Instance != null ? GameConfig.Instance.InitialGold : 15;
            CurrentStage = 1;
            GoToVesselSelect();
        }

        public void GoToTitle()
        {
            CurrentState = GameState.Title;
            GetTree().ChangeSceneToFile(TitleScenePath);
        }

        public void GoToVesselSelect()
        {
            CurrentState = GameState.VesselSelect;
            GetTree().ChangeSceneToFile(VesselSelectScenePath);
        }

        public void GoToBackpack()
        {
            CurrentState = GameState.Backpack;
            GetTree().ChangeSceneToFile(BackpackScenePath);
        }

        public void GoToMap()
        {
            CurrentState = GameState.WorldMap;
            GetTree().ChangeSceneToFile(MapScenePath);
        }

        public void StartCombatNode()
        {
            CurrentState = GameState.Combat;
            CombatManager.Instance.StartCombat();
            GetTree().ChangeSceneToFile(CombatScenePath);
        }

        public void AddGold(int amount) => Gold += amount;

        public bool SpendGold(int amount)
        {
            if (Gold >= amount)
            {
                Gold -= amount;
                return true;
            }
            return false;
        }

        public void AdvanceStage()
        {
            CurrentStage++;
            AddGold(GameConfig.Instance != null ? GameConfig.Instance.StageClearGold : 10);
            GoToMap();
        }

        public void TriggerGameOver()
        {
            CurrentState = GameState.GameOver;
        }
    }
}
```

---

## 3. Evidence Chain & Design Rationale

1. **Dual-Language Interoperability**:
   - *Observation*: Godot 4.6 Mono allows C# AutoLoads to be called globally in GDScript by node name (`GameConfig.property`).
   - *Rationale*: Setting `GameConfig.cs` as an AutoLoad guarantees single-source-of-truth configuration without requiring manual node lookup or duplicate constant definitions.

2. **Aesthetic Polish & Compliance with `AGENTS.md`**:
   - *Observation*: Item #15 of `AGENTS.md` mandates stripping unicode emojis and enforcing high-contrast dark RPG theme styles.
   - *Rationale*: All new UI components (`TitleScreen`, `VesselUI`, `MapUI`) are designed using clean text labels, custom color accents (#F59E0B gold, #38BDF8 cyan, #F472B6 pink), and structured panel containers.

3. **Seamless Navigation Flow**:
   - *Observation*: Main game scene was hardcoded directly to `BackpackUI.tscn`.
   - *Rationale*: Updating `project.godot` `run/main_scene` to `TitleScreen.tscn` and delegating all scene transitions to `GameManager.cs` provides a complete, polished RPG navigation loop: `Title` -> `Vessel Select` -> `Backpack` -> `World Map` -> `Combat` -> `World Map`.

---

## 4. Implementation Step Roadmap

```
Step 1: Create GameConfig.cs & update project.godot AutoLoad
Step 2: Update Enums.cs & GameManager.cs for navigation state machine
Step 3: Refactor Player.gd and EnemyBase.gd to read from GameConfig
Step 4: Update VesselData.cs with sensitivity & trait exports
Step 5: Create TitleScreen.tscn & TitleScreen.gd
Step 6: Create VesselUI.tscn & VesselUI.gd
Step 7: Create MapUI.tscn & MapUI.gd
Step 8: Clean up BackpackUI.tscn (remove HelpArea) & add header nav buttons
Step 9: Update project.godot main_scene to TitleScreen.tscn
Step 10: Run build & verification
```

---

## 5. Verification Method

1. **Compilation Check**:
   - Command: `dotnet build Orclimax.csproj`
   - Expected Output: Build succeeded with 0 Errors.

2. **Scene & Navigation Inspection**:
   - Verify presence of scenes: `TitleScreen.tscn`, `VesselUI.tscn`, `MapUI.tscn`, `BackpackUI.tscn`, `Level.tscn`.
   - Check `project.godot` for `GameConfig` AutoLoad and `run/main_scene="res://src/ui/title/TitleScreen.tscn"`.
