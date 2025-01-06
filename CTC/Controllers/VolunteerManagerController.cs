using CTC.Data;
using CTC.Extensions;
using CTC.Models;
using CTC.Models.Volunteer;
using CTC.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CTC.Controllers
{
    [Authorize(Roles = "VolunteerManager")]
    public class VolunteerManagerController : BaseController
    {
        private readonly IVolunteerRepository _volunteerRepository;
        private readonly IMailService _mailService;

        public VolunteerManagerController(IMailService mailService,
            IWebHostEnvironment environment,
            CtcDbContext ctcDbContext,
            UserManager<User> userManager,
            IVolunteerRepository volunteerRepository)
            : base(environment, ctcDbContext, userManager)
        {
            _volunteerRepository = volunteerRepository;
            _mailService = mailService;

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
                    if (!string.IsNullOrEmpty(updatevolunteering.ImageUrl))
                    {
                        string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "Pic", updatevolunteering.ImageUrl);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Save the new image
                    string uniqueFileName = FileExtensions.ConvertImageToString(updatevolunteering.ImageFile, _webHostEnvironment);
                    updatevolunteering.ImageUrl = $"/Pic/{uniqueFileName}"; 
                }
                await _volunteerRepository.UpdateVolunteer(updatevolunteering);
                return RedirectToAction("EditVolunteer");
            }
                return View("~/Views/LeaderDepartment/Volunteer/EditVolunteer.cshtml", updatevolunteering);

         
        }

        [HttpPost]
        public async Task<IActionResult> UpdateParticipationStatus(int id, string action )
        {

            try
            {
                var participation = await _ctcDbContext.VolunteerParticipants
                    .Include(v => v.Volunteering)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (participation == null)
                {
                    TempData["ErrorMessage"] = "Participation record not found.";
                    return RedirectToAction("Index");
                }

                if (action.ToLower() == "accept")
                {
                    var currentParticipants = await _ctcDbContext.VolunteerParticipants
                        .CountAsync(p => p.VolunteeringId == participation.VolunteeringId
                                    && p.Status == "Accepted");

                    if (currentParticipants >= participation.Volunteering.MaxParticipants)
                    {
                        TempData["ErrorMessage"] = "Cannot accept participant. Event is full.";
                        return RedirectToAction("Index");
                    }

                    participation.Status = "Accepted";
                    await SendEmail(participation.ParticipateEmail,participation.ParticipateName, true, participation.Volunteering.Organization);
                }
                else if (action.ToLower() == "reject")
                {
                    participation.Status = "Rejected";
                    await SendEmail(participation.ParticipateEmail, participation.ParticipateName, false, participation.Volunteering.Organization);
                }

                _ctcDbContext.Update(participation);
                await _ctcDbContext.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Successfully {participation.Status.ToLower()} {participation.ParticipateName}'s participation.";
                return RedirectToAction("TableParticipation", "VolunteerManager");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while processing your request.";
                return RedirectToAction("Index");
            }
        }

        private async Task SendEmail(string participationEmail,string participationName, bool isAccepted ,string Organization)
        {
            string subject = isAccepted ? "Volunteer Participation Accepted" : "Volunteer Participation Status Update";
            string message = isAccepted

            ? $@"Dear {participationName},
            Your participation in the volunteer event {Organization} has been accepted!
            Event Details:
            Organization: {Organization}
            make sure to arrive on time and bring any necessary materials.

            Best regards,
            CTC Team"


           : $@"Dear {participationName},
            We regret to inform you that your participation in the volunteer event {Organization} could not be accepted at this time.
            We encourage you to apply for other volunteer opportunities in the future.

            Best regards,
            CTC Team";

            await _mailService.SendEmailAsync(participationEmail, subject, message);
        }
    }
}
