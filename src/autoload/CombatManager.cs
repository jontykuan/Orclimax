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
            float baseMoveSpeed = 250f;
            float baseAttackSpeed = 1.0f;
            float baseArmor = 0f;
            float basePleasureRate = 1.0f;

            if (InventoryManager.Instance.CurrentFemale != null)
            {
                MaxPleasure = InventoryManager.Instance.CurrentFemale.BaseMaxPleasure;
                basePleasureRate = InventoryManager.Instance.CurrentFemale.PleasureBuildRateMultiplier;
            }
            else
            {
                MaxPleasure = 100f;
            }

            // 3. Scan equipped items in the backpack grid and sum up bonuses
            _activeWeapons.Clear();
            _weaponTimers.Clear();

            var placedItems = InventoryManager.Instance.BackpackGrid.PlacedItems.Values;
            foreach (var inst in placedItems)
            {
                ItemData item = inst.Item;
                
                // Add stat modifiers
                baseMaxHp += item.ArmorBonus * 5f; // Extra health from armor items
                baseMoveSpeed += item.SpeedBonus;
                baseAttackSpeed += item.AttackSpeedBonus;
                baseArmor += item.ArmorBonus;
                basePleasureRate += item.PleasureGain;

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

        public override void _Process(double delta)
        {
            if (GameManager.Instance.CurrentState != GameState.Combat) return;

            // 1. Automatically tick weapon cooldowns
            foreach (var inst in _activeWeapons)
            {
                string id = inst.InstanceId;
                ItemData weapon = inst.Item;

                if (_weaponTimers.TryGetValue(id, out float timer))
                {
                    timer += (float)delta;
                    
                    // Cooldown is reduced by AttackSpeed multiplier
                    float actualCooldown = Math.Max(0.1f, weapon.Cooldown / AttackSpeed);

                    if (timer >= actualCooldown)
                    {
                        // Fire weapon!
                        FireWeapon(inst);
                        timer = 0f; // Reset
                    }
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
            EmitSignal(SignalName.WeaponFired, weapon.Id, weapon.Damage);

            // Attacking also performs lower body action, which increases pleasure!
            // Let's add pleasure proportional to weapon damage/hits
            float thrustPleasure = 2.0f * PleasureAccumulationRate;
            AddPleasure(thrustPleasure);
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
            if (InventoryManager.Instance.CurrentFemale != null)
            {
                femaleId = InventoryManager.Instance.CurrentFemale.Id;
                skillName = InventoryManager.Instance.CurrentFemale.ClimaxSkillName;
            }

            EmitSignal(SignalName.ClimaxTriggered, femaleId, skillName);
        }
    }
}
