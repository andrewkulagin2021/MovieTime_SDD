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
using System.IO;

namespace MovieProjectSDD
{
    public  class EventAdapter: IMovieAdapter
    {
        //public  bool IsEnabled { get; set; } = true;
        public string Name { get; set; } = "Event Cinemas";
        public async Task<List<Movie>> GetMovies()
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            string json;
            using (var client = new HttpClient())
            {
                json = await client.GetStringAsync("https://www.eventcinemas.com.au/Movies/GetNowShowing");
            }
            dynamic results = JsonConvert.DeserializeObject<dynamic>(json);
            List<Movie> movies = new List<Movie>();
            foreach (var mov in results.Data.Movies)
            {
                Movie m = new Movie();
                m.Adapters.Add("Event Cinemas");
                m.AdapterUrls.Add("https://www.eventcinemas.com.au" + mov.MovieUrl);
                m.Title = textInfo.ToTitleCase((string)mov.Name).Trim();
                if (m.Title.Contains("IFF - ")) m.Title = m.Title.Substring(6);
                m.Description = (string)mov.Synopsis;

                m.PosterUrl = (string)mov.LargePosterUrl;
                var urlValid = await IsImageExists(m.PosterUrl);
                if (!urlValid) m.PosterUrl = "https://cdn.eventcinemas.com.au/cdn/content/img/unavailable_poster105x50.jpg";

                //  m.TrailerUrl = movie_xml.XPathSelectElement("preview/large").Value;
                m.Genre = Tools.MapGenre(textInfo.ToTitleCase((string)mov.Genres).Trim());
                m.Writers = "Writers: N/A";
                m.Directors = "Directors: " + (string)mov.Director;
                m.Actors = "Actors: "   + (string)mov.MainCast;
                m.ReleaseDateString = (string)mov.ReleasedAt;
                m.Runtime = (string)mov.RunningTime + " minutes";
                m.Content_Rating = Tools.MapRating(textInfo.ToTitleCase((string)mov.Rating).Trim());
                string date = (string)mov.ReleasedAt;
                if (date != "") m.ReleaseDate = DateTime.ParseExact(date, "yyyy-MM-ddT00:00", CultureInfo.InvariantCulture);
                if (m.Title != "" && m.Genre != "") movies.Add(m);
            }

            return movies;
        }
        static async Task<bool> IsImageExists(string url)
        {
            HttpClient httpClient = new HttpClient();
            var result = await httpClient.SendAsync(new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Head,
            });

            return result.IsSuccessStatusCode;
        }

    }

}
