using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace CTC.Models.MediaModels
{
    public class Videohome
    {
        public int Id { get; set; }
        public string? VideoUrl { get; set; } // Path to the video file stored on disk or cloud

        [NotMapped]
        public IFormFile VideoFile { get; set; } // For video file upload
    }
}
