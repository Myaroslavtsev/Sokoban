using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Sokoban
{
    class Box : IDynamicCell
    {
        public DynamicCellType CellType { get => DynamicCellType.Box; }

        public char DataFileChar { get => '%'; }

        public Point Position { get; set; }

        public DynamicAction CellAction { get; set; }

        public Box(int x, int y)
        {
            Position = new Point(x, y);
        }
    }
}
