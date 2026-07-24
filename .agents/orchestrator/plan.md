# Master Plan: Orclimax Overhaul

## 1. Setup & Heartbeat
- Start 10-minute heartbeat cron for Orchestrator.
- Spawn parallel E2E Testing Track Orchestrator (`sub_orch_e2e`).

## 2. Milestone Execution Plan
- **Milestone 1: Config & Navigation (`config_and_nav`)**
  - Create centralized `GameConfig.cs` (and GDScript wrapper/access if needed).
  - Create `TitleScreen.tscn` / GDscript/C# script (Start, Save/Load, Gallery, Settings, Quit).
  - Create `VesselUI.tscn` / script (Head, Chest, Groin, Limbs sensitivity levels, skills, pleasure multipliers, traits).
  - Create `MapUI.tscn` / script (Cleared nodes, current stage, branching combat paths).
  - Clean up `BackpackUI.tscn` (Remove old bottom hint text box).
  - Wire state transitions in `GameManager.cs` / scene navigator.
  - Verify via worker build/test pass and reviewer checks.

- **Milestone 2: Orc Directional Double-Tap Actions (`orc_actions`)**
  - Update `Player.gd` to detect directional double-taps:
    - Left/Right x2: Dodge/Dash with i-frames & cooldown.
    - Up x2: Precise Guard/Parry window, counter-attack & reflect projectiles.
    - Down x2: Heavy Thrust short-range AOE knockback & Climax/Pleasure boost.
  - Parameterize all values from `GameConfig`.
  - Verify via worker build/test pass and reviewer checks.

- **Milestone 3: Advanced Enemy Mechanics & Vessel Disarming (`enemy_mechanics`)**
  - `RangedEnemy`: Lobbed parabolic projectiles (dodge L/R) and high/low straight projectiles (dodge Crouch/Jump).
  - `ShieldedEnemy`: High physical armor, low magic defense.
  - `FlyingEnemy`: Airborne sine-wave patrol & swoop.
  - `VesselSnatcherEnemy`: Detach & carry Vessel away. Orc items disabled until Orc touches Vessel to reclaim.
  - Verify via worker build/test pass and reviewer checks.

- **Milestone 4: E2E Integration & Hardening (`integration_and_e2e`)**
  - Await `TEST_READY.md` from E2E testing track.
  - Run full E2E test suite (Tiers 1-4).
  - Run Tier 5 Adversarial Coverage Hardening (Challenger -> Worker -> Reviewer).
  - Forensic Auditor verification pass.

## 3. Finalization
- Declare completion to Sentinel.
