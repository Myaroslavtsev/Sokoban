using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;

namespace Sokoban
{
    class Portal : IMapCell, ICellWithDestination
    {
        public CellTypes CellType => CellTypes.Portal;
        public char DataFileChar => '0';
        public Point Position { get; set; }
        public MapCellAction CellAction { get; set; }
        public Point Destination { get; private set; }
        public bool AllowsToEnter(IMapCell cell, GameMap map, HashSet<GameOption> options)
        {
            if ((map.DynamicLayer.GetByPosition(Destination) is null) &&
                ((map.StaticLayer.GetByPosition(Destination) is null) ||
                map.StaticLayer.GetByPosition(Destination).AllowsToEnter(cell, map, options)))
            {
                var move = Destination.Substract(cell.Position);
                if (cell.CellAction is null)
                    cell.CellAction = new MapCellAction(move, false, null);
                else
                    cell.CellAction.Move = move;
                return false;
            }
            return true;
        }

        public Portal(int x, int y, int destinationX, int destinationY)
        {
            Position = new Point(x, y);
            Destination = new Point(destinationX, destinationY);
        }
    }
}
