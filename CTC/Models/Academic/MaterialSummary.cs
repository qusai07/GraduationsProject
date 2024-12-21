using CTC.Repository.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CTC.Models.Academic
{
    public class MaterialSummary
    {
        [Key]
        public int Id { get; set; }
        public string MaterialName { get; set; }
        public string MaterialDescription { get; set; }
        public Department materialsDepartment { get; set; }
        public DateTime UploadDate { get; set; }
        public string? PdfUrl { get; set; }

        [NotMapped] public IFormFile pdfFile { get; set; }
        public string username { get; set; }
        public string UserId { get; set; }
        public bool Approved { get; set; }




    }
}
