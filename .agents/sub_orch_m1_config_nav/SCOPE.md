# Scope: Milestone 1 - Config & Navigation (`config_and_nav`)

## Objective
Implement centralized configuration parameters, title screen, vessel management UI, world map UI, clean up backpack UI hint box, and establish GameManager scene navigation flow.

## Components & Requirements

1. **Centralized Game Constants (`GameConfig.cs`)**:
   - File: `src/autoload/GameConfig.cs` (or `src/core/GameConfig.cs`)
   - Physics, speeds, gravity, double-tap window thresholds (e.g. 0.25s), i-frame durations (e.g. 0.3s), parry window (e.g. 0.2s), knockback forces, enemy stats, UI multipliers.
   - Must be accessible by both C# scripts and GDScript nodes.

2. **Title Screen (`TitleScreen.tscn` / `.gd` or `.cs`)**:
   - Location: `src/ui/title/TitleScreen.tscn`
   - Functional buttons: Start (開始), Save/Load (存檔), Gallery (畫廊), Settings (設定), Quit (結束).
   - Aesthetic: Consistent RPG UI theme matching design rules.

3. **Vessel Management Page (`VesselUI.tscn` / `.gd` or `.cs`)**:
   - Location: `src/ui/vessel/VesselUI.tscn`
   - Displays body part sensitivity/development levels (Head, Chest, Groin, Limbs), skills, pleasure multipliers, traits.

4. **World Map Page (`MapUI.tscn` / `.gd` or `.cs`)**:
   - Location: `src/ui/map/MapUI.tscn`
   - Displays cleared nodes, current stage, branching uncleared combat paths.

5. **Backpack UI Cleanup (`BackpackUI.tscn` / `.gd`)**:
   - Remove the old bottom hint text box.

6. **Navigation State Flow in `GameManager.cs` / scene manager**:
   - Seamless transitions: Title -> Vessel Select -> Backpack -> World Map -> Combat.

## Deliverables
- Source code in `src/`
- Build verification (`dotnet build`)
- Test verification (`dotnet test` / godot verification)
