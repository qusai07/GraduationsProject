using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CTC.Models.Admin
{
    public class Achievement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public DateTime Date { get; set; }

        public string? ImageUrl { get; set; }

        // Not stored in the database, used for file upload
        [NotMapped]
        public IFormFile ImageFile { get; set; }

    }
}
