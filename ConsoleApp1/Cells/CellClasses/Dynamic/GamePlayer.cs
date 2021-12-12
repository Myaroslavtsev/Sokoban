using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Sokoban
{
    class GamePlayer : IDynamicCell
    {
        private const int moveLimit = 99999;
        private const int forceLimit = 100;

        public char DataFileChar { get => '@'; }

        public DynamicCellType CellType { get => DynamicCellType.Player; }

        public Point Position { get; set; }

        public DynamicAction CellAction { get; set; }

        public int MaxMoves { get; private set; }

        public int Moves { get; private set; }

        public int Force { get; private set; }

        public int BombCount { get; set; }

        public List<int> Keys { get; set; }

        public GamePlayer(int x, int y)
        {
            Position = new Point(x, y);
            Keys = new List<int>();
            Force = 1;
        }

        public void CountMove()
        {
            Moves++;
        }

        public bool AddMoves(ref int moveCount)
        {
            if (moveCount > 0)
            {
                if (MaxMoves + moveCount > moveLimit)
                    moveCount = moveLimit - MaxMoves;
                MaxMoves += moveCount;
                return true;
            }
            return false;
        }

        public bool SetForce(int force)
        {
            if (force > 0)
            {
                if (force <= forceLimit)
                    Force = force;
                else
                    Force = forceLimit;
                return true;
            }
            return false;
        }
    }
}
