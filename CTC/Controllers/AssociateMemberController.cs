using CTC.Data;
using CTC.Models.Volunteer;
using CTC.Models;
using CTC.Models.Event;
using CTC.Repository.Enum;
using CTC.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CTC.Controllers
{

    [Authorize(Roles = "AssociateMemberShip")]
    // this controller for Students not have a role or functionallty in app
    public class AssociateMemberController : Controller
    {
        private readonly IEventCtcRepository _eventCtcRepository;
        private readonly CtcDbContext _ctcDbContext;
        private readonly UserManager<User> _usermanger;
        private readonly IVolunteerRepository _volunteerRepository;
        public AssociateMemberController(IVolunteerRepository volunteerRepository ,IEventCtcRepository eventCtcRepository ,CtcDbContext ctcDbContext,UserManager<User> userManager)
        {
            _eventCtcRepository = eventCtcRepository;
            _ctcDbContext = ctcDbContext;
            _usermanger = userManager;
            _volunteerRepository = volunteerRepository;
        }
        public IActionResult Home()
        {
            return View();
        }
        public async Task<IActionResult> EventsAsync(int eventID)
        {
            var eventEntity = await _eventCtcRepository.GetEventByIdAsync(eventID);
            if (eventEntity == null)
            {
                return NotFound();  // Optionally redirect or return a 404 page
            }

            return View(eventEntity);
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
        public async Task<IActionResult> SubscribeToEvent( int eventId)
        {

            var user = await _usermanger.GetUserAsync(User);
            if (user == null)
            {
                TempData["Message"] = "You need to be logged in to subscribe.";
                return RedirectToAction("Login", "Account");
            }

            // Get the volunteerId from the user
            int volunteerId = user.Id;  // Assuming user.Id is the volunteerId

            // Get the event details
            var eventToSubscribe = await _volunteerRepository.GetVolunteerByIdAsync(eventId);
            if (eventToSubscribe == null)
            {
                TempData["Message"] = "Event not found.";
                return View("~/Views/AssociateMember/VolunteerWork.cshtml");
            }

            // Check if the volunteer is already subscribed to the event
            var existingSubscription = await _ctcDbContext.VolunteerParticipants
                .FirstOrDefaultAsync(vp => vp.VolunteerId == volunteerId && vp.EventId == eventId);
            if (existingSubscription != null)
            {
                TempData["Message"] = "You are already subscribed to this event.";
                return RedirectToAction("~/Views/AssociateMember/VolunteerWork.cshtm", new { id = eventId });
            }
            var participation = new VolunteerParticipants
            {
                VolunteerId = volunteerId,
                EventId = eventId,
                ParticipationDate = DateTime.Now,
                Status = "Subscribed",
                ParticipateName=user.UserName,
            };

            // Step 4: Save to the database
            _ctcDbContext.VolunteerParticipants.Add(participation);
            eventToSubscribe.CurrentParticipants++;
            await _ctcDbContext.SaveChangesAsync();

            // Step 5: Provide feedback to the user
            TempData["Message"] = "You have successfully subscribed to the event!";
            return View("~/Views/AssociateMember/VolunteerWork.cshtml");
        }


    }
}
