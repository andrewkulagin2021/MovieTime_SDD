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
using HtmlAgilityPack;

namespace MovieProjectSDD
{
    public  class SBSAdapter : IMovieAdapter
    {
        //public bool IsEnabled { get; set; } = true;
        public string Name { get; set; } = "SBS on Demand";
        public async Task<List<Movie>> GetMovies()
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            string json;
            using (var client = new HttpClient())
            {
                json = await client.GetStringAsync("https://www.sbs.com.au/api/v3/video_feed/f/Bgtm9B/sbs-section-programs?context=odwebsite&byCategories=Film%2CFilm%2FAction+Adventure%7CFilm%2FAnimation%7CFilm%2FBiography%7CFilm%2FClassic%7CFilm%2FComedy%7CFilm%2FDance%7CFilm%2FDocumentary+Feature%7CFilm%2FDrama%7CFilm%2FFantasy%7CFilm%2FHistory%7CFilm%2FHorror%7CFilm%2FMartial+Arts%7CFilm%2FMystery%2FCrime%7CFilm%2FRomance%7CFilm%2FRomantic+Comedy%7CFilm%2FScience+Fiction%7CFilm%2FThriller%7CFilm%2FWar%7C%21Film%2FShort+Film&sort=pubDate%7Cdesc");
            }
            dynamic results = JsonConvert.DeserializeObject<dynamic>(json);
            List<Movie> movies = new List<Movie>();
            foreach (var mov in results.itemListElement)
            {
                Movie m = new Movie();
                m.Title = textInfo.ToTitleCase((string)mov.name).Trim();
                m.Description = (string)mov.description;
                m.PosterUrl = (string)mov.thumbnails[7].contentUrl;
                m.Adapters.Add("SBS on Demand");
                m.AdapterUrls.Add((string)mov.id);
                m.Genre = Tools.MapGenre(textInfo.ToTitleCase((string)mov.taxonomy.genre[0].name).Trim());
                m.Writers = "Writers: N/A";
                m.Directors = "Directors: N/A";
                m.Actors = "Actors: N/A";
                m.Content_Rating = Tools.MapRating(textInfo.ToTitleCase((string)mov.contentRating).Trim());
                m.ReleaseDateString = (string)mov.datePublished;
                m.Runtime = (string)mov.duration + " seconds";
                string date = (string)mov.datePublished;
                if (date != "")
                {
                    m.ReleaseDate = DateTime.ParseExact(date, "dd/MM/yyyy hh:mm:ss", new CultureInfo("en-US"));
                }
                if (m.Title != "" && m.Genre != "") movies.Add(m);
            }

            return movies;
        }
 
    }
    
}
