# Milestone 1 (Config & Navigation) Code Review & Interop Verification Report

## Review Summary

**Verdict**: REQUEST_CHANGES

This review assessed C#/GDScript interop bindings, state index handling in `HUD.gd`, double-free safety, UI design adherence (AGENTS.md Rule 15), and C# compilation build integrity for Milestone 1.

---

## 1. Verified Claims

- **C# Project Compilation**: Executed `dotnet build d:\cykuan\Godot_v4.6.2-stable_mono_win64\Orclimax\Orclimax.csproj` via `run_command`.
  - **Status**: PASSED (0 Errors, 0 Warnings). Assembly output: `.godot/mono/temp/bin/Debug/Orclimax.dll`.
- **Double-Free Guard in `BackpackUI.gd`**: Verified that all `queue_free()` calls in `BackpackUI.gd` (lines 280, 328, 363, 407, 598, 609, 637) are safely wrapped with `is_instance_valid()`.
- **Resolution & Navigation Bindings**: F1-F3 shortcut key handling in `GameManager.cs` for 1600x1200 / fullscreen toggle compiles clean and matches Godot DisplayServer specifications.

---

## 2. Findings & Issues

### [Critical] Finding 1: C#/GDScript `GameConfig` Interop System Bypassed at Runtime

- **Where**: `project.godot`, `src/autoload/GameConfig.cs`, `src/core/GameConfig.cs`, `src/entities/player/Player.gd`, `src/entities/enemy/EnemyBase.gd`, `src/entities/enemy/EnemyShield.gd`, `src/ui/hud/HUD.gd`
- **Why**:
  1. **Missing Autoload Registration**: `GameConfig` is **NOT registered** in `project.godot` under `[autoload]`. Only `GameManager`, `InventoryManager`, and `CombatManager` are registered. As a result, in GDScript, `GameConfig` resolves to `null`.
  2. **Silently Failing Guards**: GDScript files use `if GameConfig:` checks (e.g., `Player.gd:54`, `EnemyBase.gd:35`). Because `GameConfig` is null, these checks evaluate to `false`, causing all entities to silently fall back to inline hardcoded values instead of reading central configuration.
  3. **Property Mismatches & Missing Bindings**: Even if registered as an Autoload, GDScript property names do not match `src/autoload/GameConfig.cs`:
     - `Player.gd:55`: accesses `GameConfig.BaseGravity` (property in `src/autoload/GameConfig.cs` is named `Gravity`).
     - `Player.gd:56`: accesses `GameConfig.BaseJumpVelocity` (property in `src/autoload/GameConfig.cs` is named `JumpVelocity`).
     - `Player.gd:180`: accesses `GameConfig.ParryWindowDuration` (property is `ParryWindow`).
     - `Player.gd:189`: accesses `GameConfig.ThrustCooldown` (missing from `src/autoload/GameConfig.cs`).
     - `Player.gd:192`: accesses `GameConfig.ThrustPleasureBonus` (property in `src/autoload/GameConfig.cs` is `ThrustPleasureGain`).
     - `Player.gd:196`: accesses `GameConfig.ThrustKnockbackRadius` (missing from `src/autoload/GameConfig.cs`).
     - `Player.gd:197`: accesses `GameConfig.ThrustKnockbackForce` (float) vs `KnockbackForce` (Vector2) in `src/autoload/GameConfig.cs`.
     - `EnemyShield.gd:16`: accesses `GameConfig.ShieldEnemyMagicDamageMultiplier` (missing from `src/autoload/GameConfig.cs`).
     - `EnemyShield.gd:20`: accesses `GameConfig.ShieldEnemyPhysArmorRatio` (missing from `src/autoload/GameConfig.cs`).
- **Suggestion**:
  1. Register `GameConfig="*res://src/autoload/GameConfig.cs"` in `project.godot` under `[autoload]`.
  2. Unify `src/core/GameConfig.cs` and `src/autoload/GameConfig.cs` into a single canonical C# Autoload class, exposing all required parameters.
  3. Align all GDScript property access names with the C# exported property names.

---

### [Major] Finding 2: Unicode Emoji Violation in `ItemUI.gd` (AGENTS.md Rule 15)

- **Where**: `src/ui/backpack/ItemUI.gd`, line 124
- **Why**: Line 124 contains `label.text = "🔒 " + item_ref.ItemName`. AGENTS.md Rule 15 explicitly mandates removing unicode emojis (including `🔒`) and replacing them with clean text tags like `[LOCKED]` or `[L]`.
- **Suggestion**: Replace `"🔒 "` with `"[LOCKED] "` or `"[L] "` in `ItemUI.gd:124`.

---

### [Minor] Finding 3: `HUD.gd` State Index Check & Enum Coverage

- **Where**: `src/ui/hud/HUD.gd`, line 84 (`_on_game_state_changed`)
- **Why**: `HUD.gd` checks `if new_state == 5:` using a hardcoded integer check rather than enum constants. Additionally, `_on_game_state_changed` does not handle `GameState.Victory` (`new_state == 6`).
- **Suggestion**: Use explicit match statements or enum constants for `GameState` values in `HUD.gd`, and handle state 6 (`Victory`) to show victory overlay when state changes to Victory.

---

### [Minor] Finding 4: Unwrapped `queue_free()` in `VesselUI.gd`

- **Where**: `src/ui/vessel/VesselUI.gd`, line 86
- **Why**: Line 86 calls `child.queue_free()` inside `_populate_vessel_list()` without checking `is_instance_valid(child)`. To prevent potential double-free crashes during list repopulation, all UI node destruction should follow the `is_instance_valid()` pattern established in `BackpackUI.gd`.
- **Suggestion**: Wrap `child.queue_free()` with `if is_instance_valid(child):`.

---

## 3. Challenge & Adversarial Stress Test Results

| Attack Scenario | Expected Result | Actual Result | Verdict |
|---|---|---|---|
| Modifying `Gravity` or `DashSpeedMultiplier` in `GameConfig.cs` | Player movement & enemy physics update accordingly | Physics/speeds remain unchanged because `if GameConfig:` evaluates to `false` in GDScript | **FAIL** |
| Emitting `StateChanged(6)` (Victory state) to `HUD.gd` | Victory panel is displayed | State change ignored in `_on_game_state_changed()` | **FAIL** |
| Repopulating `VesselUI` list while child nodes are queued for deletion | Gracefully ignores deleted instances | Potential double-free crash risk due to missing `is_instance_valid()` check | **WARN** |
| Rendering locked items in backpack UI | Displays clean text indicator without emojis | Displays unicode lock emoji `🔒` | **FAIL** |

---

## 4. Coverage & Unverified Items

- **Unverified**: In-editor Inspector editing of `@export` fields on `GameConfig.cs` (requires running Godot editor GUI).
- **Verified**: C# compilation, file layout compliance, cross-file property name indexing, `queue_free` safety, and emoji compliance.
