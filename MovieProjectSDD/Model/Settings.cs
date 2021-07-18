using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieProjectSDD
{
    public static class Settings
    {
        public static List<IMovieAdapter> MovieAdapters { get; set; } = new List<IMovieAdapter>();
        public static List<string> FavouriteMovies { get; set; } = new List<string>();
        public static bool UpdateMovies { get; set; } = false;
        public static bool UpdateFavourites { get; set; } = false;
    }
}
