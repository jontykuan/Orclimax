using Godot;
using System;
using System.Collections.Generic;

namespace Orclimax.Core
{
    public partial class GridData : RefCounted
    {
        public Godot.Collections.Dictionary<Vector2I, GridCell> Cells { get; private set; } = new Godot.Collections.Dictionary<Vector2I, GridCell>();
        public Godot.Collections.Dictionary<string, GridItemInstance> PlacedItems { get; private set; } = new Godot.Collections.Dictionary<string, GridItemInstance>();

        public GridData() {}

        public void AddCell(Vector2I coords, PlacementZone zone, bool isLocked = false)
        {
            Cells[coords] = new GridCell(coords, zone, isLocked);
        }

        public void RemoveCell(Vector2I coords)
        {
            Cells.Remove(coords);
        }

        public void ClearGrid()
        {
            Cells.Clear();
            PlacedItems.Clear();
        }

        public Godot.Collections.Array<Vector2I> GetCellCoords()
        {
            var arr = new Godot.Collections.Array<Vector2I>();
            foreach (var k in Cells.Keys) arr.Add(k);
            return arr;
        }

        public Godot.Collections.Array<string> GetPlacedItemIds()
        {
            var arr = new Godot.Collections.Array<string>();
            foreach (var k in PlacedItems.Keys) arr.Add(k);
            return arr;
        }

        /// <summary>
        /// Validates if an item can be placed at the specified anchor with rotation.
        /// </summary>
        /// <param name="item">The item details.</param>
        /// <param name="anchor">The target anchor cell (0,0 of item shape).</param>
        /// <param name="rotationSteps">0, 1, 2, or 3</param>
        /// <param name="ignoreInstanceId">If specified, ignores cells occupied by this instance ID (used when moving an item).</param>
        /// <returns>True if placement is valid, false otherwise.</returns>
        public bool CanPlaceItem(ItemData item, Vector2I anchor, int rotationSteps)
        {
            return CanPlaceItem(item, anchor, rotationSteps, null);
        }

        public bool CanPlaceItem(ItemData item, Vector2I anchor, int rotationSteps, string ignoreInstanceId = null)
        {
            var rotatedOffsets = item.GetRotatedOffsets(rotationSteps);
            bool satisfiesZoneRequirement = item.RequiredZone == PlacementZone.General;

            foreach (var offset in rotatedOffsets)
            {
                Vector2I cellCoords = anchor + offset;

                // 1. Check if the cell exists in the grid
                if (!Cells.TryGetValue(cellCoords, out GridCell cell))
                {
                    return false; // Out of bounds
                }

                // 2. Check if the cell is locked
                if (cell.IsLocked)
                {
                    return false;
                }

                // 3. Check if cell is occupied by another item
                if (cell.OccupyingItemInstanceId != null && cell.OccupyingItemInstanceId != ignoreInstanceId)
                {
                    return false; // Collision
                }

                // 4. Check if we overlap the required zone
                if (item.RequiredZone != PlacementZone.General && cell.Zone == item.RequiredZone)
                {
                    satisfiesZoneRequirement = true;
                }
            }

            return satisfiesZoneRequirement;
        }

        /// <summary>
        /// Places an item in the grid if valid.
        /// </summary>
        /// <returns>The placed GridItemInstance, or null if placement failed.</returns>
        public GridItemInstance PlaceItem(ItemData item, Vector2I anchor, int rotationSteps)
        {
            return PlaceItem(item, anchor, rotationSteps, null);
        }

        public GridItemInstance PlaceItem(ItemData item, Vector2I anchor, int rotationSteps, string instanceId = null)
        {
            if (!CanPlaceItem(item, anchor, rotationSteps))
            {
                return null;
            }

            var instance = new GridItemInstance(item, anchor, rotationSteps, instanceId);
            PlacedItems[instance.InstanceId] = instance;

            foreach (Vector2I cellCoords in instance.GetAbsoluteOccupiedCells())
            {
                if (Cells.TryGetValue(cellCoords, out GridCell cell))
                {
                    cell.OccupyingItemInstanceId = instance.InstanceId;
                }
            }

            return instance;
        }

        /// <summary>
        /// Removes an item from the grid.
        /// </summary>
        public bool RemoveItem(string instanceId)
        {
            if (!PlacedItems.TryGetValue(instanceId, out GridItemInstance instance))
            {
                return false;
            }

            foreach (Vector2I cellCoords in instance.GetAbsoluteOccupiedCells())
            {
                if (Cells.TryGetValue(cellCoords, out GridCell cell))
                {
                    if (cell.OccupyingItemInstanceId == instanceId)
                    {
                        cell.OccupyingItemInstanceId = null;
                    }
                }
            }

            PlacedItems.Remove(instanceId);
            return true;
        }

        /// <summary>
        /// Unlocks a cell by coordinate.
        /// </summary>
        public bool UnlockCell(Vector2I coords)
        {
            if (Cells.TryGetValue(coords, out GridCell cell))
            {
                cell.IsLocked = false;
                return true;
            }
            return false;
        }
    }
}
