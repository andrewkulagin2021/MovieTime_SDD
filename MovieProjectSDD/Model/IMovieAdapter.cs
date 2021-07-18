using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieProjectSDD
{
    public interface IMovieAdapter
    {
        //bool IsEnabled { get; set; }
        string Name { get; set; }
        Task<List<Movie>> GetMovies();
    }
}
