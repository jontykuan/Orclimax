using Godot;
using System;
using System.Collections.Generic;

namespace Orclimax.Core
{
    [GlobalClass]
    public partial class VesselData : Resource
    {
        [Export] public string Id { get; set; } = "";
        [Export] public string CharacterName { get; set; } = "";

        // UI & H-assets
        [Export] public Texture2D BustIcon { get; set; } // For bottom-right PiP
        [Export] public Texture2D ClimaxCg { get; set; } // Background H-CG for climax

        // Climax magic skill details
        [Export] public string ClimaxSkillName { get; set; } = "Magic Overflow";
        [Export] public string ClimaxSkillDescription { get; set; } = "";

        // Stats
        [Export] public float BaseMaxPleasure { get; set; } = 100f;
        [Export] public float PleasureBuildRateMultiplier { get; set; } = 0.5f;

        // Sensitivities & Development
        [Export] public float HeadSensitivity { get; set; } = 1.0f;
        [Export] public float ChestSensitivity { get; set; } = 1.2f;
        [Export] public float GroinSensitivity { get; set; } = 1.5f;
        [Export] public float LimbsSensitivity { get; set; } = 0.8f;

        [Export] public int HeadDevLevel { get; set; } = 1;
        [Export] public int ChestDevLevel { get; set; } = 1;
        [Export] public int GroinDevLevel { get; set; } = 1;
        [Export] public int LimbsDevLevel { get; set; } = 1;

        // Traits
        [Export] public Godot.Collections.Array<string> Traits { get; set; } = new Godot.Collections.Array<string>();
        [Export] public string TraitsDescription { get; set; } = "";

        // Grid Definition
        // Arrays of coordinates defining which cells belong to which zone
        [Export] public Godot.Collections.Array<Vector2I> HeadCells { get; set; } = new Godot.Collections.Array<Vector2I>();
        [Export] public Godot.Collections.Array<Vector2I> ChestCells { get; set; } = new Godot.Collections.Array<Vector2I>();
        [Export] public Godot.Collections.Array<Vector2I> GroinCells { get; set; } = new Godot.Collections.Array<Vector2I>();
        [Export] public Godot.Collections.Array<Vector2I> LimbsCells { get; set; } = new Godot.Collections.Array<Vector2I>();
        [Export] public Godot.Collections.Array<Vector2I> GeneralCells { get; set; } = new Godot.Collections.Array<Vector2I>();
        [Export] public Godot.Collections.Array<Vector2I> InactiveCells { get; set; } = new Godot.Collections.Array<Vector2I>();

        // Locked cells: list of coordinates from any of the zones above that start out locked
        [Export] public Godot.Collections.Array<Vector2I> InitiallyLockedCells { get; set; } = new Godot.Collections.Array<Vector2I>();

        /// <summary>
        /// Populates a GridData instance based on this vessel character's grid definition.
        /// </summary>
        public void InitializeGrid(GridData grid)
        {
            grid.ClearGrid();

            HashSet<Vector2I> lockedSet = new HashSet<Vector2I>(InitiallyLockedCells);

            foreach (var coord in HeadCells)
                grid.AddCell(coord, PlacementZone.Head, lockedSet.Contains(coord));

            foreach (var coord in ChestCells)
                grid.AddCell(coord, PlacementZone.Chest, lockedSet.Contains(coord));

            foreach (var coord in GroinCells)
                grid.AddCell(coord, PlacementZone.Groin, lockedSet.Contains(coord));

            foreach (var coord in LimbsCells)
                grid.AddCell(coord, PlacementZone.Limbs, lockedSet.Contains(coord));

            foreach (var coord in GeneralCells)
                grid.AddCell(coord, PlacementZone.General, lockedSet.Contains(coord));

            foreach (var coord in InactiveCells)
                grid.AddCell(coord, PlacementZone.Inactive, lockedSet.Contains(coord));
        }
    }
}
