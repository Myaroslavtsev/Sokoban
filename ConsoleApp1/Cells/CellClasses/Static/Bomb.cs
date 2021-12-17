using System;
using System.Collections.Generic;
using System.Text;

namespace Sokoban
{
    public class Bomb : IStaticCell
    {
        public char DataFileChar { get => '='; }

        public StaticCellType CellType { get => StaticCellType.Bomb; }

        public StaticAction CellAction { get; set; }

        public bool AllowsToEnter(IDynamicCell cell, GameMap map, HashSet<GameOption> options)
        {
            if (cell.CellType == DynamicCellType.Player)
                (cell as GamePlayer).BombCount++;
            CellAction = new StaticAction(true, null);
            return true;
        }
    }
}
