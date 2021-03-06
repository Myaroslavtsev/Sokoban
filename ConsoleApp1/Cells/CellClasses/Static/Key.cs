using System;
using System.Collections.Generic;
using System.Text;

namespace Sokoban
{
    class Key : IStaticCellWithID
    {
        public char DataFileChar { get => '+'; }

        public StaticCellType CellType { get => StaticCellType.Key; }

        public StaticAction CellAction { get; set; }

        public int ID { get; private set; }

        public Key(int id)
        {
            ID = id;
        }

        public bool AllowsToEnter(IDynamicCell cell, GameMap map, HashSet<GameOption> options)
        {
            if (cell.CellType == DynamicCellType.Player)            
                map.Player.Keys.Add(ID);            
            CellAction = new StaticAction(true, null);
            return true;
        }  
    }
}
