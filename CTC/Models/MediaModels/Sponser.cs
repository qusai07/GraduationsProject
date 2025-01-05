using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CTC.Models.MediaModels
{
    public class Sponser
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        public string? ImageUrl { get; set; }

        // Not stored in the database, used for file upload
        [NotMapped]
        public IFormFile ImageFile { get; set; }
        public List<string>? sponsers { get; set; }

        public string Website { get; set; }

    }
}
