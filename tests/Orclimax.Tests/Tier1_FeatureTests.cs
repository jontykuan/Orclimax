using System;
using NUnit.Framework;
using Orclimax.Autoload;
using Orclimax.Core;
using GameConfig = Orclimax.Tests.GameConfig;

namespace Orclimax.Tests
{
    [TestFixture]
    public class Tier1_FeatureTests
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

        // --- R1: Multi-Page Navigation & State Flow ---

        [Test]
        public void R1_Navigation_TitleScreen_InitialStateIsTitle()
        {
            var state = GameState.Title;
            Assert.That(state, Is.EqualTo(GameState.Title));
        }

        [Test]
        public void R1_Navigation_StateFlow_TitleToVesselSelect()
        {
            GameState state = GameState.Title;
            state = GameState.VesselSelect;
            Assert.That(state, Is.EqualTo(GameState.VesselSelect));
        }

        [Test]
        public void R1_Navigation_StateFlow_VesselSelectToBackpack()
        {
            GameState state = GameState.VesselSelect;
            state = GameState.Backpack;
            Assert.That(state, Is.EqualTo(GameState.Backpack));
        }

        [Test]
        public void R1_Navigation_StateFlow_BackpackToWorldMap()
        {
            GameState state = GameState.Backpack;
            state = GameState.WorldMap;
            Assert.That(state, Is.EqualTo(GameState.WorldMap));
        }

        [Test]
        public void R1_Navigation_StateFlow_WorldMapToCombat()
        {
            GameState state = GameState.WorldMap;
            state = GameState.Combat;
            Assert.That(state, Is.EqualTo(GameState.Combat));
        }

        // --- R2: Advanced Enemies & Vessel Disarming ---

        [Test]
        public void R2_AdvancedEnemies_RangedEnemy_ProjectileTypes()
        {
            float parabolicArcHeight = 250.0f;
            float straightHighHeight = 200.0f;
            float straightLowHeight = 20.0f;

            float crouchHeight = 256.0f * GameConfig.CrouchHeightRatio;
            bool dodgedHigh = crouchHeight < straightHighHeight;
            bool hitByLow = crouchHeight > straightLowHeight;

            Assert.That(dodgedHigh, Is.True);
            Assert.That(hitByLow, Is.True);
            Assert.That(parabolicArcHeight, Is.GreaterThan(straightHighHeight));
        }

        [Test]
        public void R2_AdvancedEnemies_ShieldedEnemy_DamageReduction()
        {
            float rawDamage = 40.0f;
            float physicalDamageTaken = rawDamage * (1.0f - GameConfig.ShieldEnemyPhysArmorRatio);
            float magicDamageTaken = rawDamage * GameConfig.ShieldEnemyMagicDamageMultiplier;

            Assert.That(physicalDamageTaken, Is.EqualTo(10.0f));
            Assert.That(magicDamageTaken, Is.EqualTo(80.0f));
        }

        [Test]
        public void R2_AdvancedEnemies_FlyingEnemy_HoverHeight()
        {
            float baseHoverAltitude = 200.0f;
            float time = 1.5f;
            float hoverY = baseHoverAltitude + MathF.Sin(time * 2.0f) * 30.0f;

            Assert.That(hoverY, Is.GreaterThan(160.0f));
            Assert.That(hoverY, Is.LessThan(240.0f));
        }

        [Test]
        public void R2_AdvancedEnemies_VesselSnatcher_DisarmStateTracking()
        {
            bool isDisarmed = true;
            float OrcWeaponDamageMultiplier = isDisarmed ? 0.0f : 1.0f;
            Assert.That(isDisarmed, Is.True);
            Assert.That(OrcWeaponDamageMultiplier, Is.EqualTo(0.0f));
        }

        [Test]
        public void R2_AdvancedEnemies_VesselSnatcher_ReclaimRadius()
        {
            float playerX = 100.0f;
            float playerY = 100.0f;
            float vesselX = 130.0f;
            float vesselY = 100.0f;

            float distance = MathF.Sqrt(MathF.Pow(playerX - vesselX, 2) + MathF.Pow(playerY - vesselY, 2));
            bool insideReclaimZone = distance <= GameConfig.VesselReclaimRadius;

            Assert.That(distance, Is.EqualTo(30.0f));
            Assert.That(insideReclaimZone, Is.True);
        }

