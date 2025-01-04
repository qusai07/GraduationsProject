using Microsoft.AspNetCore.Mvc;
using CTC.Models.MediaModels;
using CTC.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using CTC.Extensions;
using CTC.Models;
using Microsoft.AspNetCore.Identity;


namespace CTC.Controllers
{
    [Authorize(Roles = "MediaManager")]
    public class MediaController : BaseController
    {
        public MediaController(
         IWebHostEnvironment environment,
         CtcDbContext ctcDbContext,
         UserManager<User> userManager,
         ILogger<HomeController> logger)
         : base(environment, ctcDbContext, userManager, logger: logger)
        {
        } 
        // Media Dashboard
        public IActionResult Index()
        {

            ViewData["Title"] = "Media Management";
            return View("~/Views/LeaderDepartment/Media/Index.cshtml");
        }
        public async Task<IActionResult> EditVideoHome()
        {
            var videoHome = await _ctcDbContext.videohome.FirstOrDefaultAsync();
            return View("~/Views/LeaderDepartment/Media/EditVideoHome.cshtml", videoHome ?? new Videohome());
        }
        [HttpPost]
        public async Task<IActionResult> EditVideoHome(Videohome model)
        {
            if (model.VideoFile != null && model.VideoFile.Length > 0)
            {
                string[] allowedExtensions = { ".mp4", ".avi", ".mov" };
                string fileExtension = Path.GetExtension(model.VideoFile.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("VideoFile", "Invalid video file format. Allowed formats: .mp4, .avi, .mov");
                    return View("~/Views/LeaderDepartment/Media/EditVideoHome.cshtml", model);
                }

                try
                {
                    model.VideoUrl = FileExtensions.ConvertVideoToString(model.VideoFile, _webHostEnvironment);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("VideoFile", "Error processing video file: " + ex.Message);
                    return View("~/Views/LeaderDepartment/Media/EditVideoHome.cshtml", model);
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingVideo = await _ctcDbContext.videohome.FirstOrDefaultAsync();
                    if (existingVideo == null)
                    {
                        _ctcDbContext.videohome.Add(model);
                    }
                    else
                    {
                        existingVideo.VideoUrl = model.VideoUrl;
                    }
                    await _ctcDbContext.SaveChangesAsync();
                    return View("~/Views/LeaderDepartment/Media/EditVideoHome.cshtml", model);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error saving to database: " + ex.Message);
                }
            }

            // If we get here, something went wrong
            return View("~/Views/LeaderDepartment/Media/EditVideoHome.cshtml", model);
        }

        public async Task<IActionResult> EditWhoWeAre()
        {

            var whoWeAre = await _ctcDbContext.whoWeAre.FirstOrDefaultAsync();           

            if (whoWeAre == null)
            {
                whoWeAre = new WhoWeAre
                {
                    Header = "Unleash the capabilities with the creative strategy",
                    Content = "It is a non-profit student organization that aims to provide an educational and developmental environment for university students interested in the fields of information technology and computer science.",
                    CountStudent = 200,
                    Footer = "Join CTC to explore new technologies, engage in exciting projects, and excel in your academic and professional journey in IT and computer science.",
                    ImageUrl = "" 
                };
            }
            return View("~/Views/LeaderDepartment/Media/EditWhoWeAre.cshtml", whoWeAre);
        }

        [HttpPost]
        public async Task<IActionResult> EditWhoWeAre(WhoWeAre model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var whoWeAre = await _ctcDbContext.whoWeAre.FirstOrDefaultAsync();

                    if (model.ImageFile != null && model.ImageFile.Length > 0)
                    {
                        string uniqueFileName = FileExtensions.ConvertImageToString(model.ImageFile, _webHostEnvironment);
                        model.ImageUrl = "/Pic/" + uniqueFileName;
                    }

                    if (whoWeAre == null)
                    {
                        // Add new record
                        await _ctcDbContext.whoWeAre.AddAsync(model);
                    }
                    else
                    {
                        // Update existing record
                        whoWeAre.Header = model.Header;
                        whoWeAre.Content = model.Content;
                        whoWeAre.CountStudent = model.CountStudent;
                        whoWeAre.Footer = model.Footer;
                        if (!string.IsNullOrEmpty(model.ImageUrl))
                        {
                            whoWeAre.ImageUrl = model.ImageUrl;
                        }
                    }

                    await _ctcDbContext.SaveChangesAsync();
                    return View("~/Views/LeaderDepartment/Media/EditWhoWeAre.cshtml", model);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error saving data: {ex.Message}");
                }
            }

