using CTC.Repository.Enum;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CTC.ViewModels.Admin
{
    public class AddManager
    {
        [Required]
        [MaxLength(25)]
        public string Email { get; set; }
        [Required]
        [MaxLength(10)]
        public string UserName { get; set; }


        [Required]
        public string PhoneNumebr { get; set; }
        [Required]
        public string TypeOfUser { get; set; }
        [Required]
        public string FullName { get; set; }

    }
}
