using System;
using System.Collections.Generic;
using System.Text;

namespace Sokoban
{
    class Door : IStaticCell
    {
        public char DataFileChar { get => '|'; }

        public StaticCellType CellType { get => StaticCellType.Door; }

        public StaticAction CellAction { get; set; }

        public int ID { get; private set; }

        public bool AllowsToEnter(IDynamicCell cell, Game game)
        {
            if (cell is GamePlayer)
            {
                if (game.Map.Player.Keys.Contains(ID))
                {
                    game.Map.Player.Keys.Remove(ID);
                    CellAction = new StaticAction(true, null);
                    return true;
                }
            }
            return false;
        }

        public Door(int id)
        {
            ID = id;
        }
    }
}
