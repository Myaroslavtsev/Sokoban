using System.Drawing;

namespace Sokoban
{
    public class MapCellAction
    {
        public readonly bool WillTransform;
        public readonly IMapCell TransformTo;
        public readonly Point Move;

        public MapCellAction(Point move, bool willTransform, IMapCell transformTo)
        {
            Move = move;
            WillTransform = willTransform;
            TransformTo = transformTo;
        }
    }
}
