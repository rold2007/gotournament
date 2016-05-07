namespace GoTournament.UnitTest
{
    using System;
    using System.Collections.Generic;

    using GoTournament.Model;

    using Xunit;

    public class MoveTests
    {
        [Fact]
        public void MoveSpecialMoveTest()
        {
            var move = Move.SpecialMove(MoveType.Pass);
            Assert.NotNull(move);
            Assert.Equal(MoveType.Pass, move.Type);
            Assert.Equal("Pass", move.ToString());
            Assert.True(move.Pass);
            Assert.False(move.Normal);

            foreach (MoveType type in Enum.GetValues(typeof(MoveType)))
            {
                move = Move.SpecialMove(type);
                Assert.NotNull(move);
                Assert.Equal(type, move.Type);
            }
        }

        [Fact]
        public void MoveParseTest()
        {
            Move move = null;
            try
            {
                move = Move.Parse(null);
                Assert.True(false, "Should fail on previous statement");
            }
            catch (Exception ex)
            {
                Assert.IsType(typeof(ArgumentNullException), ex);
                Assert.Equal("Value cannot be null.\r\nParameter name: data", ex.Message);
            }
            move = Move.Parse("Z19");
            Assert.Equal("Z", move.Letter);
            Assert.Equal("19", move.Number);
            Assert.Equal("Z19", move.ToString());

            move = Move.Parse("= ");
            Assert.Null(move);
            var dict = new Dictionary<string, MoveType>(4)
                           {
                               { "PASS", MoveType.Pass },
                               { "rEsign", MoveType.Resign },
                               { "=illegal move", MoveType.Illegal },
                               { "in=valid coord=inate=", MoveType.Invalid }
                           };
            foreach (var element in dict)
            {
                move = Move.Parse(element.Key);
                Assert.NotNull(move);
                Assert.Equal(element.Value, move.Type);
            }
            move = Move.Parse("love");
            Assert.Null(move);
        }

        [Fact]
        public void MoveToEndGameReasonTest()
        {
            Move move = null;
            var dict = new Dictionary<MoveType, EndGameReason>(4)
                           {
                               { MoveType.Pass, EndGameReason.ConsecutivePass },
                               { MoveType.Resign, EndGameReason.Resign },
                               { MoveType.Illegal, EndGameReason.InvalidMove },
                               { MoveType.Invalid, EndGameReason.InvalidMove },
                               { MoveType.Normal, EndGameReason.None }
                           };
            foreach (var element in dict)
            {
                move = Move.SpecialMove(element.Key);
                Assert.NotNull(move);
                Assert.Equal(element.Value, move.ToEndGameReason());
            }
        }
    }
}