using System.Collections.Generic;

namespace Sokoban
{
    class Cage : IStaticCell
    {
        public StaticCellType CellType { get => StaticCellType.Cage; }

        public char DataFileChar { get => '*'; }

        public StaticAction CellAction { get; set; }

        public bool AllowsToEnter(IDynamicCell cell, GameMap map, HashSet<GameOption> options) => true;
    }
}
