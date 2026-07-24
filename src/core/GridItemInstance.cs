using Godot;
using System;
using System.Collections.Generic;

namespace Orclimax.Core
{
    public partial class GridItemInstance : RefCounted
    {
        public string InstanceId { get; private set; }
        public ItemData Item { get; private set; }
        public Vector2I AnchorCoords { get; set; }
        public int RotationSteps { get; set; } // 0 = 0, 1 = 90, 2 = 180, 3 = 270

        public GridItemInstance() {}

        public GridItemInstance(ItemData item, Vector2I anchorCoords, int rotationSteps = 0, string instanceId = null)
        {
            Item = item;
            AnchorCoords = anchorCoords;
            RotationSteps = rotationSteps;
            InstanceId = string.IsNullOrEmpty(instanceId) ? Guid.NewGuid().ToString() : instanceId;
        }

        /// <summary>
        /// Gets the absolute grid coordinates occupied by this item.
        /// </summary>
        public IEnumerable<Vector2I> GetAbsoluteOccupiedCells()
        {
            var rotatedOffsets = Item.GetRotatedOffsets(RotationSteps);
            foreach (var offset in rotatedOffsets)
            {
                yield return AnchorCoords + offset;
            }
        }

        /// <summary>
        /// Calculates the ratio of occupied cells that are active (not Inactive).
        /// </summary>
        public float GetActiveRatio(GridData grid)
        {
            int totalCells = 0;
            int activeCells = 0;

            foreach (var cellCoords in GetAbsoluteOccupiedCells())
            {
                totalCells++;
                if (grid.Cells.TryGetValue(cellCoords, out GridCell cell))
                {
                    if (cell.Zone != PlacementZone.Inactive)
                    {
                        activeCells++;
                    }
                }
            }

            return totalCells > 0 ? (float)activeCells / totalCells : 1f;
        }

        /// <summary>
        /// Gets all unique item instances placed in the Star/Synergy adjacent cells of this item.
        /// </summary>
        public HashSet<GridItemInstance> GetLinkedStarItems(GridData grid)
        {
            var linkedItems = new HashSet<GridItemInstance>();
            if (grid == null || Item == null || Item.StarOffsets == null || Item.StarOffsets.Count == 0)
                return linkedItems;

            var rotatedStarOffsets = Item.GetRotatedStarOffsets(RotationSteps);
            foreach (var offset in rotatedStarOffsets)
            {
                Vector2I starCoords = AnchorCoords + offset;
                if (grid.Cells.TryGetValue(starCoords, out GridCell cell))
                {
                    if (cell.OccupyingItemInstanceId != null && cell.OccupyingItemInstanceId != InstanceId)
                    {
                        if (grid.PlacedItems.TryGetValue(cell.OccupyingItemInstanceId, out GridItemInstance linkedInst))
                        {
                            linkedItems.Add(linkedInst);
                        }
                    }
                }
            }
            return linkedItems;
        }

        public float GetStarSynergyMultiplier(GridData grid)
        {
            int count = GetLinkedStarItems(grid).Count;
            return 1.0f + (count * Item.StarSynergyBonusPerLink);
        }
    }
}
