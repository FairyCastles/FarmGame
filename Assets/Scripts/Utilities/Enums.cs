public enum ItemType
{
    Seed, Commodity, Furniture,
    HoeTool, ChopTool, BreakTool, ReapTool, WaterTool, CollectTool,
    ReapableScenery,
}

public enum SlotType
{
    Bag, Box, Shop,
}

public enum InventoryLocation
{
    Player, Box,
}

// 玩家此时的状态
public enum PartType
{
    None, Carry, Hoe, Break, Water, Collect, Chop, Reap,
} 

public enum PartName 
{
    Body, Hair, Arm, Tool,
}

public enum Season
{
    Spring, Summer, Autumn, Winter,
}

public enum GridType
{
    Diggable, DropItem, PlaceFurniture, NPCObstacle,
}

public enum ParticalEffectType
{
    None, LeavesFalling, Rock, ReapableScenery,
}

public enum GameState
{
    Gameplay, Pause
}

public enum LightShift
{
    Morning, Night
}

public enum SoundName
{
    None, FootStepSoft, FootStepHard,
    Axe, Pickaxe, Hoe, Reap, Water, Basket, Chop,
    Pickup, Plant, TreeFalling, Restle,
    AmbientCountryside1, AmbientCountryside2, 
    MusicCalm1, MusicCalm2, MusicCalm3, MusicCalm4, MusicCalm5, MusicCalm6,
    AmbientIndoor1
}