using CTC.Data;
using CTC.Extensions;
using CTC.Models;
using CTC.Repository.IRepository;
using CTC.ViewModels.Admin;
using CTC.ViewModels.MemberShip;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Drawing;
using Microsoft.AspNetCore.Authorization;
using CTC.Models.Admin;
using CTC.Models.Leader;
using CTC.Models.Event;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace CTC.Controllers

{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {
        private readonly SignInManager<User> _signInManager;
        private readonly IServiceProvider _provider;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly IJoinerRepository _joinerRepository;

        public AdminController(
            IWebHostEnvironment environment,
            CtcDbContext ctcDbContext,
            UserManager<User> userManager,
            IUserRepository userRepository,
            IMailService mailService,
            INotificationRepository notificationRepository,
            IEventCtcRepository eventCtcRepository,
            SignInManager<User> signInManager,
            IServiceProvider provider,
            RoleManager<IdentityRole<int>> roleManager,
            IJoinerRepository joinerRepository)
            : base(environment, ctcDbContext, userManager, userRepository, mailService,
                   eventCtcRepository, notificationRepository)
        {
            _signInManager = signInManager;
            _provider = provider;
            _roleManager = roleManager;
            _joinerRepository = joinerRepository;
        }
        public async Task <IActionResult> Dash()
        {
            var userCount = await _usermanger.Users.CountAsync();
            var JoinerCount = await _joinerRepository.GetUserCountAsync();
            var EventsCount = await _eventCtcRepository.GetEventCountAsync();
            ViewBag.UserCount = userCount;
            ViewBag.JoinerCount = JoinerCount;
            ViewBag.EventsCount = EventsCount;

            return View();
        }

        public override async void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var messages = _ctcDbContext.contactMessages
                .OrderByDescending(m => m.SentAt)
                .Take(3)
                .ToList();

            ViewBag.LatestMessages = messages;

            var notification = _ctcDbContext.Notification
                .OrderByDescending(m => m.CreatedAt)
                .Take(3)
                .ToList();
            ViewBag.Latestnotification = notification;

            base.OnActionExecuting(filterContext);
        }
        public async Task<IActionResult> Table()
        {
            var users = await _joinerRepository.GetAllRequestsJoinerAsync();
            // Prepare view model
            var viewModel = new JoinerViewModel
            {
                PendingUsers = users.Where(u => u.Status == "Pending").ToList(),
                AcceptedUsers = users.Where(u => u.Status == "Accepted").ToList(),
                RejectedUsers = users.Where(u => u.Status == "Rejected").ToList()
            };
            return View(viewModel);
        }
        public async Task<IActionResult> DataTable(int id)
        
        {
            var thejoiner = _ctcDbContext.Joiners
                .Where(j => j.Status == "Pending" || j.Status== "Accepted" || j.Status == "Rejected")
                .Select(j => new JoinerViewModel
                {
                    Id = j.Id,
                    FirstName = j.FirstName,
                    LastName = j.LastName,
                    UniversityID = j.UniversityID,
                    UniversityEmail = j.UniversityEmail,
                    Phone = j.Phone,
                    Address = j.Address,
                    Gender = j.Gender,
                    Department = j.Department,
                    DateOfBirth = j.DateOfBirth,
                    LinkedIn = j.LinkedIn,
                    Facebook = j.Facebook,
                    YourMessage = j.YourMessage,
                    Status = j.Status

                }).ToList();
            return View(thejoiner);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptRequest(int id)
        {
            // Find the joiner in the database
            var joiner = await _ctcDbContext.Joiners.FindAsync(id);
            if (joiner == null)
            {
                TempData["Message"] = "Joiner not found.";
                TempData["MessageType"] = "error";
                return RedirectToAction("DataTable");
            }

            // Create a new user account based on joiner details
            var joinerUser = new User
            {
                UserName = $"{joiner.FirstName}{joiner.LastName}",
                Email = joiner.UniversityEmail,
                PhoneNumber = joiner.Phone,
                NormalizedUserName = $"{joiner.FirstName}{joiner.LastName}".ToUpper()
            };

            // Generate a random password
            var password = IdentityServiceExtensions.GenerateRandomPasswordHash();

            // Attempt to create the user account
            var creationResult = await _usermanger.CreateAsync(joinerUser, password);
            if (!creationResult.Succeeded)
            {
                TempData["Message"] = "Failed to create user account.";
                TempData["MessageType"] = "error";
                return RedirectToAction("DataTable");
            }

            // Assign the "AcademicMemberShip" role to the new user
            var roleName = "AcademicMemberShip";
            var joinerAccount = await _usermanger.FindByNameAsync(joinerUser.UserName);
            await _usermanger.AddToRoleAsync(joinerAccount, roleName);

            // Send notification email
            await SendApprovalEmail(joiner, joinerUser.UserName, password);

            // Update the joiner’s status in the database
            joiner.Status = "Accepted";
            _ctcDbContext.Update(joiner);
            await _ctcDbContext.SaveChangesAsync();

            TempData["Message"] = "Join request accepted and account created.";
            TempData["MessageType"] = "success";
            return RedirectToAction("DataTable");
        }
        private async Task SendApprovalEmail(Joiner joiner, string username, string password)
        {
            var subject = "Your Account Request Has Been Approved";
            var message = $@"
        Dear {joiner.FirstName} {joiner.LastName},

        We are pleased to inform you that your request to join the CTC has been approved. Below are your account details:

        Username: {username}
        Password: {password}

        Please keep this information secure and do not share it with anyone. If you have any questions or require assistance, feel free to reach out to our support team.

        Best regards,
        The CTC Team";

            await _mailService.SendEmailAsync(joiner.UniversityEmail, subject, message);
        }
        public IActionResult CreateAchievement()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateAchievement(Achievement model)
        {
            if (ModelState.IsValid)
            {
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    string uniqueFileName = FileExtensions.ConvertImageToString(model.ImageFile, _webHostEnvironment);
                        var achievement = new Achievement()
                        {
                            Id = model.Id,
                            Title = model.Title,
                            Description = model.Description,
                            Date = model.Date,
                            ImageUrl = model.ImageUrl = "/Pic/" + uniqueFileName

                        };
                        _ctcDbContext.Achievements.Add(model);
                        await _ctcDbContext.SaveChangesAsync();

                        TempData["SuccessMessage"] = "Achievement successfully created!";
                        return RedirectToAction("Index"); 

                }
                else
                {
                    ModelState.AddModelError("", "Please upload an image file.");
                }
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectRequest(int id)
        {
            var joiner = await _ctcDbContext.Joiners.FindAsync(id);
            if (joiner == null)
            {
                return Json(new { success = false });
            }

            joiner.Status = "Rejected";
            _ctcDbContext.Update(joiner);
            await _ctcDbContext.SaveChangesAsync();
            var subject = "Request Rejected";
            var message = $"Dear {joiner.FirstName}{joiner.LastName},\n\nYour request has been Rejected.\n\nSorry About that,\nCTC Team";
            await _mailService.SendEmailAsync(joiner.UniversityEmail, subject, message);
            return Json(new { success = true });
        }
        public IActionResult SendEmail()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SendEmail(string toEmail, string subject, string message)
        {
            if (ModelState.IsValid)
            {
                await _mailService.SendEmailAsync(toEmail, subject, message);
                return RedirectToAction("SendEmail");
            }
            return BadRequest();
        }
        public async Task<IActionResult> ContactMessage()
        {
            var messages = await _ctcDbContext.contactMessages
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            return View(messages);
        }
        public async Task<IActionResult> AllEvents(int id)
        {
            var events = await _ctcDbContext.Events.Select(m => new EventsCTC
            {
                Id = m.Id,
                Location = m.Location,
                 EventType = m.EventType,
                 Name = m.Name,
                 Description = m.Description,
                 EventDate= m.EventDate,
            }).ToListAsync();
            return View(events);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (id == 0) 
            {
                return NotFound();
            }
            var data = _ctcDbContext.Events.FirstOrDefault(m => m.Id == id);
            if (data != null)
            { 
                _ctcDbContext.Events.Remove(data);
                _ctcDbContext.SaveChanges();
                
            }
            else
            {
                return BadRequest();
            } 
            return RedirectToAction("AllEvents");
        }
        public async Task<IActionResult> TableManager()
        {
            var users = await _usermanger.Users.ToListAsync();
            return View(users);
        }
        [HttpPost]
        public async Task<IActionResult> ChangeUserRole(string id, string selectedRole)
        {
            var user = await _usermanger.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var currentRoles = await _usermanger.GetRolesAsync(user);
            var removeResult = await _usermanger.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                return BadRequest("Failed to remove user roles.");
            }

            if (!await _roleManager.RoleExistsAsync(selectedRole))
            {
                return BadRequest($"Role {selectedRole} does not exist.");

            }
            var addResult = await _usermanger.AddToRoleAsync(user, selectedRole);

            if (!addResult.Succeeded)
            {
                return BadRequest("Failed to add the user to the selected role.");
            }

            return RedirectToAction("TableManager");


        }
        public async Task<IActionResult> ReplyMessage(string toEmail, string subject)
        {

            var emailViewModel = new ContactViewModel
            {
                ToEmail = toEmail,
                Subject = subject
            };

            return View("SendEmail", emailViewModel);
        }
        public async Task<IActionResult> SelectEventForQrCode(int id)
        {
            var events = await _ctcDbContext.Events.Select(m => new EventsCTC
            {
                Id = m.Id,
                Name = m.Name,
            }).ToListAsync();
            return View(events);
        }
        public async Task<IActionResult> GenerateEventQrCode(int eventId)
        {

            if (eventId == 0)
            {
                return View();
            }

            var selectedEvent = await _eventCtcRepository.GetEventByIdAsync(eventId);
            if (selectedEvent == null)
            {
                return View("Error");
            }

        

            //await _eventCtcRepository.UpdateEventAsync(selectedEvent); // Save the QR code text
            var attendanceUrl = Url.Action("ConfirmAttendance", "Admin", new { eventId = eventId }, Request.Scheme);


            // Generate the QR code
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(attendanceUrl, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrBitmap = qrCode.GetGraphic(20);


            // Convert QR Code to byte array and Base64 string
            byte[] bitmapArray = qrBitmap.BitmapToByteArray();
            string qrUri = $"data:image/png;base64,{Convert.ToBase64String(bitmapArray)}";

            // Pass QR code URI to the view to display it
            ViewBag.QrCodeUri = qrUri;
            return View();
        }
        public async Task<IActionResult> ConfirmAttendance(int eventId)
        {
            var volunteer = await _usermanger.GetUserAsync(User);
            if (volunteer == null)
            {
                return Unauthorized("You must be logged in to confirm attendance.");
            }
            if (eventId == 0)
            {
                return View("Error");
            }
            var alreadyAttended = await _ctcDbContext.attendanceAtEveryEvents
          .AnyAsync(a => a.Student.Id == volunteer.Id && a.EventId == eventId);

            if (alreadyAttended)
            {
                return BadRequest("You have already confirmed attendance for this event and cannot earn points again.");
            }

            var attendanceRecord = new AttendanceAtEveryEvent
            {
                Student = volunteer,
                EventId = eventId
            };
            _ctcDbContext.attendanceAtEveryEvents.Add(attendanceRecord);
            await _ctcDbContext.SaveChangesAsync();
            if (!alreadyAttended)
            {
                volunteer.Points += 20;
            }

            await _usermanger.UpdateAsync(volunteer);

            TempData["SuccessMessage"] = "Attendance confirmed and points awarded!";
            return RedirectToAction("ConfirmAttendance");
        }
        public async Task<IActionResult> TableAttendanceEvents()
        {

            var attendanceRecords = await _ctcDbContext.attendanceAtEveryEvents
      .Include(a => a.Student) // Include related Student data if needed
      .Include(a => a.Event)   // Include related Event data if needed
      .ToListAsync();

            // Pass the attendance records to the view
            return View(attendanceRecords);
        }
        [HttpPost]
        public async Task<IActionResult> SendToLeader(int id)
        {
           
                var user = await _ctcDbContext.Joiners.FindAsync(id);

                if (user == null)
                {
                return Json(new { success = false, message = "User not found." });
                }
            
                var appointment = new Appointment
                {
                    UserId = user.Id,
                    Name = user.FirstName + " " + user.LastName,
                    AppointmentDate = DateTime.Now, // Or get this value from the form if needed
                    Status = "Waiting",
                    CreatedAt = DateTime.Now,
                    LinkMeeting=""
                };
                user.Status = "Waiting";
                _ctcDbContext.Appointment.Add(appointment);

                // Attempt to save the changes
                await _ctcDbContext.SaveChangesAsync();

                return View("DataTable");
          
        }
        public  IActionResult AddManager()
        {
            var model = new AddManager();
            return View(model);
        }
        [HttpPost]
        public async Task <IActionResult> AddManager(AddManager manger)
        {            
            var password = IdentityServiceExtensions.GenerateRandomPasswordHash();

            if(ModelState.IsValid)
            {
                var newmanger = new User
                {
                    UserName = manger.UserName,
                    Email = manger.Email,
                    PhoneNumber= manger.PhoneNumebr,
                    FullName = manger.FullName,
                    
                };

                var result = await _userRepository.CreateAsync(newmanger, password);
                if (result.Succeeded)
                {
                    string roleName = manger.TypeOfUser;

                    var mangerAccount = await _usermanger.FindByNameAsync(newmanger.UserName);
                    await _usermanger.AddToRoleAsync(mangerAccount, roleName);

                    var subject = "CTC Team";
                    var message = $"Dear {newmanger.UserName},\nI am the official spokesperson for the Computing Technology Club (CTC).\nYou have been selected to represent the:{roleName} department.\n" +
                                  $"Username: {newmanger.UserName}\nPassword: {password}\n\nPlease ensure to keep this information secure.\n\n" +
                                  $"If you have any questions or need assistance, feel free to reach out.\n\nBest regards,\nCTC Team";

                    await _mailService.SendEmailAsync(newmanger.Email, subject, message);
                    return RedirectToAction("AddManager"); 
                }

            }
            return View();
        }
        public IActionResult AddFounder()
        {
            var model = new Founders();
            return View(model);
        }
        [HttpPost]
        public async Task <IActionResult>AddFounder(Founders founders)
        {
            if(ModelState.IsValid)
            {
                string uniqueFileName = FileExtensions.ConvertImageToString(founders.ImageFile, _webHostEnvironment);

                var founder = new Founders
                { 
                    Name= founders.Name,
                    Description= founders.Description,  
                    ImageUrl = founders.ImageUrl = "/Pic/" + uniqueFileName,
                    position =founders.position,
                    Prefx=founders.Prefx,
                };
                 _ctcDbContext.founders.AddAsync(founder);
                await _ctcDbContext.SaveChangesAsync();
                TempData["SuccessMessage"] = "Founder added successfully!";
                return RedirectToAction("AddFounder");
            }
            return View("AddFounder");
        }
        public async Task <IActionResult> TableOfFounders()
        {
            var founders= await _ctcDbContext.founders.ToListAsync();
            return View(founders);
        }
        public async Task<IActionResult> EditFounder(int? id)
        {
            if (id == null)
            {
                return View("~/Views/Admin/EditFounder.cshtml");
            }

            var founder = await _ctcDbContext.founders.FindAsync(id);
            if (founder == null)
            {
                return NotFound();
            }
            return View("~/Views/Admin/EditFounder.cshtml", founder);

        }
        [HttpPost]
        public async Task<IActionResult> EditFounder(int id ,Founders founders)
        {
            string uniqueFileName = FileExtensions.ConvertImageToString(founders.ImageFile, _webHostEnvironment);

            if (id !=founders.Id)
            {
                return NotFound();
            }
            if(ModelState.IsValid)
            {

                founders.ImageUrl ="/Pic/" + uniqueFileName;
                _ctcDbContext.Update(founders);
                await _ctcDbContext.SaveChangesAsync();
                return RedirectToAction(nameof(TableOfFounders));
            }
            return View(founders);
        }
        public async Task<IActionResult> DeleteFounder(int id)
        {
            var founder = await _ctcDbContext.founders.FindAsync(id);
            if(founder == null)
            {
                return NotFound();
                
            }
                _ctcDbContext.founders.Remove(founder);
                await _ctcDbContext.SaveChangesAsync();
                return RedirectToAction(nameof(TableOfFounders));


        }
   
        public async Task<IActionResult> DeleteUser(int id)
        {

            var user = await _userRepository.GetByIdAsync(id.ToString());
            if(user == null)
            {
                return View("TableManager");
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUserConfirmed(int id)
        {
            var user = await _userRepository.GetByIdAsync(id.ToString());
            if (user == null)
            {
                return View("TableManager");
            }
            try
            {
                    var subject = "CTC Team";
                    var message = $"Dear {user.UserName},\nWe hope this message finds you well.\n" +
                                  $"\nWe regret to inform you that your membership with CTC Club is being removed.\n" +
                                  $"Thank you for your contributions, and we wish you the best in your future endeavors.\n" +
                                  $"Best regards,\nCTC Team";

                    await _mailService.SendEmailAsync(user.Email, subject, message);            
                    await _userRepository.DeleteAsync(user);
                    return RedirectToAction("AddManager");
                
                return RedirectToAction("TableManager"); 
            }
            catch (Exception ex)
            {
                // Handle any errors that may occur during deletion.
                ModelState.AddModelError(string.Empty, $"Error deleting user: {ex.Message}");
                return View(user); // Return to the delete confirmation page
            }
        }

        public async Task<IActionResult> SendMessagesToAnyUser()
        {
            var users = await _userRepository.GetAllUsersAsync();

            return View("SendMessagesToAnyUser",users);
        }
        [HttpPost]
        public async Task <IActionResult> SendMessagesToAnyUser(IEnumerable<string> selectedUsers, string messageBody, string subject)
        {
            if (selectedUsers == null || !selectedUsers.Any())
            {
                TempData["ErrorMessage"] = "No users selected to send messages.";
                return RedirectToAction("SendMessagesToAnyUser"); 
            }
            if (string.IsNullOrWhiteSpace(messageBody) || string.IsNullOrWhiteSpace(subject))
            {
                TempData["ErrorMessage"] = "Subject and Message Body are required.";
                return RedirectToAction("SendMessagesToAnyUser"); 
            }
            foreach (var userEmail in selectedUsers)
            {
                var user = await _usermanger.FindByEmailAsync(userEmail);
                if (user != null)
                {
                    await _mailService.SendEmailAsync(user.Email, messageBody, subject);
                }
                else
                {
                    TempData["ErrorMessage"] = $"Failed to send email to {user.Email}.";
                }

            }
            TempData["SuccessMessage"] = "Messages sent successfully!";

            var allUsers = _usermanger.Users.ToList(); // Fetch all users
            return View("SendMessagesToAnyUser", allUsers);
        }

       
        public async Task<IActionResult> EditCTCData()
        {
            var ctcdata = await _ctcDbContext.ctcData.FirstOrDefaultAsync();

            if (ctcdata == null)
            {
                ctcdata = new CtcData
                {
                    Email = "CTC@Gmail.com",
                    City = "Jordan",
                    Address = "AL-Ramtha",
                    FaceBook = "https://www.facebook.com/profile.php?id=61552529775957",
                    Instagram = "https://www.instagram.com/ctc_just/",
                    LinedIn = "https://www.linkedin.com/company/computing-technology-club-ctc/posts/?feedView=all",
                    PostalCode = "21118",
                    PhoneNumber = "+962 7 9984 2558",
                    Nahno = "https://www.nahno.org/ngo/%D9%86%D8%A7%D8%AF%D9%8A-%D8%AA%D9%83%D9%86%D9%88%D9%84%D9%88%D8%AC%D9%8A%D8%A7-%D8%A7%D9%84%D8%AD%D9%88%D8%B3%D8%A8%D8%A9-CTC-73508",
                };
            }

            return View("EditCTCData", ctcdata);
        } 
        [HttpPost]
        public async Task<IActionResult> EditCTCData(CtcData model)
        {
            if (ModelState.IsValid)
            {

                var ctcdata = await _ctcDbContext.ctcData.FirstOrDefaultAsync();
                
                    if (ctcdata == null)
                    {
                        // Add a new record if not found
                        _ctcDbContext.ctcData.Add(model);
                    }
                    else
                    {
                    // Update existing record
                    ctcdata.Email = model.Email;
                    ctcdata.City = model.City;
                    ctcdata.Address = model.Address;
                    ctcdata.FaceBook = model.FaceBook;
                    ctcdata.Instagram = model.Instagram;
                    ctcdata.LinedIn = model.LinedIn;
                    ctcdata.PostalCode = model.PostalCode;
                    ctcdata.PhoneNumber = model.PhoneNumber;
                    ctcdata.Nahno = model.Nahno;


                }
                await _ctcDbContext.SaveChangesAsync();
                }
            return View("EditCTCData", model);

        }

        public async Task<IActionResult> CreateEvent()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent(EventsCTC model )
        {
            if (ModelState.IsValid)
            {
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    string uniqueFileName = FileExtensions.ConvertImageToString(model.ImageFile, _webHostEnvironment);

                    try
                    {
                        var eventEntity = new EventsCTC
                        {
                            Name = model.Name,
                            Description = model.Description,
                            EventDate = model.EventDate,
                            Location = model.Location,
                            EventType = model.EventType, 
                            ImageUrl = "/Pic/" + uniqueFileName  
                        };
                        await _eventCtcRepository.AddEventAsync(eventEntity);
                        return RedirectToAction("CreateEvent", "Admin");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "An error occurred while creating the event. Please try again.");
                    }
                }
                else
                {
                    ModelState.AddModelError("ImageFile", "Please provide an image file.");
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                foreach (var error in errors)
                {
                    Console.WriteLine(error);
                }
            }
            return View();
        }

      

    }
    public static class BitmapExtensions
    {
        public static byte[] BitmapToByteArray(this Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
    }
    
  

}
