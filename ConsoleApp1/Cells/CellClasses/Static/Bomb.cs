using System;
using System.Collections.Generic;
using System.Text;

namespace Sokoban
{
    class Bomb : IStaticCell
    {
        public char DataFileChar { get => '='; }

        public StaticCellType CellType { get => StaticCellType.Bomb; }

        public StaticAction CellAction { get; set; }

        public bool AllowsToEnter(IDynamicCell cell, Game game)
        {
            if (cell.CellType == DynamicCellType.Player)
                game.Map.Player.BombCount++;
            CellAction = new StaticAction(true, null);
            return true;
        }
    }
}
