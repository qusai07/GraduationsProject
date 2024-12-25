using CTC.Data;
using CTC.Extensions;
using CTC.Models;
using CTC.Models.Academic;
using CTC.Repository.Enum;
using CTC.Repository.IRepository;
using CTC.ViewModels.Academic;
using CTC.ViewModels.MemberShip;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using System.Reflection;

namespace CTC.Controllers
{
    [Authorize(Roles = "AcademicManager")]
    public class AcademicController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IAcademicRepository _academicRepository;
        private readonly IUserRepository _userRepository;
        private readonly CtcDbContext _ctcDbContext;
        private readonly UserManager<User> _usermanger;

        public AcademicController(IWebHostEnvironment environment, UserManager<User> userManager, IAcademicRepository academicRepository, CtcDbContext ctcDbContext, IUserRepository userRepository)
        {
            _environment = environment;
            _academicRepository = academicRepository;
            _userRepository = userRepository;
            _ctcDbContext = ctcDbContext;
            _usermanger = userManager;

        
        
        }
        public override async void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var notification1 = _ctcDbContext.facultymembers
                .Take(3)
                .Select(f => new Notification
                {
                    NotificationType = "FacultyMember",
                    Username = f.MemberName,
                    Message = $"Add a new " + $"{f.prefx}.{f.NameDoctor}, Department: {f.department}" 

                })
                .ToList();
            var notification2 = _ctcDbContext.materialSummaries
               .Take(3)
               .Select(m => new Notification
               {
                   NotificationType = "MaterialSummary",
                   Username = m.username,
                   Message =$"Add a new "+ $"Material Name: {m.MaterialName} Department: {m.materialsDepartment}" 

               })
               .ToList();

        

