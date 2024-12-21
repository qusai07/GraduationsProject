using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CTC.ViewModels.Academic
{
    public class AssignDutiesViewModel
    {
        [BindNever]
        public List<SelectListItem> Users { get; set; }
        [Required]
        public List<string> SelectedUsers { get; set; } // Users selected for assignment
        [Required]
        public string DutyDescription { get; set; } // Description of the duty
    }
}
