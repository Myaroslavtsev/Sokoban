using System;
using System.Collections.Generic;
using System.Text;

namespace Sokoban
{
    class Plate : IStaticCellWithID
    {
        public char DataFileChar { get => '_'; }

        public StaticCellType CellType { get => StaticCellType.Plate; }

        public StaticAction CellAction { get; set; }

        public int ID { get; private set; }

        public bool AllowsToEnter(IDynamicCell cell, Game game)
        {
            if (cell is GamePlayer || cell is Box)
                foreach(var staticCellRow in game.Map.StaticLayer)
                    foreach(var staticCell in staticCellRow) 
                        if (staticCell is Door)
                            if ((staticCell as Door).ID == ID)
                            //if (staticCell.GetType().GetProperty("ID").GetValue(staticCell).Equals(ID))
                                staticCell.CellAction = new StaticAction(true, null);
            return true;
        }

        public Plate(int id)
        {
            ID = id;
        }
    }
}
