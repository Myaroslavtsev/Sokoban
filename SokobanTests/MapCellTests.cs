using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Sokoban
{
    [TestFixture]
    class MapCellTests
    {
        [TestCaseSource("DoorTestCases")]
        public void DoorTest(IMapCell enteringCell, List<int> playerKeys, bool expectedResult)
        {
            // arrange
            var door = new Door(0, 0, 1);
            var map = new GameMap();
            map.DynamicLayer.AddCell(new GamePlayer(1, 1));
            if (enteringCell is GamePlayer)
                (enteringCell as GamePlayer).Keys = playerKeys;
            // act
            var actualResult = door.AllowsToEnter(enteringCell, map, new HashSet<GameOption>());
            // assert
            Assert.AreEqual(expectedResult, actualResult);
        }

        static readonly object[] DoorTestCases =
        {
            new object[] // not player enters the door => false
            {
                new Box(1, 1),
                new List<int>(),
                false
            },
            new object[] // player with no keys enters the door => false
            {
                new GamePlayer(1, 1),
                new List<int>(),
                false
            },
            new object[] // player with worng keys enters the door => false
            {
                new GamePlayer(1, 1),
                new List<int>{ 0, 3, 7 },
                false
            },
            new object[] // player with valid keys enters the door => true
            {
                new GamePlayer(1, 1),
                new List<int>{ 1 },
                true
            }
        };

        [TestCaseSource("BombTestCases")]
        public void BombTest(IMapCell enteringCell, int initBombCount, bool expectedResult, int expectedBombCount, MapCellAction expextedAction)
        {
            // arrange
            var bomb = new Bomb(0, 0);
            var map = new GameMap();
            map.DynamicLayer.AddCell(new GamePlayer(1, 1));
            if (enteringCell is GamePlayer)
                (enteringCell as GamePlayer).BombCount = initBombCount;
            // act
            var actualResult = bomb.AllowsToEnter(enteringCell, map, new HashSet<GameOption>());
            // assert
            Assert.AreEqual(expectedResult, actualResult);
            Assert.AreEqual(expextedAction.WillTransform, bomb.CellAction.WillTransform);
            Assert.AreEqual(expextedAction.TransformTo, bomb.CellAction.TransformTo);
            if (enteringCell is GamePlayer)
                Assert.AreEqual(expectedBombCount, (enteringCell as GamePlayer).BombCount);
        }

        static readonly object[] BombTestCases =
        {
            new object[] // not player enters the door => true, bomb disappears
            {
                new Box(1, 1),
                0,
                true,
                0,
                new MapCellAction(new Point(0, 0), true, null)
            },
            new object[] // player enters the door => true, bomb disappears, player gets bomb
            {
                new Box(1, 1),
                1,
                true,
                2,
                new MapCellAction(new Point(0, 0), true, null)
            },
        };

        [TestCaseSource("KeyTestCases")]
        public void KeyTest(IMapCell enteringCell, List<int> initKeys, bool expectedResult, List<int> expectedKeys, MapCellAction expectedAction)
        {
            // arrange
            var key = new Key(0, 0, 3);
            var map = new GameMap();
            map.DynamicLayer.AddCell(new GamePlayer(1, 1));
            if (enteringCell is GamePlayer)
                (enteringCell as GamePlayer).Keys = initKeys;
            // act
            var actualResult = key.AllowsToEnter(enteringCell, map, new HashSet<GameOption>());
            // assert
            Assert.AreEqual(expectedResult, actualResult);
            Assert.AreEqual(expectedAction.WillTransform, key.CellAction.WillTransform);
            Assert.AreEqual(expectedAction.TransformTo, key.CellAction.TransformTo);
            if (enteringCell is GamePlayer)
                Assert.AreEqual(expectedKeys, (enteringCell as GamePlayer).Keys);
        }

        static readonly object[] KeyTestCases =
        {
            new object[] // not player moves over key => true, key disappears
            {
                new Box(1, 1),
                new List<int> { },
                true,
                new List<int> { },
                new MapCellAction(new Point(0, 0), true, null)
            },
            new object[] // player enters the door => true, key disappears
            {
                new GamePlayer(1, 1),
                new List<int> { },
                true,
                new List<int> { 3 },
                new MapCellAction(new Point(0, 0), true, null)
            },
            new object[] // player enters the door => true, key disappears
            {
                new GamePlayer(1, 1),
                new List<int> { 1 },
                true,
                new List<int> { 1, 3 },
                new MapCellAction(new Point(0, 0), true, null)
            },
        };

        [TestCaseSource("PlateTestCases")]
        public void PlateTest(IMapCell enteringCell, int doorID, bool expectedResult, MapCellAction expectedPlateAction, MapCellAction expectedDoorAction)
        {
            // arrange
            var plate = new Plate(0, 0, 3);
            var map = new GameMap();
            map.DynamicLayer.AddCell(new GamePlayer(1, 1));
            var gameMapTest = new GameMapTest();
            map.StaticLayer = new MapLayer(3, 3);
            map.StaticLayer.AddCell(new Door(2, 1, doorID));
            // act
            var actualResult = plate.AllowsToEnter(enteringCell, map, new HashSet<GameOption>());
            // assert
            Assert.AreEqual(expectedResult, actualResult);
            if (expectedPlateAction is null)
                Assert.That(plate.CellAction is null);
            else
            {
                Assert.AreEqual(expectedPlateAction.WillTransform, plate.CellAction.WillTransform);
                Assert.AreEqual(expectedPlateAction.TransformTo, plate.CellAction.TransformTo);
            }
            if (expectedDoorAction is null)
                Assert.That(map.StaticLayer.GetByPosition(2, 1).CellAction is null);
            else
            {
                Assert.AreEqual(expectedDoorAction.WillTransform, 
                    map.StaticLayer.GetByPosition(2, 1).CellAction.WillTransform);
                Assert.AreEqual(expectedDoorAction.TransformTo, 
                    map.StaticLayer.GetByPosition(2, 1).CellAction.TransformTo);
            }
        }

        static readonly object[] PlateTestCases =
        {
            new object[] // box enters, IDs not equal => nothing transforms
            {
                new Box(1, 1),
                1,
                true,
                null,
                null
            },
            new object[] // box enters, IDs are equal => door transforms
            {
                new Box(1, 1),
                3,
                true,
                null,
                new MapCellAction(new Point(0, 0), true, null)
            },
            new object[] // player enters, IDs not equal => nothing transforms
            {
                new GamePlayer(1, 1),
                1,
                true,
                null,
                null
            },
            new object[] // player enters, IDs are equal => door transforms
            {
                new GamePlayer(1, 1),
                3,
                true,
                null,
                new MapCellAction(new Point(0, 0), true, null)
            }
        };

        [TestCaseSource("WallTestCases")]
        public void WallTest(IMapCell enteringCell, int initBombCount, bool expectedResult, int expectedBombCount, MapCellAction expectedAction)
        {
            // arrange
            var wall = new Wall(0, 0);
            var map = new GameMap();
            if (enteringCell is GamePlayer)
                (enteringCell as GamePlayer).BombCount = initBombCount;
            // act
            var actualResult = wall.AllowsToEnter(enteringCell, map, new HashSet<GameOption>());
            // assert
            Assert.AreEqual(expectedResult, actualResult);
            if (enteringCell is GamePlayer)
                Assert.AreEqual(expectedBombCount, (enteringCell as GamePlayer).BombCount);
            if (expectedAction is null)
                Assert.That(wall.CellAction is null);
            else
            {
                Assert.AreEqual(expectedAction.WillTransform, wall.CellAction.WillTransform);
                Assert.AreEqual(expectedAction.TransformTo, wall.CellAction.TransformTo);
            }
        }

        static readonly object[] WallTestCases =
        {
            new object[] // box enters => false
            {
                new Box(1, 1),
                0,
                false,
                0,
                null
            },
            new object[] // player without bombs enters => false
            {
                new GamePlayer(1, 1),
                0,
                false,
                0,
                null
            },
            new object[] // player with bomb enters => true, wall disappears
            {
                new GamePlayer(1, 1),
                3,
                true,
                2,
                new MapCellAction(new Point(0, 0), true, null)
            }
        };
    }
}
