using CTC.Data;
using CTC.Extensions;
using CTC.Models;
using CTC.Models.Academic;
using CTC.Repository.Enum;
using CTC.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using CTC.ViewModels.Academic;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CTC.Controllers
{
    [Authorize(Roles = "AcademicMemberShip")]
    public class AcademicMemberShipController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IAcademicRepository _academicRepository;
        private readonly IUserRepository _userRepository;
        private readonly CtcDbContext _ctcDbContext;
        private readonly UserManager<User> _usermanger;

        public AcademicMemberShipController(IWebHostEnvironment environment,UserManager<User> userManager, IAcademicRepository academicRepository, CtcDbContext ctcDbContext, IUserRepository userRepository)
        {
            _environment = environment;
            _academicRepository = academicRepository;
            _userRepository = userRepository;
            _ctcDbContext = ctcDbContext;
            _usermanger = userManager;
        }
        public override async void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var notification1 = _ctcDbContext.Duties
                .Take(3)
                .Select(f => new Notification
                {
                    NotificationType = "FacultyMember",
                    Message = $"Admin send You a message "+ f.Description,
                    CreatedAt = f.AssignedDate,

                })
                .ToList();
                  ViewBag.Latestnotification = notification1;
            base.OnActionExecuting(filterContext);
        }
        public IActionResult MemberAcademic()
        {
            return View("~/Views/MemberShip/AcademicMemberShip/MemberAcademic.cshtml");
        }
        public async Task<IActionResult> AddSummaryMaterial()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User not authenticated.");
            }
            string currentUserId = _usermanger.GetUserId(User);

            var currentUser = await _userRepository.GetByIdAsync(currentUserId);
            if (currentUser == null)
            {
                return Unauthorized("User not found.");
            }


            var model = new MaterialSummaryViewModel()
            {
                UserId=currentUser.Id.ToString()
            };
            return View("~/Views/MemberShip/AcademicMemberShip/AddSummaryMaterial.cshtml", model);
        }
        [HttpPost]
        public async Task<IActionResult> AddSummaryMaterial(MaterialSummaryViewModel model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated.");
            }
            var currentUser = await _usermanger.GetUserAsync(User);
            model.UserId = currentUser.Id.ToString();
            ModelState.Remove("UserId");
            if (ModelState.IsValid)
            {
                string uniqueFileName = FileExtensions.ConvertFileToString(model.pdfFile, _environment);
                if (model.pdfFile != null && model.pdfFile.Length > 0)
                {
                    var materialSummary = new MaterialSummary
                    {
                        Id = model.Id,
                        MaterialName = model.MaterialName,
                        MaterialDescription = model.MaterialDescription,
                        materialsDepartment = model.materialsDepartment,
                        UploadDate = DateTime.Now,
                        PdfUrl =  uniqueFileName,
                        username = model.MemberName,
                        UserId=  model.UserId,
                        Approved=false
                    };
                    await _academicRepository.AddMaterialAsync(materialSummary);
                    return RedirectToAction(nameof(AddSummaryMaterial));
                }
            }
            return View("~/Views/MemberShip/Academic/AddSummaryMaterial.cshtml");
        }
    
        public async Task<IActionResult> FacultyMembers(int id)
        {
            var user = await _usermanger.GetUserAsync(User); // Get the current logged-in user
            var facultyUser = await _academicRepository.GetFacultyForUserAsync(user.Id);
            var faculty = _ctcDbContext.facultymembers.Select(j => new FacultymembersViewModel
            {
                
                Name = j.NameDoctor,
                department = j.department,
                Email = j.Email,
                prefx = j.prefx
            }).ToList();
            return View("~/Views/MemberShip/AcademicMemberShip/FacultyMembers.cshtml", faculty);

        }
        public async Task <IActionResult> AddFacultymembers()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User not authenticated.");
            }

            string currentUserId = _usermanger.GetUserId(User);
            var currentUser = await _userRepository.GetByIdAsync(currentUserId);

            if (currentUser == null)
            {
                return Unauthorized("User not found.");
            }

            var model = new FacultymembersViewModel
            {
                UserId = currentUser.Id.ToString(),

            };
            return View("~/Views/MemberShip/AcademicMemberShip/AddFacultymembers.cshtml", model);
        }
        [HttpPost]
        public async Task<IActionResult> AddFacultymembers(FacultymembersViewModel model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated.");
            }
            var currentUser = await _usermanger.GetUserAsync(User);  // Get the current user
            
            model.UserId = currentUser.Id.ToString();
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                var Facultymembers = new Facultymembers
                {
                    Id = model.Id,
                    NameDoctor = model.Name,
                    Email = model.Email,
                    MemberName= model.MemberName,
                    prefx = model.prefx,
                    department = model.department,
                    Approved = false,
                    UserId=currentUser.Id.ToString()
                };
                await _academicRepository.AddFacultymembers(Facultymembers);
                return View("~/Views/MemberShip/AcademicMemberShip/AddFacultymembers.cshtml");

            }

            return View("~/Views/MemberShip/AcademicMemberShip/AddFacultymembers.cshtml");
        }
        public async Task<IActionResult> TableSummaryMaterial(string selectedDepartment)
        {
            var user = await _usermanger.GetUserAsync(User); // Get the current logged-in user
            var academic = await _academicRepository.GetMaterialsForUserAsync(user.Id);

            if (!string.IsNullOrEmpty(selectedDepartment))
            {
                Department departmentEnum;
                if (Enum.TryParse(selectedDepartment, out departmentEnum))
                {
                    academic = academic.Where(e => e.materialsDepartment == departmentEnum).ToList();
                }
            }

            var model = academic.Select(e => new MaterialSummaryViewModel
            {
                Id = e.Id,
                MaterialName = e.MaterialName,
                MaterialDescription = e.MaterialDescription,
                materialsDepartment = e.materialsDepartment,
                UploadDate = e.UploadDate,

                
            }).ToList();

            ViewBag.SelectedDepartment = selectedDepartment;

            return View("~/Views/MemberShip/AcademicMemberShip/TableSummaryMaterial.cshtml", model);
        }
        public async Task<IActionResult> Delete(int id)
        {
            var material = await _academicRepository.GetMaterialByIDAsync(id);

            if (material == null)
            {
                return NotFound();
            }

            // Assuming 'PdfUrl' contains the relative path to the PDF file
            var filePath = Path.Combine(_environment.WebRootPath, material.PdfUrl.TrimStart('/'));

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath); // Delete the file
            }

            // Delete the record from the database
            await _academicRepository.DeleteMaterialAsync(id);

            return RedirectToAction(nameof(TableSummaryMaterial));
        }

        public async Task<IActionResult> MyDuties()
        {
            var user = await _usermanger.GetUserAsync(User); // Get the current logged-in user
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var duties = await _academicRepository.GetDutiesForMemberAsync(user.Id); // Fetch duties for the current user
            return View("~/Views/MemberShip/AcademicMemberShip/MyDuties.cshtml", duties);
        }

    }
}
