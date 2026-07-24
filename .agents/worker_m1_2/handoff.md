# Handoff Report — M1 Config & Navigation Remediation

## 1. Observation
- `GameConfig` was missing from `[autoload]` in `project.godot`. `project.godot` lines 18-23 only registered `GameManager`, `InventoryManager`, and `CombatManager`.
- Duplicate static class file `src/core/GameConfig.cs` caused namespace collision and null reference exceptions in GDScript consumers.
- GDScript consumers (`Player.gd`, `EnemyBase.gd`, `EnemyShield.gd`, `HUD.gd`) accessed `GameConfig` properties which returned `null` prior to autoload registration, falling back to hardcoded defaults.
- `ItemUI.gd` contained forbidden Unicode emoji `"🔒 "` at line 124, violating AGENTS.md Rule 15.
- `HUD.gd` line 83 checked state `5` (`GameOver`) but did not handle state `6` (`Victory`).
- `VesselUI.gd` line 86 called `child.queue_free()` directly without an `is_instance_valid` guard.

## 2. Logic Chain
1. Registered `GameConfig="*res://src/autoload/GameConfig.cs"` under `[autoload]` in `project.godot` so Godot instantiates `GameConfig` globally upon launch.
2. Merged all constants and properties into `src/autoload/GameConfig.cs` using static backing fields and exported instance properties, and deleted `src/core/GameConfig.cs` to eliminate duplicate definitions while maintaining zero-overhead static access in unit tests without invoking native Godot node constructors outside Godot engine runtime.
3. Added property aliases (e.g. `BaseGravity` -> `Gravity`, `BaseJumpVelocity` -> `JumpVelocity`, `ParryWindowDuration` -> `ParryWindow`, `ThrustPleasureBonus` -> `ThrustPleasureGain`, etc.) in `GameConfig.cs` so both GDScript and C# consumers access unified state.
4. Stripped Unicode emoji from `ItemUI.gd` line 124, changing `"🔒 "` to `"[LOCKED] "`.
5. Updated `HUD.gd` state change handler to handle state `6` (`Victory`) by triggering `show_victory()`.
6. Added `if is_instance_valid(child):` guard around `child.queue_free()` in `VesselUI.gd` to prevent double-free crashes.
7. Updated `CombatManager.cs` static member reference for `BaseMoveSpeed`.
8. Ran build and full test suite (`dotnet test`), achieving 51/51 passing tests and 0 build errors/warnings.

## 3. Caveats
- No caveats.

## 4. Conclusion
- All M1 remediation tasks specified by Reviewers 1 & 2 have been fully resolved.
- `dotnet build` passes with 0 warnings and 0 errors.
- `dotnet test` executes 51/51 unit tests successfully.

## 5. Verification Method
- **Build Command**: `dotnet build d:\cykuan\Godot_v4.6.2-stable_mono_win64\Orclimax\Orclimax.csproj`
- **Test Command**: `dotnet test d:\cykuan\Godot_v4.6.2-stable_mono_win64\Orclimax\tests\Orclimax.Tests\Orclimax.Tests.csproj`
- **Inspected Files**:
  - `project.godot` (`[autoload]` section)
  - `src/autoload/GameConfig.cs`
  - `src/autoload/CombatManager.cs`
  - `src/ui/hud/HUD.gd`
  - `src/ui/backpack/ItemUI.gd`
  - `src/ui/vessel/VesselUI.gd`
