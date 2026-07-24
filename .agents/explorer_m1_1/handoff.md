# Handoff Report: Milestone 1 (Config & Navigation)

## 1. Observation
- **Project Structure**: Multi-language Godot 4.6 (Mono) codebase located at `d:\cykuan\Godot_v4.6.2-stable_mono_win64\Orclimax`.
- **Existing Main Scene**: `project.godot:14`: `run/main_scene="res://src/ui/backpack/BackpackUI.tscn"`.
- **Existing AutoLoads**: `project.godot:18-22`:
  - `GameManager="*res://src/autoload/GameManager.cs"`
  - `InventoryManager="*res://src/autoload/InventoryManager.cs"`
  - `CombatManager="*res://src/autoload/CombatManager.cs"`
- **Game State Enum**: `src/core/Enums.cs:26-32` currently defines:
  ```csharp
  public enum GameState
  {
      Shop,        // Shop and Backpack preparation phase
      Combat,      // Side-scrolling combat action phase
      GameOver,    // Orc died
      Victory      // Run won
  }
  ```
- **Hardcoded Parameters**:
  - `src/entities/player/Player.gd:7-8`: `@export var gravity: float = 980.0`, `@export var jump_velocity: float = -550.0`.
  - `src/entities/player/Player.gd:21-28`: `dash_duration = 0.15`, `dash_cooldown = 0.6`, `DOUBLE_TAP_DELAY = 0.25`.
  - `src/entities/enemy/EnemyBase.gd:22-25`: `@export var max_hp: float = 30.0`, `@export var speed: float = 80.0`, `@export var gold_reward: int = 2`, `@export var drop_chance: float = 0.25`.
- **BackpackUI Bottom Hint Text Box**: `src/ui/backpack/BackpackUI.tscn:251-263` contains the `HelpArea` node containing `HelpLabel`.
- **Dotnet Build Status**: Ran `dotnet build Orclimax.csproj` via `run_command`. Output: `建置成功。0 個警告, 0 個錯誤`.

## 2. Logic Chain
1. **Observation 1**: Constants such as gravity (980.0), jump velocity (-550.0), double tap delay (0.25s), enemy stats, and UI sizes are scattered across GDScript files (`Player.gd`, `EnemyBase.gd`) and C# files (`CombatManager.cs`).
   **Inference 1**: Creating a unified `GameConfig.cs` registered as an AutoLoad node `GameConfig` in `project.godot` will provide a single source of truth for both C# and GDScript.

2. **Observation 2**: Currently `GameState` in `Enums.cs` only has 4 states (`Shop`, `Combat`, `GameOver`, `Victory`), and `project.godot` launches directly into `BackpackUI.tscn`.
   **Inference 2**: Expanding `GameState` to include `Title`, `VesselSelect`, `Backpack`, `WorldMap`, `Combat`, `GameOver`, `Victory` allows `GameManager.cs` to orchestrate a seamless navigation flow (`Title` -> `Vessel Select` -> `Backpack` -> `World Map` -> `Combat`).

3. **Observation 3**: `TitleScreen`, `VesselUI`, and `MapUI` scenes do not currently exist in `src/ui/`.
   **Inference 3**: Designing specifications for `TitleScreen.tscn/.gd` (5 styled buttons + modals), `VesselUI.tscn/.gd` (inspection of sensitivities, skills, traits, grid preview), and `MapUI.tscn/.gd` (branching stage map nodes) will satisfy all UI requirements for Milestone 1.

4. **Observation 4**: `BackpackUI.tscn:251-263` contains `HelpArea` which holds the bottom hint box.
   **Inference 4**: Deleting the `HelpArea` node removes the hint box cleanly without affecting other grid UI containers.

## 3. Caveats
- No existing save file format is implemented yet; `SaveLoadModal` on `TitleScreen` is designed as a functional UI overlay with placeholder slot selection ready for a persistence module.
- `MapUI` map node layout is generated programmatically in GDScript for flexibility across stages.

## 4. Conclusion
The codebase analysis is complete and a detailed, machine-applicable implementation design has been written to `d:\cykuan\Godot_v4.6.2-stable_mono_win64\Orclimax\.agents\explorer_m1_1\analysis.md`. The design fulfills all 6 objectives for Milestone 1:
1. Centralized `GameConfig.cs` engine.
2. Complete `TitleScreen.tscn` / `.gd` design.
3. Detailed `VesselUI.tscn` / `.gd` design.
4. Interactive `MapUI.tscn` / `.gd` design.
5. Cleanup of `BackpackUI.tscn` (removal of `HelpArea` node).
6. Enhanced state machine navigation in `GameManager.cs`.

## 5. Verification Method
1. Build verification:
   `dotnet build Orclimax.csproj`
2. Inspect `analysis.md` at `d:\cykuan\Godot_v4.6.2-stable_mono_win64\Orclimax\.agents\explorer_m1_1\analysis.md`.
3. Verify project layout compliance: all source code resides under `src/`, all agent metadata resides under `.agents/explorer_m1_1/`.
