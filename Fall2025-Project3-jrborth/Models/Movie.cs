using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Fall2025_Project3_jrborth.Models
{
    public class Movie
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Genre { get; set; }
        public int Year { get; set; }

        [Url]
        public string ImdbLink { get; set; }

        public byte[]? Poster { get; set; }

        // Initialize to avoid ModelState "required" validation error
        public ICollection<ActorMovie> ActorMovies { get; set; } = new List<ActorMovie>();
    }
}
