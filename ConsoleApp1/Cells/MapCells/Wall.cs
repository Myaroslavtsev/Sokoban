using System;
using System.Collections.Generic;
using System.Drawing;

namespace Sokoban
{
    public class Wall : IMapCell
    {
        public char DataFileChar => '#';
        public CellTypes CellType => CellTypes.Wall;
        public Point Position { get; set ; }
        public MapCellAction CellAction { get; set; }
        public bool AllowsToEnter(IMapCell cell, GameMap map, HashSet<GameOption> options)
        {
            if (cell is GamePlayer)
            {
                if (options.Contains(GameOption.Iddqd))
                    return true;
                if ((cell as GamePlayer).BombCount > 0)
                {
                    (cell as GamePlayer).BombCount--;
                    CellAction = new MapCellAction(new Point(0, 0), true, null);
                    return true;
                }
                return false;
            }                
            return false;
        }

        public Wall(int x, int y)
        {
            Position = new Point(x, y);
        }
    }
}
