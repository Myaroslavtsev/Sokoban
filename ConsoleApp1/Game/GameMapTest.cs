using System.Drawing;
using System.Collections.Generic;
using NUnit.Framework;

namespace Sokoban
{
    [TestFixture]
    class GameMapTest
    {
        [TestCase(0, 0)]
        [TestCase(1, 1)]
        [TestCase(5, 3)]
        public void GenerateDynamicLayerTest(int width, int height)
        {
            // arrange
            var map = new GameMap();
            map.StaticLayer = EmptyStaticLayer(width, height);
            var expectedDynamicLayer = EmptyDynamicLayer(width, height);
            // act
            map.GenerateDynamicLayer();
            // assert
            Assert.AreEqual(height, map.Height);
            Assert.AreEqual(width, map.Width);
            Assert.AreEqual(height, map.DynamicLayer.Count);
            for (var y = 0; y < height; y++)
                Assert.AreEqual(width, map.DynamicLayer[y].Count);
            Assert.AreEqual(expectedDynamicLayer, map.DynamicLayer);
        }

        [TestCaseSource("UpdateDynLayerSource")]
        public void UpdateDynamicLayerTest(List<List<IStaticCell>> staticLayer, 
            List<List<IDynamicCell>> expectedDynamicLayer)
        {
            // arrange
            var map = new GameMap();                        
            map.StaticLayer = staticLayer;
            map.GenerateDynamicLayer();
            // act
            map.UpdateDynamicLayer();
            // assert
            Assert.AreEqual(expectedDynamicLayer.Count, map.DynamicLayer.Count);
            for (var y = 0; y < map.Height; y++)
            {
                Assert.AreEqual(expectedDynamicLayer[y].Count, map.DynamicLayer[y].Count);
                for (var x = 0; x < map.Width; x++)
                    Assert.IsInstanceOf(expectedDynamicLayer[y][x].GetType(), map.DynamicLayer[y][x]);
            }
        }

        static readonly object[] UpdateDynLayerSource =
        {
            new object[]
            {
                new List<List<IStaticCell>>
                {
                    new List<IStaticCell> { null,        new Wall(), new Wall(), new Wall(), new Wall(), new Wall() },
                    new List<IStaticCell> { new Wall(),  new Wall(), null,       null,      new Plate(3), new Door(5) },
                    new List<IStaticCell> { new Wall(),  new Cage(), null,       new Wall(), new Bomb(), new Wall() },
                    new List<IStaticCell> { new Wall(),  new Cage(), null,       null,       new Key(5), new Door(3) },
                    new List<IStaticCell> { new Wall(),  new Wall(), new Wall(), new Wall(), new Wall(), new Wall() },
                },            
                new List<List<IDynamicCell>>
                {
                    new List<IDynamicCell> { null,      null,       null,       null,       null,       null },
                    new List<IDynamicCell> { null,      null,       null, new GamePlayer(3, 1), null,   null },
                    new List<IDynamicCell> { null,   new Box(1, 2), null,       null,       null,       null },
                    new List<IDynamicCell> { null,      null,    new Box(2, 3), new Box(3, 3), null,    null },
                    new List<IDynamicCell> { null,      null,       null,       null,       null,       null },
                }
            }
        };

        [TestCase(1, 1, 0, 0, true, TestName = "small layer")]
        [TestCase(5, 4, 2, 2, true, TestName = "center of large layer")]
        [TestCase(5, 4, 4, 3, true, TestName = "corner")]
        [TestCase(5, 4, 3, 0, true, TestName = "top side")]
        [TestCase(5, 4, 3, 3, true, TestName = "bottom side")]
        [TestCase(5, 4, 4, 2, true, TestName = "rigth side")]
        [TestCase(5, 4, -1, 2, false, TestName = "negative x")]
        [TestCase(5, 4, 5, 1, false, TestName = "too big x")]
        public void PositionPossibleTest(int width, int height, int x, int y, bool expectedResult)
        {
            // arrange
            var map = new GameMap();
            map.StaticLayer = EmptyStaticLayer(width, height);
            // act
            var actualResult = map.PositionPossible(new System.Drawing.Point(x, y));
            // assert
            Assert.AreEqual(expectedResult, actualResult);
        }

        [TestCaseSource("MovableBoxesSource")]
        public void MovableBoxesTest(int width, int height, GamePlayer player, List<IDynamicCell> dynamicCells, Point direction, int expectedResult)
        {
            // arrange
            var map = new GameMap();
            map.StaticLayer = EmptyStaticLayer(width, height);
            map.GenerateDynamicLayer();
            map.DynamicCells = dynamicCells;
            map.Player = player;
            map.UpdateDynamicLayer();
            // act
            var actualResult = map.MovableBoxes(map.Player.Position, direction, new HashSet<GameOption>());
            // assert
            Assert.AreEqual(expectedResult, actualResult);
        }

        static readonly object[] MovableBoxesSource =
        {
            new object[] // force = 3, boxCount = 2, direction = right  =>  2 
            {   
                5, 1, new GamePlayer(1, 0){ Force = 3 }, 
                new List<IDynamicCell>{ new Box(2, 0), new Box(3, 0) },
                new Point(1, 0),
                2
            },
            new object[] // force = 2, boxCount = 2, direction = right  =>  2 
            {
                5, 1, new GamePlayer(1, 0){ Force = 2 },
                new List<IDynamicCell>{ new Box(2, 0), new Box(3, 0) },
                new Point(1, 0),
                2
            },
            new object[] // force = 1, boxCount = 0, direction = right  =>  0 
            {
                5, 1, new GamePlayer(1, 0){ Force = 1 },
                new List<IDynamicCell>{ },
                new Point(1, 0),
                0
            },
            new object[] // force = 3, boxCount = 2, direction = right, no space  =>  -1
            {
                4, 1, new GamePlayer(1, 0){ Force = 2 },
                new List<IDynamicCell>{ new Box(2, 0), new Box(3, 0) },
                new Point(1, 0),
                -1
            },
            new object[] // force = 1, boxCount = 2, direction = right  =>  2 
            {
                5, 1, new GamePlayer(1, 0){ },
                new List<IDynamicCell>{ new Box(2, 0), new Box(3, 0) },
                new Point(1, 0),
                -1
            },
            new object[] // force = 2, boxCount = 2, direction = left  =>  2 
            {
                5, 1, new GamePlayer(3, 0){ Force = 2 },
                new List<IDynamicCell>{ new Box(1, 0), new Box(2, 0) },
                new Point(-1, 0),
                2
            },
        };

        private List<List<IStaticCell>> EmptyStaticLayer(int width, int height)
        {
            var result = new List<List<IStaticCell>>(height);
            for (var y = 0; y < height; y++)
            {
                result.Add(new List<IStaticCell>(width));
                for (var x = 0; x < width; x++)
                    result[y].Add(null);
            }
            return result;
        }

        private List<List<IDynamicCell>> EmptyDynamicLayer(int width, int height)
        {
            var result = new List<List<IDynamicCell>>(height);
            for (var y = 0; y < height; y++)
            {
                result.Add(new List<IDynamicCell>(width));
                for (var x = 0; x < width; x++)
                    result[y].Add(null);
            }
            return result;
        }
    }
}