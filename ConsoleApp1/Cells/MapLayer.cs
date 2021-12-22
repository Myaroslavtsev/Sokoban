using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Sokoban
{
    public class MapLayer
    {
        public List<IMapCell> Cells { get; private set; }
        public int Width { get; set; }
        public int Height { get; set; }

        private int playerIndex;

        public bool AddCell(IMapCell cell)
        {
            if (GetByPosition(cell.Position) is null)
            {
                Cells.Add(cell);
                Cells.Sort();
                if (playerIndex > -1 || cell is GamePlayer)
                    playerIndex = FindPlayer();
                return true;
            }
            return false;
        }

        public IMapCell GetByPosition(int x, int y)
        {
            return GetByPosition(new Point(x, y));
        }

        public GamePlayer Player()
        {
            return playerIndex > -1 ? Cells[playerIndex] as GamePlayer : null;
        }

        public IMapCell GetByPosition(Point pos)
        {            
            var left = -1;
            var right = Cells.Count;
            while (right - left > 1)
            {
                var middle = (left + right) / 2;
                if (Cells[middle].Position == pos)
                    return Cells[middle];
                if (Cells[middle].CompareTo(pos) > 0)
                    right = middle;
                else
                    left = middle;
            }
            return null;
        }

        public bool DoCellActions()
        {
            var mapChanged = false;
            for(var i = 0; i < Cells.Count; i++)
            {
                if (!(Cells[i].CellAction is null))
                {
                    if (Cells[i].CellAction.WillTransform && Cells[i].CellAction.TransformTo is null)
                    {
                        Cells.RemoveAt(i);
                        mapChanged = true;
                    }
                    else
                    {
                        if (Cells[i].CellAction.Move.X != 0 || Cells[i].CellAction.Move.Y != 0)
                        {
                            Cells[i].Position = Cells[i].Position.Add(Cells[i].CellAction.Move);
                            if (Cells[i] is GamePlayer)
                                (Cells[i] as GamePlayer).CountMove();                            
                            mapChanged = true;
                        }
                        if (Cells[i].CellAction.WillTransform)
                        {
                            Cells[i] = Cells[i].CellAction.TransformTo;                                                              
                            mapChanged = true;
                        }
                        Cells[i].CellAction = null;
                    }
                }                    
            }
            if (mapChanged)
            {
                Cells.Sort();
                if (playerIndex > -1)
                    playerIndex = FindPlayer();
            }
            return mapChanged;
        }        

        public MapLayer()
        {
            Cells = new List<IMapCell>();
            playerIndex = -1;
        }

        public MapLayer(int width, int height)
        {
            Width = width;
            Height = height;
            Cells = new List<IMapCell>(width * height);
            playerIndex = -1;
        }

        private int FindPlayer()
        {
            for (var i = 0; i < Cells.Count; i++)
                if (Cells[i] is GamePlayer) 
                    return i;
            return -1;
        }
    }
}
