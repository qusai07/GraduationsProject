using System.ComponentModel.DataAnnotations;

namespace CTC.ViewModels.Admin
{
    public class ContactViewModel
    {
        [Required]
        [EmailAddress]
        public string ToEmail { get; set; }

        [Required]
        [StringLength(100)]
        public string Subject { get; set; }

        [Required]
        [StringLength(1000)]
        public string Message { get; set; }
    }
}
