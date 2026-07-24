using Godot;
using System;

namespace Orclimax.Autoload
{
    [GlobalClass]
    public partial class GameConfig : Node
    {
        public static GameConfig Instance { get; set; }

        // --- Static Backing Fields ---
        private static float _gravity = 980.0f;
        private static float _jumpVelocity = -550.0f;
        private static float _baseMoveSpeed = 250.0f;
        private static float _dashSpeedMultiplier = 2.8f;
        private static float _dashDuration = 0.15f;
        private static float _dashIFrameDuration = 0.18f;
        private static float _dashCooldown = 0.6f;
        private static float _doubleTapDelay = 0.25f;
        private static float _crouchSpeedMultiplier = 0.4f;
        private static float _crouchHeightRatio = 0.6f;
        private static float _visualBaseScale = 4.0f;

        private static float _iFrameDuration = 0.3f;
        private static float _parryWindow = 0.22f;
        private static float _parryCounterDamage = 15.0f;
        private static float _parryReflectSpeed = 700.0f;
        private static float _parryCooldown = 1.0f;

        private static Vector2 _knockbackForce = new Vector2(300.0f, -150.0f);
        private static float _heavyThrustKnockbackForce = 450.0f;
        private static float _thrustKnockbackRadius = 140.0f;
        private static float _thrustPleasureGain = 15.0f;
        private static float _thrustCooldown = 1.2f;
        private static float _climaxBlastDamage = 50.0f;

        private static float _defaultEnemyMaxHp = 30.0f;
        private static float _defaultEnemySpeed = 80.0f;
        private static float _defaultEnemyGravity = 980.0f;
        private static int _defaultGoldReward = 2;
        private static float _defaultDropChance = 0.25f;

        private static float _clawSlashCooldown = 1.5f;
        private static float _clawSlashDamage = 1.5f;
        private static float _clawSlashRange = 110.0f;

        private static float _heavyCleaveCooldown = 4.0f;
        private static float _heavyCleaveDamage = 3.5f;
        private static float _heavyCleaveRange = 130.0f;

        private static float _shieldEnemyPhysArmorRatio = 0.75f;
        private static float _shieldEnemyMagicDamageMultiplier = 2.0f;

        private static float _vesselReclaimRadius = 60.0f;
        private static float _defaultBaseMaxPleasure = 100.0f;
        private static float _defaultPleasureRateMultiplier = 0.5f;

        private static float _cellSize = 64.0f;
        private static int _shopRerollCost = 2;
        private static int _stageClearGold = 10;
        private static int _initialGold = 15;

        // Instance Export Properties (accessed via Instance or GDScript Autoload node)
        [Export] public float Gravity { get => _gravity; set => _gravity = value; }
        [Export] public float BaseGravity { get => _gravity; set => _gravity = value; }
        [Export] public float JumpVelocity { get => _jumpVelocity; set => _jumpVelocity = value; }
        [Export] public float BaseJumpVelocity { get => _jumpVelocity; set => _jumpVelocity = value; }
        [Export] public float BaseMoveSpeed { get => _baseMoveSpeed; set => _baseMoveSpeed = value; }
        [Export] public float DashSpeedMultiplier { get => _dashSpeedMultiplier; set => _dashSpeedMultiplier = value; }
        [Export] public float DashDuration { get => _dashDuration; set => _dashDuration = value; }
        [Export] public float DashIFrameDuration { get => _dashIFrameDuration; set => _dashIFrameDuration = value; }
        [Export] public float DashCooldown { get => _dashCooldown; set => _dashCooldown = value; }
        [Export] public float DoubleTapDelay { get => _doubleTapDelay; set => _doubleTapDelay = value; }
        [Export] public float CrouchSpeedMultiplier { get => _crouchSpeedMultiplier; set => _crouchSpeedMultiplier = value; }
        [Export] public float CrouchHeightRatio { get => _crouchHeightRatio; set => _crouchHeightRatio = value; }
        [Export] public float VisualBaseScale { get => _visualBaseScale; set => _visualBaseScale = value; }

