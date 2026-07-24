# E2E Test Suite Architecture & Feasibility Report (`handoff.md`)

## 1. Observation

Direct observations from examining the codebase at `d:\cykuan\Godot_v4.6.2-stable_mono_win64\Orclimax`:

### Project Configuration & Build System
- **Engine & SDK Version**: `project.godot` specifies `config/features=PackedStringArray("4.6", "C#", "Forward Plus")` and `[dotnet] project/assembly_name="Orclimax"`.
- **Project C# Configuration** (`Orclimax.csproj`):
  ```xml
  <Project Sdk="Godot.NET.Sdk/4.6.2">
    <PropertyGroup>
      <TargetFramework>net8.0</TargetFramework>
      <EnableDynamicLoading>true</EnableDynamicLoading>
    </PropertyGroup>
  </Project>
  ```
- **Solution File** (`Orclimax.sln`): Currently includes only single project `Orclimax.csproj` (`{A87DE98C-D137-47DF-B268-A39B2BFCEE88}`).
- **Build Status**: Command `dotnet build Orclimax.sln` succeeded, producing output `Orclimax -> D:\cykuan\Godot_v4.6.2-stable_mono_win64\Orclimax\.godot\mono\temp\bin\Debug\Orclimax.dll`.
- **Godot Executable**: Located at `d:\cykuan\Godot_v4.6.2-stable_mono_win64\Godot_v4.6.2-stable_mono_win64_console.exe`.

