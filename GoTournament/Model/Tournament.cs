﻿namespace GoTournament.Model
{
   using System.Collections.Generic;

   public class Tournament
    {
        public string Name { get; set; }

        public int BoardSize { get; set; }

        public Dictionary<string, int> Ratings { get; set; }

        public RatingCalculationType RatingType { get; set; }
    }
}