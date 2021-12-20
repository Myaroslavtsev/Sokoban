using NUnit.Framework;

namespace Sokoban
{
    [TestFixture]
    public class GamePlayerTest
    {
        [TestCase(0, 10, 10, 10, true, TestName = "adding below limit => all moves added")]
        [TestCase(99999, 1, 99999, 0, true, TestName = "limit reached => nothing added")]
        [TestCase(0, 100000, 99999, 99999, true, TestName = "addition over limit => part only added")]
        [TestCase(99000, 2000, 99999, 999, true, TestName = "sum over limit => part only added")]
        [TestCase(10, -2, 10, -2, false, TestName = "negative count cannot be added")]
        [TestCase(10, 0, 10, 0, false, TestName = "zero count cannot be added")]
        public void AddMovesTest(int initMoves, int addMoves, int expectedMoves, int expectedAddMoves, bool expectedResult)
        {
            // arrange
            var player = new GamePlayer(0, 0);
            player.MaxMoves = initMoves;
            // act
            var result = player.AddMoves(ref addMoves);
            // assert
            Assert.AreEqual(expectedResult, result);
            Assert.AreEqual(expectedMoves, player.MaxMoves);
            Assert.AreEqual(expectedAddMoves, addMoves);            
        }

        [TestCase(1, 2, 2, true, TestName = "Normal behavior")]
        [TestCase(1, 200, 100, true, TestName = "Value over limit")]
        [TestCase(1, 0, 1, false, TestName = "Set to zero")]
        [TestCase(1, -2, 1, false, TestName = "Negative value")]
        public void SetForceTets(int initForce, int newForce, int expectedForce, bool expectedResult)
        {
            // arrange
            var player = new GamePlayer(0, 0);
            player.Force = initForce;
            // act
            var result = player.SetForce(newForce);
            // assert
            Assert.AreEqual(expectedResult, result);
            Assert.AreEqual(expectedForce, player.Force);            
        }
    }
}
