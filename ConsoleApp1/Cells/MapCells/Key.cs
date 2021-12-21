using System;
using System.Collections.Generic;
using System.Drawing;

namespace Sokoban
{
    public class Key : IMapCell
    {
        public char DataFileChar => '+'; 

        public CellTypes CellType => CellTypes.Key;
        public Point Position { get; set; }
        public MapCellAction CellAction { get; set; }
        public int ID { get; private set; }

        public Key(int x, int y, int id)
        {
            Position = new Point(x, y);
            ID = id;
        }

        public bool AllowsToEnter(IMapCell cell, GameMap map, HashSet<GameOption> options)
        {
            if (cell is GamePlayer)
                (cell as GamePlayer).Keys.Add(ID);            
            CellAction = new MapCellAction(new Point(0, 0), true, null);
            return true;
        }  
    }
}
