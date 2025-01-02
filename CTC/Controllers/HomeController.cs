using CTC.Data;
using CTC.Models;
using CTC.Repository.Enum;
using CTC.Repository.IRepository;
using CTC.ViewModels.MemberShip;
using CTC.ViewModels.Academic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CTC.Models.MediaModels;
using CTC.Models.Academic;
using CTC.Models.Admin;
using CTC.Models.Leader;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace CTC.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IJoinerRepository _joinerRepository;
        private readonly IAcademicRepository _academicRepository;

        public HomeController(
            IWebHostEnvironment environment,
            CtcDbContext ctcDbContext,
            UserManager<User> userManager,
            IUserRepository userRepository,
            IMailService mailService,
            IEventCtcRepository eventCtcRepository,
            INotificationRepository notificationRepository,
            ILogger<HomeController> logger,
            IJoinerRepository joinerRepository,
            IAcademicRepository academicRepository)
            : base(environment, ctcDbContext, userManager, userRepository, mailService,
                   eventCtcRepository, notificationRepository, logger)
        {
            _joinerRepository = joinerRepository;
            _academicRepository = academicRepository;
        }

        public async Task<IActionResult> Index()
        {
            var whoWeAre = await _ctcDbContext.whoWeAre.FirstOrDefaultAsync();
            var nahno = await _ctcDbContext.nahno.FirstOrDefaultAsync();
            var contactMessage = await _ctcDbContext.contactMessages.FirstOrDefaultAsync();
            var features = _ctcDbContext.featuresApp.ToList();
            var media = await _ctcDbContext.videohome.FirstOrDefaultAsync();
            var founderlist = await _ctcDbContext.founders.ToListAsync();
            var esportsEntry = await _ctcDbContext.esports.FirstOrDefaultAsync();
            var sponser = _ctcDbContext.sponsers.ToList();
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

            if (whoWeAre == null)
            {
                whoWeAre = new WhoWeAre
                {
                    Header = "Unleash the capabilities with the creative strategy",
                    Content = "It is a non-profit student organization that aims to provide an educational and developmental environment for university students interested in the fields of information technology and computer science.",
                    CountStudent = 200,
                    Footer = "Join CTC to explore new technologies, engage in exciting projects, and excel in your academic and professional journey in IT and computer science."

                };
            }

            if (nahno == null)
            {
                nahno = new Nahno
                {
                    Content = "We are pleased to provide you with our documented achievements on the Nahu platform since the club’s opening. We aspire to achieve more achievements with you.",
                    subjectone = "1603 hours of teaching.",
                    subjecttwo = "273 hours of assistance.",
                    subjectThree = "720 hours of leadership.",
                    Link = "https://www.nahno.org/ngo/%D9%86%D8%A7%D8%AF%D9%8A-%D8%AA%D9%83%D9%86%D9%88%D9%84%D9%88%D8%AC%D9%8A%D8%A7-%D8%A7%D9%84%D8%AD%D9%88%D8%B3%D8%A8%D8%A9-CTC-73508"
                };
            }

            if (features == null)
            {
                features = new List<FeaturesApp>
            {
                new FeaturesApp{Header="Up Coming Event",Content="Stay updated with our constantly evolving events calendar, packed with workshops, seminars, and hackathons designed to sharpen your tech skills.",ImageUrl=""},
                new FeaturesApp{Header="Up Coming Volunteering Work",Content="This topic could explore how volunteering helps individuals develop essential life skills, such as leadership, communication, and teamwork.",ImageUrl=""},
            };
            }

            if (media == null)
            {
                media = new Videohome();
            }

            if (esportsEntry == null)
            {
                esportsEntry = new Esports
                {
                    HeaderEsports = "CTC Esports Team",
                    ContentEsports = "CTC Esports is the competitive gaming arm , dedicated to fostering a community of passionate gamers and esports enthusiasts. Our goal is to create a platform where students can excel in the world of competitive gaming while developing skills in teamwork, strategy, and communication.",
                    Games = new List<string> { "Valorant", "EA FC 25" },
                    ContentGames = new List<string>
                    {
                            "Valorant is a tactical FPS Teams of five compete in strategic rounds, either attacking or defending objectives.",
                            "EA Sports FC 25 is the new football game offering realistic gameplay, enhanced graphics, and various game modes.",
                    },
                    ImageUrl = ""
                };


            }

            if (sponser == null)
            {
                sponser = new List<Sponser>
                {
                    new Sponser{Name="Client 1" ,Description="Client description",ImageUrl=""},
                    new Sponser{Name="Client 2" ,Description="Client description",ImageUrl=""},
                    new Sponser{Name="Client 3" ,Description="Client description",ImageUrl=""},
                    new Sponser{Name="Client 4" ,Description="Client description",ImageUrl=""},
                    new Sponser{Name="Client 5" ,Description="Client description",ImageUrl=""}

                };
            }

            var model = new Combination
            {
                WhoWeAre = whoWeAre,
                Nahno = nahno,
                FeatureApp = features,
                VideoHome = media,
                Founders = founderlist,
                esports = esportsEntry,
                sponsers = sponser,
                CtcData = ctcdata,
                BachelorPrograms = _ctcDbContext.bachelorPrograms.ToList(),

            };

            return View(model);
        }
        public async Task<IActionResult> Events()
        {
            try
            {
                var events = await _eventCtcRepository.GetAllEventsAsync();
                events ??= new List<CTC.Models.Event.EventsCTC>();

                return View("~/Views/Home/Events.cshtml", events);
            }
            catch (Exception ex)
            {
                return View("~/Views/Home/Events.cshtml", new List<CTC.Models.Event.EventsCTC>());
            }
        }
        public async Task<IActionResult> About()
        {
            var achievements = await _ctcDbContext.Achievements
                 .OrderByDescending(a => a.Date)
                 .ToListAsync();

            return View(achievements);
        }
        [HttpPost]
        public async Task<IActionResult> Index(ContactMessage contactMessage)
        {
            if (ModelState.IsValid)
            {
                _ctcDbContext.contactMessages.Add(contactMessage);
                await _ctcDbContext.SaveChangesAsync();
                TempData["MessageSent"] = "Your message has been sent. Thank you!";
                return RedirectToAction(nameof(Index));

            }
            return View(contactMessage);
        }
        public async Task<IActionResult> Facultymembers(string selectedDepartment)
        {
            // Retrieve all faculty members from the database
            var facultyMembers = await _academicRepository.GetAllFactualMemberAsync();
            facultyMembers = facultyMembers.Where(e => e.Approved).ToList();

            // Apply department filter if provided
            if (!string.IsNullOrEmpty(selectedDepartment))
            {
                Department departmentEnum;
                if (Enum.TryParse(selectedDepartment, out departmentEnum))
                {
                    facultyMembers = facultyMembers.Where(e => e.department == departmentEnum).ToList();
                }
            }

            // Map the filtered or unfiltered faculty members to the view model
            var viewModel = facultyMembers.Select(member => new FacultymembersViewModel
            {
                Name = member.NameDoctor,
                Email = member.Email,
                prefx = member.prefx,
                department = member.department,
            }).ToList();

            // Pass selected department back to the view
            ViewBag.SelectedDepartment = selectedDepartment;

            return View(viewModel);
        }
        public IActionResult BachelorPrograms()
        {
            var bachelorPrograms = _ctcDbContext.bachelorPrograms.ToList();
            if (bachelorPrograms == null)
            {
                bachelorPrograms = new List<BachelorPrograms>
                {
                 new BachelorPrograms { Name="Computer Engineering", Description="Learn anout the Computer Engineering department—our workshops offer practical experience in the latest technologies.",PdfUrl=""},

                };
            }
            return View(bachelorPrograms);
        }
        public IActionResult Join()
        {
            var model = new JoinerViewModel();

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(JoinerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model); // Return view if model validation fails
            }

            var joiner = MapJoinerViewModelToEntity(model);
            await _joinerRepository.JoinMemberAsync(joiner);

            // Send email to the joiner confirming their request was received
            await SendJoinConfirmationEmail(model);

            // Notify admin of the new join request via notification and email
            await NotifyAdminOfJoinRequest(model);

            return View(model);
        }
        private Joiner MapJoinerViewModelToEntity(JoinerViewModel model)
        {
            return new Joiner
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Address = model.Address,
                DateOfBirth = model.DateOfBirth,
                Department = model.Department,
                Facebook = model.Facebook,
                Gender = model.Gender,
                LinkedIn = model.LinkedIn,
                Phone = model.Phone,
                UniversityEmail = model.UniversityEmail,
                UniversityID = model.UniversityID,
                YourMessage = model.YourMessage,
                Status = "Pending"
            };
        }
        private async Task SendJoinConfirmationEmail(JoinerViewModel model)
        {
            var subject = "Your Join Request Received";
            var message = $@"Dear {model.FirstName},

            Thank you for your request to join the club. Your request is currently under review. We will get back to you soon.

            Best regards,
            CTC Team";
            await _mailService.SendEmailAsync(model.UniversityEmail, subject, message);
        }
        private async Task NotifyAdminOfJoinRequest(JoinerViewModel model)
        {
            var notification = new Notification
            {
                Username = $"{model.FirstName} {model.LastName}",
                Message = $"{model.FirstName} {model.LastName} has submitted a join request.",
                IsRead = false,
                CreatedAt = DateTime.Now,
                NotificationType = "JoinRequest"
            };
            await _notificationRepository.AddNotification(notification);

            // Optionally, send an email to the admin
            var adminEmailSubject = "New Join Request Submitted";
            var adminEmailMessage = $@"Dear Admin,

            A new join request has been submitted by {model.FirstName} {model.LastName}. Please review it in the admin panel.

            Best regards,
            CTC Team";
            await _mailService.SendEmailAsync("admin@ctc.com", adminEmailSubject, adminEmailMessage);
        }
        public async Task<IActionResult> SummaryMaterial(string selectedDepartment)
        {
            var material = await _academicRepository.GetAllMaterialsAsync();
            material = material.Where(e => e.Approved).ToList();

            // Filter by selected department if provided
            if (!string.IsNullOrEmpty(selectedDepartment))
            {
                Department departmentEnum;
                if (Enum.TryParse(selectedDepartment, out departmentEnum))
                {
                    material = material.Where(e => e.materialsDepartment == departmentEnum).ToList();
                }
            }

            var model = material.Select(e => new MaterialSummaryViewModel
            {
                Id = e.Id,
                MaterialName = e.MaterialName,
                MaterialDescription = e.MaterialDescription,
                materialsDepartment = e.materialsDepartment,
                UploadDate = e.UploadDate,
            });

            // Pass selected department back to the view
            ViewBag.SelectedDepartment = selectedDepartment;

            return View(model);
        }
        public async Task<IActionResult> Download(int id)
        {

            var material = await _academicRepository.GetMaterialByIDAsync(id);

            if (material == null)
            {
                _logger.LogWarning("Material not found for ID: {id}", id);
                return NotFound(); // Return 404 if material is not found
            }

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, material.PdfUrl.TrimStart('/'));

            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogWarning("File not found at path: {filePath}", filePath);
                return NotFound(); // Return 404 if the file doesn't exist
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "application/pdf", Path.GetFileName(filePath));
        }
        public IActionResult DownloadPdf(string department)
        {
            // Construct the file path based on department name
            var fileName = department + ".pdf";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "PDF", fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf", fileName);
        }
        public IActionResult CalculateGPA()
        {
            return View();
        }
        [HttpPost]
        public IActionResult CalculateGPA(string[] courseNames, int[] creditHours, string[] grades)
        {
            if (courseNames == null || creditHours == null || grades == null ||
                courseNames.Length == 0 || creditHours.Length == 0 || grades.Length == 0 ||
                courseNames.Length != creditHours.Length || courseNames.Length != grades.Length)
            {
                ModelState.AddModelError("", "Please provide valid input for all courses.");
                return View();
            }

            double totalCreditHours = 0;
            double totalGradePoints = 0;

            for (int i = 0; i < courseNames.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(courseNames[i]) || creditHours[i] <= 0 || string.IsNullOrWhiteSpace(grades[i]))
                {
                    ModelState.AddModelError("", $"Please provide valid details for course: {courseNames[i] ?? "Unnamed Course"}.");
                    return View();
                }

                totalCreditHours += creditHours[i];
                totalGradePoints += FindGradePoints(grades[i]) * creditHours[i];
            }

            double gpa = totalCreditHours > 0 ? totalGradePoints / totalCreditHours : 0;
            ViewBag.GPA = gpa;

            return View();
        }
        private double FindGradePoints(string grade)
        {
            return grade switch
            {
                "A+" => 4.3,
                "A" => 4.0,
                "A-" => 3.7,
                "B+" => 3.3,
                "B" => 3.0,
                "B-" => 2.7,
                "C+" => 2.3,
                "C" => 2.0,
                "C-" => 1.7,
                "D+" => 1.3,
                "D" => 1.0,
                "D-" => 0.7,
                "F" => 0.0,
                _ => 0.0,
            };
        }
        public async Task<IActionResult> VolunteerWork()
        {
            var volunteerWorks = await _ctcDbContext.volunteering.ToListAsync();
            return View(volunteerWorks);
        }

        public async Task<IActionResult> AdminstrationDepartment()
        {
            var users = await _usermanger.Users.ToListAsync();
            return View(users);
        }

    }
}
