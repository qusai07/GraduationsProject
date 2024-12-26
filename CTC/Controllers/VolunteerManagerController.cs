using CTC.Data;
using CTC.Extensions;
using CTC.Models;
using CTC.Models.Volunteer;
using CTC.Repository.IRepository;
using CTC.Repository.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;

namespace CTC.Controllers
{
    [Authorize(Roles = "VolunteerManager")]
    public class VolunteerManagerController : BaseController
    {
        private readonly IVolunteerRepository _volunteerRepository;

        public VolunteerManagerController(
            IWebHostEnvironment environment,
            CtcDbContext ctcDbContext,
            UserManager<User> userManager,
            IVolunteerRepository volunteerRepository)
            : base(environment, ctcDbContext, userManager)
        {
            _volunteerRepository = volunteerRepository;
        }

        public async Task<IActionResult> HomeAdmin()
        {
            var VolunteeCount = await _ctcDbContext.volunteering.CountAsync();
            var VolunterParticipationCount = await _ctcDbContext.VolunteerParticipants.CountAsync();

            ViewBag.VolunteeCount = VolunteeCount;
            ViewBag.VolunterParticipationCount = VolunterParticipationCount;
            return View("~/Views/LeaderDepartment/Volunteer/HomeAdmin.cshtml");
        }


        public IActionResult AddVolunteerwork()
        {
            return View("~/Views/LeaderDepartment/Volunteer/AddVolunteerwork.cshtml");
        }
        [HttpPost]
        public async Task<IActionResult> AddVolunteerwork(Volunteering model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = FileExtensions.ConvertImageToString(model.ImageFile, _webHostEnvironment);

                var newVoluntering = new Volunteering()
                {
                    Id = model.Id,
                    Organization = model.Organization,
                    Date = model.Date,
                    Description = model.Description,
                    Location = model.Location,
                    Type = model.Type,
                    ImageFile = model.ImageFile,
                    ImageUrl = model.ImageUrl = "/Pic/" + uniqueFileName,
                    MaxParticipants = model.MaxParticipants

                };
                _ctcDbContext.volunteering.Add(newVoluntering);
                await _ctcDbContext.SaveChangesAsync();
                return View("~/Views/LeaderDepartment/Volunteer/AddVolunteerwork.cshtml");
            }
            return View("~/Views/LeaderDepartment/Volunteer/AddVolunteerwork.cshtml");
        }
        public IActionResult TableVolunteerWork(int id)
        {
            var volunteer = _ctcDbContext.volunteering.Select(x => new Volunteering()
            {
                Id = x.Id,
                Organization = x.Organization,
                Date = x.Date,
                Description = x.Description,
                Location = x.Location,
                Type = x.Type,
                ImageFile = x.ImageFile

            }).ToList();
            if (volunteer == null)
            {
                return View("~/Views/LeaderDepartment/Volunteer/TableVolunteerWork.cshtml");

            }
            return View("~/Views/LeaderDepartment/Volunteer/TableVolunteerWork.cshtml", volunteer);
        }
        public async Task<IActionResult> TableParticipation()
        {
            var participatios = await _volunteerRepository.GetAllVolunteerParticipationsAsync();
            return View("~/Views/LeaderDepartment/Volunteer/TableParticipation.cshtml", participatios);


        }

        public async Task<IActionResult> EditVolunteer(int id)
        {
            var volunteering = await _volunteerRepository.GetVolunteerByIdAsync(id);
            if (volunteering == null)
            {
                TempData["Error"] = "Volunteer work not found";
                return RedirectToAction("TableParticipation");
            }

            return View("~/Views/LeaderDepartment/Volunteer/EditVolunteer.cshtml", volunteering);
        }
        [HttpPost]
        public async Task<IActionResult> EditVolunteer(Volunteering updatevolunteering)
        {
            if(ModelState.IsValid)
            {
                if (updatevolunteering.ImageFile != null && updatevolunteering.ImageFile.Length > 0)
                {
                    // If an old image exists, delete it
                    if (!string.IsNullOrEmpty(updatevolunteering.ImageUrl))
                    {
                        string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "Pic", updatevolunteering.ImageUrl);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath); // Delete the old image
                        }
                    }

                    // Save the new image
                    string uniqueFileName = FileExtensions.ConvertImageToString(updatevolunteering.ImageFile, _webHostEnvironment);
                    updatevolunteering.ImageUrl = $"/Pic/{uniqueFileName}"; // Update the ImageUrl with the new image's path
                }
                await _volunteerRepository.UpdateVolunteer(updatevolunteering);
                return RedirectToAction("EditVolunteer");
            }
                return View("~/Views/LeaderDepartment/Volunteer/EditVolunteer.cshtml", updatevolunteering);

         
        }

    }
}
