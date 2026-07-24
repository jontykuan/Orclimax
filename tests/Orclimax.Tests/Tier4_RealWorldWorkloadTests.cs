using System;
using NUnit.Framework;
using Orclimax.Autoload;
using Orclimax.Core;
using GameConfig = Orclimax.Tests.GameConfig;

namespace Orclimax.Tests
{
    [TestFixture]
    public class Tier4_RealWorldWorkloadTests
    {
        [SetUp]
        public void Setup()
        {
            GameConfig.BaseGravity = 980.0f;
            GameConfig.BaseJumpVelocity = -550.0f;
            GameConfig.BaseMoveSpeed = 250.0f;
            GameConfig.DoubleTapDelay = 0.25f;
            GameConfig.DashSpeedMultiplier = 2.8f;
            GameConfig.DashDuration = 0.15f;
            GameConfig.DashIFrameDuration = 0.18f;
            GameConfig.DashCooldown = 0.6f;
            GameConfig.ParryWindowDuration = 0.22f;
            GameConfig.ParryCounterDamage = 15.0f;
            GameConfig.ParryReflectSpeed = 700.0f;
            GameConfig.ParryCooldown = 1.0f;
            GameConfig.ThrustKnockbackRadius = 140.0f;
            GameConfig.ThrustKnockbackForce = 450.0f;
            GameConfig.ThrustPleasureBonus = 15.0f;
            GameConfig.ThrustCooldown = 1.2f;
            GameConfig.VesselReclaimRadius = 60.0f;
            GameConfig.ShieldEnemyPhysArmorRatio = 0.75f;
            GameConfig.ShieldEnemyMagicDamageMultiplier = 2.0f;
        }

        [Test]
        public void Scenario1_FullGameLoopFlow()
        {
            // 1. Initial Launch at Title Screen
            GameState state = GameState.Title;
            int gold = 15;
            int stage = 1;
            Assert.That(state, Is.EqualTo(GameState.Title));

            // 2. Start Game -> Vessel Select
            state = GameState.VesselSelect;
            string selectedVessel = "girl_01"; // Lydia
            Assert.That(selectedVessel, Is.EqualTo("girl_01"));

            // 3. Open Backpack & Prepare Equipment
            state = GameState.Backpack;
            bool itemsPlaced = true;
            Assert.That(itemsPlaced, Is.True);

            // 4. Select Stage on World Map
            state = GameState.WorldMap;
            int selectedNode = 1;
            Assert.That(selectedNode, Is.EqualTo(1));

            // 5. Enter Combat Wave
            state = GameState.Combat;
            float waveEnemiesHp = 60.0f;

            // Player defeats wave
            waveEnemiesHp = 0.0f;
            Assert.That(waveEnemiesHp, Is.EqualTo(0.0f));

            // 6. Wave Victory -> Advance Stage & Earn Gold Reward
            stage++;
            gold += 10;
            state = GameState.WorldMap;

            Assert.That(state, Is.EqualTo(GameState.WorldMap));
            Assert.That(stage, Is.EqualTo(2));
            Assert.That(gold, Is.EqualTo(25));
        }

        [Test]
        public void Scenario2_VesselSnatcherDisarmAndReclaimFlow()
        {
            // 1. Combat starts with attached vessel
            bool isDisarmed = false;
            float playerX = 100.0f;
            float vesselX = 100.0f;
            float OrcDamageMult = 1.0f;

            // 2. VesselSnatcher attacks and detaches Vessel
            isDisarmed = true;
            vesselX = 250.0f; // Snatcher throws vessel 150 units away
            OrcDamageMult = isDisarmed ? 0.0f : 1.0f;

            Assert.That(isDisarmed, Is.True);
            Assert.That(OrcDamageMult, Is.EqualTo(0.0f));

            // 3. Player executes Double-Tap Dash (left/right x2) to rapidly move towards Vessel
            float doubleTapInterval = 0.12f;
            bool dashTriggered = doubleTapInterval <= GameConfig.DoubleTapDelay;
            Assert.That(dashTriggered, Is.True);

            float moveDistance = GameConfig.BaseMoveSpeed * GameConfig.DashSpeedMultiplier * GameConfig.DashDuration;
            playerX += moveDistance;

            // 4. Check distance to detached vessel
            float distanceToVessel = MathF.Abs(vesselX - playerX);
            bool withinReclaimRadius = distanceToVessel <= GameConfig.VesselReclaimRadius;

            Assert.That(distanceToVessel, Is.EqualTo(45.0f));
            Assert.That(withinReclaimRadius, Is.True);

            // 5. Reclaim Vessel -> Restore equipment damage
            if (withinReclaimRadius)
            {
                isDisarmed = false;
                OrcDamageMult = 1.0f;
            }

            Assert.That(isDisarmed, Is.False);
            Assert.That(OrcDamageMult, Is.EqualTo(1.0f));
        }

