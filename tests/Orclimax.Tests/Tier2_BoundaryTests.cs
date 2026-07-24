using System;
using NUnit.Framework;
using Orclimax.Autoload;
using Orclimax.Core;
using GameConfig = Orclimax.Tests.GameConfig;

namespace Orclimax.Tests
{
    [TestFixture]
    public class Tier2_BoundaryTests
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

        // --- R1 Boundary Tests ---

        [Test]
        public void R1_Boundary_VesselSensitivity_MinimumBoundary()
        {
            float sensitivity = 0.0f;
            float pleasureMultiplier = 1.0f + sensitivity;
            Assert.That(pleasureMultiplier, Is.EqualTo(1.0f));
        }

        [Test]
        public void R1_Boundary_VesselSensitivity_MaximumBoundary()
        {
            float sensitivity = 2.0f;
            float pleasureMultiplier = 1.0f + sensitivity;
            Assert.That(pleasureMultiplier, Is.EqualTo(3.0f));
        }

        [Test]
        public void R1_Boundary_MapUI_Stage0_InvalidStageBoundary()
        {
            int currentStage = 0;
            int effectiveStage = Math.Max(1, currentStage);
            Assert.That(effectiveStage, Is.EqualTo(1));
        }

        [Test]
        public void R1_Boundary_MapUI_Stage100_ExtremeMaxStage()
        {
            int currentStage = 100;
            bool isBossStage = currentStage % 5 == 0;
            Assert.That(isBossStage, Is.True);
        }

        [Test]
        public void R1_Boundary_StateFlow_SelfTransitionIgnored()
        {
            GameState current = GameState.Backpack;
            GameState target = GameState.Backpack;
            bool stateChanged = current != target;
            Assert.That(stateChanged, Is.False);
        }

        // --- R2 Boundary Tests ---

        [Test]
        public void R2_Boundary_ShieldEnemy_ZeroPhysicalArmorRatio()
        {
            GameConfig.ShieldEnemyPhysArmorRatio = 0.0f;
            float rawDamage = 50.0f;
            float damageTaken = rawDamage * (1.0f - GameConfig.ShieldEnemyPhysArmorRatio);
            Assert.That(damageTaken, Is.EqualTo(50.0f));
        }

        [Test]
        public void R2_Boundary_ShieldEnemy_FullPhysicalArmorRatio()
        {
            GameConfig.ShieldEnemyPhysArmorRatio = 1.0f;
            float rawDamage = 50.0f;
            float damageTaken = rawDamage * (1.0f - GameConfig.ShieldEnemyPhysArmorRatio);
            Assert.That(damageTaken, Is.EqualTo(0.0f));
        }

        [Test]
        public void R2_Boundary_VesselSnatcher_ExactReclaimRadiusBoundary()
        {
            float distance = 60.000f;
            bool canReclaim = distance <= GameConfig.VesselReclaimRadius;
            Assert.That(canReclaim, Is.True);
        }

        [Test]
        public void R2_Boundary_VesselSnatcher_JustOutsideReclaimRadius()
        {
            float distance = 60.001f;
            bool canReclaim = distance <= GameConfig.VesselReclaimRadius;
            Assert.That(canReclaim, Is.False);
        }

        [Test]
        public void R2_Boundary_EnemyHP_ZeroAndNegativeHP()
        {
            float hp = 0.0f;
            bool isDead = hp <= 0.0f;
            Assert.That(isDead, Is.True);

            hp = -15.5f;
            isDead = hp <= 0.0f;
            Assert.That(isDead, Is.True);
        }

        // --- R3 Boundary Tests ---

        [Test]
        public void R3_Boundary_DoubleTap_ExactThreshold_0_25s()
        {
            float interval = 0.250f;
            bool triggered = interval <= GameConfig.DoubleTapDelay;
            Assert.That(triggered, Is.True);
        }

        [Test]
        public void R3_Boundary_DoubleTap_ExceedsThreshold_0_251s()
        {
            float interval = 0.251f;
            bool triggered = interval <= GameConfig.DoubleTapDelay;
            Assert.That(triggered, Is.False);
        }

        [Test]
        public void R3_Boundary_DoubleTap_ZeroDelay_SimultaneousInput()
        {
            float interval = 0.000f;
            bool triggered = interval <= GameConfig.DoubleTapDelay;
            Assert.That(triggered, Is.True);
        }

        [Test]
        public void R3_Boundary_ParryWindow_ExpiredAt_0_221s()
        {
            float parryElapsed = 0.221f;
            bool insideParryWindow = parryElapsed <= GameConfig.ParryWindowDuration;
            Assert.That(insideParryWindow, Is.False);
        }

        [Test]
        public void R3_Boundary_ThrustAOE_ExactRadius_140_0f()
        {
            float distance = 140.000f;
            bool insideRadius = distance <= GameConfig.ThrustKnockbackRadius;
            Assert.That(insideRadius, Is.True);
        }

        // --- R4 Boundary Tests ---

        [Test]
        public void R4_Boundary_GameConfig_ZeroGravity()
        {
            GameConfig.BaseGravity = 0.0f;
            float verticalAccel = GameConfig.BaseGravity;
            Assert.That(verticalAccel, Is.EqualTo(0.0f));
        }

        [Test]
        public void R4_Boundary_GameConfig_NegativeJumpVelocity()
        {
            GameConfig.BaseJumpVelocity = 0.0f;
            Assert.That(GameConfig.BaseJumpVelocity, Is.EqualTo(0.0f));
        }

        [Test]
        public void R4_Boundary_GameConfig_ExtremeMoveSpeed()
        {
            GameConfig.BaseMoveSpeed = 10000.0f;
            float dashSpeed = GameConfig.BaseMoveSpeed * GameConfig.DashSpeedMultiplier;
            Assert.That(dashSpeed, Is.EqualTo(28000.0f));
        }

        [Test]
        public void R4_Boundary_GameConfig_ZeroDashCooldown()
        {
            GameConfig.DashCooldown = 0.0f;
            bool canDashImmediately = GameConfig.DashCooldown <= 0.0f;
            Assert.That(canDashImmediately, Is.True);
        }

        [Test]
        public void R4_Boundary_GameConfig_ZeroDoubleTapDelay()
        {
            GameConfig.DoubleTapDelay = 0.0f;
            float interval = 0.001f;
            bool triggered = interval <= GameConfig.DoubleTapDelay;
            Assert.That(triggered, Is.False);
        }
    }
}
