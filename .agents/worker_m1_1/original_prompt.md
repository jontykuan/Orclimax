## 2026-07-24T09:34:35Z

You are worker_m1_1.
Your working directory is: d:\cykuan\Godot_v4.6.2-stable_mono_win64\Orclimax\.agents\worker_m1_1

MANDATORY INTEGRITY WARNING:
DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A Forensic Auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.

Objective: Implement Milestone 1: Config & Navigation (`config_and_nav`)

Read the design and handoff reports from the 3 Explorers:
- `d:\cykuan\Godot_v4.6.2-stable_mono_win64\Orclimax\.agents\explorer_m1_1\analysis.md`
- `d:\cykuan\Godot_v4.6.2-stable_mono_win64\Orclimax\.agents\explorer_m1_2\analysis.md`
- `d:\cykuan\Godot_v4.6.2-stable_mono_win64\Orclimax\.agents\explorer_m1_3\analysis.md`

Tasks:
1. Centralized Game Constants (`GameConfig.cs`):
   - Create `src/autoload/GameConfig.cs` extending `Godot.Node`. Register as Autoload `GameConfig="*res://src/autoload/GameConfig.cs"` in `project.godot`.
   - Include physics, speeds, gravity, double-tap window thresholds, i-frame durations, parry windows, knockback forces, enemy stats, UI multipliers. Make parameters accessible by both C# and GDScript nodes.
   - Refactor `Player.gd`, `EnemyBase.gd`, `CombatManager.cs`, `HUD.gd` where relevant to read from `GameConfig`.

2. Title Screen (`TitleScreen.tscn` / `TitleScreen.gd`):
   - Create `src/ui/title/TitleScreen.tscn` and `TitleScreen.gd`.
   - Buttons: Start (開始), Save/Load (存檔), Gallery (畫廊), Settings (設定), Quit (結束). Styled per dark RPG theme (no emojis).
   - Set `run/main_scene="res://src/ui/title/TitleScreen.tscn"` in `project.godot`.

3. Vessel Management Page (`VesselUI.tscn` / `VesselUI.gd`):
   - Update `VesselData.cs` to add sensitivity and trait fields (`HeadSensitivity`, `ChestSensitivity`, `GroinSensitivity`, `LimbsSensitivity`, `HeadDevLevel`, `ChestDevLevel`, `GroinDevLevel`, `LimbsDevLevel`, `TraitsDescription`).
   - Create `src/ui/vessel/VesselUI.tscn` and `VesselUI.gd` displaying body part sensitivity levels (Head, Chest, Groin, Limbs), skills, pleasure multipliers, traits, and vessel selection.

4. World Map Page (`MapUI.tscn` / `MapUI.gd`):
   - Create `src/ui/map/MapUI.tscn` and `MapUI.gd`.
   - Display cleared nodes, current stage, branching combat paths.

5. Backpack UI Cleanup (`BackpackUI.tscn`):
   - Remove bottom hint text container (`HelpArea`).

6. Navigation State Flow in `GameManager.cs`:
   - Expand `GameState` enum in `src/core/Enums.cs`: `Title`, `VesselSelect`, `Backpack`, `WorldMap`, `Combat`, `GameOver`, `Victory`.
   - Implement `ChangeState(GameState newState)` in `GameManager.cs` mapping states to scene files. Update `HUD.gd` state checks accordingly.

7. Build & Test Verification:
   - Run `dotnet build` using run_command tool.
   - Run test verification.
   - Write handoff report with exact command outputs to `d:\cykuan\Godot_v4.6.2-stable_mono_win64\Orclimax\.agents\worker_m1_1\handoff.md`.
   - Send handoff message when done.
