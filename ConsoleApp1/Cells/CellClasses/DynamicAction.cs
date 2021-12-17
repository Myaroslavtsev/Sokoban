using System.Drawing;

namespace Sokoban
{
    public class DynamicAction
    {
        public bool WillTransform;
        public IDynamicCell TransformTo;
        public Point Move;

        public DynamicAction(Point move, bool willTransform, IDynamicCell transformTo)
        {
            Move = move;
            WillTransform = willTransform;
            TransformTo = transformTo;
        }
    }
}
