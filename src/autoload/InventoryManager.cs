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

        [Signal] public delegate void FemaleChangedEventHandler(FemaleData female);
        [Signal] public delegate void GridUpdatedEventHandler();
        [Signal] public delegate void StashUpdatedEventHandler();

        public FemaleData CurrentFemale { get; private set; }
        public GridData BackpackGrid { get; private set; } = new GridData();
        public Godot.Collections.Array<ItemData> Stash { get; private set; } = new Godot.Collections.Array<ItemData>();
        
        // List of all available recipes
        public Godot.Collections.Array<FusionRecipe> Recipes { get; private set; } = new Godot.Collections.Array<FusionRecipe>();

        public Godot.Collections.Array<ItemData> CurrentShopItems { get; set; } = new Godot.Collections.Array<ItemData> { null, null, null };
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

        public void SetFemale(FemaleData female)
        {
            if (female == null) return;

            // 1. Return all currently placed items to stash so player doesn't lose them
            if (CurrentFemale != null)
            {
                var instances = BackpackGrid.PlacedItems.Values.ToList();
                foreach (var inst in instances)
                {
                    Stash.Add(inst.Item);
                    BackpackGrid.RemoveItem(inst.InstanceId);
                }
            }

            CurrentFemale = female;
            CurrentFemale.InitializeGrid(BackpackGrid);

            EmitSignal(SignalName.FemaleChanged, CurrentFemale);
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
