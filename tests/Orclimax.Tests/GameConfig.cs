namespace Orclimax.Tests
{
    public class GameConfigProxy
    {
        public float BaseGravity { get; set; } = 980.0f;
        public float BaseJumpVelocity { get; set; } = -550.0f;
        public float BaseMoveSpeed { get; set; } = 250.0f;
        public float DashSpeedMultiplier { get; set; } = 2.8f;
        public float DashDuration { get; set; } = 0.15f;
        public float DashIFrameDuration { get; set; } = 0.18f;
        public float DashCooldown { get; set; } = 0.6f;
        public float DoubleTapDelay { get; set; } = 0.25f;
        public float CrouchSpeedMultiplier { get; set; } = 0.4f;
        public float CrouchHeightRatio { get; set; } = 0.6f;
        public float VisualBaseScale { get; set; } = 4.0f;
        public float IFrameDuration { get; set; } = 0.3f;
        public float ParryWindowDuration { get; set; } = 0.22f;
        public float ParryCounterDamage { get; set; } = 15.0f;
        public float ParryReflectSpeed { get; set; } = 700.0f;
        public float ParryCooldown { get; set; } = 1.0f;
        public float ThrustKnockbackRadius { get; set; } = 140.0f;
        public float ThrustKnockbackForce { get; set; } = 450.0f;
        public float ThrustPleasureBonus { get; set; } = 15.0f;
        public float ThrustCooldown { get; set; } = 1.2f;
        public float VesselReclaimRadius { get; set; } = 60.0f;
        public float DefaultEnemyMaxHp { get; set; } = 30.0f;
        public float DefaultEnemySpeed { get; set; } = 80.0f;
        public float ShieldEnemyPhysArmorRatio { get; set; } = 0.75f;
        public float ShieldEnemyMagicDamageMultiplier { get; set; } = 2.0f;
    }

    public static class GameConfig
    {
        public static GameConfigProxy Instance { get; set; } = new GameConfigProxy();

        public static float BaseGravity { get => Instance.BaseGravity; set => Instance.BaseGravity = value; }
        public static float BaseJumpVelocity { get => Instance.BaseJumpVelocity; set => Instance.BaseJumpVelocity = value; }
        public static float BaseMoveSpeed { get => Instance.BaseMoveSpeed; set => Instance.BaseMoveSpeed = value; }
        public static float DashSpeedMultiplier { get => Instance.DashSpeedMultiplier; set => Instance.DashSpeedMultiplier = value; }
        public static float DashDuration { get => Instance.DashDuration; set => Instance.DashDuration = value; }
        public static float DashIFrameDuration { get => Instance.DashIFrameDuration; set => Instance.DashIFrameDuration = value; }
        public static float DashCooldown { get => Instance.DashCooldown; set => Instance.DashCooldown = value; }
        public static float DoubleTapDelay { get => Instance.DoubleTapDelay; set => Instance.DoubleTapDelay = value; }
        public static float CrouchSpeedMultiplier { get => Instance.CrouchSpeedMultiplier; set => Instance.CrouchSpeedMultiplier = value; }
        public static float CrouchHeightRatio { get => Instance.CrouchHeightRatio; set => Instance.CrouchHeightRatio = value; }
        public static float VisualBaseScale { get => Instance.VisualBaseScale; set => Instance.VisualBaseScale = value; }
        public static float IFrameDuration { get => Instance.IFrameDuration; set => Instance.IFrameDuration = value; }
        public static float ParryWindowDuration { get => Instance.ParryWindowDuration; set => Instance.ParryWindowDuration = value; }
        public static float ParryCounterDamage { get => Instance.ParryCounterDamage; set => Instance.ParryCounterDamage = value; }
        public static float ParryReflectSpeed { get => Instance.ParryReflectSpeed; set => Instance.ParryReflectSpeed = value; }
        public static float ParryCooldown { get => Instance.ParryCooldown; set => Instance.ParryCooldown = value; }
        public static float ThrustKnockbackRadius { get => Instance.ThrustKnockbackRadius; set => Instance.ThrustKnockbackRadius = value; }
        public static float ThrustKnockbackForce { get => Instance.ThrustKnockbackForce; set => Instance.ThrustKnockbackForce = value; }
        public static float ThrustPleasureBonus { get => Instance.ThrustPleasureBonus; set => Instance.ThrustPleasureBonus = value; }
        public static float ThrustCooldown { get => Instance.ThrustCooldown; set => Instance.ThrustCooldown = value; }
        public static float VesselReclaimRadius { get => Instance.VesselReclaimRadius; set => Instance.VesselReclaimRadius = value; }
        public static float DefaultEnemyMaxHp { get => Instance.DefaultEnemyMaxHp; set => Instance.DefaultEnemyMaxHp = value; }
        public static float DefaultEnemySpeed { get => Instance.DefaultEnemySpeed; set => Instance.DefaultEnemySpeed = value; }
        public static float ShieldEnemyPhysArmorRatio { get => Instance.ShieldEnemyPhysArmorRatio; set => Instance.ShieldEnemyPhysArmorRatio = value; }
        public static float ShieldEnemyMagicDamageMultiplier { get => Instance.ShieldEnemyMagicDamageMultiplier; set => Instance.ShieldEnemyMagicDamageMultiplier = value; }
    }
}
