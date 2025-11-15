using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Fall2025_Project3_jrborth.Models
{
    public class Actor
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }

        [Url]
        public string ImdbLink { get; set; }

        public byte[]? Photo { get; set; }

        // Initialize to avoid ModelState "required" validation error
        public ICollection<ActorMovie> ActorMovies { get; set; } = new List<ActorMovie>();
    }
}