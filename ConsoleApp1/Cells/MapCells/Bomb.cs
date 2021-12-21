using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Sokoban
{
    public class Bomb : IMapCell
    {
        public char DataFileChar => '='; 

        public CellTypes CellType => CellTypes.Bomb;
        public Point Position { get; set; }
        public MapCellAction CellAction { get; set; }
        public bool AllowsToEnter(IMapCell cell, GameMap map, HashSet<GameOption> options)
        {
            if (cell is GamePlayer)
                (cell as GamePlayer).BombCount++;
            CellAction = new MapCellAction(new System.Drawing.Point(0, 0), true, null);
            return true;
        }

        public Bomb(int x, int y)
        {
            Position = new Point(x, y);
        }
    }
}
