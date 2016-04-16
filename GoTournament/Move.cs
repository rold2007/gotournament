using System;

namespace GoTournament
{
    public enum MoveType { None, Normal, Pass, Resign, Invalid, Illegal }

    public class Move
    {

        public MoveType Type { get; private set; }
        public string Letter { get; set; }
        public string Number { get; set; }

        public bool Pass
        {
            get { return Type == MoveType.Pass; }
        }

        public bool Resign
        {
            get { return Type == MoveType.Resign; }
        }

        public bool Invalid
        {
            get { return Type == MoveType.Invalid; }
        }

        public bool Illegal
        {
            get { return Type == MoveType.Illegal; }
        }

        public bool Normal
        {
            get { return Type == MoveType.Normal; }
        }
        
        public override string ToString()
        {
            if (Type == MoveType.Normal)
                return Letter + Number;
            return Type.ToString();
        }

        public Move(string letter, string number)
        {
            Letter = letter;
            Number = number;
            Type = MoveType.Normal;
        }

        public EndGameReason ToEndGameReason()
        {
            switch (Type)
            {
                case MoveType.Pass:
                    return EndGameReason.ConsecutivePass;
                case MoveType.Resign:
                    return EndGameReason.Resign;
                case MoveType.Invalid:
                case MoveType.Illegal:
                    return EndGameReason.InvalidMove;
                default:
                    return EndGameReason.None;
            }
        }

        public Move()
        {
        }

        public static Move SpecialMove(MoveType type)
        {
            return new Move {Type = type};
        }


        public static Move Parse(string data)
        {
            var clean = data.Replace(" ", "").Replace("=", "");
            if (clean.Length < 2)
                return null;
            if (clean.Length < 4) //it should be the most often case that's why it is reletivly in the beginning
                return new Move(clean[0].ToString(), clean.Substring(1));
            if (clean.ToLower().Contains("pass"))
                return SpecialMove(MoveType.Pass);
            if (clean.ToLower().Contains("resign"))
                return SpecialMove(MoveType.Resign);
            if (clean.ToLower().Contains("illegal move"))
                return SpecialMove(MoveType.Illegal);
            if (clean.ToLower().Contains("invalid coordinate"))
                return SpecialMove(MoveType.Invalid);
            return null;
        }
    }
}