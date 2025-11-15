namespace Fall2025_Project3_jrborth.Models
{
    public class MovieDetailsViewModel
    {
        public Movie Movie { get; set; }
        public List<Actor> Actors { get; set; }
        public List<string> Reviews { get; set; }
        public List<string> Sentiments { get; set; }
        public double AverageSentiment { get; set; }
    }
}
