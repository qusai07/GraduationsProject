using CTC.Data;
using CTC.Models;
using CTC.Models.Leader;
using CTC.Repository.IRepository;
using CTC.ViewModels.MemberShip;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CTC.Controllers
{
    [Authorize(Roles = "LeaderMember")]
    public class LeaderMemberController : BaseController
    {
        public LeaderMemberController(
         CtcDbContext ctcDbContext,
         IUserRepository userRepository,
         IMailService mailService,
         IWebHostEnvironment environment,
         UserManager<User> userManager)
         : base(environment, ctcDbContext, userManager, userRepository, mailService)
        {
        }
        public async  Task <IActionResult> TableAppointment()
        
        {
            var appointments = new Appointment();



            appointments.Waiting = await _ctcDbContext.Appointment
           .Where(j => j.Status == "Waiting")
           .OrderBy(a => a.CreatedAt)
           .ToListAsync();

            appointments.Pending = await _ctcDbContext.Appointment
           .Where(j => j.Status == "Pending")
           .OrderBy(a => a.CreatedAt)
           .ToListAsync();

            appointments.Accepted = await _ctcDbContext.Appointment
           .Where(j => j.Status == "Accepted")
           .OrderBy(a => a.CreatedAt)
           .ToListAsync();

            return View("~/Views/LeaderDepartment/LeaderMember/TableAppointment.cshtml", appointments);
        }
        [HttpPost]
        public async Task<IActionResult> BookAppointment(int id, DateTime appointmentDate , string linkMeeting)
        {
            var appointment = await _ctcDbContext.Appointment
                    .Include(a => a.Joiner)  
                    .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null || appointment.Status != "Waiting")
            {
                return NotFound();
            }

            var joiner = appointment.Joiner;
            if (joiner != null)
            {
                appointment.LinkMeeting = linkMeeting;


                var subject = "CTC Team";
                var message = $"Dear {joiner.FirstName}\n" +
                              $"\nWe have sehcduled your appointment for {appointment.AppointmentDate}.\n" +
                              $"Meeting link {linkMeeting}\n" +
                              $"Best regards,\nCTC Team";
                

                await _mailService.SendEmailAsync(joiner.UniversityEmail, subject, message);
            }
            appointment.Status = "Pending";
            appointment.AppointmentDate = appointmentDate;

            _ctcDbContext.Update(appointment);
            await _ctcDbContext.SaveChangesAsync();

            return RedirectToAction(nameof(TableAppointment));
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsBooked(int id , string action)
        {
            var appointment = await _ctcDbContext.Appointment
                .Include(a => a.Joiner) 
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
            {
                return NotFound(); 
            }

            if (appointment.Status != "Pending" && appointment.Joiner.Status != "Pending")
            {
                return BadRequest("The appointment or joiner is not in a pending status."); 
            }
            if (action == "Accept")
            {
                
                 appointment.Status = "Accepted";  
                 appointment.AppointmentDate = DateTime.Now;  
                 appointment.Joiner.Status = "Accepted"; 

                _ctcDbContext.Appointment.Update(appointment);
                _ctcDbContext.Joiners.Update(appointment.Joiner);  
                 await _ctcDbContext.SaveChangesAsync(); 
                
                return RedirectToAction(nameof(TableAppointment));


            }
            else if (action == "Reject")
            {
                appointment.Status = "Rejected";
                _ctcDbContext.Joiners.Update(appointment.Joiner);
                await _ctcDbContext.SaveChangesAsync();

            }
            return RedirectToAction(nameof(TableAppointment));


        }
        public async Task<IActionResult> HomeLeader()
        {
            // Fetch  data for the leader's dashboard
            var totalAppointments = await _ctcDbContext.Appointment.CountAsync();
            var waitingAppointments = await _ctcDbContext.Appointment.CountAsync(a => a.Status == "Waiting");
            var pendingAppointments = await _ctcDbContext.Appointment.CountAsync(a => a.Status == "Pending");
            var scheduledAppointments = await _ctcDbContext.Appointment.CountAsync(a => a.Status == "Scheduled");

            var recentAppointments = await _ctcDbContext.Appointment
                .Include(a => a.Joiner)
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync();




            ViewBag.TotalAppointments = totalAppointments;
            ViewBag.WaitingAppointments = waitingAppointments;
            ViewBag.PendingAppointments = pendingAppointments;
            ViewBag.ScheduledAppointments = scheduledAppointments;
            ViewBag.RecentAppointments = recentAppointments;

            return View("~/Views/LeaderDepartment/LeaderMember/HomeLeader.cshtml");
        }

        public async Task<IActionResult> TableAllAppointment()
        {
            var allAppointment = await _ctcDbContext.Appointment
                .Where(j => j.Status == "Accepted")
                .Include (a => a.Joiner)
                .OrderByDescending
                (a => a.CreatedAt) .ToListAsync();
         

            return View("~/Views/LeaderDepartment/LeaderMember/TableAllAppointment.cshtml", allAppointment);
        }


    }
}
