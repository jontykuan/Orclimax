using Godot;
using System;
using System.Collections.Generic;
using Orclimax.Core;

namespace Orclimax.Autoload
{
    public partial class CombatManager : Node
    {
        public static CombatManager Instance { get; private set; }

        [Signal] public delegate void HpChangedEventHandler(float currentHp, float maxHp);
        [Signal] public delegate void PleasureChangedEventHandler(float currentPleasure, float maxPleasure);
        [Signal] public delegate void ClimaxTriggeredEventHandler(string femaleId, string skillName);
        [Signal] public delegate void WeaponFiredEventHandler(string itemId, float damage);

        // Player Stats
        public float MaxHp { get; private set; } = 100f;
        public float CurrentHp { get; private set; } = 100f;
        public float MoveSpeed { get; private set; } = 250f;
        public float AttackSpeed { get; private set; } = 1.0f; // Multiplier: higher is faster
        public float Armor { get; private set; } = 0f;

        // Pleasure Stats (linked to mounted female)
        public float MaxPleasure { get; private set; } = 100f;
        public float CurrentPleasure { get; private set; } = 0f;
        public float PleasureAccumulationRate { get; private set; } = 1.0f;

        // List of weapons active in combat, mapping Item Instance ID -> Cooldown Timer
        private Dictionary<string, float> _weaponTimers = new Dictionary<string, float>();
        private List<GridItemInstance> _activeWeapons = new List<GridItemInstance>();

        // Buff / Debuff Timers
        private float _itemSpeedBuffTimer = 0f;
        private float _itemSpeedBuffMultiplier = 1.0f;

        [Export]
        public float TempAttackSpeedMultiplierTimer
        {
            get => _itemSpeedBuffTimer;
            set => _itemSpeedBuffTimer = value;
        }

        [Export] public int BurnStacks { get; set; } = 0;
        [Export] public float BurnTimer { get; set; } = 0f;
        private float _burnTickTimer = 0f;