        [Export] public float IFrameDuration { get => _iFrameDuration; set => _iFrameDuration = value; }
        [Export] public float ParryWindowDuration { get => _parryWindow; set => _parryWindow = value; }
        [Export] public float ParryCounterDamage { get => _parryCounterDamage; set => _parryCounterDamage = value; }
        [Export] public float ParryReflectSpeed { get => _parryReflectSpeed; set => _parryReflectSpeed = value; }
        [Export] public float ParryCooldown { get => _parryCooldown; set => _parryCooldown = value; }

        [Export] public Vector2 KnockbackForce { get => _knockbackForce; set => _knockbackForce = value; }
        [Export] public float ThrustKnockbackForce { get => _heavyThrustKnockbackForce; set => _heavyThrustKnockbackForce = value; }
        [Export] public float ThrustKnockbackRadius { get => _thrustKnockbackRadius; set => _thrustKnockbackRadius = value; }
        [Export] public float ThrustPleasureBonus { get => _thrustPleasureGain; set => _thrustPleasureGain = value; }
        [Export] public float ThrustPleasureGain { get => _thrustPleasureGain; set => _thrustPleasureGain = value; }
        [Export] public float ThrustCooldown { get => _thrustCooldown; set => _thrustCooldown = value; }
        [Export] public float ClimaxBlastDamage { get => _climaxBlastDamage; set => _climaxBlastDamage = value; }

        [Export] public float DefaultEnemyMaxHp { get => _defaultEnemyMaxHp; set => _defaultEnemyMaxHp = value; }
        [Export] public float EnemyBaseMaxHp { get => _defaultEnemyMaxHp; set => _defaultEnemyMaxHp = value; }
        [Export] public float DefaultEnemySpeed { get => _defaultEnemySpeed; set => _defaultEnemySpeed = value; }
        [Export] public float EnemyBaseSpeed { get => _defaultEnemySpeed; set => _defaultEnemySpeed = value; }
        [Export] public float DefaultEnemyGravity { get => _defaultEnemyGravity; set => _defaultEnemyGravity = value; }
        [Export] public int DefaultGoldReward { get => _defaultGoldReward; set => _defaultGoldReward = value; }
        [Export] public float DefaultDropChance { get => _defaultDropChance; set => _defaultDropChance = value; }

        [Export] public float ClawSlashCooldown { get => _clawSlashCooldown; set => _clawSlashCooldown = value; }
        [Export] public float ClawSlashDamage { get => _clawSlashDamage; set => _clawSlashDamage = value; }
        [Export] public float ClawSlashRange { get => _clawSlashRange; set => _clawSlashRange = value; }

        [Export] public float HeavyCleaveCooldown { get => _heavyCleaveCooldown; set => _heavyCleaveCooldown = value; }
        [Export] public float HeavyCleaveDamage { get => _heavyCleaveDamage; set => _heavyCleaveDamage = value; }
        [Export] public float HeavyCleaveRange { get => _heavyCleaveRange; set => _heavyCleaveRange = value; }

        [Export] public float ShieldEnemyPhysArmorRatio { get => _shieldEnemyPhysArmorRatio; set => _shieldEnemyPhysArmorRatio = value; }
        [Export] public float ShieldEnemyMagicDamageMultiplier { get => _shieldEnemyMagicDamageMultiplier; set => _shieldEnemyMagicDamageMultiplier = value; }

        [Export] public float VesselReclaimRadius { get => _vesselReclaimRadius; set => _vesselReclaimRadius = value; }
        [Export] public float DefaultBaseMaxPleasure { get => _defaultBaseMaxPleasure; set => _defaultBaseMaxPleasure = value; }
        [Export] public float DefaultPleasureRateMultiplier { get => _defaultPleasureRateMultiplier; set => _defaultPleasureRateMultiplier = value; }

        [Export] public float CellSize { get => _cellSize; set => _cellSize = value; }
        [Export] public int ShopRerollCost { get => _shopRerollCost; set => _shopRerollCost = value; }
        [Export] public int StageClearGold { get => _stageClearGold; set => _stageClearGold = value; }
        [Export] public int InitialGold { get => _initialGold; set => _initialGold = value; }

        public override void _EnterTree()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                QueueFree();
            }
        }
    }
}
