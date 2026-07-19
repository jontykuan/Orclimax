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
        Shop,        // Shop and Backpack preparation phase
        Combat,      // Side-scrolling combat action phase
        GameOver,    // Orc died
        Victory      // Run won
    }
}
