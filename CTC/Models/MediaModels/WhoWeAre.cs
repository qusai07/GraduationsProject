using System.ComponentModel.DataAnnotations.Schema;

namespace CTC.Models.MediaModels
{
    public class WhoWeAre
    {
        public int Id { get; set; }

        public string Header { get; set; }
        public string Content { get; set; }
        public int CountStudent { get; set; }
        public string Footer { get; set; }
        public string? ImageUrl { get; set; }

        // Not stored in the database, used for file upload
        [NotMapped]
        public IFormFile ImageFile { get; set; }
    }
}
