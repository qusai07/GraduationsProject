using CTC.Models.Leader;
using CTC.Repository.Enum;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CTC.ViewModels.MemberShip
{

    public class JoinerViewModel
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(10)]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(10)]

        public string LastName { get; set; }
        [Required]
        [MaxLength(6)]

        public string UniversityID { get; set; }
        [Required]

        public string UniversityEmail { get; set; }

        [Required]
        [MaxLength(13)]

        public string Phone { get; set; }

        [Required]

        public string Address { get; set; }

        [Required]
        public Gender Gender { get; set; }
        public List<SelectListItem> GenderList => Enum.GetValues(typeof(Gender))
      .Cast<Gender>()
      .Select(d => new SelectListItem
      {
          Value = d.ToString(),
          Text = d.ToString()
      }).ToList();


        [Required]
        public Department Department { get; set; }
        public List<SelectListItem> DepartmentList => Enum.GetValues(typeof(Department))
        .Cast<Department>()
        .Select(d => new SelectListItem
        {
            Value = d.ToString(),
            Text = d.ToString()
        }).ToList();

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Url]
        public string LinkedIn { get; set; }
        [Url]  
        public string Facebook { get; set; }

        [StringLength(500)]
        [Display(Name = "Your Message")]
        public string YourMessage { get; set; }
        public string? Status { get; set; }
        public List<Joiner>? PendingUsers { get; set; }
        public List<Joiner>? AcceptedUsers { get; set; }
        public List<Joiner>? RejectedUsers { get; set; }
    }
}