            return View("~/Views/LeaderDepartment/Media/EditWhoWeAre.cshtml", model);
        }
        public async Task<IActionResult> EditNahnoInfo()
        {
            var nahno = await _ctcDbContext.nahno.FirstOrDefaultAsync();
            if (nahno == null)
            {
                nahno = new Nahno
                {
                    Content = "We are pleased to provide you with our documented achievements on the Nahu platform since the club’s opening. We aspire to achieve more achievements with you.",
                    subjectone= "1603 hours of teaching.",
                    subjecttwo= "273 hours of assistance.",
                    subjectThree= "720 hours of leadership.",
                    Link = "https://www.nahno.org/ngo/%D9%86%D8%A7%D8%AF%D9%8A-%D8%AA%D9%83%D9%86%D9%88%D9%84%D9%88%D8%AC%D9%8A%D8%A7-%D8%A7%D9%84%D8%AD%D9%88%D8%B3%D8%A8%D8%A9-CTC-73508",
                    ImageUrl=""
                };
            }
            return View("~/Views/LeaderDepartment/Media/EditNahnoInfo.cshtml", nahno);
        }

        [HttpPost]
        public async Task<IActionResult> EditNahnoInfo(Nahno model)
        {
            if (ModelState.IsValid && model.ImageFile != null && model.ImageFile.Length > 0)
            {
                try
                {
                    string uniqueFileName = FileExtensions.ConvertImageToString(model.ImageFile, _webHostEnvironment);
                    var nahno = await _ctcDbContext.nahno.FirstOrDefaultAsync();

                    if (nahno == null)
                    {
                        // Add a new record if not found
                        model.ImageUrl = "/Pic/" + uniqueFileName;
                        await _ctcDbContext.nahno.AddAsync(model);
                    }
                    else
                    {
                        // Update existing record
                        nahno.Content = model.Content;
                        nahno.subjectone = model.subjectone;
                        nahno.subjecttwo = model.subjecttwo;
                        nahno.subjectThree = model.subjectThree;
                        nahno.Link = model.Link;
                        nahno.ImageUrl = "/Pic/" + uniqueFileName;
                    }

                    await _ctcDbContext.SaveChangesAsync();
                    return View("~/Views/LeaderDepartment/Media/EditNahnoInfo.cshtml", model);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error saving data: {ex.Message}");
                }
            }

            return View("~/Views/LeaderDepartment/Media/EditNahnoInfo.cshtml", model);
        }

        public async Task<IActionResult> EditFeatureInfo()
        {
            if (!_ctcDbContext.featuresApp.Any())
            {
                var DefaultFeatures = new List<FeaturesApp>
                {
                    new FeaturesApp{Header="Up Coming Event",Content="Stay updated with our constantly evolving events calendar, packed with workshops, seminars, and hackathons designed to sharpen your tech skills.",ImageUrl=""},
                    new FeaturesApp{Header="Up Coming Volunteering Work",Content="This topic could explore how volunteering helps individuals develop essential life skills, such as leadership, communication, and teamwork.",ImageUrl=""},
                };
                _ctcDbContext.featuresApp.AddRange(DefaultFeatures);
                await _ctcDbContext.SaveChangesAsync();
            }
            var features = await _ctcDbContext.featuresApp.ToListAsync();
            return View("~/Views/LeaderDepartment/Media/EditFeatureInfo.cshtml", features);
        }
        [HttpPost]
        public async Task<IActionResult> EditFeatureInfo(FeaturesApp model)
        {
            if (ModelState.IsValid)
            {

                var featureApp = await _ctcDbContext.featuresApp.FirstOrDefaultAsync(f => f.Id == model.Id);
                string uniqueFileName = FileExtensions.ConvertImageToString(model.ImageFile, _webHostEnvironment);
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    if (featureApp == null)
                    {
                        // Add new record if it doesn't exist
                        _ctcDbContext.featuresApp.Add(model);
                    }
                    else
                    {
                        // Update existing record
                        featureApp.Header = model.Header;
                        featureApp.Content = model.Content;
                        featureApp.Features = model.Features ?? new List<string>();
                        featureApp.ImageUrl = model.ImageUrl = "/Pic/" + uniqueFileName;
                    }
                }
                await _ctcDbContext.SaveChangesAsync();
                return View("~/Views/LeaderDepartment/Media/EditFeatureInfo.cshtml");
            }

            return View("~/Views/LeaderDepartment/Media/EditFeatureInfo.cshtml",model);
        }
        public async Task<IActionResult> EditEsportInfo()
        {
            var esport = await _ctcDbContext.esports.FirstOrDefaultAsync();
            if (esport == null)
            {
                esport = new Esports
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
            return View("~/Views/LeaderDepartment/Media/EditEsportInfo.cshtml", esport);
        }

        [HttpPost]
        public async Task<IActionResult> EditEsportInfo(Esports model)
        {
            if (ModelState.IsValid)
            {
                // Retrieve the existing esport record
                var esport = await _ctcDbContext.esports.FirstOrDefaultAsync();

                // Handle image file upload if provided
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    // Generate the unique file name for the image
                    string uniqueFileName = FileExtensions.ConvertImageToString(model.ImageFile, _webHostEnvironment);

                    if (esport == null)
                    {
                        // Add new record if esport doesn't exist
                        model.ImageUrl = "/Pic/" + uniqueFileName; // Set the image URL
                        _ctcDbContext.esports.Add(model); // Add the new esport record
                    }
                    else
                    {
                        // Update existing esport record
                        esport.HeaderEsports = model.HeaderEsports;
                        esport.ContentEsports = model.ContentEsports;
                        esport.ContentGames = model.ContentGames;
                        esport.Games = model.Games;
                        esport.ImageUrl = "/Pic/" + uniqueFileName; // Set the image URL
                    }
                }
                else if (esport != null)
                {
                    // If no image file is provided, update other properties only
                    esport.HeaderEsports = model.HeaderEsports;
                    esport.ContentEsports = model.ContentEsports;
                    esport.ContentGames = model.ContentGames;
                    esport.Games = model.Games;
                }

                // Save changes to the database
                await _ctcDbContext.SaveChangesAsync();

                // Return to the same view with the updated model
                return View("~/Views/LeaderDepartment/Media/EditEsportInfo.cshtml", esport);
            }

            // In case of validation failure, return the view with the current model and errors
            return View("~/Views/LeaderDepartment/Media/EditEsportInfo.cshtml", model);
        }


        [HttpPost]
        public async Task<IActionResult> DeleteGame(int esportId, int gameIndex)
        {
            // Retrieve the specific esport record by Id
            var esport = await _ctcDbContext.esports.FirstOrDefaultAsync(e => e.Id == esportId);

            if (esport != null)
            {
                // Check if the index is valid
                if (gameIndex >= 0 && gameIndex < esport.Games.Count)
                {
                    // Remove the game and content at the specified index
                    esport.Games.RemoveAt(gameIndex);
                    esport.ContentGames.RemoveAt(gameIndex);

                    // Save the changes to the database
                    await _ctcDbContext.SaveChangesAsync();
                }
            }

            // Return a partial view or updated model after deletion
            return RedirectToAction("EditEsportInfo");
        }



        public async Task<IActionResult> EditSponserInfo()
        {
            if(!_ctcDbContext.sponsers.Any())
            {
                var DefaultSponser = new List<Sponser>
                {
                    new Sponser{Name="Client 1" ,Description="Client description",ImageUrl=""},
                    new Sponser{Name="Client 2" ,Description="Client description",ImageUrl=""},
                    new Sponser{Name="Client 3" ,Description="Client description",ImageUrl=""},
                    new Sponser{Name="Client 4" ,Description="Client description",ImageUrl=""},
                    new Sponser{Name="Client 5" ,Description="Client description",ImageUrl=""}

                };
                _ctcDbContext.sponsers.AddRange(DefaultSponser);
                await _ctcDbContext.SaveChangesAsync();
            }
            var sponser = await _ctcDbContext.sponsers.ToListAsync();
            return View("~/Views/LeaderDepartment/Media/EditSponserInfo.cshtml", sponser);
        }
        [HttpPost]
        public async Task<IActionResult> EditSponserInfo(Sponser model)
        {
            if (ModelState.IsValid)
            {
                // Check if the sponsor already exists in the database
                var sponser = await _ctcDbContext.sponsers.FirstOrDefaultAsync(s => s.Id == model.Id);

                if (sponser == null)
                {
                    // If not, create a new sponsor entry
                    _ctcDbContext.sponsers.Add(model);
                }
                else
                {
                    // If sponsor exists, update the existing one
                    sponser.Name = model.Name;
                    sponser.Description = model.Description;
                    sponser.sponsers = model.sponsers ?? new List<string>();

                    // Handle image file upload, if a new file is uploaded
                    if (model.ImageFile != null && model.ImageFile.Length > 0)
                    {
                        // Generate unique file name and save the image
                        string uniqueFileName = FileExtensions.ConvertImageToString(model.ImageFile, _webHostEnvironment);
                        sponser.ImageUrl = "/Pic/" + uniqueFileName;  // Update the image URL in the sponsor object
                    }
                }

                // Save changes to the database
                await _ctcDbContext.SaveChangesAsync();

                // Redirect to the same page with the updated sponsor object
                return RedirectToAction("EditSponserInfo", new { id = model.Id });
            }
            else
            {
                // If model is invalid, return the same view with the model to show validation errors
                return View("~/Views/LeaderDepartment/Media/EditSponserInfo.cshtml", model);
            }
        }


    }

}
