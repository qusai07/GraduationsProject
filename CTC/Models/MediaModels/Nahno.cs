using System.ComponentModel.DataAnnotations.Schema;

namespace CTC.Models.MediaModels
{
    public class Nahno
    {
        public int Id { get; set; }

        public string Content { get; set; }
        public string? ImageUrl { get; set; }

        // Not stored in the database, used for file upload
        [NotMapped]
        public IFormFile ImageFile { get; set; }

        public string subjectone { get; set; }
        public string subjecttwo { get; set; }
        public string subjectThree { get; set; }
        public string Link { get; set; }
    }
}
