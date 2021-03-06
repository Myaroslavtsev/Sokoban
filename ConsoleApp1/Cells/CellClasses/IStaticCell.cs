using System.Collections.Generic;

namespace Sokoban
{
    interface IStaticCell : IMapCell
    {
        // Represents a cell in the static (maze construction) layer
        public new char DataFileChar { get; }
        public StaticCellType CellType { get; }
        public bool AllowsToEnter(IDynamicCell cell, GameMap map, HashSet<GameOption> options);
        public StaticAction CellAction { get; set; }
    }
}