            var combinedNotifications = notification1.Concat(notification2).ToList();
            ViewBag.Latestnotification = combinedNotifications;
            base.OnActionExecuting(filterContext);
        }
        public async Task <IActionResult> HomeAdmin()
        {
            var facultymembersCount = await _ctcDbContext.facultymembers.CountAsync();
            var materialsummariescount = await _ctcDbContext.materialSummaries.CountAsync();
            var members = await _academicRepository.GetAcademicMemberShipCount("AcademicMemberShip");

            ViewBag.materialsummariescount = materialsummariescount;
            ViewBag.facultymembersCount = facultymembersCount;
            ViewBag.members=members;
            return View("~/Views/LeaderDepartment/Academic/HomeAdmin.cshtml");
        }
        public IActionResult TrashTableAcademic()
        {
            return View("~/Views/MemberShip/Academic/TrashTableAcademic.cshtml");
        }
        public async Task<IActionResult> FacultyMembers(int id)
        {
            var faculty = _ctcDbContext.facultymembers.Select(j => new FacultymembersViewModel
            {
                Id = j.Id,
                Name = j.NameDoctor,
                department = j.department,
                Email = j.Email,
                prefx = j.prefx
            }).ToList();
            ViewBag.Title = "Add Faculty Members";

            return View("~/Views/LeaderDepartment/Academic/FacultyMembers.cshtml", faculty);
        }
        public async Task<IActionResult> TableSummaryMaterial(string selectedDepartment)
        {
            var academic = await _academicRepository.GetAllMaterialsAsync();

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

            return View("~/Views/LeaderDepartment/Academic/TableSummaryMaterial.cshtml", model);
        }
        public async Task<IActionResult> Delete(int id)
        {
            var material = await _academicRepository.GetMaterialByIDAsync(id);

            if (material == null)
            {
                return NotFound();
            }
            var filePath = Path.Combine(_environment.WebRootPath, material.PdfUrl.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath); // Delete the file
            }
            // Delete the record from the database
            await _academicRepository.DeleteMaterialAsync(id);
            return RedirectToAction(nameof(TableSummaryMaterial));
        }
        public async Task<IActionResult> DeleteFacultyMember(int id)
        {
            var material = await _academicRepository.GetFacultymembersByIDAsync(id);
            if (material == null)
            {
                return NotFound();
            }
            // Delete the record from the database
            await _academicRepository.DeleteFacultyMemberAsync(id);
            return RedirectToAction(nameof(TableSummaryMaterial));
        }
        public List<FacultymembersViewModel> MapToViewModel(IEnumerable<Facultymembers> members)
        {
            return members.Select(m => new FacultymembersViewModel
            {
                Id = m.Id,
                prefx = m.prefx,
                Name = m.NameDoctor,
                department = m.department,
                Email = m.Email,
                MemberName = m.MemberName // Assuming Username is a property in your model
            }).ToList();
        }
        public async Task<IActionResult> ReviewFacultyRequests()
        {
            var pendingRequests = await _academicRepository.GetAllFactualMemberAsync();
            pendingRequests = pendingRequests.Where(f => !f.Approved).ToList();
            var viewModel = pendingRequests.Select(member => new FacultymembersViewModel
            {
                Id = member.Id, // Ensure to map Id here
                Name = member.NameDoctor,
                Email = member.Email,
                prefx = member.prefx,
                department = member.department,
                MemberName=member.MemberName,
                Approved = member.Approved // Ensure to map the IsApproved property
            }).ToList();

             viewModel = MapToViewModel(pendingRequests);
            return View("~/Views/LeaderDepartment/Academic/ReviewFacultyRequests.cshtml", viewModel);
        }
        public async Task<IActionResult> ApproveFacultyRequest(int id)
        {
            try
            {
                var request = await _academicRepository.GetPendingFacultyRequestById(id);

                if (request == null)
                {
                    return Json(new Dictionary<string, object>
            {
                { "success", false },
                { "message", "Request not found" }
            });
                }

                request.Approved = true;
                await _academicRepository.UpdateFacultyMemberAsync(request);

                return Json(new Dictionary<string, object>
        {
            { "success", true },
            { "message", "Request approved successfully" }
        });
            }
            catch (Exception ex)
            {
                return Json(new Dictionary<string, object>
        {
            { "success", false },
            { "message", "An error occurred while processing the request" }
        });
            }
        }
        public async Task<IActionResult> ApproveMaterialRequest(int id)
        {
            try
            {
                var request = await _academicRepository.GetPendingMaterialRequestById(id);
                if (request != null)
                {
                    request.Approved = true;
                    await _academicRepository.UpdateMaterialAsync(request);
                    return Json(new
                    {
                        success = true,
                        message = "Material request approved successfully"
                    });
                }

                return Json(new
                {
                    success = false,
                    message = "Material request not found"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "An error occurred while processing the request"
                });
            }
        }
        [HttpPost]
        public async Task<IActionResult> RejectFacultyRequest(int Id)
        {
            var request = await _academicRepository.GetPendingFacultyRequestById(Id);
            if (request != null)
            {
                await _academicRepository.DeletePendingFacultyRequest(Id);  // Remove the request
                return Json(new { success = true });
            }

            return Json(new { success = false });

        }
        [HttpPost]
        public async Task<IActionResult> RejectMaterialRequest(int Id)
        {
            var request = await _academicRepository.GetPendingMaterialRequestById(Id);
            if (request != null)
            {
                await _academicRepository.DeleteMaterialAsync(Id);  // Remove the request
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }
        public async Task<IActionResult> ReviewMaterialRequests()
        {
            var pendingRequests = await _academicRepository.GetAllMaterialsAsync();
            pendingRequests = pendingRequests.Where(f => !f.Approved).ToList();
            var viewModel = pendingRequests.Select(material => new MaterialSummary
            {
                Id = material.Id, // Ensure to map Id here
                MaterialName= material.MaterialName,
                MaterialDescription= material.MaterialDescription,
                materialsDepartment=material.materialsDepartment,
                username=material.username,

                Approved = material.Approved 
            }).ToList();

          //  viewModel = MapToViewModel(pendingRequests);
            return View("~/Views/LeaderDepartment/Academic/ReviewMaterialRequests.cshtml", pendingRequests);
        }
        public async Task <IActionResult> AllMemberShipAcademic()
        {
            var members = await _academicRepository.GetAcademicMemberShipAsync();
            return View("~/Views/LeaderDepartment/Academic/AllMemberShipAcademic.cshtml", members);
        }
        public async Task<IActionResult> AssignDuties()
        {

            var member = await _academicRepository.GetAcademicMemberShipAsync();
            var model = new AssignDutiesViewModel
            {
                Users = member.Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.UserName
                }).ToList(),
                SelectedUsers = new List<string>(),
                DutyDescription = ""
            };

            return View("~/Views/LeaderDepartment/Academic/AssignDuties.cshtml", model);

        }
        [HttpPost]
        public async Task<IActionResult> AssignDuties(AssignDutiesViewModel model)
        {
            if (!ModelState.IsValid)
            { 
                model.Users = (await _academicRepository.GetAcademicMemberShipAsync()).Select(u => new SelectListItem
                {
                Value = u.Id.ToString(),
                Text = u.UserName
                }).ToList();
               
                foreach (var state in ModelState)
                {
                    Console.WriteLine($"{state.Key}: {state.Value.ValidationState}");
                }
            }
            
            if (model.SelectedUsers?.Any() == true)
                {
                    foreach (var userId in model.SelectedUsers)
                    {
                        var user = await _usermanger.FindByIdAsync(userId);
                        if (user != null && await _usermanger.IsInRoleAsync(user, "AcademicMemberShip"))
                        {
                            var duty = new Duty
                            {

                                MemberId = int.Parse(userId),
                                Description = model.DutyDescription,
                                AssignedDate = DateTime.Now,
                                Status = "Assigned",
                                Member = user
                            };
                            await _academicRepository.AssignDutyToMemberAsync(duty);
                        }
                    }

                }        
            return RedirectToAction(nameof(AssignDuties));


           
        }

        public async Task<IActionResult> AddBachelorPrograms()
        {
            // Fetch bachelor programs from the database
            var bachelorPrograms = await _ctcDbContext.bachelorPrograms.ToListAsync();

            // If no programs are found, create default programs
            if (bachelorPrograms == null || bachelorPrograms.Count == 0)
            {
                bachelorPrograms = new List<BachelorPrograms>
        {
            new BachelorPrograms
            {
                Name = "Computer Engineering",
                Description = "Learn about the Computer Engineering department—our workshops offer practical experience in the latest technologies.",
                PdfUrl = ""
            },
            new BachelorPrograms
            {
                Name = "Software Engineering",
                Description = "Software Engineering program offers students the skills to design, develop, and maintain software systems.",
                PdfUrl = ""
            }
            // Add more default programs as needed
        };
            }

            return View("~/Views/LeaderDepartment/Academic/AddBachelorPrograms.cshtml", bachelorPrograms);
        }

        [HttpPost]
        public async Task <IActionResult> AddBachelorPrograms(BachelorPrograms model)
        {
            if (ModelState.IsValid)
            {
                // Handle file upload
                if (model.pdfFile != null && model.pdfFile.Length > 0)
                {
                    // Call ConvertFileToString to save the file and get its URL
                    model.PdfUrl = FileExtensions.ConvertFileToString(model.pdfFile, _environment);
                }

                // Save the Bachelor Program to the database
                _ctcDbContext.Add(model);
                await _ctcDbContext.SaveChangesAsync();
                var bachelorPrograms = await _ctcDbContext.bachelorPrograms.ToListAsync();

                return View("~/Views/LeaderDepartment/Academic/AddBachelorPrograms.cshtml", bachelorPrograms);
            }

            return View("~/Views/LeaderDepartment/Academic/AddBachelorPrograms.cshtml", model);

        }
        [HttpPost]
        public async Task<IActionResult> DeleteBachelorProgram(int id)
        {
            var program = await _ctcDbContext.bachelorPrograms.FindAsync(id);
            if (program != null)
            {
                _ctcDbContext.bachelorPrograms.Remove(program);
                await _ctcDbContext.SaveChangesAsync();
            }

            return RedirectToAction("AddBachelorPrograms");
        }

    }

}


