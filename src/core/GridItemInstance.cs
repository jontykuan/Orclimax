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
    }
}