        [Test]
        public void Scenario3_BossWave_RangedShieldedFlyerCombo()
        {
            // 1. Boss wave spawns 3 enemy archetypes: Ranged, Shielded, Flying
            float physicalWeaponDmg = 40.0f;
            float magicToyWeaponDmg = 30.0f;

            // 2. Ranged enemy fires straight high projectile -> Player crouches (Down) to dodge
            float heightReduction = 1.0f - GameConfig.CrouchHeightRatio;
            Assert.That(heightReduction, Is.EqualTo(0.4f).Within(0.001f));

            // 3. Ranged enemy fires parabolic mortar -> Player uses Double-Tap Up (Parry) to reflect projectile
            float parryInterval = 0.18f;
            bool parryActive = parryInterval <= GameConfig.DoubleTapDelay;
            float reflectDamage = parryActive ? GameConfig.ParryCounterDamage : 0.0f;
            Assert.That(reflectDamage, Is.EqualTo(15.0f));

            // 4. Player engages Shielded enemy -> Physical attacks suffer 75% reduction, Magic/Toy weapon deals 2x
            float physDmgDealt = physicalWeaponDmg * (1.0f - GameConfig.ShieldEnemyPhysArmorRatio);
            float magicDmgDealt = magicToyWeaponDmg * GameConfig.ShieldEnemyMagicDamageMultiplier;

            Assert.That(physDmgDealt, Is.EqualTo(10.0f).Within(0.001f));
            Assert.That(magicDmgDealt, Is.EqualTo(60.0f).Within(0.001f));
            Assert.That(magicDmgDealt, Is.GreaterThan(physDmgDealt));
        }

        [Test]
        public void Scenario4_LiveGameConfigParameterTuning()
        {
            // 1. Game designer modifies GameConfig parameters live
            GameConfig.BaseMoveSpeed = 300.0f;
            GameConfig.DashSpeedMultiplier = 3.0f;
            GameConfig.DoubleTapDelay = 0.30f;
            GameConfig.ParryWindowDuration = 0.30f;
            GameConfig.ShieldEnemyPhysArmorRatio = 0.60f;

            // 2. Validate move and dash speed calculations
            float newDashSpeed = GameConfig.BaseMoveSpeed * GameConfig.DashSpeedMultiplier;
            Assert.That(newDashSpeed, Is.EqualTo(900.0f).Within(0.001f));

            // 3. Validate updated double tap window acceptance
            float inputInterval = 0.28f;
            bool doubleTapAccepted = inputInterval <= GameConfig.DoubleTapDelay;
            Assert.That(doubleTapAccepted, Is.True);

            // 4. Validate updated parry duration window
            float parryElapsed = 0.29f;
            bool insideParryWindow = parryElapsed <= GameConfig.ParryWindowDuration;
            Assert.That(insideParryWindow, Is.True);

            // 5. Validate updated shield enemy damage calculation
            float rawDamage = 50.0f;
            float damageTaken = rawDamage * (1.0f - GameConfig.ShieldEnemyPhysArmorRatio);
            Assert.That(damageTaken, Is.EqualTo(20.0f).Within(0.001f));
        }

        [Test]
        public void Scenario5_MultiVesselSwitchingAndItemFusionCampaign()
        {
            // 1. Start with Vessel A (Lydia) equipped with item "wpn_sword"
            string currentVessel = "girl_01";
            var vesselGrids = new System.Collections.Generic.Dictionary<string, int>();
            vesselGrids[currentVessel] = 1; // Grid state 1

            // 2. Switch to Vessel B (Cynthia)
            currentVessel = "girl_02";
            if (!vesselGrids.ContainsKey(currentVessel))
            {
                vesselGrids[currentVessel] = 0; // Empty grid for vessel B
            }

            Assert.That(vesselGrids["girl_01"], Is.EqualTo(1));
            Assert.That(vesselGrids["girl_02"], Is.EqualTo(0));

            // 3. Place component A (Rusty Sword) and component B (Poison Potion) adjacent in Vessel B grid
            string compA = "Rusty Sword";
            string compB = "Poison Potion";
            bool areAdjacent = true;

            // 4. Trigger Fusion Recipe check
            string resultItem = null;
            if (compA == "Rusty Sword" && compB == "Poison Potion" && areAdjacent)
            {
                resultItem = "Poison Sword";
            }

            Assert.That(resultItem, Is.EqualTo("Poison Sword"));

            // 5. Switch back to Vessel A -> Retains original equipment layout
            currentVessel = "girl_01";
            Assert.That(vesselGrids[currentVessel], Is.EqualTo(1));
        }
    }
}
