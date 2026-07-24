using Godot;
using System;
using System.Collections.Generic;

namespace Orclimax.Core
{
    [GlobalClass]
    public partial class ItemData : Resource
    {
        [Export] public string Id { get; set; } = "";
        [Export] public string ItemName { get; set; } = "";
        [Export] public ItemCategory Category { get; set; } = ItemCategory.Accessory;
        [Export] public PlacementZone RequiredZone { get; set; } = PlacementZone.General;
        [Export] public bool IsLocked { get; set; } = false;

        // Visual properties
        [Export] public Texture2D Icon { get; set; }
        
        // Grid properties: default shape offsets relative to the anchor cell (0, 0)
        // For example:
        // 1x1: [(0,0)]
        // 1x2: [(0,0), (0,1)]
        // L-shape: [(0,0), (0,1), (1,1)]
        [Export] public Godot.Collections.Array<Vector2I> ShapeOffsets { get; set; } = new Godot.Collections.Array<Vector2I> { new Vector2I(0, 0) };

        // Combat Stats
        [Export] public float Damage { get; set; } = 0f;
        [Export] public float Cooldown { get; set; } = 1.0f; // Attack interval in seconds
        [Export] public float PleasureGain { get; set; } = 0f; // Extra pleasure generated per second / action
        [Export] public float ArmorBonus { get; set; } = 0f;
        [Export] public float SpeedBonus { get; set; } = 0f;
        [Export] public float AttackSpeedBonus { get; set; } = 0f;

        // Shop Properties
        [Export] public int BasePrice { get; set; } = 10;

        // Backpack Battles Style Star/Synergy Link properties
        [Export] public Godot.Collections.Array<Vector2I> StarOffsets { get; set; } = new Godot.Collections.Array<Vector2I>();
        [Export] public string StarSynergyType { get; set; } = "Cooldown"; // "Cooldown", "Damage", "Armor", "Pleasure"
        [Export] public float StarSynergyBonusPerLink { get; set; } = 0.15f; // 15% bonus per linked item

        // Synergy description
        [Export] public string SynergyDescription { get; set; } = "";

        public Godot.Collections.Array<Vector2I> GetRotatedStarOffsets(int rotationSteps)
        {
            int steps = (rotationSteps % 4 + 4) % 4;
            var rotated = new Godot.Collections.Array<Vector2I>();
            foreach (var offset in StarOffsets)
            {
                int x = offset.X;
                int y = offset.Y;
                for (int s = 0; s < steps; s++)
                {
                    int temp = x;
                    x = -y;
                    y = temp;
                }
                rotated.Add(new Vector2I(x, y));
            }
            return rotated;
        }

        public Godot.Collections.Array<Vector2I> GetRotatedOffsets(int rotationSteps)
        {
            int steps = (rotationSteps % 4 + 4) % 4;
            var rotated = new Godot.Collections.Array<Vector2I>();
            foreach (var offset in ShapeOffsets)
            {
                int x = offset.X;
                int y = offset.Y;
                for (int s = 0; s < steps; s++)
                {
                    // Clockwise rotation: (x, y) -> (-y, x)
                    int temp = x;
                    x = -y;
                    y = temp;
                }
                rotated.Add(new Vector2I(x, y));
            }
            return rotated;
        }
    }
}
