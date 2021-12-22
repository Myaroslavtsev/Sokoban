using System.Collections.Generic;
using System.Drawing;

namespace Sokoban
{
    public class GameMap
    {
        public MapLayer StaticLayer { get; set; }        
        public MapLayer DynamicLayer { get; set; }
        public GamePlayer Player { get => DynamicLayer.Player(); }

        public int Width { get => StaticLayer is null ? 0 : StaticLayer.Width;
            set 
            {
                StaticLayer.Width = value;
                DynamicLayer.Width = value;
            }
        }

        public int Height
        {
            get => StaticLayer is null ? 0 : StaticLayer.Height;
            set
            {
                StaticLayer.Height = value;
                DynamicLayer.Height = value;
            }
        }

        public GameMap()
        {         
            StaticLayer = new MapLayer();
            DynamicLayer = new MapLayer();
            DynamicLayer.AddCell(new GamePlayer(0, 0));
        }

        public bool PositionPossible(Point position)
        {
            return position.X >= 0 && position.Y >= 0 && position.X < Width && position.Y < Height;
        }     

        public int MovableBoxes(Point position, Point direction, HashSet<GameOption> options)
        {
            int boxCount = 0;
            while (PositionPossible(position) && (DynamicLayer.GetByPosition(position) is Box))
            {
                boxCount++;
                position = position.Add(direction);
            }
            if (boxCount > Player.Force)
                return -1;
            if (boxCount == 0)
                return 0;
            if (PositionPossible(position) && ((StaticLayer.GetByPosition(position) is null) ||
                StaticLayer.GetByPosition(position).AllowsToEnter(
                DynamicLayer.GetByPosition(position.Substract(direction)), this, options)))
                return boxCount;
            return -1;
        }
    }
}