        public override void _EnterTree()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                QueueFree();
            }
        }

        /// <summary>
        /// Call this when entering a combat wave to aggregate stats from the backpack and reset status.
        /// </summary>
        public void StartCombat()
        {
            // 1. Reset health and pleasure
            CurrentHp = MaxHp;
            CurrentPleasure = 0f;

            // 2. Fetch base stats
            float baseMaxHp = 100f;
            float baseMoveSpeed = GameConfig.Instance != null ? GameConfig.Instance.BaseMoveSpeed : 250.0f;
            float baseAttackSpeed = 1.5f;
            float baseArmor = 0f;
            float basePleasureRate = GameConfig.Instance != null ? GameConfig.Instance.DefaultPleasureRateMultiplier : 0.5f;

            if (InventoryManager.Instance.CurrentVessel != null)
            {
                MaxPleasure = InventoryManager.Instance.CurrentVessel.BaseMaxPleasure;
                basePleasureRate = InventoryManager.Instance.CurrentVessel.PleasureBuildRateMultiplier;
            }
            else
            {
                MaxPleasure = GameConfig.Instance != null ? GameConfig.Instance.DefaultBaseMaxPleasure : 100f;
            }

            // 3. Scan equipped items in the backpack grid and sum up bonuses
            _activeWeapons.Clear();
            _weaponTimers.Clear();

            var placedItems = InventoryManager.Instance.BackpackGrid.PlacedItems.Values;
            foreach (var inst in placedItems)
            {
                ItemData item = inst.Item;
                float ratio = inst.GetActiveRatio(InventoryManager.Instance.BackpackGrid);
                
                // Add stat modifiers scaled by active ratio
                baseMaxHp += item.ArmorBonus * ratio * 5f; // Extra health from armor items
                baseMoveSpeed += item.SpeedBonus * ratio;
                baseAttackSpeed += item.AttackSpeedBonus * ratio;
                baseArmor += item.ArmorBonus * ratio;
                basePleasureRate += item.PleasureGain * ratio;

                // If it is a weapon, register it for auto-combat trigger
                if (item.Category == ItemCategory.Weapon)
                {
                    _activeWeapons.Add(inst);
                    _weaponTimers[inst.InstanceId] = 0f; // Start ready or on CD
                }
            }

            MaxHp = baseMaxHp;
            CurrentHp = MaxHp;
            MoveSpeed = baseMoveSpeed;
            AttackSpeed = baseAttackSpeed;
            Armor = baseArmor;
            PleasureAccumulationRate = basePleasureRate;

            EmitSignal(SignalName.HpChanged, CurrentHp, MaxHp);
            EmitSignal(SignalName.PleasureChanged, CurrentPleasure, MaxPleasure);
        }

        public Godot.Collections.Dictionary GetCurrentStats()
        {
            float baseMaxHp = 100f;
            float baseMoveSpeed = GameConfig.Instance != null ? GameConfig.Instance.BaseMoveSpeed : 250.0f;
            float baseAttackSpeed = 1.5f;
            float baseArmor = 0f;
            float basePleasureRate = GameConfig.Instance != null ? GameConfig.Instance.DefaultPleasureRateMultiplier : 0.5f;
            float maxPleasure = GameConfig.Instance != null ? GameConfig.Instance.DefaultBaseMaxPleasure : 100f;

            if (InventoryManager.Instance.CurrentVessel != null)
            {
                maxPleasure = InventoryManager.Instance.CurrentVessel.BaseMaxPleasure;
                basePleasureRate = InventoryManager.Instance.CurrentVessel.PleasureBuildRateMultiplier;
            }

            var placedItems = InventoryManager.Instance.BackpackGrid.PlacedItems.Values;
            foreach (var inst in placedItems)
            {
                ItemData item = inst.Item;
                float ratio = inst.GetActiveRatio(InventoryManager.Instance.BackpackGrid);
                
                baseMaxHp += item.ArmorBonus * ratio * 5f;
                baseMoveSpeed += item.SpeedBonus * ratio;
                baseAttackSpeed += item.AttackSpeedBonus * ratio;
                baseArmor += item.ArmorBonus * ratio;
                basePleasureRate += item.PleasureGain * ratio;
            }

            return new Godot.Collections.Dictionary
            {
                { "MaxHp", baseMaxHp },
                { "MoveSpeed", baseMoveSpeed },
                { "AttackSpeed", baseAttackSpeed },
                { "Armor", baseArmor },
                { "PleasureRate", basePleasureRate },
                { "MaxPleasure", maxPleasure }
            };
        }

        public override void _Process(double delta)
        {
            UpdateCombat(delta);
        }

        public void UpdateCombat(double delta)
        {
            if (GameManager.Instance.CurrentState != GameState.Combat) return;

            // Handle Maye Climax speed buff timer
            if (_itemSpeedBuffTimer > 0f)
            {
                _itemSpeedBuffTimer -= (float)delta;
                if (_itemSpeedBuffTimer <= 0)
                {
                    _itemSpeedBuffTimer = 0f;
                    _itemSpeedBuffMultiplier = 1.0f;
                }
            }

            // Update Burn Debuff Tick
            if (BurnTimer > 0)
            {
                BurnTimer -= (float)delta;
                _burnTickTimer += (float)delta;
                if (_burnTickTimer >= 1.0f)
                {
                    _burnTickTimer = 0f;
                    TakeDamage(1.5f * BurnStacks);
                }
                if (BurnTimer <= 0)
                {
                    BurnTimer = 0f;
                    BurnStacks = 0;
                    _burnTickTimer = 0f;
                }
            }

            // 1. Update active weapon cooldown timers
            var keys = new List<string>(_weaponTimers.Keys);
            foreach (var id in keys)
            {
                var inst = _activeWeapons.Find(w => w.InstanceId == id);
                if (inst == null) continue;

                float ratio = Math.Max(0.05f, inst.GetActiveRatio(InventoryManager.Instance.BackpackGrid));
                float starMult = inst.GetStarSynergyMultiplier(InventoryManager.Instance.BackpackGrid);
                float effectiveCooldown = Math.Max(0.15f, inst.Item.Cooldown / (AttackSpeed * ratio * starMult));

                float timer = _weaponTimers[id] + (float)delta * _itemSpeedBuffMultiplier;
                if (timer >= effectiveCooldown)
                {
                    FireWeapon(inst);
                    _weaponTimers[id] = 0f;
                }
                else
                {
                    _weaponTimers[id] = timer;
                }
            }

            // 2. Automatically generate passive pleasure (e.g. from toys or passive rate)
            float passivePleasure = PleasureAccumulationRate * (float)delta;
            AddPleasure(passivePleasure);
        }

        private void FireWeapon(GridItemInstance inst)
        {
            ItemData weapon = inst.Item;
            float ratio = Math.Max(0.05f, inst.GetActiveRatio(InventoryManager.Instance.BackpackGrid));
            float starMult = inst.GetStarSynergyMultiplier(InventoryManager.Instance.BackpackGrid);
            float finalDamage = weapon.Damage * ratio * starMult;
            EmitSignal(SignalName.WeaponFired, weapon.Id, finalDamage);

            // Attacking also performs lower body action, which increases pleasure!
            float baseThrust = GameConfig.Instance != null ? GameConfig.Instance.ThrustPleasureGain : 2.0f;
            float thrustPleasure = baseThrust * PleasureAccumulationRate;
            AddPleasure(thrustPleasure);
        }

        public void ApplyBurn(int stacks = 1, float duration = 4.0f)
        {
            BurnStacks = Math.Min(5, BurnStacks + stacks);
            BurnTimer = Math.Max(BurnTimer, duration);
        }

        public void TakeDamage(float amount)
        {
            if (GameManager.Instance.CurrentState != GameState.Combat) return;

            // Reduce damage by armor
            float finalDamage = Math.Max(1f, amount - Armor);
            CurrentHp -= finalDamage;

            if (CurrentHp <= 0)
            {
                CurrentHp = 0;
                GameManager.Instance.TriggerGameOver();
            }

            EmitSignal(SignalName.HpChanged, CurrentHp, MaxHp);
        }

        public void Heal(float amount)
        {
            CurrentHp = Math.Min(MaxHp, CurrentHp + amount);
            EmitSignal(SignalName.HpChanged, CurrentHp, MaxHp);
        }

        public void AddPleasure(float amount)
        {
            if (GameManager.Instance.CurrentState != GameState.Combat) return;

            CurrentPleasure += amount;
            if (CurrentPleasure >= MaxPleasure)
            {
                TriggerClimax();
            }
            else
            {
                EmitSignal(SignalName.PleasureChanged, CurrentPleasure, MaxPleasure);
            }
        }

        public void TriggerClimax()
        {
            CurrentPleasure = 0f;
            EmitSignal(SignalName.PleasureChanged, CurrentPleasure, MaxPleasure);

            string femaleId = "None";
            string skillName = "Magic Spillover";
            if (InventoryManager.Instance.CurrentVessel != null)
            {
                femaleId = InventoryManager.Instance.CurrentVessel.Id;
                skillName = InventoryManager.Instance.CurrentVessel.ClimaxSkillName;
            }

            // Human Maiden Maye Climax Skill: Grants 120% item trigger speed for 0.8 seconds
            if (femaleId == "girl_maye")
            {
                _itemSpeedBuffTimer = 0.8f;
                _itemSpeedBuffMultiplier = 1.2f;
            }

            EmitSignal(SignalName.ClimaxTriggered, femaleId, skillName);
        }
    }
}
