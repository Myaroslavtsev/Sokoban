using System;
using System.Collections.Generic;
using System.Text;

namespace Sokoban
{
    class StaticAction
    {
        public bool WillTransform;
        public IStaticCell TransformTo;

        public StaticAction(bool willTrasnform, IStaticCell transformTo)
        {
            WillTransform = willTrasnform;
            TransformTo = transformTo;
        }
    }
}
