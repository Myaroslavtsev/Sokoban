using System.Collections.Generic;
using System.Drawing;

namespace Sokoban
{
    public class Cage : IMapCell
    {
        public CellTypes CellType => CellTypes.Cage; 
        public char DataFileChar => '*';
        public Point Position { get; set; }
        public MapCellAction CellAction { get; set; }
        public bool AllowsToEnter(IMapCell cell, GameMap map, HashSet<GameOption> options) => true;

        public Cage(int x, int y)
        {
            Position = new Point(x, y);            
        }
    }
}
