# Forensic Audit Analysis & Remediation Strategy — Milestone 1

## Overview
This report presents a thorough forensic investigation of the INTEGRITY VIOLATION reported by the Forensic Auditor (`auditor_m1_1`) for Milestone 1. It details the root causes of both audit findings and provides a precise, concrete remediation strategy.

---

## 1. Analysis of Forensic Audit Findings

### Finding 1: Production Project Build Failure (CS0176)
- **Location**: `src/autoload/CombatManager.cs` (Lines 56, 110)
- **Root Cause**:
  In `src/autoload/GameConfig.cs`, `BaseMoveSpeed` was declared as a static property (`public static float BaseMoveSpeed`).
  In `src/autoload/CombatManager.cs` (lines 56 and 110), the code attempted to access `BaseMoveSpeed` via an instance reference expression:
  ```csharp
  float baseMoveSpeed = GameConfig.Instance != null ? GameConfig.Instance.BaseMoveSpeed : 250.0f;
  ```
  In C#, accessing a static member via an instance expression produces compiler error **CS0176**: `"成員 'GameConfig.BaseMoveSpeed' 無法以執行個體參考進行存取; 請改用類型名稱"` (Member 'GameConfig.BaseMoveSpeed' cannot be accessed with an instance reference; qualify it with a type name instead).
