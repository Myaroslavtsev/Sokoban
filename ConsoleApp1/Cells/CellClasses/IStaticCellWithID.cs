using System.Collections.Generic;

namespace Sokoban
{
    public interface IStaticCellWithID : IStaticCell
    {
        // Represents a cell in the static (maze construction) layer
        public new char DataFileChar { get; }
        public new StaticCellType CellType { get; }
        public new bool AllowsToEnter(IDynamicCell cell, GameMap map, HashSet<GameOption> options);
        public new StaticAction CellAction { get; set; }
        public int ID { get; }
    }
}
