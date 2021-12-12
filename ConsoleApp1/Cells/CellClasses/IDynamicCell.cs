using System.Drawing;

namespace Sokoban
{
    interface IDynamicCell : IMapCell
    {
        // Represents a cell in the dynamic (moving objects) layer
        public char DataFileChar { get; }
        public DynamicCellType CellType { get; }
        public Point Position { get; set; }
        public DynamicAction CellAction { get; set; }
    }
}
