# Review Report — Milestone 1 Remediation (Config & Navigation)

## Review Summary

**Verdict**: REQUEST_CHANGES

The remediation work for Milestone 1 successfully registered `GameConfig` in `project.godot`, removed duplicate `src/core/GameConfig.cs`, and established scene navigation across `TitleScreen`, `VesselUI`, `MapUI`, `BackpackUI`, and `GameManager.cs`. However, **the project fails to compile (`dotnet build`)** due to a C# compilation error in `src/autoload/CombatManager.cs`.

---

## Findings

### [Critical] Compilation Error CS0176 in `CombatManager.cs`

- **What**: C# compiler error `CS0176: Member 'GameConfig.BaseMoveSpeed' cannot be accessed with an instance reference; use a type name instead`.
- **Where**: `d:\cykuan\Godot_v4.6.2-stable_mono_win64\Orclimax\src\autoload\CombatManager.cs`, lines 56 and 110.
- **Why**: In `src/autoload/GameConfig.cs` (line 65), `BaseMoveSpeed` is defined as a `static` property:
  `public static float BaseMoveSpeed { get => _baseMoveSpeed; set => _baseMoveSpeed = value; }`
  However, `CombatManager.cs` accesses it via the instance reference `GameConfig.Instance.BaseMoveSpeed`:
  `float baseMoveSpeed = GameConfig.Instance != null ? GameConfig.Instance.BaseMoveSpeed : 250f;` (lines 56 and 110).
  In C#, static members must be accessed via class name (`GameConfig.BaseMoveSpeed`) or `GameConfig.cs` needs an instance property wrapper for `BaseMoveSpeed`.
- **Suggestion**: Either change `CombatManager.cs` lines 56 and 110 to `GameConfig.BaseMoveSpeed` (or check `GameConfig.Instance != null`), or expose an instance property `[Export] public float BaseMoveSpeed { get => _baseMoveSpeed; set => _baseMoveSpeed = value; }` in `src/autoload/GameConfig.cs` matching `DefaultEnemyMaxHp` and `DefaultEnemySpeed`.

---

## Verified Claims

1. **Autoload Registration (`project.godot`)**:
   - `GameConfig="*res://src/autoload/GameConfig.cs"` is correctly registered under `[autoload]` at line 20. -> **PASS**

2. **Duplicate File Removal**:
   - `src/core/GameConfig.cs` is confirmed removed. Only `src/autoload/GameConfig.cs` exists. -> **PASS**

3. **Property Accessibility in `GameConfig.cs`**:
   - Properties (`BaseGravity`, `BaseJumpVelocity`, `ParryWindowDuration`, `ThrustPleasureBonus`, etc.) are declared in `src/autoload/GameConfig.cs`. -> **PASS**

4. **Scene Navigation Review**:
   - `TitleScreen` (`TitleScreen.tscn` / `TitleScreen.gd`), `VesselUI` (`VesselUI.tscn` / `VesselUI.gd`), `MapUI` (`MapUI.tscn` / `MapUI.gd`), `BackpackUI` (`BackpackUI.tscn` / `BackpackUI.gd`), and `GameManager.cs` properly implement multi-page scene flow and state management. -> **PASS**

5. **`dotnet build` Verification**:
   - Executed `dotnet build d:\cykuan\Godot_v4.6.2-stable_mono_win64\Orclimax\Orclimax.csproj`.
   - Result: **FAIL** (2 C# compiler errors: CS0176 in `CombatManager.cs`). -> **FAIL**

6. **`dotnet test` Verification**:
   - Executed `dotnet test tests\Orclimax.Tests\Orclimax.Tests.csproj`.
   - Result: **FAIL** (Blocked by `Orclimax.csproj` compilation failure). -> **FAIL**

---

## Coverage Gaps

- No coverage gaps in inspection scope. All specified files were verified.

---

## Stress Test / Adversarial Findings

- **Build Integrity Failure**: The solution cannot be built or tested in its current state. The project build pipeline breaks immediately on `dotnet build`.
