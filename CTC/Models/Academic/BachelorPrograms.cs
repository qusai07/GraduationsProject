using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CTC.Models.Academic
{
    public class BachelorPrograms
    {
        [Key] public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        public string? PdfUrl { get; set; }
        [NotMapped] public IFormFile pdfFile { get; set; }
    }
}
