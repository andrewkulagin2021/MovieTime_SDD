using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieProjectSDD
{
    public static class Tools
    {
        public static string MapGenre (string genre)
        {
            if (genre.Contains("Action")) return "Action And Adventure";
            if (genre.Contains("Adventure")) return "Action And Adventure";
            if (genre.Contains("Fiction")) return "Action And Adventure";
            if (genre.Contains("Romance")) return "Drama";
            if (genre.Contains("Documentary")) return "Documentary";
            return genre;
        }
        public static string MapRating(string rating)
        {
            if (rating.Contains("CTC")) return "Unrated";
            if (rating.Contains("Ma15+")) return "M";
            if (rating.Contains("MA15+")) return "M";
            if (rating.Contains("N/A")) return "Unrated";
            if (rating.Contains("Not Rated")) return "Unrated";
            if (rating.Contains("Not Yet Rated")) return "Unrated";
            if (rating.Contains("Pg")) return "PG";
            if (rating.Contains("PG-13")) return "PG";
            if (rating.Contains("R18+")) return "R";
            return rating;
        }
    }
}
