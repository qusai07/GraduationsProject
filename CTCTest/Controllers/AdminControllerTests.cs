using Microsoft.VisualStudio.TestTools.UnitTesting;
using CTC.Controllers;
using CTC.Models.Admin;
using CTC.Models.Event;
using CTC.Models.Leader;
using CTC.Models;
using CTC.Repository.Enum;
using CTC.Repository.IRepository;
using CTC.ViewModels.MemberShip;
using CTCTest.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CTC.Controllers.Tests
{
    [TestClass()]
    public class AdminControllerTests : TestBase
    {
        private AdminController _controller;
        private Mock<IJoinerRepository> _mockJoinerRepository;
        private Mock<IEventCtcRepository> _mockEventRepository;
        private Mock<INotificationRepository> _mockNotificationRepository;
        private Mock<IMailService> _mockMailService;
        private Mock<IUserRepository> _mockUserRepository;

        [TestInitialize]
        public override void BaseSetup()
        {
            base.BaseSetup();

            // Initialize mocks
            _mockUserRepository = new Mock<IUserRepository>();
            _mockMailService = new Mock<IMailService>();
            _mockNotificationRepository = new Mock<INotificationRepository>();
            _mockEventRepository = new Mock<IEventCtcRepository>();
            _mockJoinerRepository = new Mock<IJoinerRepository>();

            _controller = new AdminController(
                _mockEnvironment.Object,              // IWebHostEnvironment
                _dbContext,                           // CtcDbContext
                _mockUserManager.Object,              // UserManager<User>
                _mockUserRepository.Object,           // IUserRepository
                _mockMailService.Object,              // IMailService
                _mockNotificationRepository.Object,   // INotificationRepository
                _mockEventRepository.Object,          // IEventCtcRepository
                _mockSignInManager.Object,            // SignInManager<User>
                Mock.Of<IServiceProvider>(),          // IServiceProvider
                _mockRoleManager.Object,              // RoleManager<IdentityRole<int>>
                _mockJoinerRepository.Object          // IJoinerRepository
            );

            // Set up controller context
            SetupControllerContext(_controller);
        }


        [TestMethod]
        public async Task Dash_ReturnsViewWithCorrectCounts()
        {
            // Arrange
            var userCount = 10;
            var joinerCount = 5;
            var eventsCount = 3;

            // Clear any existing users
            _dbContext.Users.RemoveRange(_dbContext.Users);
            await _dbContext.SaveChangesAsync();

            // Create users with unique IDs
            var users = new List<User>();
            for (int i = 1; i <= userCount; i++)
            {
                users.Add(new User
                {
                    Id = i,
                    UserName = $"TestUser{i}",
                    Email = $"test{i}@example.com",
                    NormalizedUserName = $"TESTUSER{i}",
                    NormalizedEmail = $"TEST{i}@EXAMPLE.COM",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString()
                });
            }

            // Use AddRange instead of AddRangeAsync for better control
            _dbContext.Users.AddRange(users);
            await _dbContext.SaveChangesAsync();

            // Setup repository mocks
            _mockJoinerRepository.Setup(x => x.GetUserCountAsync())
                .ReturnsAsync(joinerCount);
            _mockEventRepository.Setup(x => x.GetEventCountAsync())
                .ReturnsAsync(eventsCount);

            // Setup UserManager mock to return the correct count
            _mockUserManager.Setup(x => x.Users)
                .Returns(_dbContext.Users.AsQueryable());

            // Act
            var result = await _controller.Dash() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(userCount, _controller.ViewBag.UserCount);
            Assert.AreEqual(joinerCount, _controller.ViewBag.JoinerCount);
            Assert.AreEqual(eventsCount, _controller.ViewBag.EventsCount);
        }

        [TestMethod]
        public async Task Table_ReturnsViewWithCorrectJoinerViewModel()
        {
            // Arrange
            var joiners = new List<Joiner>
        {
            CreateTestJoiner(1, "Pending"),
            CreateTestJoiner(2, "Accepted"),
            CreateTestJoiner(3, "Rejected")
        };

            _mockJoinerRepository.Setup(x => x.GetAllRequestsJoinerAsync())
                .ReturnsAsync(joiners);

            // Act
            var result = await _controller.Table() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            var model = result.Model as JoinerViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.PendingUsers.Count);
            Assert.AreEqual(1, model.AcceptedUsers.Count);
            Assert.AreEqual(1, model.RejectedUsers.Count);
        }

        [TestMethod]
        public async Task AcceptRequest_ValidId_CreatesUserAndSendsEmail()
        {
            // Arrange
            var joiner = CreateTestJoiner(1, "Pending");
            await _dbContext.Joiners.AddAsync(joiner);
            await _dbContext.SaveChangesAsync();

            var expectedUsername = $"{joiner.FirstName}{joiner.LastName}";
            var createdUser = new User
            {
                Id = 1,
                UserName = expectedUsername,
                Email = joiner.UniversityEmail
            };

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(x => x.FindByNameAsync(expectedUsername))
                .ReturnsAsync(createdUser);
            _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "AcademicMemberShip"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.AcceptRequest(1) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("DataTable", result.ActionName);

            // Verify email was sent
            _mockMailService.Verify(x => x.SendEmailAsync(
                joiner.UniversityEmail,
                "Your Account Request Has Been Approved",
                It.IsAny<string>()),
                Times.Once);

            // Verify user was created with correct role
            _mockUserManager.Verify(x => x.CreateAsync(
                It.Is<User>(u => u.UserName == expectedUsername && u.Email == joiner.UniversityEmail),
                It.IsAny<string>()),
                Times.Once);

            _mockUserManager.Verify(x => x.AddToRoleAsync(
                It.Is<User>(u => u.UserName == expectedUsername),
                "AcademicMemberShip"),
                Times.Once);

            // Verify joiner status was updated
            var updatedJoiner = await _dbContext.Joiners.FindAsync(1);
            Assert.IsNotNull(updatedJoiner);
            Assert.AreEqual("Accepted", updatedJoiner.Status);
        }

        [TestMethod]
        public async Task CreateEvent_ValidModel_CreatesEventAndRedirects()
        {
            // Arrange
            var model = new EventsCTC
            {
                Name = "Test Event",
                Description = "Test Description",
                EventDate = DateTime.Now.AddDays(1),
                Location = "Test Location",
                EventType = "Workshop",
                ImageFile = CreateMockImageFile()
            };

            _mockEnvironment.Setup(x => x.WebRootPath)
                .Returns(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));

            // Act
            var result = await _controller.CreateEvent(model) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Events", result.ActionName);
            Assert.AreEqual("Admin", result.ControllerName);
        }
        [TestMethod]
        public async Task EditCTCData_ValidModel_UpdatesData()
        {
            // Arrange
            var model = new CtcData
            {
                Email = "test@ctc.com",
                City = "Test City",
                Address = "Test Address",
                PhoneNumber = "1234567890",
                // Add missing required properties
                Country = "Test Country",
                FaceBook = "https://facebook.com/ctc",
                Instagram = "https://instagram.com/ctc",
                LinedIn = "https://linkedin.com/ctc",
                Nahno = "https://nahno.org/ctc",
                CaptionHome = "Empowering Innovation Through Tech || Join us to enrich your knowledge, explore and have fun!"
            };

            // Act
            var result = await _controller.EditCTCData(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            var savedData = await _dbContext.ctcData.FirstOrDefaultAsync();
            Assert.IsNotNull(savedData);

            // Assert all properties
            Assert.AreEqual(model.Email, savedData.Email);
            Assert.AreEqual(model.City, savedData.City);
            Assert.AreEqual(model.Address, savedData.Address);
            Assert.AreEqual(model.PhoneNumber, savedData.PhoneNumber);
            Assert.AreEqual(model.Country, savedData.Country);
            Assert.AreEqual(model.FaceBook, savedData.FaceBook);
            Assert.AreEqual(model.Instagram, savedData.Instagram);
            Assert.AreEqual(model.LinedIn, savedData.LinedIn);
            Assert.AreEqual(model.Nahno, savedData.Nahno);
            Assert.AreEqual(model.CaptionHome, savedData.CaptionHome);
        }
        // Helper methods
        private Joiner CreateTestJoiner(int id, string status)
        {
            return new Joiner
            {
                Id = id,
                FirstName = $"Test{id}",
                LastName = $"User{id}",
                UniversityEmail = $"test{id}@test.com",
                Phone = "1234567890",
                Status = status,
                UniversityID = $"ID{id}",
                Address = "Test Address",
                Gender = Gender.Male,
                Department = Department.ComputerScience,
                DateOfBirth = DateTime.Now.AddYears(-20),
                // Add the missing required properties
                Facebook = "https://facebook.com/testuser",
                LinkedIn = "https://linkedin.com/testuser",
                YourMessage = "Test message"
            };
        }

        private IFormFile CreateMockImageFile()
        {
            var content = "fake image content";
            var fileName = "test.jpg";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();
            stream.Position = 0;

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(x => x.OpenReadStream()).Returns(stream);
            fileMock.Setup(x => x.FileName).Returns(fileName);
            fileMock.Setup(x => x.Length).Returns(stream.Length);

            return fileMock.Object;
        }

        [TestCleanup]
        public void Cleanup()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

    }
}

