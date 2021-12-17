using System;
using System.Collections.Generic;
using System.Text;

namespace Sokoban
{
    public class Door : IStaticCellWithID
    {
        public char DataFileChar { get => '|'; }

        public StaticCellType CellType { get => StaticCellType.Door; }

        public StaticAction CellAction { get; set; }

        public int ID { get; private set; }

        public bool AllowsToEnter(IDynamicCell cell, GameMap map, HashSet<GameOption> options)
        {
            if (cell is GamePlayer)
            {
                if ((cell as GamePlayer).Keys.Contains(ID))
                {
                    (cell as GamePlayer).Keys.Remove(ID);
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
