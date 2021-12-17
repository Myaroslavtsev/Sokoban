using System.Drawing;

namespace Sokoban
{
    public interface IDynamicCell : IMapCell
    {
        // Represents a cell in the dynamic (moving objects) layer
        public new char DataFileChar { get; }
        public DynamicCellType CellType { get; }
        public Point Position { get; set; }
        public DynamicAction CellAction { get; set; }
    }
}
