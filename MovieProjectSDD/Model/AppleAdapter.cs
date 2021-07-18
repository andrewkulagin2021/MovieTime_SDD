using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MovieProjectSDD
{
    public  class AppleAdapter: IMovieAdapter
    {      
        public string Name { get; set; } = "Apple";
        public  async Task<List<Movie>> GetMovies()
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            string xml;
            using (var client = new HttpClient())
            {
                xml = await client.GetStringAsync("http://trailers.apple.com/trailers/home/xml/current.xml");
            }
            List<Movie> movies = new List<Movie>();
            var movies_xml = XDocument.Parse(xml);
            var movie_infos = movies_xml.XPathSelectElements("//movieinfo");
            foreach (var movie_xml in movie_infos)
            {
                Movie m = new Movie();
                m.Adapters.Add("Apple");
                string appleURL = Regex.Split(movie_xml.XPathSelectElement("poster/xlarge").Value, @"\/images")[0];                            
                m.AdapterUrls.Add(appleURL);
                m.Title = textInfo.ToTitleCase(movie_xml.XPathSelectElement("info/title").Value).Trim();
                m.Description = movie_xml.XPathSelectElement("info/description").Value;
                m.PosterUrl = movie_xml.XPathSelectElement("poster/xlarge").Value;
                m.TrailerUrl = movie_xml.XPathSelectElement("preview/large").Value;
                m.Genre = Tools.MapGenre(textInfo.ToTitleCase(movie_xml.XPathSelectElement("genre/name").Value).Trim());
                string date = movie_xml.XPathSelectElement("info/releasedate").Value;
                m.Writers = "Writers: N/A";
                m.Directors = "Directors: " + movie_xml.XPathSelectElement("info/director").Value;
                m.Actors = "Actors: N/A";
                m.ReleaseDateString = movie_xml.XPathSelectElement("info/releasedate").Value;
                m.Runtime = movie_xml.XPathSelectElement("info/runtime").Value;
                m.Content_Rating = Tools.MapRating(textInfo.ToTitleCase(movie_xml.XPathSelectElement("info/rating").Value).Trim());
                if (date != "") m.ReleaseDate = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                if (m.Title != "" && m.Genre != "") movies.Add(m);
            }
            return movies;
        }
    }

}
