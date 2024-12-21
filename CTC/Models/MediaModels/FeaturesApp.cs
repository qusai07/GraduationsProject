using System.ComponentModel.DataAnnotations.Schema;

namespace CTC.Models.MediaModels
{
    public class FeaturesApp
    {
        public int Id { get; set; }

        public string Header { get; set; }
        public string Content { get; set; }
        public List<string>? Features { get; set; }

        public string? ImageUrl { get; set; }

        // Not stored in the database, used for file upload
        [NotMapped]
        public IFormFile ImageFile { get; set; }

    }
}
