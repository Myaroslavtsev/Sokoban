using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Sokoban
{
    [TestFixture]
    class StaticCellTests
    {
        [TestCaseSource("DoorTestCases")]
        public void DoorTest(IDynamicCell enteringCell, List<int> playerKeys, bool expectedResult)
        {
            // arrange
            var door = new Door(1);
            var map = new GameMap();
            map.Player = new GamePlayer(1, 1);
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
        public void BombTest(IDynamicCell enteringCell, int initBombCount, bool expectedResult, int expectedBombCount, StaticAction expextedAction)
        {
            // arrange
            var bomb = new Bomb();
            var map = new GameMap();
            map.Player = new GamePlayer(1, 1);
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
                new StaticAction(true, null)
            },
            new object[] // player enters the door => true, bomb disappears
            {
                new Box(1, 1),
                1,
                true,
                2,
                new StaticAction(true, null)
            },
        };

        [TestCaseSource("KeyTestCases")]
        public void KeyTest(IDynamicCell enteringCell, List<int> initKeys, bool expectedResult, List<int> expectedKeys, StaticAction expextedAction)
        {
            // arrange
            var key = new Key(3);
            var map = new GameMap();
            map.Player = new GamePlayer(1, 1);
            if (enteringCell is GamePlayer)
                (enteringCell as GamePlayer).Keys = initKeys;
            // act
            var actualResult = key.AllowsToEnter(enteringCell, map, new HashSet<GameOption>());
            // assert
            Assert.AreEqual(expectedResult, actualResult);
            Assert.AreEqual(expextedAction.WillTransform, key.CellAction.WillTransform);
            Assert.AreEqual(expextedAction.TransformTo, key.CellAction.TransformTo);
            if (enteringCell is GamePlayer)
                Assert.AreEqual(expectedKeys, (enteringCell as GamePlayer).Keys);
        }

        static readonly object[] KeyTestCases =
        {
            new object[] // not player enters the door => true, bomb disappears
            {
                new Box(1, 1),
                new List<int> { },
                true,
                new List<int> { },
                new StaticAction(true, null)
            },
            new object[] // player enters the door => true, bomb disappears
            {
                new Box(1, 1),
                new List<int> { },
                true,
                new List<int> { 3 },
                new StaticAction(true, null)
            },
            new object[] // player enters the door => true, bomb disappears
            {
                new Box(1, 1),
                new List<int> { 1 },
                true,
                new List<int> { 1, 3 },
                new StaticAction(true, null)
            },
        };

        [TestCaseSource("PlateTestCases")]
        public void PlateTest(IDynamicCell enteringCell, List<int> initKeys, bool expectedResult, List<int> expectedKeys, StaticAction expextedAction)
        {
            // arrange
            var plate = new Plate(3);
            var map = new GameMap();
            map.Player = new GamePlayer(1, 1);
            var gameMapTest = new GameMapTest();
            map.StaticLayer = gameMapTest.EmptyStaticLayer(3, 3);
            // act
            var actualResult = plate.AllowsToEnter(enteringCell, map, new HashSet<GameOption>());
            // assert
            Assert.AreEqual(expectedResult, actualResult);
            Assert.AreEqual(expextedAction.WillTransform, plate.CellAction.WillTransform);
            Assert.AreEqual(expextedAction.TransformTo, plate.CellAction.TransformTo);
            if (enteringCell is GamePlayer)
                Assert.AreEqual(expectedKeys, (enteringCell as GamePlayer).Keys);
        }


    }
}
