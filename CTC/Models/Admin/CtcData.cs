using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CTC.Models.Admin
{
    public class CtcData
    {

        [Key] public int Id { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string FaceBook { get; set; }
        public string LinedIn { get; set; }
        public string Instagram { get; set; }
        public string Nahno { get; set; }     
        
        public string CaptionHome { get; set; }
        public string? LogoUrl { get; set; }

        // Not stored in the database, used for file upload
        [NotMapped]
        public IFormFile LogoFile { get; set; }



    }
}