### Source Architecture
- **Autoload Singletons (C#)** (`project.godot:18-23`):
  - `GameManager` (`src/autoload/GameManager.cs:8`): Inherits `Node`. Manages `GameState` enum (`Shop=0`, `Combat=1`, `GameOver=2`, `Victory=3`), `Gold`, `CurrentStage`, `UnlockedFemaleIds`. Emits `StateChanged`, `GoldChanged`, `StageChanged` signals.
  - `InventoryManager` (`src/autoload/InventoryManager.cs:9`): Inherits `Node`. Manages `CurrentVessel`, `VesselGrids` dictionary, `BackpackGrid`, `Stash` array, `Recipes`, `CurrentShopItems`, `ShopSlotsLocked`. Emits `VesselChanged`, `GridUpdated`, `StashUpdated`. Contains `SetVessel()`, `TryPlaceItemFromStash()`, `TryTakeItemToStash()`, `RotatePlacedItem()`, `BuyItem()`, `SellPlacedItem()`, `SellStashedItem()`, `TriggerFusions()`.
  - `CombatManager` (`src/autoload/CombatManager.cs:8`): Inherits `Node`. Manages `MaxHp`, `CurrentHp`, `MoveSpeed`, `AttackSpeed`, `Armor`, `MaxPleasure`, `CurrentPleasure`, `PleasureAccumulationRate`. Emits `HpChanged`, `PleasureChanged`, `ClimaxTriggered`, `WeaponFired`. Contains `StartCombat()`, `GetCurrentStats()`, `TakeDamage()`, `Heal()`, `AddPleasure()`, `TriggerClimax()`, and `_Process()` auto-weapon cooldown firing.
- **Core C# Data Models** (`src/core/`):
  - `Enums.cs`: Defines `PlacementZone` (General, Head, Chest, Groin, Limbs, Inactive), `ItemCategory` (Weapon, Toy, Clothing, Accessory, Consumable), `GameState`.
  - `ItemData.cs`: `Resource` with `Id`, `ItemName`, `Category`, `RequiredZone`, `ShapeOffsets`, `Damage`, `Cooldown`, `PleasureGain`, `ArmorBonus`, `SpeedBonus`, `AttackSpeedBonus`, `BasePrice`. Contains `GetRotatedOffsets(rotationSteps)`.
  - `GridCell.cs`: Grid coordinate mapping with `Zone`, `IsLocked`, `OccupyingItemInstanceId`.
  - `GridData.cs`: `RefCounted` holding `Cells` dictionary and `PlacedItems` dictionary. Contains `CanPlaceItem()`, `PlaceItem()`, `RemoveItem()`, `UnlockCell()`.
  - `GridItemInstance.cs`: `RefCounted` wrapping `ItemData`, `AnchorCoords`, `RotationSteps`, `InstanceId`. Contains `GetAbsoluteOccupiedCells()` and `GetActiveRatio(grid)`.
  - `VesselData.cs`: `Resource` defining character grid layout (`HeadCells`, `ChestCells`, `GroinCells`, `LimbsCells`, `GeneralCells`, `InactiveCells`, `InitiallyLockedCells`). Contains `InitializeGrid(grid)`.
  - `FusionRecipe.cs`: `RefCounted` linking `ComponentA`, `ComponentB`, `Result`.
- **Existing Test Script**:
  - `src/core/TestGrid.cs`: Standalone C# script inheriting from `SceneTree`. Contains custom test runner logic in `RunTests()` testing `GridData.CanPlaceItem`, zone restrictions, locked cell overlaps, rotation snapping, occupancy mapping, and cell removal.

### GDScript Entities & UI Systems
- **Enemy Class** (`src/entities/enemy/EnemyBase.gd:3`):
  - Inherits `CharacterBody2D`. Class `EnemyAction` handles action rotation (`Claw Slash` cd: 1.5, dmg: 3.0, range: 110.0; `Heavy Cleave` cd: 4.0, dmg: 8.0, range: 130.0).
  - Target tracking (`get_nodes_in_group("player")`), `take_damage()`, `_die()` (awards gold via `GameManager.AddGold()`, drop chance `InventoryManager.AddItemToStash()`).
- **Player Double-Tap & Physics** (`src/entities/player/Player.gd:1`):
  - `DOUBLE_TAP_DELAY = 0.25s` (`Player.gd:28`). Double press of `ui_left` or `ui_right` within 0.25s triggers `_start_dash(dir)` when `dash_cooldown_timer <= 0`. Sets `velocity.x = dir.x * move_speed * 2.8`.
  - Crouch handling (`is_crouching = Input.is_action_pressed("ui_down")`, `crouch_speed_multiplier = 0.4`, collision shape resizing).
- **Scene Navigation Interfaces**:
  - `BackpackUI.gd:637`: `_on_start_combat_pressed()` sets `GameManager.CurrentState = GameState.Combat`, calls `CombatManager.StartCombat()`, executes `get_tree().change_scene_to_file("res://src/entities/player/Level.tscn")`.
  - `Level.gd:50-64`: `_complete_wave()` stops spawn timer, displays victory HUD.
  - `HUD.gd:98-101`: `_on_return_button_pressed()` calls `GameManager.AdvanceStage()`, executes `get_tree().change_scene_to_file("res://src/ui/backpack/BackpackUI.tscn")`.
  - `HUD.gd:82-94`: `_on_game_state_changed(2)` handles Game Over, waits 1.5s, sets `GameState.Shop`, changes scene to `BackpackUI.tscn`.

---

## 2. Logic Chain

1. **Test Project Infrastructure Requirements**:
   - Creating a test project at `tests/Orclimax.Tests/Orclimax.Tests.csproj` targeting `net8.0` with `NUnit` (4.x), `NUnit3TestAdapter`, and `Microsoft.NET.Test.Sdk` allows running unit tests via `dotnet test` directly from terminal or IDE.
   - Adding `<ProjectReference Include="..\..\Orclimax.csproj" />` grants direct access to all `Orclimax.Core` and `Orclimax.Autoload` C# classes.

2. **C# Object Lifecycle & Godot Headless Runner Integration**:
   - Classes inheriting `RefCounted`, `Resource`, or `Node` (`ItemData`, `GridData`, `GameManager`, `CombatManager`) use Godot interop bindings.
   - For fast C# unit testing via `dotnet test`, core domain logic (rotations, grid placement calculations, stat aggregation math, recipe checking, state transitions) can be executed cleanly.
   - For complete end-to-end (E2E) testing of GDScript nodes (`Player.gd`, `EnemyBase.gd`, `Level.gd`, `BackpackUI.gd`, `HUD.gd`), double-tap input timing, and scene transitions (`change_scene_to_file`), a Godot headless runner script (similar to `TestGrid.cs` or using GdUnit4NET) can be launched via `Godot_v4.6.2-stable_mono_win64_console.exe --headless` or integrated into NUnit setup fixtures.

3. **Detailed Test Coverage Architecture**:

   - **Module 1: `GameConfig` & Core Models (`Orclimax.Core`)**
     - Target: `ItemData.cs`, `GridData.cs`, `GridItemInstance.cs`, `VesselData.cs`, `FusionRecipe.cs`.
     - Test Cases:
       - `ItemData_GetRotatedOffsets_ReturnsCorrectRotatedCoordinates`: Verify 0, 90, 180, 270 degree rotation transformation math `(x, y) -> (-y, x)`.
       - `GridData_CanPlaceItem_ValidatesZoneLockAndOccupancy`: Test valid placement, out-of-bound placement, locked cell rejection, occupied cell rejection, matching zone validation, and inactive cell bypass.
       - `GridItemInstance_GetActiveRatio_CalculatesCoverage`: Verify ratio calculation when shape overlaps active vs inactive cells.
       - `VesselData_InitializeGrid_PopulatesZonesAndLocks`: Verify grid cell zone types and initial locked cells match vessel definitions (e.g. Lydia vs Cynthia).

   - **Module 2: `GameManager` Autoload (`Orclimax.Autoload`)**
     - Target: `GameManager.cs`.
     - Test Cases:
       - `AddGold_And_SpendGold_Behavior`: Verify gold increase, successful spend when sufficient, rejection when insufficient.
       - `AdvanceStage_IncrementsStageAndAwardsGold`: Verify `CurrentStage` increments, gold += 10, state resets to `GameState.Shop`.
       - `TriggerGameOver_ChangesStateToGameOver`: Verify state transitions to `GameState.GameOver`.
       - `Signals_EmittedOnStateGoldStageChange`: Assert event handler invocations for `StateChanged`, `GoldChanged`, `StageChanged`.

   - **Module 3: `InventoryManager` Autoload (`Orclimax.Autoload`)**
     - Target: `InventoryManager.cs`.
     - Test Cases:
       - `SetVessel_KeepEquippedFalse_StripsItemsToStash`: Verify switching vessel with `keepEquipped=false` moves all placed items on current grid into `Stash` and resets grid.
       - `SetVessel_KeepEquippedTrue_RetainsEquippedOnInactiveVessel`: Verify switching vessel with `keepEquipped=true` preserves items on vessel's specific `GridData`.
       - `TryPlaceItemFromStash_And_TryTakeItemToStash`: Verify moving items between Stash array and Backpack grid.
       - `RotatePlacedItem_ValidatesPlacementOrRollsBack`: Verify successful rotation updates grid state; invalid rotation preserves original position and orientation.
       - `TriggerFusions_MergesAdjacentRecipeComponents`: Test adjacent matching items (e.g. `Rusty Sword` + `Poison Potion` -> `Poison Sword`), component removal from grid, and result placement/stashing.

   - **Module 4: `CombatManager` Autoload (`Orclimax.Autoload`)**
     - Target: `CombatManager.cs`.
     - Test Cases:
       - `StartCombat_AggregatesStatsFromBackpackGrid`: Test stat summation (`MaxHp`, `MoveSpeed`, `AttackSpeed`, `Armor`, `PleasureAccumulationRate`) scaled by `GetActiveRatio()` of placed weapons, armor, and toys.
       - `GetCurrentStats_ReturnsCorrectDictionaryForUI`: Verify stat calculation matching preparation phase UI needs.
       - `TakeDamage_AppliesArmorReductionAndTriggersGameOver`: Test damage formula `max(1, damage - Armor)`, HP decrement, and game over trigger when HP reaches 0.
       - `AddPleasure_TriggersClimaxWhenMaxReached`: Test pleasure accumulation and `TriggerClimax()` signal emission resetting pleasure to 0.

   - **Module 5: Enemy Classes (`EnemyBase.gd`)**
     - Target: `src/entities/enemy/EnemyBase.gd`.
     - Test Cases:
       - `EnemyAction_RotationSequence`: Verify rotation between `Claw Slash` and `Heavy Cleave` based on cooldown timers and range.
       - `TakeDamage_And_DeathReward`: Verify HP deduction, death condition, gold award (`GameManager.AddGold`), and loot drop generation to `InventoryManager.Stash`.

   - **Module 6: Player Double-Tap Logic (`Player.gd`)**
     - Target: `src/entities/player/Player.gd`.
     - Test Cases:
       - `DoubleTap_WithinDelay_TriggersDash`: Test two directional inputs within `DOUBLE_TAP_DELAY` (0.25s) trigger `_start_dash()`, setting dash speed multiplier (2.8x) and setting `dash_cooldown_timer = 0.6`.
       - `DoubleTap_ExceedingDelay_DoesNotTriggerDash`: Test two inputs separated by > 0.25s do not trigger dash.
       - `Crouch_ReducesSpeedAndResizesCollision`: Test `ui_down` input applies `crouch_speed_multiplier` (0.4) and resizes collision height.

   - **Module 7: Scene Navigation Interfaces**
     - Target: `BackpackUI.gd`, `HUD.gd`, `Level.gd`.
     - Test Cases:
       - `BackpackToCombat_Transition`: Verify clicking Start Combat transitions `GameState.Combat`, initializes `CombatManager.StartCombat()`, and loads `Level.tscn`.
       - `CombatToShop_VictoryTransition`: Verify wave completion and Return to Shop button increment stage via `GameManager.AdvanceStage()` and return scene to `BackpackUI.tscn`.
       - `GameOverToShop_Transition`: Verify player death triggers Game Over UI overlay and returns to `BackpackUI.tscn`.

---

## 3. Caveats

1. **User Permission Timeout on Interactive Commands**:
   - `run_command` calls targeting interactive Godot window processes may require user prompt confirmation in environment. Command execution can be automated via standard `dotnet test` or non-interactive headless CLI execution.
2. **C# Signal Connection Verification**:
   - Godot C# signals generated via `[Signal] public delegate void ...` require proper event connection testing using C# event handlers or Godot signal connection checks.
3. **No External Dependencies**:
   - Operation is restricted to CODE_ONLY mode (no external network/NuGet downloading). Test packages should rely on local .NET SDK assemblies or pre-installed SDK packages.

---

## 4. Conclusion

Setting up an NUnit E2E test project in `tests/Orclimax.Tests/Orclimax.Tests.csproj` is fully feasible, clean, and directly aligned with the project's C# .NET 8.0 architecture.

### Recommended Implementation Structure:
```
Orclimax/
├── Orclimax.sln                          # Add tests/Orclimax.Tests/Orclimax.Tests.csproj
├── tests/
│   └── Orclimax.Tests/
│       ├── Orclimax.Tests.csproj        # NUnit + Microsoft.NET.Test.Sdk
│       ├── Unit/
│       │   ├── Core/
│       │   │   ├── ItemDataTests.cs
│       │   │   ├── GridDataTests.cs
│       │   │   ├── GridItemInstanceTests.cs
│       │   │   ├── VesselDataTests.cs
│       │   │   └── FusionRecipeTests.cs
│       │   └── Autoload/
│       │       ├── GameManagerTests.cs
│       │       ├── InventoryManagerTests.cs
│       │       └── CombatManagerTests.cs
│       └── E2E/
│           ├── EnemyBehaviorTests.cs
│           ├── PlayerMovementTests.cs
│           ├── SceneNavigationTests.cs
│           └── HeadlessTestRunner.cs     # Godot headless test executor
```

---

## 5. Verification Method

To independently verify the test setup and recommendations:

1. **Inspect Codebase Files**:
   - View `Orclimax.csproj` and `Orclimax.sln` to confirm .NET 8.0 target.
   - View `src/core/TestGrid.cs` to inspect existing headless test tree structure.
   - View `src/autoload/GameManager.cs`, `InventoryManager.cs`, `CombatManager.cs` to verify singleton methods and signals.
   - View `src/entities/player/Player.gd:114-125` to inspect double-tap tracking variables (`last_press_left_time`, `DOUBLE_TAP_DELAY = 0.25`).
   - View `src/entities/enemy/EnemyBase.gd:38-42` to inspect enemy action rotation definitions.
2. **Build Project via dotnet**:
   - Run command: `dotnet build Orclimax.sln`
   - Invalidation Condition: Build failure or compiler errors.
3. **Run Test Suite via dotnet test**:
   - Run command: `dotnet test tests/Orclimax.Tests/Orclimax.Tests.csproj`
   - Invalidation Condition: Test runner fails to locate or execute unit tests.
