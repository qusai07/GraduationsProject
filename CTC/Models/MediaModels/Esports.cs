using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CTC.Models.MediaModels
{
    public class Esports
    {
        [Key]
        public int Id { get; set; }
        public string HeaderEsports { get; set; }

        public string ContentEsports { get; set; }
        public string? ImageUrl { get; set; }
        [NotMapped]
        public IFormFile ImageFile { get; set; }

        public List<string> Games { get; set; } = new List<string> { };
        public List<string> ContentGames { get; set; } = new List<string> { };


    }
}
