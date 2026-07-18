using Godot;
using System;
using Orclimax.Core;

namespace Orclimax.Core
{
    public partial class TestGrid : SceneTree
    {
        public override void _Initialize()
        {
            GD.Print("\n=== STARTING GRID VALIDATION TESTS ===");
            try
            {
                RunTests();
                GD.Print("=== ALL TESTS PASSED SUCCESSFULLY! ===\n");
            }
            catch (Exception e)
            {
                GD.PrintErr($"\n=== TEST FAILED: {e.Message} ===");
                GD.PrintErr(e.StackTrace + "\n");
            }
            Quit();
        }

        private void RunTests()
        {
            // 1. Create items
            ItemData dildo = new ItemData();
            dildo.Id = "dildo";
            dildo.ItemName = "Vibrating Dildo";
            dildo.RequiredZone = PlacementZone.Groin;
            // 1x2 vertical item
            dildo.ShapeOffsets = new Godot.Collections.Array<Vector2I> { new Vector2I(0, 0), new Vector2I(0, 1) }; 

            ItemData gag = new ItemData();
            gag.Id = "gag";
            gag.ItemName = "Mouth Gag";
            gag.RequiredZone = PlacementZone.Head;
            // 1x1 item
            gag.ShapeOffsets = new Godot.Collections.Array<Vector2I> { new Vector2I(0, 0) }; 

            // 2. Create FemaleData
            FemaleData girl = new FemaleData();
            girl.CharacterName = "Test Girl";
            girl.HeadCells = new Godot.Collections.Array<Vector2I> { new Vector2I(0, 0) };
            girl.GroinCells = new Godot.Collections.Array<Vector2I> { new Vector2I(2, 2), new Vector2I(2, 3) };
            girl.GeneralCells = new Godot.Collections.Array<Vector2I> { new Vector2I(1, 1), new Vector2I(1, 2) };
            girl.InitiallyLockedCells = new Godot.Collections.Array<Vector2I> { new Vector2I(2, 3) }; // (2,3) is locked

            // 3. Create Grid and Initialize
            GridData grid = new GridData();
            girl.InitializeGrid(grid);

            // Test 1: Place Groin item on General cells (1, 1) and (1, 2)
            // It should fail because no cell matches Groin
            bool result1 = grid.CanPlaceItem(dildo, new Vector2I(1, 1), 0);
            GD.Print($"Test 1 (Groin item on General cells): {(result1 ? "Allowed (INCORRECT)" : "Blocked (CORRECT)")}");
            if (result1) throw new Exception("Should not allow Groin item to be placed solely on General cells!");

            // Test 2: Place Groin item on (2, 2) pointing downwards to (2, 3) (which is locked)
            // It should fail because (2, 3) is locked
            bool result2 = grid.CanPlaceItem(dildo, new Vector2I(2, 2), 0);
            GD.Print($"Test 2 (Groin item overlapping locked cell): {(result2 ? "Allowed (INCORRECT)" : "Blocked (CORRECT)")}");
            if (result2) throw new Exception("Should not allow placement overlapping locked cells!");

            // Test 3: Place Groin item on (2, 2) rotated 90 degrees clockwise.
            // Rotated shape offsets: (0,0) and (-1,0) (since (0,0) -> (0,0) and (0,1) -> (-1,0)).
            // So absolute cells are (2, 2) (Groin) and (1, 2) (General).
            // Both are unlocked. One cell matches the required Groin zone.
            // This should SUCCEED!
            bool result3 = grid.CanPlaceItem(dildo, new Vector2I(2, 2), 1);
            GD.Print($"Test 3 (Groin item rotated to overlap Groin + General): {(result3 ? "Allowed (CORRECT)" : "Blocked (INCORRECT)")}");
            if (!result3) throw new Exception("Should allow placement overlapping Groin + General!");

            // Test 4: Perform placement and verify cells are occupied
            var inst = grid.PlaceItem(dildo, new Vector2I(2, 2), 1);
            if (inst == null) throw new Exception("Placement returned null!");
            
            bool occupied1 = grid.Cells[new Vector2I(2, 2)].OccupyingItemInstanceId == inst.InstanceId;
            bool occupied2 = grid.Cells[new Vector2I(1, 2)].OccupyingItemInstanceId == inst.InstanceId;
            GD.Print($"Test 4 (Placement occupancy check): {(occupied1 && occupied2 ? "PASSED" : "FAILED")}");
            if (!occupied1 || !occupied2) throw new Exception("Grid occupancy mapping was incorrect!");

            // Test 5: Place Head item on Head cell (0, 0) - should succeed
            bool result5 = grid.CanPlaceItem(gag, new Vector2I(0, 0), 0);
            GD.Print($"Test 5 (Head item on Head cell): {(result5 ? "Allowed (CORRECT)" : "Blocked (INCORRECT)")}");
            if (!result5) throw new Exception("Should allow Head item on Head cell!");

            // Test 6: Remove item and verify cells are cleared
            grid.RemoveItem(inst.InstanceId);
            bool cleared1 = grid.Cells[new Vector2I(2, 2)].OccupyingItemInstanceId == null;
            bool cleared2 = grid.Cells[new Vector2I(1, 2)].OccupyingItemInstanceId == null;
            GD.Print($"Test 6 (Removal cleanup check): {(cleared1 && cleared2 ? "PASSED" : "FAILED")}");
            if (!cleared1 || !cleared2) throw new Exception("Grid cells were not properly cleared after removal!");
        }
    }
}