- **Secondary Discovery**:
  `GameConfig.cs` suffered from property access asymmetry:
  - Some properties (`BaseMoveSpeed`, `BaseGravity`, `BaseJumpVelocity`, `ParryWindowDuration`, `ThrustPleasureBonus`) existed only as static properties.
  - Other properties (`DefaultBaseMaxPleasure`, `DefaultPleasureRateMultiplier`, `InitialGold`, `StageClearGold`, `ThrustPleasureGain`) existed only as instance/export properties.
  - Because `GameConfig.Instance` can be `null` in headless unit test runners (outside Godot's SceneTree execution context), accessing static properties via `GameConfig.BaseMoveSpeed` directly is both syntactically correct and runtime safe.

---

### Finding 2: Facade Mock Proxy Shadowing Production Class
- **Location**: `tests/Orclimax.Tests/GameConfig.cs` & test runner files (`Tier1_FeatureTests.cs`, `Tier2_BoundaryTests.cs`, `Tier3_PairwiseTests.cs`, `Tier4_RealWorldWorkloadTests.cs`)
- **Root Cause**:
  The test project `tests/Orclimax.Tests/` contained a file `GameConfig.cs` defining a dummy static `Orclimax.Tests.GameConfig` class and `GameConfigProxy` object model.
  Every test file in `tests/Orclimax.Tests/` included:
  ```csharp
  using GameConfig = Orclimax.Tests.GameConfig;
  ```
  This aliased `GameConfig` to `Orclimax.Tests.GameConfig`, shadowing the actual production class `Orclimax.Autoload.GameConfig`.
- **Impact**:
  1. The test suite tested a mock proxy instead of exercising the production class in `src/autoload/GameConfig.cs`.
  2. Because the mock proxy had a different static interface, the syntax error in `CombatManager.cs` was completely masked from `dotnet test`.
  3. This created a false green test suite while the production build failed.

---

## 2. Concrete Remediation Strategy

### Strategy for Finding 1: Production Code Alignment
1. **Synchronize Static and Instance Accessors in `src/autoload/GameConfig.cs`**:
   - Ensure all configuration properties have clean static accessors backed by static fields so they can be accessed statically (`GameConfig.BaseMoveSpeed`, `GameConfig.DefaultBaseMaxPleasure`, `GameConfig.InitialGold`, `GameConfig.StageClearGold`, etc.) without requiring `GameConfig.Instance`.
   - Maintain `[Export]` instance properties for GDScript and Godot Inspector binding that delegate directly to the underlying static backing fields.
2. **Update `src/autoload/CombatManager.cs`**:
   - Replace invalid instance static calls with direct static accesses:
     ```csharp
     // Before (Line 56 & Line 110):
     float baseMoveSpeed = GameConfig.Instance != null ? GameConfig.Instance.BaseMoveSpeed : 250.0f;

     // After:
     float baseMoveSpeed = GameConfig.BaseMoveSpeed;
     ```
   - Update pleasure rate and max pleasure accessors:
     ```csharp
     // Lines 59, 68, 113, 114, 187:
     float basePleasureRate = GameConfig.DefaultPleasureRateMultiplier;
     MaxPleasure = GameConfig.DefaultBaseMaxPleasure;
     float baseThrust = GameConfig.ThrustPleasureBonus;
     ```
3. **Update `src/autoload/GameManager.cs`**:
   - Update gold accesses to static properties:
     ```csharp
     // Lines 87, 101, 138:
     _gold = GameConfig.InitialGold;
     Gold = GameConfig.InitialGold;
     int bonus = GameConfig.StageClearGold;
     ```

---

### Strategy for Finding 2: Test Suite De-Shadowing & Direct Refactor
1. **Delete Shadow Proxy File**:
   - Remove `tests/Orclimax.Tests/GameConfig.cs`.
2. **Refactor All Test Files**:
   - In `Tier1_FeatureTests.cs`, `Tier2_BoundaryTests.cs`, `Tier3_PairwiseTests.cs`, and `Tier4_RealWorldWorkloadTests.cs`:
     - Remove the explicit alias line: `using GameConfig = Orclimax.Tests.GameConfig;`.
     - Ensure `using Orclimax.Autoload;` is present so `GameConfig` resolves directly to `Orclimax.Autoload.GameConfig`.

---

## 3. Precise Code Blueprints

### 3.1 Proposed `src/autoload/GameConfig.cs`
```csharp
using Godot;
using System;

namespace Orclimax.Autoload
{
    public partial class GameConfig : Node
    {
        public static GameConfig Instance { get; set; }

        // --- Static Backing Fields ---
        private static float _gravity = 980.0f;
        private static float _jumpVelocity = -550.0f;
        private static float _baseMoveSpeed = 250.0f;
        private static float _dashSpeedMultiplier = 2.8f;
        private static float _dashDuration = 0.15f;
        private static float _dashIFrameDuration = 0.18f;
        private static float _dashCooldown = 0.6f;
        private static float _doubleTapDelay = 0.25f;
        private static float _crouchSpeedMultiplier = 0.4f;
        private static float _crouchHeightRatio = 0.6f;
        private static float _visualBaseScale = 4.0f;

        private static float _iFrameDuration = 0.3f;
        private static float _parryWindow = 0.22f;
        private static float _parryCounterDamage = 15.0f;
        private static float _parryReflectSpeed = 700.0f;
        private static float _parryCooldown = 1.0f;

        private static Vector2 _knockbackForce = new Vector2(300.0f, -150.0f);
        private static float _heavyThrustKnockbackForce = 450.0f;
        private static float _thrustKnockbackRadius = 140.0f;
        private static float _thrustPleasureGain = 15.0f;
        private static float _thrustCooldown = 1.2f;
        private static float _climaxBlastDamage = 50.0f;

        private static float _defaultEnemyMaxHp = 30.0f;
        private static float _defaultEnemySpeed = 80.0f;
        private static float _defaultEnemyGravity = 980.0f;
        private static int _defaultGoldReward = 2;
        private static float _defaultDropChance = 0.25f;

        private static float _clawSlashCooldown = 1.5f;
        private static float _clawSlashDamage = 3.0f;
        private static float _clawSlashRange = 110.0f;

        private static float _heavyCleaveCooldown = 4.0f;
        private static float _heavyCleaveDamage = 8.0f;
        private static float _heavyCleaveRange = 130.0f;

        private static float _shieldEnemyPhysArmorRatio = 0.75f;
        private static float _shieldEnemyMagicDamageMultiplier = 2.0f;

        private static float _vesselReclaimRadius = 60.0f;
        private static float _defaultBaseMaxPleasure = 100.0f;
        private static float _defaultPleasureRateMultiplier = 0.5f;

        private static float _cellSize = 64.0f;
        private static int _shopRerollCost = 2;
        private static int _stageClearGold = 10;
        private static int _initialGold = 15;

        // --- Static Properties for C# Direct Access & Unit Tests ---
        public static float BaseGravity { get => _gravity; set => _gravity = value; }
        public static float BaseJumpVelocity { get => _jumpVelocity; set => _jumpVelocity = value; }
        public static float BaseMoveSpeed { get => _baseMoveSpeed; set => _baseMoveSpeed = value; }
        public static float DashSpeedMultiplier { get => _dashSpeedMultiplier; set => _dashSpeedMultiplier = value; }
        public static float DashDuration { get => _dashDuration; set => _dashDuration = value; }
        public static float DashIFrameDuration { get => _dashIFrameDuration; set => _dashIFrameDuration = value; }
        public static float DashCooldown { get => _dashCooldown; set => _dashCooldown = value; }
        public static float DoubleTapDelay { get => _doubleTapDelay; set => _doubleTapDelay = value; }
        public static float CrouchSpeedMultiplier { get => _crouchSpeedMultiplier; set => _crouchSpeedMultiplier = value; }
        public static float CrouchHeightRatio { get => _crouchHeightRatio; set => _crouchHeightRatio = value; }
        public static float VisualBaseScale { get => _visualBaseScale; set => _visualBaseScale = value; }

        public static float IFrameDuration { get => _iFrameDuration; set => _iFrameDuration = value; }
        public static float ParryWindowDuration { get => _parryWindow; set => _parryWindow = value; }
        public static float ParryCounterDamage { get => _parryCounterDamage; set => _parryCounterDamage = value; }
        public static float ParryReflectSpeed { get => _parryReflectSpeed; set => _parryReflectSpeed = value; }
        public static float ParryCooldown { get => _parryCooldown; set => _parryCooldown = value; }

        public static Vector2 KnockbackForce { get => _knockbackForce; set => _knockbackForce = value; }
        public static float ThrustKnockbackForce { get => _heavyThrustKnockbackForce; set => _heavyThrustKnockbackForce = value; }
        public static float ThrustKnockbackRadius { get => _thrustKnockbackRadius; set => _thrustKnockbackRadius = value; }
        public static float ThrustPleasureBonus { get => _thrustPleasureGain; set => _thrustPleasureGain = value; }
        public static float ThrustCooldown { get => _thrustCooldown; set => _thrustCooldown = value; }
        public static float ClimaxBlastDamage { get => _climaxBlastDamage; set => _climaxBlastDamage = value; }

        public static float EnemyBaseMaxHp { get => _defaultEnemyMaxHp; set => _defaultEnemyMaxHp = value; }
        public static float EnemyBaseSpeed { get => _defaultEnemySpeed; set => _defaultEnemySpeed = value; }

        public static float ShieldEnemyPhysArmorRatio { get => _shieldEnemyPhysArmorRatio; set => _shieldEnemyPhysArmorRatio = value; }
        public static float ShieldEnemyMagicDamageMultiplier { get => _shieldEnemyMagicDamageMultiplier; set => _shieldEnemyMagicDamageMultiplier = value; }

        public static float VesselReclaimRadius { get => _vesselReclaimRadius; set => _vesselReclaimRadius = value; }

        public static float DefaultBaseMaxPleasure { get => _defaultBaseMaxPleasure; set => _defaultBaseMaxPleasure = value; }
        public static float DefaultPleasureRateMultiplier { get => _defaultPleasureRateMultiplier; set => _defaultPleasureRateMultiplier = value; }
        public static int InitialGold { get => _initialGold; set => _initialGold = value; }
        public static int StageClearGold { get => _stageClearGold; set => _stageClearGold = value; }

        // --- Instance Export Properties for Godot Autoload & GDScript ---
        [Export] public float Gravity { get => _gravity; set => _gravity = value; }
        [Export] public float JumpVelocity { get => _jumpVelocity; set => _jumpVelocity = value; }
        [Export] public float DefaultEnemyMaxHp { get => _defaultEnemyMaxHp; set => _defaultEnemyMaxHp = value; }
        [Export] public float DefaultEnemySpeed { get => _defaultEnemySpeed; set => _defaultEnemySpeed = value; }
        [Export] public float DefaultEnemyGravity { get => _defaultEnemyGravity; set => _defaultEnemyGravity = value; }
        [Export] public int DefaultGoldReward { get => _defaultGoldReward; set => _defaultGoldReward = value; }
        [Export] public float DefaultDropChance { get => _defaultDropChance; set => _defaultDropChance = value; }
        [Export] public float ClawSlashCooldown { get => _clawSlashCooldown; set => _clawSlashCooldown = value; }
        [Export] public float ClawSlashDamage { get => _clawSlashDamage; set => _clawSlashDamage = value; }
        [Export] public float ClawSlashRange { get => _clawSlashRange; set => _clawSlashRange = value; }
        [Export] public float HeavyCleaveCooldown { get => _heavyCleaveCooldown; set => _heavyCleaveCooldown = value; }
        [Export] public float HeavyCleaveDamage { get => _heavyCleaveDamage; set => _heavyCleaveDamage = value; }
        [Export] public float HeavyCleaveRange { get => _heavyCleaveRange; set => _heavyCleaveRange = value; }
        [Export] public float ParryWindow { get => _parryWindow; set => _parryWindow = value; }
        [Export] public float HeavyThrustKnockbackForce { get => _heavyThrustKnockbackForce; set => _heavyThrustKnockbackForce = value; }
        [Export] public float ThrustPleasureGain { get => _thrustPleasureGain; set => _thrustPleasureGain = value; }
        [Export] public float HeavyThrustPleasureBoost { get => _thrustPleasureGain; set => _thrustPleasureGain = value; }
        [Export] public float HeavyThrustCooldown { get => _thrustCooldown; set => _thrustCooldown = value; }
        [Export] public float DefaultBaseMaxPleasure { get => _defaultBaseMaxPleasure; set => _defaultBaseMaxPleasure = value; }
        [Export] public float DefaultPleasureRateMultiplier { get => _defaultPleasureRateMultiplier; set => _defaultPleasureRateMultiplier = value; }
        [Export] public float CellSize { get => _cellSize; set => _cellSize = value; }
        [Export] public int ShopRerollCost { get => _shopRerollCost; set => _shopRerollCost = value; }
        [Export] public int StageClearGold { get => _stageClearGold; set => _stageClearGold = value; }
        [Export] public int InitialGold { get => _initialGold; set => _initialGold = value; }

        public override void _EnterTree()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                QueueFree();
            }
        }
    }
}
```

---

## 4. Verification Protocol
1. **Production Build Verification**:
   ```powershell
   dotnet build Orclimax.csproj
   ```
   Must produce **0 errors** and **0 warnings**.

2. **Unit Test Suite Verification**:
   ```powershell
   dotnet test tests/Orclimax.Tests/Orclimax.Tests.csproj
   ```
   Must pass **100% of unit tests** directly exercising `Orclimax.Autoload.GameConfig`.
