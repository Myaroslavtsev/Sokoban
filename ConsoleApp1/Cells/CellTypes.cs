namespace Sokoban
{
    public enum StaticCellType
    {           // symbols in data file:
        NoCell, // space
        Wall,   // #
        Cage,   // *
        Plate,  // _
        Key,    // +
        Door,   // |
        Bomb    // =
    }

    public enum DynamicCellType
    {
        NoCell, // space
        Box,    // %
        Player  // @
    }
}