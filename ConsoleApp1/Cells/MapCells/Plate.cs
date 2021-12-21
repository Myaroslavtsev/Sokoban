using System;
using System.Collections.Generic;
using System.Drawing;

namespace Sokoban
{
    public class Plate : IMapCell
    {
        public char DataFileChar => '_'; 

        public CellTypes CellType => CellTypes.Plate;
        public Point Position { get; set; }
        public MapCellAction CellAction { get; set; }
        public int ID { get; private set; }

        public bool AllowsToEnter(IMapCell cell, GameMap map, HashSet<GameOption> options)
        {
            if (cell is GamePlayer || cell is Box)
                foreach(var staticCell in map.StaticLayer.Cells)
                    if (staticCell is Door)
                        if ((staticCell as Door).ID == ID)
                            staticCell.CellAction = new MapCellAction(new Point(0, 0), true, null);
            return true;
        }

        public Plate(int x, int y, int id)
        {
            Position = new Point(x, y);
            ID = id;
        }
    }
}
