using System;
using System.Collections.Generic;
using System.Drawing;

namespace Sokoban
{
    public class Door : IMapCell
    {
        public char DataFileChar => '|';

        public CellTypes CellType => CellTypes.Door;
        public Point Position { get; set; }
        public MapCellAction CellAction { get; set; }
        public int ID { get; private set; }

        public bool AllowsToEnter(IMapCell cell, GameMap map, HashSet<GameOption> options)
        {
            if (cell is GamePlayer)
            {
                if ((cell as GamePlayer).Keys.Contains(ID))
                {
                    (cell as GamePlayer).Keys.Remove(ID);
                    CellAction = new MapCellAction(new Point(0, 0), true, null);
                    return true;
                }
            }
            return false;
        }

        public Door(int x, int y, int id)
        {
            Position = new Point(x, y);
            ID = id;
        }
    }
}
