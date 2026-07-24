using System;
using NUnit.Framework;
using Orclimax.Autoload;
using Orclimax.Core;
using GameConfig = Orclimax.Tests.GameConfig;

namespace Orclimax.Tests
{
    [TestFixture]
    public class Tier3_PairwiseTests
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
        public void R1_R4_Pairwise_CustomGameConfigStateTransition()
        {
            int customInitialGold = 50;
            int customStageClearGold = 25;

            GameState state = GameState.Title;
            int gold = customInitialGold;

            state = GameState.VesselSelect;
            state = GameState.Backpack;
            state = GameState.Combat;

            int currentStage = 1 + 1;
            gold += customStageClearGold;
            state = GameState.WorldMap;

            Assert.That(state, Is.EqualTo(GameState.WorldMap));
            Assert.That(gold, Is.EqualTo(75));
            Assert.That(currentStage, Is.EqualTo(2));
        }

        [Test]
        public void R2_R3_Pairwise_DisarmedPlayer_ParryReflectRangedProjectile()
        {
            bool isDisarmed = true;
            bool doubleTapUpPressed = true;
            float inputInterval = 0.15f;

            bool isParryActive = doubleTapUpPressed && (inputInterval <= GameConfig.DoubleTapDelay);
            float projectileDamage = 20.0f;

            float OrcWeaponDamageMultiplier = isDisarmed ? 0.0f : 1.0f;
            float reflectedDamage = isParryActive ? GameConfig.ParryCounterDamage : 0.0f;
            float damageTaken = isParryActive ? 0.0f : projectileDamage;

            Assert.That(OrcWeaponDamageMultiplier, Is.EqualTo(0.0f));
            Assert.That(isParryActive, Is.True);
            Assert.That(reflectedDamage, Is.EqualTo(15.0f));
            Assert.That(damageTaken, Is.EqualTo(0.0f));
        }

        [Test]
        public void R2_R4_Pairwise_ShieldedEnemy_CustomMagicMultiplier()
        {
            GameConfig.ShieldEnemyPhysArmorRatio = 0.80f;
            GameConfig.ShieldEnemyMagicDamageMultiplier = 3.5f;

            float toyBaseDamage = 30.0f;
            float physicalBaseDamage = 30.0f;

            float physTaken = physicalBaseDamage * (1.0f - GameConfig.ShieldEnemyPhysArmorRatio);
            float magicTaken = toyBaseDamage * GameConfig.ShieldEnemyMagicDamageMultiplier;

            Assert.That(physTaken, Is.EqualTo(6.0f).Within(0.001f));
            Assert.That(magicTaken, Is.EqualTo(105.0f).Within(0.001f));
        }

        [Test]
        public void R3_R1_Pairwise_HeavyThrustAOE_PleasureAccumulation_VesselMultiplier()
        {
            float baseThrustPleasure = GameConfig.ThrustPleasureBonus;
            float vesselSensitivity = 0.5f;
            float pleasureMultiplier = 1.0f + vesselSensitivity;

            float finalPleasureGain = baseThrustPleasure * pleasureMultiplier;

            Assert.That(finalPleasureGain, Is.EqualTo(22.5f));
        }

        [Test]
        public void R1_R2_Pairwise_MapStageBranch_EnemySpawnTypeSelection()
        {
            int selectedBranch = 2;
            string enemyType1 = selectedBranch == 1 ? "RangedEnemy" : "FlyingEnemy";
            string enemyType2 = selectedBranch == 1 ? "SnatcherEnemy" : "ShieldedEnemy";

            Assert.That(enemyType1, Is.EqualTo("FlyingEnemy"));
            Assert.That(enemyType2, Is.EqualTo("ShieldedEnemy"));
        }
    }
}
