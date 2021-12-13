﻿using System.Drawing;

namespace Sokoban
{
    static class PointExtension
    {
        public static Point Add(this Point p1, Point p2)
        {
            return new Point(p1.X + p2.X, p1.Y + p2.Y);
        }
    }
}
