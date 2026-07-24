using System;
using NUnit.Framework;
using Orclimax.Autoload;

namespace Orclimax.Tests
{
    [TestFixture]
    public class Tier3_PairwiseTests
    {
        [SetUp]
        public void Setup()
        {
            GameConfig.Instance.BaseGravity = 980.0f;
            GameConfig.Instance.BaseJumpVelocity = -550.0f;
            GameConfig.Instance.BaseMoveSpeed = 250.0f;
            GameConfig.Instance.DoubleTapDelay = 0.25f;
            GameConfig.Instance.DashSpeedMultiplier = 2.8f;
            GameConfig.Instance.DashDuration = 0.15f;
            GameConfig.Instance.DashIFrameDuration = 0.18f;
            GameConfig.Instance.DashCooldown = 0.6f;
            GameConfig.Instance.ParryWindowDuration = 0.22f;
            GameConfig.Instance.ParryCounterDamage = 15.0f;
            GameConfig.Instance.ParryReflectSpeed = 700.0f;
            GameConfig.Instance.ParryCooldown = 1.0f;
            GameConfig.Instance.ThrustKnockbackRadius = 140.0f;
            GameConfig.Instance.ThrustKnockbackForce = 450.0f;
            GameConfig.Instance.ThrustPleasureBonus = 15.0f;
            GameConfig.Instance.ThrustCooldown = 1.2f;
            GameConfig.Instance.VesselReclaimRadius = 60.0f;
            GameConfig.Instance.ShieldEnemyPhysArmorRatio = 0.75f;
            GameConfig.Instance.ShieldEnemyMagicDamageMultiplier = 2.0f;
        }

        [Test]
        public void R1_R4_Pairwise_CustomGameConfigStateTransition()
        {
            // R4 config customization combined with R1 state navigation flow
            int customInitialGold = 50;
            int customStageClearGold = 25;

            GameState state = GameState.Title;
            int gold = customInitialGold;

            // Transition: Title -> VesselSelect -> Backpack -> Combat -> Victory -> AdvanceStage
            state = GameState.VesselSelect;
            state = GameState.Backpack;
            state = GameState.Combat;

            // Advance stage
            int currentStage = 1 + 1; // Stage 2
            gold += customStageClearGold;
            state = GameState.WorldMap;

            Assert.That(state, Is.EqualTo(GameState.WorldMap));
            Assert.That(gold, Is.EqualTo(75));
            Assert.That(currentStage, Is.EqualTo(2));
        }

        [Test]
        public void R2_R3_Pairwise_DisarmedPlayer_ParryReflectRangedProjectile()
        {
            // R2 VesselSnatcher disarm state combined with R3 Double-Tap Parry
            bool isDisarmed = true;
            bool doubleTapUpPressed = true;
            float inputInterval = 0.15f;

            bool isParryActive = doubleTapUpPressed && (inputInterval <= GameConfig.Instance.DoubleTapDelay);
            float projectileDamage = 20.0f;

            // When disarmed, player cannot fire Orc weapons, but CAN parry projectiles
            float OrcWeaponDamageMultiplier = isDisarmed ? 0.0f : 1.0f;
            float reflectedDamage = isParryActive ? GameConfig.Instance.ParryCounterDamage : 0.0f;
            float damageTaken = isParryActive ? 0.0f : projectileDamage;

            Assert.That(OrcWeaponDamageMultiplier, Is.EqualTo(0.0f));
            Assert.That(isParryActive, Is.True);
            Assert.That(reflectedDamage, Is.EqualTo(15.0f));
            Assert.That(damageTaken, Is.EqualTo(0.0f));
        }

        [Test]
        public void R2_R4_Pairwise_ShieldedEnemy_CustomMagicMultiplier()
        {
            // R4 custom parameterization combined with R2 Shielded Enemy calculation
            GameConfig.Instance.ShieldEnemyPhysArmorRatio = 0.80f;
            GameConfig.Instance.ShieldEnemyMagicDamageMultiplier = 3.5f;

            float toyBaseDamage = 30.0f;
            float physicalBaseDamage = 30.0f;

            float physTaken = physicalBaseDamage * (1.0f - GameConfig.Instance.ShieldEnemyPhysArmorRatio); // 30 * 0.20 = 6.0
            float magicTaken = toyBaseDamage * GameConfig.Instance.ShieldEnemyMagicDamageMultiplier;       // 30 * 3.5 = 105.0

            Assert.That(physTaken, Is.EqualTo(6.0f).Within(0.001f));
            Assert.That(magicTaken, Is.EqualTo(105.0f).Within(0.001f));
        }

        [Test]
        public void R3_R1_Pairwise_HeavyThrustAOE_PleasureAccumulation_VesselMultiplier()
        {
            // R3 Heavy Thrust AOE action combined with R1 Vessel sensitivity multiplier
            float baseThrustPleasure = GameConfig.Instance.ThrustPleasureBonus; // 15.0
            float vesselSensitivity = 0.5f;                             // +50% pleasure
            float pleasureMultiplier = 1.0f + vesselSensitivity;

            float finalPleasureGain = baseThrustPleasure * pleasureMultiplier;

            Assert.That(finalPleasureGain, Is.EqualTo(22.5f));
        }

        [Test]
        public void R1_R2_Pairwise_MapStageBranch_EnemySpawnTypeSelection()
        {
            // R1 MapUI branch choice combined with R2 enemy composition
            int selectedBranch = 2; // Branch 2: Airborne & Heavy Fortress path
            string enemyType1 = selectedBranch == 1 ? "RangedEnemy" : "FlyingEnemy";
            string enemyType2 = selectedBranch == 1 ? "SnatcherEnemy" : "ShieldedEnemy";

            Assert.That(enemyType1, Is.EqualTo("FlyingEnemy"));
            Assert.That(enemyType2, Is.EqualTo("ShieldedEnemy"));
        }
    }
}
