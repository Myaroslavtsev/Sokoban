using NUnit.Framework;

namespace Sokoban
{
    [TestFixture]
    public class GamePlayerTest
    {
        [TestCase(0, 10, 10, 10, true, TestName = "Typical behavior")]
        [TestCase(0, 100000, 99999, 99999, true, TestName = "add over limit")]
        [TestCase(99000, 2000, 99999, 999, true, TestName = "sum over limit")]
        [TestCase(10, -2, 10, -2, false, TestName = "add negative count")]
        public void AddMovesTest(int initMoves, int addMoves, int expectedMoveCount, int expectedAddMoves, bool expectedResult)
        {
            // arrange
            var player = new GamePlayer(0, 0);
            player.Moves = initMoves;
            // act
            var result = player.AddMoves(ref addMoves);
            // assert
            Assert.AreEqual(expectedResult, result);
            Assert.AreEqual(expectedMoveCount, player.Moves);
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
