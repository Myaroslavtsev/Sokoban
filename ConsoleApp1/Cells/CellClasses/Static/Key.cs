using System;
using System.Collections.Generic;
using System.Text;

namespace Sokoban
{
    class Key : IStaticCell
    {
        public char DataFileChar { get => '+'; }

        public StaticCellType CellType { get => StaticCellType.Key; }

        public StaticAction CellAction { get; set; }

        public int ID { get; private set; }

        public Key(int id)
        {
            ID = id;
        }

        public bool AllowsToEnter(IDynamicCell cell, Game game)
        {
            if (cell.CellType == DynamicCellType.Player)            
                game.Map.Player.Keys.Add(ID);            
            CellAction = new StaticAction(true, null);
            return true;
        }  
    }
}
