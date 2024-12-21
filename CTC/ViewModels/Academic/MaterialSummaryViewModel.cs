using CTC.Models;
using CTC.Repository.Enum;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace CTC.ViewModels.Academic
{
    public class MaterialSummaryViewModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(20)]
        public string MaterialName { get; set; }
        [Required]
        [MaxLength(200)]
        public string MaterialDescription { get; set; }
        [Required]
        public Department materialsDepartment { get; set; }

        public List<SelectListItem> MaterialsDepartmentList => Enum.GetValues(typeof(Department))
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
        public DateTime UploadDate { get; set; }
        public string? PdfUrl { get; set; }
        [NotMapped] public IFormFile pdfFile { get; set; }

        [Required]
        public string MemberName { get; set; }
        [BindNever]
        public string UserId { get; set; }
        [Required]
        public bool Approved { get; set; }





    }
}
