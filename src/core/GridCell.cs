using Godot;
using System;

namespace Orclimax.Core
{
    public partial class GridCell : RefCounted
    {
        public Vector2I Coords { get; set; }
        public PlacementZone Zone { get; set; } = PlacementZone.General;
        public bool IsLocked { get; set; } = false;
        public string OccupyingItemInstanceId { get; set; } = null; // null if empty

        public GridCell() {}

        public GridCell(Vector2I coords, PlacementZone zone, bool isLocked = false)
        {
            Coords = coords;
            Zone = zone;
            IsLocked = isLocked;
        }
    }
}
