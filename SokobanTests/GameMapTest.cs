using System.Drawing;
using System.Collections.Generic;
using NUnit.Framework;

namespace Sokoban
{
    [TestFixture]
    public class GameMapTest
    {
        [TestCaseSource("UpdateDynLayerSource")]
        public void UpdateLayerTest(int width, int height, IMapCell initialCell, int expectedX, int expectedY, IMapCell ExpectedCell)
        {
            // arrange
            var map = new GameMap();                        
            map.StaticLayer = new MapLayer(width, height);
            map.DynamicLayer = new MapLayer(width, height);
            map.StaticLayer.AddCell(initialCell);
            // act
            map.StaticLayer.DoCellActions();
            var actualCell = map.StaticLayer.GetByPosition(expectedX, expectedY);
            // assert
            if (ExpectedCell is null)
                Assert.That(actualCell is null);
            else
            {                
                Assert.AreEqual(ExpectedCell.GetType(), actualCell.GetType());
                Assert.AreEqual(ExpectedCell.Position, actualCell.Position);
                Assert.AreEqual(ExpectedCell.CellAction, actualCell.CellAction);
            }
        }

        static readonly object[] UpdateDynLayerSource =
        {
            new object[] // no action => no changes, action cleared
            {
                3, 3,
                new Wall(1, 1) { CellAction = new MapCellAction(new Point(0, 0), false, null) },
                1, 1,
                new Wall(1, 1) { CellAction = null }
            },
            new object[] // transform to null => cell deleted
            {
                3, 3,
                new Wall(1, 1) { CellAction = new MapCellAction(new Point(0, 0), true, null) },
                1, 1,
                null
            },
            new object[] // move by (1, 1) => cell moved
            {
                3, 3,
                new Wall(1, 1) { CellAction = new MapCellAction(new Point(1, 1), false, null) },
                2, 2,
                new Wall(2, 2)
            },
            new object[] // transform to cage => cell transformed
            {
                3, 3,
                new Wall(1, 1) { CellAction = new MapCellAction(new Point(0, 0), 
                    true, 
                    new Cage(2, 2)) },
                2, 2,
                new Cage(2, 2)
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
            map.StaticLayer = new MapLayer(width, height);
            // act
            var actualResult = map.PositionPossible(new System.Drawing.Point(x, y));
            // assert
            Assert.AreEqual(expectedResult, actualResult);
        }

        [TestCaseSource("MovableBoxesSource")]
        public void MovableBoxesTest(int width, int height, GamePlayer player, List<IMapCell> dynamicCells, Point direction, int expectedResult)
        {
            // arrange
            var map = new GameMap();
            map.StaticLayer = new MapLayer(width, height);
            map.DynamicLayer = new MapLayer(width, height);
            map.DynamicLayer.AddCell(player);
            foreach (var cell in dynamicCells)
                map.DynamicLayer.AddCell(cell);
            var newPos = new Point(
                player.Position.X + direction.X,
                player.Position.Y + direction.Y);
            // act
            var actualResult = map.MovableBoxes(newPos, direction, new HashSet<GameOption>());
            // assert
            Assert.AreEqual(expectedResult, actualResult);
        }

        static readonly object[] MovableBoxesSource =
        {
            new object[] // force = 3, boxCount = 2, direction = right  =>  2 
            {   
                5, 1, new GamePlayer(1, 0){ Force = 3 }, 
                new List<IMapCell>{ new Box(2, 0), new Box(3, 0) },
                new Point(1, 0),
                2
            },
            new object[] // force = 2, boxCount = 2, direction = right  =>  2 
            {
                5, 1, new GamePlayer(1, 0){ Force = 2 },
                new List<IMapCell>{ new Box(2, 0), new Box(3, 0) },
                new Point(1, 0),
                2
            },
            new object[] // force = 1, boxCount = 0, direction = right  =>  0 
            {
                5, 1, new GamePlayer(1, 0){ Force = 1 },
                new List<IMapCell>{ },
                new Point(1, 0),
                0
            },
            new object[] // force = 3, boxCount = 2, direction = right, no space  =>  -1
            {
                4, 1, new GamePlayer(1, 0){ Force = 2 },
                new List<IMapCell>{ new Box(2, 0), new Box(3, 0) },
                new Point(1, 0),
                -1
            },
            new object[] // force = 1, boxCount = 2, direction = right  =>  2 
            {
                5, 1, new GamePlayer(1, 0){ },
                new List<IMapCell>{ new Box(2, 0), new Box(3, 0) },
                new Point(1, 0),
                -1
            },
            new object[] // force = 2, boxCount = 2, direction = left  =>  2 
            {
                5, 1, new GamePlayer(3, 0){ Force = 2 },
                new List<IMapCell>{ new Box(1, 0), new Box(2, 0) },
                new Point(-1, 0),
                2
            },
        };
    }
}