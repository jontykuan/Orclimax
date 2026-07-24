# Milestone 1 (Config & Navigation) Architectural Analysis

## Executive Summary
This report analyzes the codebase architecture and cross-language (C# / GDScript) bindings for **Milestone 1 (Config & Navigation)** in Orclimax. It covers global config declaration (`GameConfig.cs`), clean UI modifications in `BackpackUI`, data structures and UI design for vessel attributes (`VesselData.cs` / `VesselUI`), node map navigation (`MapUI`), and state-driven scene flow management (`GameManager.cs`).

---

## 1. GameConfig.cs & Cross-Language GDScript / C# Interop

### 1.1 Declaration Structure
`GameConfig.cs` should be created under `src/autoload/GameConfig.cs` as a Godot Autoload Singleton extending `Godot.Node`.

```csharp
using Godot;
using System;

namespace Orclimax.Autoload
{
    public partial class GameConfig : Node
    {
        public static GameConfig Instance { get; private set; }

        // Core Physics & Movement Configs
        [Export] public float Gravity { get; set; } = 980.0f;
        [Export] public float PlayerJumpVelocity { get; set; } = -550.0f;
        [Export] public float BasePlayerSpeed { get; set; } = 250.0f;
        [Export] public float BaseMonsterSpeed { get; set; } = 80.0f;
        [Export] public float BaseMonsterHp { get; set; } = 30.0f;

        // Economy & Stage Configs
        [Export] public int StartingGold { get; set; } = 15;
        [Export] public int StageClearGoldReward { get; set; } = 10;
        [Export] public int RerollCost { get; set; } = 2;

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

### 1.2 Registration in `project.godot`
Add `GameConfig` to the `[autoload]` section:
```ini
[autoload]

GameManager="*res://src/autoload/GameManager.cs"
InventoryManager="*res://src/autoload/InventoryManager.cs"
CombatManager="*res://src/autoload/CombatManager.cs"
GameConfig="*res://src/autoload/GameConfig.cs"
```

### 1.3 Interop Rules & Access Patterns
1. **From GDScript (`Player.gd`, `EnemyBase.gd`, `HUD.gd`, `BackpackUI.gd`)**:
   - Access directly via global Autoload identifier: `GameConfig.BasePlayerSpeed`, `GameConfig.Gravity`, `GameConfig.StartingGold`.
   - Godot 4 Mono maps exported properties to GDScript automatically.
2. **From C# (`CombatManager.cs`, `InventoryManager.cs`)**:
   - Access via static singleton instance: `GameConfig.Instance.BasePlayerSpeed`.
3. **Key Requirements for Godot 4.6 C# / GDScript Bindings**:
   - Class MUST extend `Godot.Node` (or `GodotObject`).
   - Class MUST be declared with `public partial class`.
   - Property getter/setter syntax `public T Prop { get; set; }` with `[Export]` attribute ensures full visibility in Godot Inspector and GDScript scope.

---

## 2. BackpackUI Bottom Hint Text Node Removal

### 2.1 Target Node Identification
In `res://src/ui/backpack/BackpackUI.tscn`:
- **Node Path**: `MainLayout/HelpArea` (containing child `HelpLabel` of type `RichTextLabel`).
- **Exact lines in `BackpackUI.tscn`**: Lines 251–263.

```tscn
[node name="HelpArea" type="MarginContainer" parent="MainLayout"]
layout_mode = 2
theme_override_constants/margin_top = 5

[node name="HelpLabel" type="RichTextLabel" parent="MainLayout/HelpArea"]
layout_mode = 2
...
```

### 2.2 Dependency Verification
- `src/ui/backpack/BackpackUI.gd` does **NOT** declare `@onready var help_area` nor does it reference `HelpArea` / `HelpLabel` in any functions.
- Removing `MainLayout/HelpArea` node from `BackpackUI.tscn` removes the bottom hint text cleanly without breaking any script execution.

---

## 3. VesselData.cs Expansion & VesselUI.tscn / .gd Design

### 3.1 `VesselData.cs` Enhancements
To support rendering body part sensitivity/development levels, skills, pleasure multipliers, and traits, `VesselData.cs` needs the following fields:

```csharp
// Body Part Sensitivity / Development Multipliers
[Export] public float HeadSensitivity { get; set; } = 1.0f;
[Export] public float ChestSensitivity { get; set; } = 1.0f;
[Export] public float GroinSensitivity { get; set; } = 1.0f;
[Export] public float LimbsSensitivity { get; set; } = 1.0f;

[Export] public int HeadDevLevel { get; set; } = 1;
[Export] public int ChestDevLevel { get; set; } = 1;
[Export] public int GroinDevLevel { get; set; } = 1;
[Export] public int LimbsDevLevel { get; set; } = 1;

// Passive Traits
[Export] public Godot.Collections.Array<string> Traits { get; set; } = new Godot.Collections.Array<string>();
[Export] public string TraitsDescription { get; set; } = "";
```

Existing fields in `VesselData.cs` already cover:
- `CharacterName`, `BustIcon`, `ClimaxCg`
- `ClimaxSkillName`, `ClimaxSkillDescription`
- `BaseMaxPleasure`, `PleasureBuildRateMultiplier`

### 3.2 `VesselUI.tscn` / `VesselUI.gd` Rendering Strategy
`VesselUI.tscn` will serve as the detailed inspection card / vessel view in Vessel Selection and Backpack prep phase.

- **UI Layout Hierarchy**:
  - `VesselUI` (Control / PanelContainer)
    - `HBoxContainer`
      - `LeftPanel` (Portrait `BustIcon`, Name Label, Trait Badges)
      - `RightPanel` (VBoxContainer)
        - `StatsHeader`: Max Pleasure (`BaseMaxPleasure`), Gain Rate (`PleasureBuildRateMultiplier`).
        - `BodyPartsGrid`: 2x2 grid showing Head, Chest, Groin, Limbs with development level progress bars and sensitivity multipliers.
        - `SkillBox`: Climax Skill Name (`ClimaxSkillName`), Description (`ClimaxSkillDescription`).
        - `TraitsBox`: Passive Trait badges and description (`TraitsDescription`).

---

## 4. MapUI.tscn Requirements & World Navigation

### 4.1 Data Structure & State Tracking
`MapUI` represents the branching stage progression (similar to Roguelike node maps):
- **Cleared Nodes**: `Godot.Collections.Array<int>` or `HashSet<int>` stored in `GameManager` tracking completed node IDs.
- **Current Stage / Node**: `GameManager.CurrentStage` tracking current depth.
- **Branching Paths**: Array of `MapNodeData` objects representing stages, connected via predecessor/successor links.
  - Node Types: `Combat` (Normal enemy), `Elite` (Mini-boss), `Shop` (Black market event), `Boss` (Stage climax boss).

### 4.2 Scene Architecture (`MapUI.tscn` & `MapUI.gd`)
- **Visual Rendering**:
  - `MapScrollContainer` containing `MapCanvas` (Control node).
  - Custom `_draw()` function drawing connection lines (`draw_line`) between parent and child nodes.
  - `MapNodeUI` instances (Button nodes) placed at calculated coordinate positions.
- **Node Statuses**:
  - `Cleared`: Beaten stage (dimmed/green checkmark).
  - `Current`: Active position (glowing highlight).
  - `Available`: Valid next selectable paths (interactive button).
  - `Locked`: Future unreached nodes (disabled/darkened).
- **Navigation Action**: Clicking an `Available` node calls `GameManager.SelectMapNode(node_id)` which advances `CurrentStage`, sets `CurrentState = GameState.Backpack` (for prep phase) or `GameState.Combat`.

---

## 5. GameManager.cs Scene Flow Management

### 5.1 GameState Enum Expansion (`src/core/Enums.cs`)
Expand `GameState` to represent the full application flow:

```csharp
namespace Orclimax.Core
{
    public enum GameState
    {
        Title,          // Title Screen
        VesselSelect,   // Character / Vessel Selection
        Backpack,       // Shop & Equipment Preparation Phase
        WorldMap,       // Stage Node Navigation Map
        Combat,         // Side-scrolling Action Combat Phase
        GameOver,       // Defeat screen
        Victory         // Run Complete screen
    }
}
```

### 5.2 Centralized Scene Transition in `GameManager.cs`
Update `GameManager.cs` to manage scene paths and transition logic:

```csharp
private readonly Dictionary<GameState, string> _scenePaths = new Dictionary<GameState, string>
{
    { GameState.Title, "res://src/ui/title/TitleUI.tscn" },
    { GameState.VesselSelect, "res://src/ui/vessel_select/VesselSelectUI.tscn" },
    { GameState.Backpack, "res://src/ui/backpack/BackpackUI.tscn" },
    { GameState.WorldMap, "res://src/ui/map/MapUI.tscn" },
    { GameState.Combat, "res://src/entities/player/Level.tscn" }
};

public void ChangeState(GameState newState)
{
    CurrentState = newState;
    if (_scenePaths.TryGetValue(newState, out string path))
    {
        GetTree().ChangeSceneToFile(path);
    }
}
```

### 5.3 Complete Scene Flow Cycle
```
[TitleUI] -> (Start Game) -> [VesselSelectUI] -> (Select Character) -> [MapUI] -> (Select Stage Node) -> [BackpackUI] -> (Enter Combat) -> [Level (Combat)] -> (Clear Stage) -> [MapUI]
```

---

## Evidence Chain Summary
- `project.godot`: Line 18–22 shows C# Autoload configuration (`GameManager`, `InventoryManager`, `CombatManager`).
- `BackpackUI.tscn`: Lines 251–263 contains `MainLayout/HelpArea` node.
- `BackpackUI.gd`: No bindings to `HelpArea`.
- `VesselData.cs`: Contains base vessel stats and grid layout data.
- `Enums.cs`: Lines 26–32 contains initial 4 `GameState` values.
