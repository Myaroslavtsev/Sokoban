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

        public bool AllowsToEnter(IDynamicCell cell, Game game)
        {
            if (cell is GamePlayer)
            {
                if (game.GameOptions.Contains(GameOption.Iddqd))
                    return true;
                if (game.Map.Player.BombCount > 0)
                {
                    game.Map.Player.BombCount--;
                    CellAction = new StaticAction(true, null);
                    return true;
                }
                return false;
            }                
            return false;
        }
    }
}
