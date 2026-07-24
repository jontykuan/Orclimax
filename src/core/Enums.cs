using System;
// Self-updating knowledge graph active


namespace Orclimax.Core
{
    public enum PlacementZone
    {
        General = 0,
        Head = 1,
        Chest = 2,
        Groin = 3,
        Limbs = 4,
        Inactive = 5
    }

    public enum ItemCategory
    {
        Weapon,      // Deals damage, triggers automatically on CD
        Toy,         // Increases pleasure/climax generation
        Clothing,    // Modifies armor, stats, and climax damage
        Accessory,   // Passive stat buffs
        Consumable   // Quick-use buffs or healing
    }

    public enum GameState
    {
        Title = 0,       // Main title screen
        VesselSelect = 1, // Vessel management & selection
        Backpack = 2,    // Shop & equipment preparation
        WorldMap = 3,    // Stage node navigation map
        Combat = 4,      // Side-scrolling action combat
        GameOver = 5,    // Orc defeated
        Victory = 6      // Stage / run complete
    }
}
