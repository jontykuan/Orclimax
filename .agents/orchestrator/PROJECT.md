# Project: Orclimax Feature Overhaul & Expansion

## Architecture Overview
Orclimax is a Godot 4.x (Mono / C# + GDScript) RPG game with inventory management, player-vessel interactions, and 2D combat.

- **Core & Config (`src/core/`, `src/autoload/`)**: `GameConfig.cs` centralizes parameters (physics, double-tap windows, i-frames, parry windows, knockback forces, enemy stats, UI multipliers). `GameManager.cs`, `CombatManager.cs`, `InventoryManager.cs` manage state transitions and game logic.
- **UI & Navigation (`src/ui/`)**: Multi-page navigation spanning `TitleScreen.tscn`, `VesselUI.tscn`, `MapUI.tscn`, `BackpackUI.tscn` (with hint box removed), and HUD/Combat.
- **Player & Mechanics (`src/entities/player/`)**: Orc `Player.gd` with directional double-tap actions (Dodge/Dash, Precise Parry, Heavy Thrust).
- **Enemy Mechanics (`src/entities/enemy/`)**: 4 enemy types (Ranged, Shielded, Flying, Vessel-Snatcher).

## Code Layout
```
src/
├── autoload/
│   ├── CombatManager.cs
│   ├── GameManager.cs
│   ├── InventoryManager.cs
│   └── GameConfig.cs [NEW]
├── core/
│   ├── Enums.cs
│   ├── ItemData.cs
│   └── VesselData.cs
├── entities/
│   ├── enemy/
│   │   ├── EnemyBase.gd / .tscn
│   │   ├── RangedEnemy.gd / .tscn [NEW]
│   │   ├── ShieldedEnemy.gd / .tscn [NEW]
│   │   ├── FlyingEnemy.gd / .tscn [NEW]
│   │   └── VesselSnatcherEnemy.gd / .tscn [NEW]
│   ├── player/
│   │   ├── Player.gd / .tscn
│   │   └── Level.gd / .tscn
│   └── weapons/
│       ├── Projectile.gd / .tscn
│       └── EnemyProjectile.gd / .tscn [NEW]
└── ui/
    ├── title/
    │   └── TitleScreen.tscn / .gd [NEW]
    ├── vessel/
    │   └── VesselUI.tscn / .gd [NEW]
    ├── map/
    │   └── MapUI.tscn / .gd [NEW]
    ├── backpack/
    │   └── BackpackUI.tscn / .gd (Cleaned up)
    └── hud/
        └── HUD.tscn / .gd
```

## Milestones

| # | Name | Scope | Dependencies | Status |
|---|------|-------|-------------|--------|
| M1 | `config_and_nav` | `GameConfig.cs`, TitleScreen, VesselUI, MapUI, BackpackUI cleanup, GameManager state transitions | none | IN_PROGRESS |
| M2 | `orc_actions` | Player directional double-tap controls (Dodge/Dash, Precise Parry, Heavy Thrust) | M1 | PLANNED |
| M3 | `enemy_mechanics` | 4 Enemy Types (Ranged, Shielded, Flying, Vessel-Snatcher) & Vessel Disarming logic | M1, M2 | PLANNED |
| M4 | `integration_and_e2e` | Dual Track E2E suite validation & adversarial hardening | M1, M2, M3 | PLANNED |

## Interface Contracts
- **GameConfig.cs**: Centralized constants (`DoubleTapWindow`, `DashSpeed`, `IFrameDuration`, `ParryWindowDuration`, `HeavyThrustAOERadius`, `EnemyRangedSpeed`, `EnemyShieldPhysicalReduction`, `VesselSnatchSpeed`, etc.).
- **GameManager.cs / State Flow**: Transitions across `Title` -> `VesselSelect` -> `Backpack` -> `WorldMap` -> `Combat`.
- **Vessel Snatching Contract**: When Vessel-Snatcher attaches/detaches Vessel, `InventoryManager`/`CombatManager` sets `IsVesselDisarmed = true`, preventing item active/passive triggers until Orc collides with dropped Vessel node (`ReclaimVessel()`).
