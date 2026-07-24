using Godot;
using System;

namespace Orclimax.Autoload
{
    public partial class GameConfig : Node
    {
        public static GameConfig Instance { get; set; }

        public GameConfig()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        // --- Physics & Player Movement ---
        [Export] public float Gravity { get; set; } = 980.0f;
        [Export] public float BaseGravity
        {
            get => Gravity;
            set => Gravity = value;
        }

        [Export] public float JumpVelocity { get; set; } = -550.0f;
        [Export] public float BaseJumpVelocity
        {
            get => JumpVelocity;
            set => JumpVelocity = value;
        }

        [Export] public float BaseMoveSpeed { get; set; } = 250.0f;
        [Export] public float DashSpeedMultiplier { get; set; } = 2.8f;
        [Export] public float DashDuration { get; set; } = 0.15f;
        [Export] public float DashIFrameDuration { get; set; } = 0.18f;
        [Export] public float DashCooldown { get; set; } = 0.6f;
        [Export] public float DoubleTapDelay { get; set; } = 0.25f;
        [Export] public float CrouchSpeedMultiplier { get; set; } = 0.4f;
        [Export] public float CrouchHeightRatio { get; set; } = 0.6f;
        [Export] public float VisualBaseScale { get; set; } = 4.0f;

        // --- Combat Action Windows & Timings ---
        [Export] public float IFrameDuration { get; set; } = 0.3f;
        [Export] public float ParryWindow { get; set; } = 0.2f;
        [Export] public float ParryWindowDuration
        {
            get => ParryWindow;
            set => ParryWindow = value;
        }
        [Export] public float ParryCounterDamage { get; set; } = 15.0f;
        [Export] public float ParryReflectSpeed { get; set; } = 700.0f;
        [Export] public float ParryCooldown { get; set; } = 1.0f;

        // --- Heavy Thrust & AOE Knockback ---
        [Export] public Vector2 KnockbackForce { get; set; } = new Vector2(300.0f, -150.0f);
        [Export] public float HeavyThrustKnockbackForce { get; set; } = 450.0f;
        [Export] public float ThrustKnockbackForce
        {
            get => HeavyThrustKnockbackForce;
            set => HeavyThrustKnockbackForce = value;
        }
        [Export] public float ThrustKnockbackRadius { get; set; } = 140.0f;

        [Export] public float ThrustPleasureGain { get; set; } = 15.0f;
        [Export] public float ThrustPleasureBonus
        {
            get => ThrustPleasureGain;
            set => ThrustPleasureGain = value;
        }
        [Export] public float HeavyThrustPleasureBoost
        {
            get => ThrustPleasureGain;
            set => ThrustPleasureGain = value;
        }

        [Export] public float ThrustCooldown { get; set; } = 1.2f;
        [Export] public float HeavyThrustCooldown
        {
            get => ThrustCooldown;
            set => ThrustCooldown = value;
        }

        [Export] public float ClimaxBlastDamage { get; set; } = 50.0f;

        // --- Enemy Base Parameters ---
        [Export] public float DefaultEnemyMaxHp { get; set; } = 30.0f;
        [Export] public float EnemyBaseMaxHp
        {
            get => DefaultEnemyMaxHp;
            set => DefaultEnemyMaxHp = value;
        }

        [Export] public float DefaultEnemySpeed { get; set; } = 80.0f;
        [Export] public float EnemyBaseSpeed
        {
            get => DefaultEnemySpeed;
            set => DefaultEnemySpeed = value;
        }

        [Export] public float DefaultEnemyGravity { get; set; } = 980.0f;
        [Export] public int DefaultGoldReward { get; set; } = 2;
        [Export] public float DefaultDropChance { get; set; } = 0.25f;

        [Export] public float ClawSlashCooldown { get; set; } = 1.5f;
        [Export] public float ClawSlashDamage { get; set; } = 3.0f;
        [Export] public float ClawSlashRange { get; set; } = 110.0f;

        [Export] public float HeavyCleaveCooldown { get; set; } = 4.0f;
        [Export] public float HeavyCleaveDamage { get; set; } = 8.0f;
        [Export] public float HeavyCleaveRange { get; set; } = 130.0f;

        [Export] public float ShieldEnemyPhysArmorRatio { get; set; } = 0.75f;
        [Export] public float ShieldEnemyMagicDamageMultiplier { get; set; } = 2.0f;

        // --- Vessel & Pleasure Mechanics ---
        [Export] public float VesselReclaimRadius { get; set; } = 60.0f;
        [Export] public float DefaultBaseMaxPleasure { get; set; } = 100.0f;
        [Export] public float DefaultPleasureRateMultiplier { get; set; } = 0.5f;

        // --- Economy & UI Multipliers ---
        [Export] public float CellSize { get; set; } = 64.0f;
        [Export] public int ShopRerollCost { get; set; } = 2;
        [Export] public int StageClearGold { get; set; } = 10;
        [Export] public int InitialGold { get; set; } = 15;

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
