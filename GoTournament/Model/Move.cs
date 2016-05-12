namespace GoTournament.Model
{
    using System;

    public class Move
    {
        public Move()
        {
        }

        public Move(string letter, string number)
        {
            this.Letter = letter;
            this.Number = number;
            this.Type = MoveType.Normal;
        }

        public MoveType Type { get; private set; }

        public string Letter { get; set; }

        public string Number { get; set; }

        public bool Pass
        {
            get { return this.Type == MoveType.Pass; }
        }

        public bool Normal
        {
            get { return this.Type == MoveType.Normal; }
        }
        
        public static Move SpecialMove(MoveType type)
        {
            return new Move { Type = type };
        }

        public static Move Parse(string data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var clean = data.Replace(" ", string.Empty).Replace("=", string.Empty);
            if (clean.Length < 2)
            {
                return null;
            }

            if (clean.Length < 4)
            {
                // it should be the most often case that's why it is reletivly in the beginning
                return new Move(clean[0].ToString(), clean.Substring(1));
            }

            if (clean.ToLower().Contains("pass"))
            {
                return SpecialMove(MoveType.Pass);
            }

            if (clean.ToLower().Contains("resign"))
            {
                return SpecialMove(MoveType.Resign);
            }

            if (clean.ToLower().Contains("illegalmove"))
            {
                return SpecialMove(MoveType.Illegal);
            }

            if (clean.ToLower().Contains("invalidcoordinate"))
            {
                return SpecialMove(MoveType.Invalid);
            }

            return null;
        }

        public override string ToString()
        {
            if (this.Type == MoveType.Normal)
            {
                return this.Letter + this.Number;
            }

            return this.Type.ToString();
        }
        
        public EndGameReason ToEndGameReason()
        {
            switch (this.Type)
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
    }
}