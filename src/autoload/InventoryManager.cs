using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Orclimax.Core;

namespace Orclimax.Autoload
{
    public partial class InventoryManager : Node
    {
        public static InventoryManager Instance { get; private set; }

        [Signal] public delegate void VesselChangedEventHandler(VesselData vessel);
        [Signal] public delegate void GridUpdatedEventHandler();
        [Signal] public delegate void StashUpdatedEventHandler();

        public VesselData CurrentVessel { get; private set; }
        public Dictionary<string, GridData> VesselGrids { get; private set; } = new Dictionary<string, GridData>();
        public GridData BackpackGrid
        {
            get
            {
                if (CurrentVessel == null) return null;
                if (!VesselGrids.TryGetValue(CurrentVessel.Id, out GridData grid))
                {
                    grid = new GridData();
                    CurrentVessel.InitializeGrid(grid);
                    VesselGrids[CurrentVessel.Id] = grid;
                }
                return grid;
            }
        }
        public Godot.Collections.Array<ItemData> Stash { get; private set; } = new Godot.Collections.Array<ItemData>();
        
        // List of all available recipes
        public Godot.Collections.Array<FusionRecipe> Recipes { get; private set; } = new Godot.Collections.Array<FusionRecipe>();

        public Godot.Collections.Array<ItemData> CurrentShopItems { get; set; } = new Godot.Collections.Array<ItemData>();
        public Godot.Collections.Array<bool> ShopSlotsLocked { get; set; } = new Godot.Collections.Array<bool> { false, false, false };

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

        public override void _Ready()
        {
            LoadDefaultRecipes();
        }

        private void LoadDefaultRecipes()
        {
            // We will dynamically load or define recipes here.
            // For example:
            // FusionRecipe recipe = new FusionRecipe();
            // ...
        }

        public void SetVessel(VesselData vessel, bool keepEquipped = false)
        {
            if (vessel == null) return;

            // 1. If not keeping equipped state when switching to a different vessel, move all items from current vessel to stash
            if (!keepEquipped && CurrentVessel != null && CurrentVessel.Id != vessel.Id)
            {
                var grid = BackpackGrid;
                if (grid != null)
                {
                    var instances = new List<GridItemInstance>(grid.PlacedItems.Values);
                    foreach (var inst in instances)
                    {
                        if (inst != null && inst.Item != null)
                        {
                            Stash.Add(inst.Item);
                        }
                    }
                    grid.ClearGrid();
                    CurrentVessel.InitializeGrid(grid);
                }
            }

            // 2. Switch current active Vessel
            CurrentVessel = vessel;

            // 3. Initialize target Vessel's grid if not exists
            if (!VesselGrids.ContainsKey(vessel.Id))
            {
                var newGrid = new GridData();
                vessel.InitializeGrid(newGrid);
                VesselGrids[vessel.Id] = newGrid;
            }

            EmitSignal(SignalName.VesselChanged, CurrentVessel);
            EmitSignal(SignalName.GridUpdated);
            EmitSignal(SignalName.StashUpdated);
        }

        public bool TryPlaceItemFromStash(int stashIndex, Vector2I anchor, int rotationSteps)
        {
            if (stashIndex < 0 || stashIndex >= Stash.Count) return false;

            ItemData item = Stash[stashIndex];
            if (BackpackGrid.CanPlaceItem(item, anchor, rotationSteps))
            {
                BackpackGrid.PlaceItem(item, anchor, rotationSteps);
                Stash.RemoveAt(stashIndex);
                EmitSignal(SignalName.GridUpdated);
                EmitSignal(SignalName.StashUpdated);
                return true;
            }
            return false;
        }

        public bool TryTakeItemToStash(string instanceId)
        {
            if (BackpackGrid.PlacedItems.TryGetValue(instanceId, out GridItemInstance instance))
            {
                Stash.Add(instance.Item);
                BackpackGrid.RemoveItem(instanceId);
                EmitSignal(SignalName.GridUpdated);
                EmitSignal(SignalName.StashUpdated);
                return true;
            }
            return false;
        }

        public bool RotatePlacedItem(string instanceId)
        {
            if (!BackpackGrid.PlacedItems.TryGetValue(instanceId, out GridItemInstance instance))
            {
                return false;
            }

            int nextRotation = (instance.RotationSteps + 1) % 4;
            ItemData item = instance.Item;
            Vector2I anchor = instance.AnchorCoords;

            // Temporarily remove to check placement
            BackpackGrid.RemoveItem(instanceId);

            if (BackpackGrid.CanPlaceItem(item, anchor, nextRotation))
            {
                BackpackGrid.PlaceItem(item, anchor, nextRotation, instanceId);
                EmitSignal(SignalName.GridUpdated);
                return true;
            }
            else
            {
                // Revert
                BackpackGrid.PlaceItem(item, anchor, instance.RotationSteps, instanceId);
                return false;
            }
        }

