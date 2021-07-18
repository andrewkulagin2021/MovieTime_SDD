using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MovieProjectSDD
{
    public class Movie : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public string Title { get; set; }
        public string TitleShort
        {
            get
            {
                if (Title.Length < 27) return Title;
                else return Title.Substring(0, 23) + "...";
            }
        }
        public string Description { get; set; }
        public string PosterUrl { get; set; }
        public string TrailerUrl { get; set; }
        public string Genre { get; set; }
        public string ImdbGenre { get; set; }
        public List<string> Adapters { get; set; } = new List<string>();
        public List<string> AdapterUrls { get; set; } = new List<string>();
        public string Writers { get; set; }
        public string Content_Rating { get; set; }
        public string Directors { get; set; }
        public string Actors { get; set; }
        public string ImdbRating
        {
            get { return this.imdbRating; }
            set
            {
                imdbRating = value;
                OnPropertyChanged();
            }
        }
        private string imdbRating { get; set; }
        private string imdbVotes { get; set; }
        public string ImdbID { get; set; }
        public string ImdbVotes
        {
            get { return this.imdbVotes; }
            set
            {
                imdbVotes = value;
                OnPropertyChanged();
            }
        }
        public string RTRating { get; set; }
        public string ReleaseDateString { get; set; }
        public string Runtime { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string ReleaseYear => "(" + ReleaseDate.Year + ")";
        public void OnPropertyChanged(string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