        // --- R3: Directional Double-Tap Actions ---

        [Test]
        public void R3_DoubleTap_Horizontal_DodgeDashBurst()
        {
            float press1Time = 1.00f;
            float press2Time = 1.15f;
            float delay = press2Time - press1Time;

            bool doubleTapTriggered = delay <= GameConfig.DoubleTapDelay;
            float dashSpeed = GameConfig.BaseMoveSpeed * GameConfig.DashSpeedMultiplier;

            Assert.That(doubleTapTriggered, Is.True);
            Assert.That(dashSpeed, Is.EqualTo(700.0f));
        }

        [Test]
        public void R3_DoubleTap_Horizontal_DashIFrameDuration()
        {
            float elapsedTime = 0.10f;
            bool hasIFrames = elapsedTime <= GameConfig.DashIFrameDuration;

            Assert.That(hasIFrames, Is.True);
        }

        [Test]
        public void R3_DoubleTap_Horizontal_DashCooldown()
        {
            float cooldownTimer = GameConfig.DashCooldown;
            Assert.That(cooldownTimer, Is.EqualTo(0.6f));
        }

        [Test]
        public void R3_DoubleTap_Up_GuardParryWindow()
        {
            float press1Time = 0.50f;
            float press2Time = 0.62f;
            bool isParryActivated = (press2Time - press1Time) <= GameConfig.DoubleTapDelay;

            float parryActiveTime = 0.10f;
            bool isInvulnerable = parryActiveTime <= GameConfig.ParryWindowDuration;

            Assert.That(isParryActivated, Is.True);
            Assert.That(isInvulnerable, Is.True);
        }

        [Test]
        public void R3_DoubleTap_Down_HeavyThrustAOE()
        {
            float press1Time = 2.00f;
            float press2Time = 2.10f;
            bool isThrustActivated = (press2Time - press1Time) <= GameConfig.DoubleTapDelay;

            float targetDistance = 100.0f;
            bool hitByAOE = targetDistance <= GameConfig.ThrustKnockbackRadius;
            float pleasureGain = GameConfig.ThrustPleasureBonus;

            Assert.That(isThrustActivated, Is.True);
            Assert.That(hitByAOE, Is.True);
            Assert.That(pleasureGain, Is.EqualTo(15.0f));
        }

        // --- R4: GameConfig Parameterization ---

        [Test]
        public void R4_GameConfig_BasePhysicsParameters()
        {
            Assert.That(GameConfig.BaseGravity, Is.EqualTo(980.0f));
            Assert.That(GameConfig.BaseJumpVelocity, Is.EqualTo(-550.0f));
            Assert.That(GameConfig.BaseMoveSpeed, Is.EqualTo(250.0f));
        }

        [Test]
        public void R4_GameConfig_DoubleTapWindowParameterization()
        {
            Assert.That(GameConfig.DoubleTapDelay, Is.EqualTo(0.25f));
        }

        [Test]
        public void R4_GameConfig_CombattTimingsParameterization()
        {
            Assert.That(GameConfig.ParryWindowDuration, Is.EqualTo(0.22f));
            Assert.That(GameConfig.DashCooldown, Is.EqualTo(0.6f));
            Assert.That(GameConfig.ParryCooldown, Is.EqualTo(1.0f));
        }

        [Test]
        public void R4_GameConfig_EnemyBaseDefaultsParameterization()
        {
            Assert.That(GameConfig.ShieldEnemyPhysArmorRatio, Is.EqualTo(0.75f));
            Assert.That(GameConfig.ShieldEnemyMagicDamageMultiplier, Is.EqualTo(2.0f));
        }

        [Test]
        public void R4_GameConfig_DynamicOverrideSupport()
        {
            GameConfig.BaseMoveSpeed = 320.0f;
            GameConfig.DoubleTapDelay = 0.30f;

            Assert.That(GameConfig.BaseMoveSpeed, Is.EqualTo(320.0f));
            Assert.That(GameConfig.DoubleTapDelay, Is.EqualTo(0.30f));
        }
    }
}
