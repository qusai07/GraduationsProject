using Microsoft.EntityFrameworkCore.Metadata;
using NuGet.Protocol;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CTC.Models.Admin
{
    public class Founders
    {
        [Key]
        public int Id { get; set; }
        public string? Prefx { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string? ImageUrl { get; set; }
        [NotMapped]
        public IFormFile ImageFile { get; set; }
        public string position { get; set; }


    }
}
