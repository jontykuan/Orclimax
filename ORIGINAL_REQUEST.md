# Original User Request

## Initial Request — 2026-07-24T09:32:34+08:00

Orclimax multi-page scene architecture overhaul, advanced enemy mechanics, directional double-tap actions, and parameterized game constants.

Working directory: d:\cykuan\Godot_v4.6.2-stable_mono_win64\Orclimax
Integrity mode: development

## Requirements

### R1. Scene Structure & Multi-Page Navigation
Implement a modular multi-page scene navigation architecture:
- **Title Screen (`TitleScreen.tscn`)**: Features Start (開始), Save/Load (存檔), Gallery (畫廊), Settings (設定), and Quit (結束) buttons.
- **Vessel Management Page (`VesselUI.tscn`)**: Displays Vessel body part sensitivity/development levels (Head, Chest, Groin, Limbs), skills, pleasure multipliers, and traits.
- **World Map Page (`MapUI.tscn`)**: Displays cleared nodes, current stage, and branching uncleared combat paths.
- **Backpack Preparation Phase (`BackpackUI.tscn`)**: Remove the old bottom hint text box.

### R2. Advanced Enemy Mechanics & Vessel Disarming
Implement 4 distinct enemy types in Godot:
1. **Ranged Enemy**: Shoots lobbed parabolic projectiles (dodged left/right) and straight high/low projectiles (dodged by crouching/jumping).
2. **Shielded Enemy**: Slow movement, high physical armor, low magic defense.
3. **Flying Enemy**: Airborne sine-wave hovering and swooping movement.
4. **Vessel-Snatcher Enemy**: Special attack that visually detaches and carries away the Vessel. While disarmed, Orc cannot trigger items until walking over the Vessel to reclaim it.

### R3. Directional Double-Tap Actions
Implement double-tap directional key controls on the Orc player:
1. **Left/Right x2 (Dodge/Dash)**: Rapid burst movement with i-frames and CD.
2. **Up x2 (Precise Guard/Parry)**: Brief parry window; counters physical attacks and reflects ranged projectiles back to enemies; has CD.
3. **Down x2 (Heavy Thrust)**: Short-range AOE knockback to surrounding enemies, forceful thrust into Vessel to accelerate Climax/Pleasure gauge; has CD.

### R4. Parameterized Game Constants
All physics, speeds, gravity, double-tap window thresholds, i-frame durations, parry windows, knockback forces, and enemy stats must be read from centralized constants (`GameConfig.cs`) to support future equipment and skill modifiers.

## Acceptance Criteria

### Scene Navigation & Multi-Page Flow
- [ ] Title Screen contains functional Start, Save/Load, Gallery, Settings, and Quit options.
- [ ] Game state seamlessly transitions across Title -> Vessel Select -> Backpack -> World Map -> Combat.
- [ ] Bottom hint text container is removed from BackpackUI.

### Advanced Enemy Mechanics
- [ ] Ranged enemies spawn parabolic projectiles and high/low straight projectiles.
- [ ] Shielded enemies take reduced damage from physical weapons and normal damage from magic/toys.
- [ ] Flying enemies patrol in airborne paths above ground.
- [ ] Vessel-Snatchers detach the Vessel on contact/attack; Orc items are disabled until Orc touches the Vessel again.

### Orc Double-Tap Controls
- [ ] Double-tapping Left/Right triggers Dodge/Dash with i-frames and cooldown.
- [ ] Double-tapping Up triggers Precise Parry with counter-damage/reflection and cooldown.
- [ ] Double-tapping Down triggers Heavy Thrust with AOE knockback, Climax gauge boost, and cooldown.

### Codebase Parameterization
- [ ] All movement, combat, double-tap timing, and enemy stats are read from `GameConfig.cs`.
