using System.Collections.Generic;
using System.Drawing;

namespace Sokoban
{
    public class GamePlayer : IMapCell
    {
        private const int moveLimit = 99999;
        private const int forceLimit = 100;

        public char DataFileChar => '@'; 

        public CellTypes CellType => CellTypes.Player;
        public Point Position { get; set; }
        public MapCellAction CellAction { get; set; }
        public int MaxMoves { get; set; }

        public int Moves { get; set; }

        public int Force { get; set; }

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

        public bool AllowsToEnter(IMapCell cell, GameMap map, HashSet<GameOption> options) => false;
    }
}