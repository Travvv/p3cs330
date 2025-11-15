namespace Fall2025_Project3_jrborth.Models
{
    public class ActorDetailsViewModel
    {
        public Actor Actor { get; set; }
        public List<Movie> Movies { get; set; }
        public List<string> Tweets { get; set; }
        public List<string> Sentiments { get; set; }
        public double AverageSentiment { get; set; }
    }
}
