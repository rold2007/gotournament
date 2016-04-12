namespace GoTournament
{
    public class Move
    {
        public string Letter { get; set; }
        public string Number { get; set; }

        public bool Pass { get; set; }

        public override string ToString()
        {
            if (Pass)
                return "PASS";
            return Letter + Number;
        }

        public Move(string letter, string number)
        {
            Letter = letter;
            Number = number;
        }

        public Move()
        {
            
        }
        
        public static Move PassMove()
        {
            return new Move {Pass = true};
        }

        public static Move Parse(string data)
        {
            var clean = data.Replace(" ","").Replace("=","");
            if (clean.Length < 2)
                return null; 
            if (clean.ToLower().Contains("pass"))
                return PassMove();
            if (clean.Length > 3)
                return null;
            return new Move(clean[0].ToString(), clean.Substring(1));
        }
    }
}