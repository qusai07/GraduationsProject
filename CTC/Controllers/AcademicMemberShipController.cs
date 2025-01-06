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
    public class AcademicMemberShipController : BaseController
    {
        private readonly IAcademicRepository _academicRepository;

        public AcademicMemberShipController(
            IWebHostEnvironment environment,
            CtcDbContext ctcDbContext,
            UserManager<User> userManager,
            IUserRepository userRepository,
            IAcademicRepository academicRepository)
            : base(environment, ctcDbContext, userManager, userRepository)
        {
            _academicRepository = academicRepository;
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
            if (string.IsNullOrWhiteSpace(model.MaterialName))
            {
                ModelState.AddModelError("MaterialName", "Material Name is required.");
            }

            if (string.IsNullOrWhiteSpace(model.MaterialDescription))
            {
                ModelState.AddModelError("MaterialDescription", "Material Description is required.");
            }

            if (model.materialsDepartment == 0) 
            {
                ModelState.AddModelError("materialsDepartment", "Please select a department.");
            }

            if (model.pdfFile == null || model.pdfFile.Length == 0)
            {
                ModelState.AddModelError("pdfFile", "Please upload a PDF file.");
            }

            if (string.IsNullOrWhiteSpace(model.MemberName))
            {
                ModelState.AddModelError("MemberName", "Your Name is required.");
            }

            if (!ModelState.IsValid)
            {
                return View("~/Views/MemberShip/AcademicMemberShip/AddSummaryMaterial.cshtml", model);
            }

            try
            {
                string uniqueFileName = FileExtensions.ConvertFileToString(model.pdfFile, _webHostEnvironment);

                var materialSummary = new MaterialSummary
                {
                    MaterialName = model.MaterialName,
                    MaterialDescription = model.MaterialDescription,
                    materialsDepartment = model.materialsDepartment,
                    UploadDate = DateTime.Now,
                    PdfUrl = uniqueFileName,
                    username = model.MemberName,
                    UserId = model.UserId,
                    Approved = false
                };

                await _academicRepository.AddMaterialAsync(materialSummary);
                TempData["SuccessMessage"] = "Material summary uploaded successfully!";

                return RedirectToAction("AddSummaryMaterial");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while uploading the material.");
                return View("~/Views/MemberShip/AcademicMemberShip/AddSummaryMaterial.cshtml", model);
            }
        }
    
        public async Task<IActionResult> FacultyMembers(int id)
        {
            var user = await _usermanger.GetUserAsync(User); 
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
            if (model == null)
            {
                return BadRequest("Invalid request data.");
            }

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated.");
            }
            var currentUser = await _usermanger.GetUserAsync(User);
            if (currentUser == null)
            {
                _logger.LogError("Current user is null");
                return Unauthorized("Unable to identify current user.");
            }
            model.UserId = currentUser.Id.ToString();
            ModelState.Remove("UserId");

            if (model.department == 0)
            {
                ModelState.AddModelError("materialsDepartment", "Please select a department.");
            }
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Doctor's Name is required.");
            }

            if (model.department == 0)
            {
                ModelState.AddModelError("department", "Please select a department.");
            }
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError("Email", "Email is required.");
            }
            else
            {
                try
                {
                    var emailExists = await _academicRepository.CheckFacultyMemberEmailExists(model.Email);
                    if (emailExists)
                    {
                        ModelState.AddModelError("Email", "This email is already registered.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error checking email existence");
                    return View("~/Views/MemberShip/AcademicMemberShip/AddFacultymembers.cshtml");
                }
            }
            if (!ModelState.IsValid)
            {
                return View("~/Views/MemberShip/AcademicMemberShip/AddFacultymembers.cshtml", model);
            }
            var facultyMember = new Facultymembers
            {
                NameDoctor = model.Name?.Trim(),
                Email = model.Email?.Trim(),
                MemberName = model.MemberName?.Trim(),
                prefx = model.prefx?.Trim(),
                department = model.department,
                Approved = false,
                UserId = currentUser.Id.ToString()
            };

            try
            {
                await _academicRepository.AddFacultymembers(facultyMember);
                TempData["SuccessMessage"] = "Faculty member added successfully and is pending approval.";
                return RedirectToAction("AddFacultymembers");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("","Unexpected error while adding faculty member");
                return View("~/Views/MemberShip/AcademicMemberShip/AddFacultymembers.cshtml", model);
            }

        }
        public async Task<IActionResult> TableSummaryMaterial(string selectedDepartment)
        {
            var user = await _usermanger.GetUserAsync(User); 
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

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, material.PdfUrl.TrimStart('/'));

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath); 
            }
            await _academicRepository.DeleteMaterialAsync(id);

            return RedirectToAction(nameof(TableSummaryMaterial));
        }

        public async Task<IActionResult> MyDuties()
        {
            var user = await _usermanger.GetUserAsync(User); 
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var duties = await _academicRepository.GetDutiesForMemberAsync(user.Id); 
            return View("~/Views/MemberShip/AcademicMemberShip/MyDuties.cshtml", duties);
        }

    }
}
