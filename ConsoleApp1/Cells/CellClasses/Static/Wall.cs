using System;
using System.Collections.Generic;
using System.Text;

namespace Sokoban
{
    class Wall : IStaticCell
    {
        public StaticCellType CellType { get => StaticCellType.Wall; }

        public char DataFileChar { get => '#'; }

        public StaticAction CellAction { get; set; }

        public bool AllowsToEnter(IDynamicCell cell, GameMap map, HashSet<GameOption> options)
        {
            if (cell is GamePlayer)
            {
                if (options.Contains(GameOption.Iddqd))
                    return true;
                if (map.Player.BombCount > 0)
                {
                    map.Player.BombCount--;
                    CellAction = new StaticAction(true, null);
                    return true;
                }
                return false;
            }                
            return false;
        }
    }
}
