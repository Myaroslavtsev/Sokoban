using System.Drawing;

namespace Sokoban
{
    public class MapCellAction
    {
        public bool WillTransform;
        public IMapCell TransformTo;
        public Point Move;

        public MapCellAction(Point move, bool willTransform, IMapCell transformTo)
        {
            Move = move;
            WillTransform = willTransform;
            TransformTo = transformTo;
        }
    }
}
