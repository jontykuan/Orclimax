# Forensic Audit Report — Milestone 1 (Config & Navigation)

**Work Product**: Milestone 1 Implementation (`src/autoload/GameConfig.cs`, `src/autoload/GameManager.cs`, `src/core/Enums.cs`, `src/core/VesselData.cs`, `src/ui/title/TitleScreen.gd`, `src/ui/vessel/VesselUI.gd`, `src/ui/map/MapUI.gd`, `src/ui/backpack/BackpackUI.gd`, `tests/Orclimax.Tests/`)  
**Profile**: General Project / Integrity Forensics  
**Date**: 2026-07-24  
**Verdict**: **INTEGRITY VIOLATION**

---

## 1. Executive Summary

A forensic integrity audit was conducted on Milestone 1 (Config & Navigation). The audit revealed critical build failures and facade test practices that prevent the production project from building and executing properly.

While GDScript UI scripts and C# data enums/resources are present, the project fails to compile during `dotnet build Orclimax.csproj` due to syntax errors in `src/autoload/CombatManager.cs`. Furthermore, the unit test assembly (`tests/Orclimax.Tests/`) uses a facade proxy class (`Orclimax.Tests.GameConfig`) that shadows the actual production `GameConfig` singleton, masking integration errors.

Per the Forensic Verification Procedure (General), a build failure or non-executing test suite constitutes an immediate **INTEGRITY VIOLATION**.

---

## 2. Forensic Findings & Violations

### Finding 1: Production Project Build Failure (CS0176)
- **File**: `src/autoload/CombatManager.cs` (Lines 56, 110)
- **Violation**: The project fails to build from source.
- **Evidence**:
  ```text
  D:\cykuan\Godot_v4.6.2-stable_mono_win64\Orclimax\src\autoload\CombatManager.cs(56,65): error CS0176: 成員 'GameConfig.BaseMoveSpeed' 無法以執行個體參考進行存取; 請改用類型名稱 [D:\cykuan\Godot_v4.6.2-stable_mono_win64\Orclimax\Orclimax.csproj]
  D:\cykuan\Godot_v4.6.2-stable_mono_win64\Orclimax\src\autoload\CombatManager.cs(110,65): error CS0176: 成員 'GameConfig.BaseMoveSpeed' 無法以執行個體參考進行存取; 請改用類型名稱 [D:\cykuan\Godot_v4.6.2-stable_mono_win64\Orclimax\Orclimax.csproj]
  ```
- **Analysis**: In `src/autoload/GameConfig.cs`, `BaseMoveSpeed` is an instance property (`[Export] public float BaseMoveSpeed { get; set; } = 250.0f;`). In `CombatManager.cs`, lines 56 and 110 attempt to access `BaseMoveSpeed` statically via `GameConfig.BaseMoveSpeed` instead of accessing instance property `GameConfig.Instance.BaseMoveSpeed`. This breaks C# compilation for `Orclimax.csproj`.

### Finding 2: Facade Mock Proxy Shadowing Production Class
- **File**: `tests/Orclimax.Tests/GameConfig.cs`
- **Violation**: Prohibited Pattern #2 (Facade Implementations) & Facade Detection.
- **Evidence**:
  `tests/Orclimax.Tests/GameConfig.cs` defines a static `Orclimax.Tests.GameConfig` and `GameConfigProxy` class structure. All test files (`Tier1_FeatureTests.cs`, `Tier2_BoundaryTests.cs`, `Tier3_PairwiseTests.cs`, `Tier4_RealWorldWorkloadTests.cs`) alias `GameConfig` to `Orclimax.Tests.GameConfig` via `using GameConfig = Orclimax.Tests.GameConfig;`.
- **Analysis**: The test suite tests the mock proxy class rather than testing `src/autoload/GameConfig.cs`. This facade structure obscured the syntax error in `CombatManager.cs` because the test suite did not compile against or exercise the production `GameConfig` instance contract.

---

## 3. Phase Results

| # | Check Name | Verdict | Details |
|---|------------|---------|---------|
| 1 | Hardcoded Test Results | **PASS** | No hardcoded string comparisons or fake test outputs. |
| 2 | Facade / Proxy Detection | **FAIL** | `tests/Orclimax.Tests/GameConfig.cs` acts as a facade proxy shadowing production `GameConfig`. |
| 3 | Pre-populated Verification Artifacts | **PASS** | No pre-populated result files found. |
| 4 | Self-Certifying Tests Check | **PASS** | Test formulas test math and boundaries. |
| 5 | Execution Delegation Check | **PASS** | Deliverables are implemented directly. |
| 6 | Build & Behavioral Verification | **FAIL** | `dotnet build Orclimax.csproj` fails with 2 CS0176 compilation errors. |

---

## 4. Final Verdict

**Verdict**: **INTEGRITY VIOLATION**

**Required Remediation**:
1. Fix `CombatManager.cs` lines 56 and 110 to use `GameConfig.Instance != null ? GameConfig.Instance.BaseMoveSpeed : 250.0f` (or equivalent instance accessor).
2. Ensure `Orclimax.csproj` and `Orclimax.Tests.csproj` compile without errors or proxy shadowing.
