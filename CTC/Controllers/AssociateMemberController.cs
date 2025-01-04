using CTC.Data;
using CTC.Models.Volunteer;
using CTC.Models;
using CTC.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CTC.Models.Event;
namespace CTC.Controllers
{

    [Authorize(Roles = "AssociateMemberShip")]
    // this controller for Students not have a role or functionallty in app
    public class AssociateMemberController : BaseController
    {
        private readonly IVolunteerRepository _volunteerRepository;
        public AssociateMemberController(IWebHostEnvironment environment,CtcDbContext ctcDbContext,UserManager<User> userManager,IEventCtcRepository eventCtcRepository,IVolunteerRepository volunteerRepository): base(environment, ctcDbContext, userManager, eventCtcRepository: eventCtcRepository)
        {
            _volunteerRepository = volunteerRepository;
        }
        public IActionResult Home()
        {
            return View();
        }
        public async Task<IActionResult> Events(int eventID)
        {
            var events = await _ctcDbContext.Events.Select(m => new EventsCTC
            {
                Id = m.Id,
                Location = m.Location,
                EventType = m.EventType,
                Name = m.Name,
                Description = m.Description,
                EventDate = m.EventDate,
                ImageUrl = m.ImageUrl,
            }).ToListAsync();
          //  return View(events);

            return View("~/Views/AssociateMember/Events.cshtml", events);
        }
        public async Task <IActionResult> Tables()
        {
            var user = await _usermanger.GetUserAsync(User);
            var volunteerParticipations = await _volunteerRepository.GetVolunteerParticipationsByVolunteerIdAsync(user.Id);

            return View(volunteerParticipations);
        }
        public IActionResult VolunteerWork(int id)
        {
            var volunteer = _ctcDbContext.volunteering
                .Select(x => new Volunteering
                {
                    Id = x.Id,
                    Organization = x.Organization,
                    Date = x.Date,
                    Description = x.Description,
                    Location = x.Location,
                    Type = x.Type,
                    ImageUrl = x.ImageUrl,
                    MaxParticipants = x.MaxParticipants,
                    CurrentParticipants = x.CurrentParticipants,
                }).ToList();

            if (volunteer == null)
            {
                return View("~/Views/AssociateMember/VolunteerWork.cshtml");
            }

            return View("~/Views/AssociateMember/VolunteerWork.cshtml", volunteer);
        }
        [HttpPost]
        public async Task<IActionResult> SubscribeToVolunteer(int eventId)
        {
            try
            {
                var user = await _usermanger.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "You need to be logged in to subscribe.";
                    return RedirectToAction("Login", "Account");
                }

                var eventToSubscribe = await _volunteerRepository.GetVolunteerByIdAsync(eventId);
                if (eventToSubscribe == null)
                {
                    TempData["ErrorMessage"] = "Event not found.";
                    return RedirectToAction("VolunteerWork", "AssociateMember");
                }

                var existingSubscription = await _ctcDbContext.VolunteerParticipants
                    .AnyAsync(vp => vp.VolunteerId == user.Id && vp.EventId == eventId);
                if (existingSubscription)
                {
                    TempData["ErrorMessage"] = "You are already subscribed to this event!";
                    return RedirectToAction("VolunteerWork", "AssociateMember");
                }
                if (eventToSubscribe.CurrentParticipants >= eventToSubscribe.MaxParticipants)
                {
                    TempData["ErrorMessage"] = "This event is already full!";
                    return RedirectToAction("VolunteerWork", "AssociateMember");
                }
                var participation = new VolunteerParticipants
                {
                    VolunteerId = user.Id,
                    EventId = eventId,
                    ParticipationDate = DateTime.Now,
                    Status = "Subscribed",
                    ParticipateName = user.UserName,
                    ParticipateEmail=user.Email
                };

                _ctcDbContext.VolunteerParticipants.Add(participation);
                eventToSubscribe.CurrentParticipants++;
                await _ctcDbContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "You have successfully subscribed to the event!";
                return RedirectToAction("Tables", "AssociateMember");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while processing your request.";
                return RedirectToAction("VolunteerWork", "AssociateMember");
            }
        }


    }
}
