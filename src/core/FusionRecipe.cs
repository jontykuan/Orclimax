using Godot;
using System;

namespace Orclimax.Core
{
    [GlobalClass]
    public partial class FusionRecipe : Resource
    {
        [Export] public ItemData ComponentA { get; set; }
        [Export] public ItemData ComponentB { get; set; }
        [Export] public ItemData Result { get; set; }
    }
}