        public bool BuyItem(ItemData item)
        {
            if (item == null) return false;
            
            if (GameManager.Instance.SpendGold(item.BasePrice))
            {
                var duplicateItem = (ItemData)item.Duplicate(true);
                Stash.Add(duplicateItem);
                EmitSignal(SignalName.StashUpdated);
                return true;
            }
            return false;
        }

        public void AddItemToStash(ItemData item)
        {
            if (item == null) return;
            var duplicateItem = (ItemData)item.Duplicate(true);
            Stash.Add(duplicateItem);
            EmitSignal(SignalName.StashUpdated);
        }

        public void AddItemToStashDirectly(ItemData item)
        {
            if (item == null) return;
            Stash.Add(item);
            EmitSignal(SignalName.StashUpdated);
        }

        public void SellItemDirectly(ItemData item)
        {
            if (item == null) return;
            int sellPrice = Math.Max(1, item.BasePrice / 2);
            GameManager.Instance.AddGold(sellPrice);
        }

        public void SellPlacedItem(string instanceId)
        {
            if (BackpackGrid.PlacedItems.TryGetValue(instanceId, out GridItemInstance instance))
            {
                int sellPrice = Math.Max(1, instance.Item.BasePrice / 2);
                GameManager.Instance.AddGold(sellPrice);
                BackpackGrid.RemoveItem(instanceId);
                EmitSignal(SignalName.GridUpdated);
            }
        }

        public void SellStashedItem(int stashIndex)
        {
            if (stashIndex >= 0 && stashIndex < Stash.Count)
            {
                ItemData item = Stash[stashIndex];
                int sellPrice = Math.Max(1, item.BasePrice / 2);
                GameManager.Instance.AddGold(sellPrice);
                Stash.RemoveAt(stashIndex);
                EmitSignal(SignalName.StashUpdated);
            }
        }

        /// <summary>
        /// Scans the grid for adjacent items matching recipes and merges them.
        /// </summary>
        public bool TriggerFusions()
        {
            bool fusedAny = false;
            bool loop = true;

            while (loop)
            {
                loop = false;
                var items = BackpackGrid.PlacedItems.Values.ToList();

                for (int i = 0; i < items.Count; i++)
                {
                    for (int j = i + 1; j < items.Count; j++)
                    {
                        var itemA = items[i];
                        var itemB = items[j];

                        // Find matching recipe
                        FusionRecipe matchingRecipe = FindRecipe(itemA.Item, itemB.Item);
                        if (matchingRecipe != null && !itemA.Item.IsLocked && !itemB.Item.IsLocked && AreAdjacent(itemA, itemB))
                        {
                            Vector2I mergeAnchor = itemA.AnchorCoords;
                            int mergeRotation = itemA.RotationSteps;

                            // Remove components
                            BackpackGrid.RemoveItem(itemA.InstanceId);
                            BackpackGrid.RemoveItem(itemB.InstanceId);

                            // Try to place the result where item A was
                            var resultDuplicate = (ItemData)matchingRecipe.Result.Duplicate(true);
                            if (BackpackGrid.CanPlaceItem(resultDuplicate, mergeAnchor, mergeRotation))
                            {
                                BackpackGrid.PlaceItem(resultDuplicate, mergeAnchor, mergeRotation);
                            }
                            else
                            {
                                // Otherwise, send to stash
                                Stash.Add(resultDuplicate);
                            }

                            loop = true;
                            fusedAny = true;
                            break;
                        }
                    }
                    if (loop) break;
                }
            }

            if (fusedAny)
            {
                EmitSignal(SignalName.GridUpdated);
                EmitSignal(SignalName.StashUpdated);
            }

            return fusedAny;
        }

        private FusionRecipe FindRecipe(ItemData a, ItemData b)
        {
            foreach (var recipe in Recipes)
            {
                if (recipe.ComponentA == null || recipe.ComponentB == null || recipe.Result == null) continue;

                if ((recipe.ComponentA.Id == a.Id && recipe.ComponentB.Id == b.Id) ||
                    (recipe.ComponentA.Id == b.Id && recipe.ComponentB.Id == a.Id))
                {
                    return recipe;
                }
            }
            return null;
        }

        private bool AreAdjacent(GridItemInstance a, GridItemInstance b)
        {
            HashSet<Vector2I> cellsA = new HashSet<Vector2I>(a.GetAbsoluteOccupiedCells());
            HashSet<Vector2I> cellsB = new HashSet<Vector2I>(b.GetAbsoluteOccupiedCells());

            foreach (var coordA in cellsA)
            {
                // Check 4 directions
                Vector2I[] dirs = { Vector2I.Up, Vector2I.Down, Vector2I.Left, Vector2I.Right };
                foreach (var dir in dirs)
                {
                    if (cellsB.Contains(coordA + dir))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
