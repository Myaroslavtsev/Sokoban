using System;
using System.Collections.Generic;
using System.Drawing;

namespace Sokoban
{
    class DynamicAction
    {
        public bool WillTransform;
        public IDynamicCell TransformTo;
        public Point Move;

        public DynamicAction(Point move, bool WillTransform, IDynamicCell transformTo)
        {
            Move = move;
        }
    }
}
