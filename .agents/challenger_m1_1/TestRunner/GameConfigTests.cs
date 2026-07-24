using NUnit.Framework;
using Orclimax.Autoload;
using Godot;

namespace Orclimax.EmpiricalTests
{
    [TestFixture]
    public class GameConfigTests
    {
        private GameConfig _config;

        [SetUp]
        public void Setup()
        {
            _config = new GameConfig();
        }

        [Test]
        public void Verify_PhysicsAndMovement_DefaultsAreValidAndNonZero()
        {
            Assert.That(_config.Gravity, Is.EqualTo(980.0f));
            Assert.That(_config.BaseGravity, Is.EqualTo(980.0f));

            Assert.That(_config.JumpVelocity, Is.EqualTo(-550.0f));
            Assert.That(_config.BaseJumpVelocity, Is.EqualTo(-550.0f));

            Assert.That(_config.BaseMoveSpeed, Is.EqualTo(250.0f));
            Assert.That(_config.DashSpeedMultiplier, Is.EqualTo(2.8f));
            Assert.That(_config.DashDuration, Is.EqualTo(0.15f));
            Assert.That(_config.DashIFrameDuration, Is.EqualTo(0.18f));
            Assert.That(_config.DashCooldown, Is.EqualTo(0.6f));
            Assert.That(_config.DoubleTapDelay, Is.EqualTo(0.25f));
            Assert.That(_config.CrouchSpeedMultiplier, Is.EqualTo(0.4f));
            Assert.That(_config.CrouchHeightRatio, Is.EqualTo(0.6f));
            Assert.That(_config.VisualBaseScale, Is.EqualTo(4.0f));
        }

        [Test]
        public void Verify_CombatActionTimings_DefaultsAreValidAndNonZero()
        {
            Assert.That(_config.IFrameDuration, Is.EqualTo(0.3f));
            Assert.That(_config.ParryWindow, Is.EqualTo(0.2f));
            Assert.That(_config.ParryWindowDuration, Is.EqualTo(0.2f));
            Assert.That(_config.ParryCounterDamage, Is.EqualTo(15.0f));
            Assert.That(_config.ParryReflectSpeed, Is.EqualTo(700.0f));
            Assert.That(_config.ParryCooldown, Is.EqualTo(1.0f));
        }

        [Test]
        public void Verify_HeavyThrustAndAOE_DefaultsAreValidAndNonZero()
        {
            Assert.That(_config.KnockbackForce, Is.EqualTo(new Vector2(300.0f, -150.0f)));
            Assert.That(_config.HeavyThrustKnockbackForce, Is.EqualTo(450.0f));
            Assert.That(_config.ThrustKnockbackForce, Is.EqualTo(450.0f));
            Assert.That(_config.ThrustKnockbackRadius, Is.EqualTo(140.0f));
            Assert.That(_config.ThrustPleasureGain, Is.EqualTo(15.0f));
            Assert.That(_config.ThrustPleasureBonus, Is.EqualTo(15.0f));
            Assert.That(_config.HeavyThrustPleasureBoost, Is.EqualTo(15.0f));
            Assert.That(_config.ThrustCooldown, Is.EqualTo(1.2f));
            Assert.That(_config.HeavyThrustCooldown, Is.EqualTo(1.2f));
            Assert.That(_config.ClimaxBlastDamage, Is.EqualTo(50.0f));
        }

        [Test]
        public void Verify_EnemyDefaults_AreValidAndNonZero()
        {
            Assert.That(_config.DefaultEnemyMaxHp, Is.EqualTo(30.0f));
            Assert.That(_config.EnemyBaseMaxHp, Is.EqualTo(30.0f));
            Assert.That(_config.DefaultEnemySpeed, Is.EqualTo(80.0f));
            Assert.That(_config.EnemyBaseSpeed, Is.EqualTo(80.0f));
            Assert.That(_config.DefaultEnemyGravity, Is.EqualTo(980.0f));
            Assert.That(_config.DefaultGoldReward, Is.EqualTo(2));
            Assert.That(_config.DefaultDropChance, Is.EqualTo(0.25f));
            Assert.That(_config.ClawSlashCooldown, Is.EqualTo(1.5f));
            Assert.That(_config.ClawSlashDamage, Is.EqualTo(3.0f));
            Assert.That(_config.ClawSlashRange, Is.EqualTo(110.0f));
            Assert.That(_config.HeavyCleaveCooldown, Is.EqualTo(4.0f));
            Assert.That(_config.HeavyCleaveDamage, Is.EqualTo(8.0f));
            Assert.That(_config.HeavyCleaveRange, Is.EqualTo(130.0f));
            Assert.That(_config.ShieldEnemyPhysArmorRatio, Is.EqualTo(0.75f));
            Assert.That(_config.ShieldEnemyMagicDamageMultiplier, Is.EqualTo(2.0f));
        }

        [Test]
        public void Verify_EconomyAndUI_DefaultsAreValidAndNonZero()
        {
            Assert.That(_config.VesselReclaimRadius, Is.EqualTo(60.0f));
            Assert.That(_config.DefaultBaseMaxPleasure, Is.EqualTo(100.0f));
            Assert.That(_config.DefaultPleasureRateMultiplier, Is.EqualTo(0.5f));
            Assert.That(_config.CellSize, Is.EqualTo(64.0f));
            Assert.That(_config.ShopRerollCost, Is.EqualTo(2));
            Assert.That(_config.StageClearGold, Is.EqualTo(10));
            Assert.That(_config.InitialGold, Is.EqualTo(15));
        }

        [Test]
        public void Verify_PropertyAliases_MutateUnderlyingFields()
        {
            _config.BaseGravity = 1200.0f;
            Assert.That(_config.Gravity, Is.EqualTo(1200.0f));

            _config.BaseJumpVelocity = -700.0f;
            Assert.That(_config.JumpVelocity, Is.EqualTo(-700.0f));

            _config.ParryWindowDuration = 0.35f;
            Assert.That(_config.ParryWindow, Is.EqualTo(0.35f));

            _config.ThrustKnockbackForce = 500.0f;
            Assert.That(_config.HeavyThrustKnockbackForce, Is.EqualTo(500.0f));

            _config.ThrustPleasureBonus = 25.0f;
            Assert.That(_config.ThrustPleasureGain, Is.EqualTo(25.0f));

            _config.HeavyThrustPleasureBoost = 30.0f;
            Assert.That(_config.ThrustPleasureGain, Is.EqualTo(30.0f));

            _config.HeavyThrustCooldown = 2.0f;
            Assert.That(_config.ThrustCooldown, Is.EqualTo(2.0f));

            _config.EnemyBaseMaxHp = 50.0f;
            Assert.That(_config.DefaultEnemyMaxHp, Is.EqualTo(50.0f));

            _config.EnemyBaseSpeed = 100.0f;
            Assert.That(_config.DefaultEnemySpeed, Is.EqualTo(100.0f));
        }
    }
}
