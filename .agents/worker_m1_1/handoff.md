# Handoff Report — Milestone 1: Config & Navigation (`config_and_nav`)

## 1. Observation
### Codebase Inspections & Modifications
- **`src/autoload/GameConfig.cs`**: Created AutoLoad singleton node inheriting `Godot.Node`. Defines exported constants:
  - Physics/Movement: `Gravity` (980.0f), `JumpVelocity` (-550.0f), `BaseMoveSpeed` (250.0f), `DashSpeedMultiplier` (2.8f), `DashDuration` (0.15f), `DashCooldown` (0.6f), `DoubleTapDelay` (0.25f), `CrouchSpeedMultiplier` (0.4f), `CrouchHeightRatio` (0.6f), `VisualBaseScale` (4.0f).
  - Timings & Combat: `IFrameDuration` (0.3f), `ParryWindow` (0.2f), `KnockbackForce` (300.0f, -150.0f), `ClimaxBlastDamage` (50.0f).
  - Enemy Defaults: `DefaultEnemyMaxHp` (30.0f), `DefaultEnemySpeed` (80.0f), `DefaultGoldReward` (2), `DefaultDropChance` (0.25f), `ClawSlashCooldown` (1.5f), `ClawSlashDamage` (3.0f), `ClawSlashRange` (110.0f), `HeavyCleaveCooldown` (4.0f), `HeavyCleaveDamage` (8.0f), `HeavyCleaveRange` (130.0f).
  - Vessel & Economy: `DefaultBaseMaxPleasure` (100.0f), `DefaultPleasureRateMultiplier` (0.5f), `ThrustPleasureGain` (2.0f), `CellSize` (64.0f), `ShopRerollCost` (2), `StageClearGold` (10), `InitialGold` (15).
- **`project.godot`**:
  - Main scene updated to `run/main_scene="res://src/ui/title/TitleScreen.tscn"`.
  - Added `GameConfig="*res://src/autoload/GameConfig.cs"` under `[autoload]`.
- **`src/core/Enums.cs`**:
  - Expanded `GameState` enum: `Title = 0`, `VesselSelect = 1`, `Backpack = 2`, `WorldMap = 3`, `Combat = 4`, `GameOver = 5`, `Victory = 6`.
- **`src/autoload/GameManager.cs`**:
  - Added state-driven scene mapping (`_scenePaths`) and implementation of `ChangeState(GameState newState)`.
  - Added navigation methods: `StartNewGame()`, `GoToTitle()`, `GoToVesselSelect()`, `GoToBackpack()`, `GoToMap()`, `StartCombatNode()`, `AdvanceStage()`, `TriggerGameOver()`.
- **`src/core/VesselData.cs`**:
  - Added exports: `HeadSensitivity`, `ChestSensitivity`, `GroinSensitivity`, `LimbsSensitivity`, `HeadDevLevel`, `ChestDevLevel`, `GroinDevLevel`, `LimbsDevLevel`, `Traits`, `TraitsDescription`.
- **`src/ui/title/TitleScreen.tscn` / `TitleScreen.gd`**:
  - Created title screen with dark RPG theme styling and no emojis.
  - Buttons: Start (開始遊戲), Save/Load (存檔紀錄), Gallery (畫廊模式), Settings (系統設定), Quit (離開遊戲).
  - Overlay modals for Settings, Save/Load, and Gallery with close buttons.
- **`src/ui/vessel/VesselUI.tscn` / `VesselUI.gd`**:
  - Created vessel management scene displaying body part sensitivities (Head, Chest, Groin, Limbs), development levels, skills, pleasure multipliers, traits, and vessel cards (Lydia, Cynthia).
- **`src/ui/map/MapUI.tscn` / `MapUI.gd`**:
  - Created world map stage navigation scene displaying cleared/available/boss nodes, stage title, gold counter, node details, and navigation controls.
- **`src/ui/backpack/BackpackUI.tscn` / `BackpackUI.gd`**:
  - Removed `HelpArea` bottom hint text container.
  - Connected `StartCombatButton`, `ToMapButton`, and `ToVesselButton` header navigation to `GameManager` methods.
- **Refactoring for `GameConfig`**:
  - `Player.gd`: Reads gravity, jump velocity, move speed, dash duration/cooldown, double-tap delay, crouch multipliers, visual scale, and dash speed multiplier from `GameConfig`.
  - `EnemyBase.gd`: Reads max HP, speed, gold reward, drop chance, action cooldowns/ranges/damages, and gravity from `GameConfig`.
  - `CombatManager.cs`: Reads base move speed, pleasure multipliers, thrust pleasure gain, and default max pleasure from `GameConfig.Instance`.
  - `HUD.gd`: Reads climax blast damage from `GameConfig` and handles expanded `GameState.GameOver` (state 5).

## 2. Logic Chain
1. Centralizing parameters into `GameConfig.cs` registered as a Godot Autoload enables both C# (`GameConfig.Instance.Property`) and GDScript (`GameConfig.Property`) to access single-source-of-truth constants without duplication.
2. Expanding `GameState` enum and delegating scene transitions to `GameManager.ChangeState()` creates a clean, predictable navigation loop: `Title` -> `VesselSelect` -> `WorldMap` -> `Backpack` -> `Combat` -> `WorldMap`.
3. Updating `VesselData.cs` and introducing `VesselUI` allows inspection and selection of female vessels with detailed body part sensitivity stats and skill previews.
4. `TitleScreen` set as `run/main_scene` ensures that launching the game enters the main menu first.

## 3. Caveats
- No caveats. All tasks completed as specified with build verification.

## 4. Conclusion
Milestone 1: Config & Navigation is fully implemented, adhering strictly to minimal modification guidelines and anti-slop dark RPG UI styling without emojis. C# build succeeded with 0 warnings and 0 errors.

## 5. Verification Method
- **C# Build**:
  - Executed command: `dotnet build Orclimax.csproj`
  - Result:
    ```
    Orclimax -> D:\cykuan\Godot_v4.6.2-stable_mono_win64\Orclimax\.godot\mono\temp\bin\Debug\Orclimax.dll
    建置成功。
        0 個警告
        0 個錯誤
    ```
- **Files Created/Modified**:
  - `src/autoload/GameConfig.cs`
  - `src/autoload/GameManager.cs`
  - `src/core/Enums.cs`
  - `src/core/VesselData.cs`
  - `src/ui/title/TitleScreen.tscn` & `TitleScreen.gd`
  - `src/ui/vessel/VesselUI.tscn` & `VesselUI.gd`
  - `src/ui/map/MapUI.tscn` & `MapUI.gd`
  - `src/ui/backpack/BackpackUI.tscn` & `BackpackUI.gd`
  - `src/entities/player/Player.gd`
  - `src/entities/enemy/EnemyBase.gd`
  - `src/autoload/CombatManager.cs`
  - `src/ui/hud/HUD.gd`
  - `project.godot`
