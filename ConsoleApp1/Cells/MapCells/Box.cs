using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Sokoban
{
    public class Box : IMapCell
    {
        public CellTypes CellType => CellTypes.Box; 

        public char DataFileChar => '%';
        public Point Position { get; set; }
        public MapCellAction CellAction { get; set; }
        public Box(int x, int y)
        {
            Position = new Point(x, y);
        }

        public bool AllowsToEnter(IMapCell cell, GameMap map, HashSet<GameOption> options) => false;        
    }
}
