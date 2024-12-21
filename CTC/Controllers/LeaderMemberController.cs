using CTC.Data;
using CTC.Models.Leader;
using CTC.Repository.IRepository;
using CTC.ViewModels.MemberShip;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CTC.Controllers
{
    [Authorize(Roles = "LeaderMember")]
    public class LeaderMemberController : Controller
    {
        private readonly CtcDbContext _context;
        private readonly IUserRepository _userRepository;
        private readonly IMailService _mailService;


        public LeaderMemberController (IMailService  mailService, CtcDbContext context,IUserRepository userRepository)
        {
            _context = context;
            _userRepository = userRepository;
            _mailService = mailService;
        }
        public async  Task <IActionResult> TableAppointment()
        
        {
            var appointments = new Appointment();



            appointments.Waiting = await _context.Appointment
           .Where(j => j.Status == "Waiting")
           .OrderBy(a => a.CreatedAt)
           .ToListAsync();

            appointments.Pending = await _context.Appointment
           .Where(j => j.Status == "Pending")
           .OrderBy(a => a.CreatedAt)
           .ToListAsync();

            appointments.Accepted = await _context.Appointment
           .Where(j => j.Status == "Accepted")
           .OrderBy(a => a.CreatedAt)
           .ToListAsync();

            return View("~/Views/LeaderDepartment/LeaderMember/TableAppointment.cshtml", appointments);
        }
        [HttpPost]
        public async Task<IActionResult> BookAppointment(int id, DateTime appointmentDate , string linkMeeting)
        {
            var appointment = await _context.Appointment
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

            _context.Update(appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(TableAppointment));
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsBooked(int id , string action)
        {
            var appointment = await _context.Appointment
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

                _context.Appointment.Update(appointment);
                _context.Joiners.Update(appointment.Joiner);  
                 await _context.SaveChangesAsync(); 
                
                return RedirectToAction(nameof(TableAppointment));


            }
            else if (action == "Reject")
            {
                appointment.Status = "Rejected";
                _context.Joiners.Update(appointment.Joiner);
                await _context.SaveChangesAsync();

            }
            return RedirectToAction(nameof(TableAppointment));


        }
        public async Task<IActionResult> HomeLeader()
        {
            // Fetch summary data for the leader's dashboard
            var totalAppointments = await _context.Appointment.CountAsync();
            var waitingAppointments = await _context.Appointment.CountAsync(a => a.Status == "Waiting");
            var pendingAppointments = await _context.Appointment.CountAsync(a => a.Status == "Pending");
            var scheduledAppointments = await _context.Appointment.CountAsync(a => a.Status == "Scheduled");

            var recentAppointments = await _context.Appointment
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
            var allAppointment = await _context.Appointment
                .Where(j => j.Status == "Accepted")
                .Include (a => a.Joiner)
                .OrderByDescending
                (a => a.CreatedAt) .ToListAsync();
         

            return View("~/Views/LeaderDepartment/LeaderMember/TableAllAppointment.cshtml", allAppointment);
        }


    }
}
