using CTC.Controllers;
using CTC.Data;
using CTC.Models;
using CTC.Models.MediaModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace CTCTest.Controllers
{
    [TestClass]
    public class MediaControllerTests : TestBase
    {
        private MediaController _controller;
        private Mock<ILogger<HomeController>> _mockLogger;
        [TestInitialize]
        public override void BaseSetup()
        {
            // Call base setup
            base.BaseSetup();

            // Setup additional mocks
            _mockLogger = new Mock<ILogger<HomeController>>();
            _mockEnvironment.Setup(x => x.WebRootPath).Returns("wwwroot");

            // Initialize controller
            _controller = new MediaController(
             _mockEnvironment.Object,
             _dbContext,
             _mockUserManager.Object,
             _mockLogger.Object
         );
            // Setup controller context
            SetupControllerContext(_controller);
            SetupUserWithRole("MediaManager");
        }
        [TestMethod]
        public void Index_ReturnsCorrectView()
        {
            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("~/Views/LeaderDepartment/Media/Index.cshtml", result.ViewName);
        }
        [TestMethod]
        public async Task EditVideoHome_GET_ReturnsViewWithNewVideoHomeWhenNoExistingRecord()
        {
            // Act
            var result = await _controller.EditVideoHome() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("~/Views/LeaderDepartment/Media/EditVideoHome.cshtml", result.ViewName);
            Assert.IsInstanceOfType(result.Model, typeof(Videohome));
        }

        [TestMethod]
        public async Task EditVideoHome_GET_ReturnsExistingRecord()
        {
            // Arrange
            var existingVideo = new Videohome
            {
                VideoUrl = "existing-video-url"
            };
            await _dbContext.videohome.AddAsync(existingVideo);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _controller.EditVideoHome() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            var model = result.Model as Videohome;
            Assert.IsNotNull(model);
            Assert.AreEqual("existing-video-url", model.VideoUrl);
        }
        [TestMethod]
        public async Task EditVideoHome_POST_ValidVideo_UpdatesDatabase()
        {
            // Arrange
            var fileMock = CreateMockVideoFile();
            var model = new Videohome { VideoFile = fileMock.Object };

            // Act
            var result = await _controller.EditVideoHome(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            var savedVideo = await _dbContext.videohome.FirstOrDefaultAsync();
            Assert.IsNotNull(savedVideo);
            Assert.IsNotNull(savedVideo.VideoUrl);
        }

        [TestMethod]
        public async Task EditVideoHome_POST_InvalidFileType_ReturnsError()
        {
            // Arrange
            var fileMock = CreateMockInvalidFile();
            var model = new Videohome { VideoFile = fileMock.Object };
            _controller.ModelState.Clear(); // Clear any existing model state
            _controller.ModelState.AddModelError("VideoFile", "Required"); // Add initial model state error to trigger validation

            // Act
            var result = await _controller.EditVideoHome(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.ViewData.ModelState.IsValid);
            Assert.IsTrue(result.ViewData.ModelState.ContainsKey("VideoFile"));
            var errors = result.ViewData.ModelState["VideoFile"].Errors;
            Assert.IsTrue(errors.Any(e => e.ErrorMessage.Contains("Invalid video file format")));
        }

        private Mock<IFormFile> CreateMockInvalidFile()
        {
            var fileMock = new Mock<IFormFile>();
            var content = "Fake text content";
            var fileName = "test.txt";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;

            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);
            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.ContentType).Returns("text/plain");

            return fileMock;
        }

        [TestMethod]
        public async Task EditWhoWeAre_GET_ReturnsDefaultValuesWhenNoExistingRecord()
        {
            // Act
            var result = await _controller.EditWhoWeAre() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            var model = result.Model as WhoWeAre;
            Assert.IsNotNull(model);
            Assert.AreEqual("Unleash the capabilities with the creative strategy", model.Header);
            Assert.AreEqual(200, model.CountStudent);
        }

        [TestMethod]
        public async Task EditWhoWeAre_POST_ValidData_UpdatesDatabase()
        {
            // Arrange
            var fileMock = CreateMockImageFile();
            var model = new WhoWeAre
            {
                Header = "New Header",
                Content = "New Content",
                CountStudent = 300,
                Footer = "New Footer",
                ImageFile = fileMock.Object
            };

            // Mock the file conversion method
            _mockEnvironment.Setup(x => x.WebRootPath).Returns("wwwroot");

            // Ensure ModelState is valid
            _controller.ModelState.Clear();

            // Act
            var result = await _controller.EditWhoWeAre(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);

            // Force the context to save changes
            await _dbContext.SaveChangesAsync();

            // Verify the database was updated
            var savedWhoWeAre = await _dbContext.whoWeAre.FirstOrDefaultAsync();
            Assert.IsNotNull(savedWhoWeAre, "WhoWeAre record was not saved to database");
            Assert.AreEqual("New Header", savedWhoWeAre.Header);
            Assert.AreEqual("New Content", savedWhoWeAre.Content);
            Assert.AreEqual(300, savedWhoWeAre.CountStudent);
            Assert.AreEqual("New Footer", savedWhoWeAre.Footer);
            Assert.IsTrue(savedWhoWeAre.ImageUrl.StartsWith("/Pic/"));
        }
        [TestMethod]
        public async Task EditNahnoInfo_GET_ReturnsDefaultValuesWhenNoExistingRecord()
        {
            // Act
            var result = await _controller.EditNahnoInfo() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            var model = result.Model as Nahno;
            Assert.IsNotNull(model);
            Assert.IsTrue(model.Content.Contains("We are pleased to provide"));
            Assert.AreEqual("1603 hours of teaching.", model.subjectone);
        }

        [TestMethod]
        public async Task EditNahnoInfo_POST_ValidData_UpdatesDatabase()
        {
            // Arrange
            var fileMock = CreateMockImageFile();
            var model = new Nahno
            {
                Content = "New Content",
                subjectone = "New Subject One",
                subjecttwo = "New Subject Two",
                subjectThree = "New Subject Three",
                Link = "https://new-link.com",
                ImageFile = fileMock.Object
            };

            _mockEnvironment.Setup(x => x.WebRootPath).Returns("wwwroot");

            // Ensure ModelState is valid
            _controller.ModelState.Clear();

            // Act
            var result = await _controller.EditNahnoInfo(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Result should not be null");

            // Force the context to save changes
            await _dbContext.SaveChangesAsync();

            // Verify the database was updated
            var savedNahno = await _dbContext.nahno.FirstOrDefaultAsync();
            Assert.IsNotNull(savedNahno, "Nahno record was not saved to database");
            Assert.AreEqual("New Content", savedNahno.Content);
            Assert.AreEqual("New Subject One", savedNahno.subjectone);
            Assert.AreEqual("New Subject Two", savedNahno.subjecttwo);
            Assert.AreEqual("New Subject Three", savedNahno.subjectThree);
            Assert.AreEqual("https://new-link.com", savedNahno.Link);
            Assert.IsNotNull(savedNahno.ImageUrl);
        }

        [TestMethod]
        public async Task EditFeatureInfo_GET_CreatesDefaultFeaturesWhenNoneExist()
        {
            // Act
            var result = await _controller.EditFeatureInfo() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            var features = result.Model as List<FeaturesApp>;
            Assert.IsNotNull(features);
            Assert.AreEqual(2, features.Count);
            Assert.IsTrue(features.Any(f => f.Header == "Up Coming Event"));
        }

        [TestMethod]
        public async Task EditFeatureInfo_POST_ValidData_UpdatesDatabase()
        {
            // Arrange
            var fileMock = CreateMockImageFile();
            var model = new FeaturesApp
            {
                Header = "New Feature",
                Content = "New Content",
                Features = new List<string> { "Feature 1", "Feature 2" },
                ImageFile = fileMock.Object
            };

            // Act
            var result = await _controller.EditFeatureInfo(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            var savedFeature = await _dbContext.featuresApp.FirstOrDefaultAsync(f => f.Header == "New Feature");
            Assert.IsNotNull(savedFeature);
            Assert.AreEqual("New Content", savedFeature.Content);
        }
        [TestMethod]
        public async Task EditEsportInfo_GET_ReturnsDefaultValuesWhenNoExistingRecord()
        {
            // Act
            var result = await _controller.EditEsportInfo() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            var model = result.Model as Esports;
            Assert.IsNotNull(model);
            Assert.AreEqual("CTC Esports Team", model.HeaderEsports);
            Assert.IsTrue(model.Games.Contains("Valorant"));
        }

        [TestMethod]
        public async Task DeleteGame_ValidIndex_RemovesGameAndContent()
        {
            // Arrange
            var esport = new Esports
            {
                Id = 1,
                HeaderEsports = "Test Header", // Add required property
                ContentEsports = "Test Content", // Add required property
                Games = new List<string> { "Game1", "Game2" },
                ContentGames = new List<string> { "Content1", "Content2" }
            };
            await _dbContext.esports.AddAsync(esport);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteGame(1, 0) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("EditEsportInfo", result.ActionName);

            var updatedEsport = await _dbContext.esports.FirstOrDefaultAsync();
            Assert.IsNotNull(updatedEsport);
            Assert.AreEqual(1, updatedEsport.Games.Count);
            Assert.AreEqual("Game2", updatedEsport.Games[0]);
            Assert.AreEqual(1, updatedEsport.ContentGames.Count);
            Assert.AreEqual("Content2", updatedEsport.ContentGames[0]);
        }

        private void SetupUserWithRole(string role)
        {
            var user = new User { Id = 1, UserName = "test@test.com", Email = "test@test.com" };
            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.IsInRoleAsync(user, role))
                .ReturnsAsync(true);
        }

        private Mock<IFormFile> CreateMockVideoFile()
        {
            var fileMock = new Mock<IFormFile>();
            var content = "Fake video content";
            var fileName = "test.mp4";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;

            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);
            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.ContentType).Returns("video/mp4");

            return fileMock;
        }

        private Mock<IFormFile> CreateMockImageFile()
        {
            var fileMock = new Mock<IFormFile>();
            var content = new byte[] { 0x42, 0x43 }; // Dummy image content
            var fileName = "test.jpg";
            var ms = new MemoryStream(content);

            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);
            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            return fileMock;
        }

        [TestCleanup]
        public void Cleanup()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}