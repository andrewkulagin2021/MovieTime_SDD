using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System.Diagnostics;

namespace MovieProjectSDD
{
    public static class IMDBAdapter
    {
        public static async Task AddRating(List<Movie> movies)
        {
            foreach (Movie movie in movies)
            {
                string json;
                using (var client = new HttpClient())
                {
                    string title = movie.Title.Replace(' ', '+');                   
                    json = await client.GetStringAsync("http://www.omdbapi.com/?t=" + title + "&y=+&plot=full&apikey=141f3bf8");

                }
                dynamic results = JsonConvert.DeserializeObject<dynamic>(json);
                if (results.Response == "True")
                {
                    movie.ImdbRating = (string)results.imdbRating;
                    movie.ImdbVotes = (string)results.imdbVotes;
                    movie.ImdbID = (string)results.imdbID;
                    movie.ImdbGenre = (string)results.Genre;
                    if ((string)results.Writer != null) movie.Writers = "Writers: " + (string)results.Writer;
                    if ((string)results.Director != null) movie.Directors = "Directors: " + (string)results.Director;
                    if ((string)results.Actors != null) movie.Actors = "Actors: " + (string)results.Actors;
                    if ((string)results.Released != null) movie.ReleaseDateString = (string)results.Released;
                    if ((string)results.Runtime != null) movie.Runtime = (string)results.Runtime;
                    
                    foreach (var rating in results.Ratings)
                    {      
                        if (rating.Source == "Rotten Tomatoes") movie.RTRating = (string)rating.Value;
                    }
                }
                else
                {
                    movie.ImdbRating = "N/A";
                    movie.ImdbVotes = "N/A";
                    movie.ImdbID = "";
                    movie.RTRating = "N/A";
                    movie.ImdbGenre = "";
                }
            }

            // return movies;
        }
    }

}
