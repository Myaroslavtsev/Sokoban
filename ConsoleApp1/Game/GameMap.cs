using System;
using System.Collections.Generic;
using System.Drawing;

namespace Sokoban
{
    class GameMap
    {
        public List<List<IStaticCell>> StaticLayer { get; private set; }
        public List<IDynamicCell> DynamicCells { get; private set; }
        public List<List<IDynamicCell>> DynamicLayer { get; private set; }
        public GamePlayer Player { get; private set; }
        public int Height { get => StaticLayer is null ? 0 : StaticLayer.Count; }
        public int Width { get
            {
                if (StaticLayer is null || StaticLayer.Count == 0 || StaticLayer[0] is null)
                    return 0;
                return StaticLayer[0].Count; 
            }
        }

        public GameMap()
        {
            StaticLayer = new List<List<IStaticCell>>();
            DynamicCells = new List<IDynamicCell>();
            Player = new GamePlayer(0, 0);     
            
        }

        public void MakeDynamicLayer()
        {
            for (var y = 0; y < Height; y++)
                for (var x = 0; x < Width; x++)
                    DynamicLayer[y][x] = null;
            foreach (var dynamicCell in DynamicCells)
                DynamicLayer[dynamicCell.Position.Y][dynamicCell.Position.X] = dynamicCell;
            DynamicLayer[Player.Position.Y][Player.Position.X] = Player;
        }

        public void GenerateDynamicLayer()
        {
            DynamicLayer = new List<List<IDynamicCell>>();
            for (var y = 0; y < Height; y++)
            {
                DynamicLayer.Add(new List<IDynamicCell>(Width));
                for (var x = 0; x < Width; x++)
                    DynamicLayer[y].Add(null);
            }
        }

        public bool Possible(Point position)
        {
            return position.X >= 0 && position.Y >= 0 && position.X < Width && position.Y < Height;
        }

        public void GenerateTestMap()
        {
            StaticLayer = new List<List<IStaticCell>> {
                new List<IStaticCell> { null,        new Wall(), new Wall(), new Wall(), new Wall(), new Wall() },
                new List<IStaticCell> { new Wall(),  new Wall(), null,       null,      new Plate(3), new Door(5) },
                new List<IStaticCell> { new Wall(),  new Cage(), null,       new Wall(), new Bomb(), new Wall() },
                new List<IStaticCell> { new Wall(),  new Cage(), null,       null,       new Key(5), new Door(3) },
                new List<IStaticCell> { new Wall(),  new Wall(), new Wall(), new Wall(), new Wall(), new Wall() },
            };
            DynamicCells.Add(new Box(2, 3));
            DynamicCells.Add(new Box(3, 3));
            DynamicCells.Add(new Box(1, 2));
            Player.Position = new Point(3, 1);
            Player.BombCount += 15;
            GenerateDynamicLayer();
            MakeDynamicLayer();            
        }
    }
}
