using CTC.Models;
using CTC.Repository.Enum;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace CTC.ViewModels.Academic
{
    public class FacultymembersViewModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string prefx { get; set; }

        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }

        [Required]
        public Department department { get; set; }
        public List<SelectListItem> DepartmentList => Enum.GetValues(typeof(Department))
        .Cast<Department>()
        .Select(d => new SelectListItem
        {
            Value = ((int)d).ToString(),
            Text = d.GetType()
                        .GetMember(d.ToString())
                        .First()
                        .GetCustomAttribute<DisplayAttribute>()
                        ?.Name ?? d.ToString()
        }).ToList();

        [Required]
        public bool Approved { get; set; }
        [Required]
        public string MemberName { get; set; }
        [BindNever]
        public string UserId { get; set; }
    }
}
