using CTC.Repository.Enum;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CTC.Models
{
    public class User : IdentityUser<int>
    {

        [Url]
        public string? LinkedIn { get; set; }
        [Url]
        public string? Facebook { get; set; }

        public string? FullName { get; set; }

        public int? Points { get; set; }
        public string? ImageUrl { get; set; }
        [NotMapped]
        public IFormFile ImageFile { get; set; }
    }
}
