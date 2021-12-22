using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;

namespace Sokoban
{
    public interface IMapCell : IComparable
    {
        public CellTypes CellType { get; }
        public char DataFileChar { get; }
        public bool AllowsToEnter(IMapCell cell, GameMap map, HashSet<GameOption> options);
        public Point Position { get; set; }
        public MapCellAction CellAction { get; set; }
        int IComparable.CompareTo(object obj)
        {
            if (obj is Point)
            {
                var p = (Point)obj;
                return this.CompareTo(p);
            }
            if (obj is IMapCell)
            {
                return this.CompareTo(obj as IMapCell);
            }
            throw new ArgumentException("Object is not a point or map cell");
        }
        internal int CompareTo(IMapCell cell)
        {
            return this.CompareTo(cell.Position);
        }
        internal int CompareTo(Point point)
        {
            if (this.Position.Y == point.Y)
                return this.Position.X.CompareTo(point.X);
            else
                return this.Position.Y.CompareTo(point.Y);
        }
    }

    public class MapCellCompare : IComparer<IMapCell>
    {
        public int Compare(IMapCell cell1, IMapCell cell2)
        {
            return cell1.CompareTo(cell2);
        }
    }
}
