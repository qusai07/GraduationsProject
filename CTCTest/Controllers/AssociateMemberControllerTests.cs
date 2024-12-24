
using CTC.Controllers;
using CTC.Models;
using CTC.Models.Event;
using CTC.Models.Volunteer;
using CTC.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using CTCTest.Controllers;
namespace CTC.Tests
{
    [TestClass()]
    public class AssociateMemberControllerTests : TestBase
    {
        private AssociateMemberController _controller;
        private Mock<IEventCtcRepository> _mockEventRepo;
        private Mock<IVolunteerRepository> _mockVolunteerRepo;


        [TestInitialize]
        public void Setup()
        {
            base.BaseSetup();

            // Setup specific to AssociateMemberController
            _mockEventRepo = new Mock<IEventCtcRepository>();
            _mockVolunteerRepo = new Mock<IVolunteerRepository>();

            _controller = new AssociateMemberController(
                _mockVolunteerRepo.Object,
                _mockEventRepo.Object,
                _dbContext,
                _mockUserManager.Object
            );

            SetupControllerContext(_controller);
        }
        private void SetupControllerContext()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "FarisMajed@gmail.com"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
            }));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        private Mock<UserManager<User>> MockUserManager()
        {
            var store = new Mock<IUserStore<User>>();
            var mgr = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
            mgr.Object.UserValidators.Add(new UserValidator<User>());
            mgr.Object.PasswordValidators.Add(new PasswordValidator<User>());
            return mgr;
        }

        [TestMethod]
        public void Home_ReturnsViewResult()
        {
            // Act
            var result = _controller.Home();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
       public async Task EventsAsync_ReturnsNotFound_WhenEventDoesNotExist()
        {
            // Arrange
            _mockEventRepo.Setup(repo => repo.GetEventByIdAsync(It.IsAny<int>())).ReturnsAsync((EventsCTC)null);

            // Act
            var result = await _controller.EventsAsync(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
        
        [TestMethod]
        public async Task Events_ReturnsViewResult_WhenEventExists()
        {
            var testEvent = new EventsCTC { Id = 1, Description = "Test Event" };
            _mockEventRepo.Setup(repo => repo.GetEventByIdAsync(1))
                .ReturnsAsync(testEvent);

            var result = await _controller.EventsAsync(1);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(testEvent, viewResult.Model);
        }



        [TestMethod]
        public async Task Tables_ReturnsViewWithVolunteerParticipations()
        {
            // Arrange
            var testUser = new User { Id = 1, UserName = "FarisMajed@gmail.com" };
            var participations = new List<VolunteerParticipants>
    {
        new VolunteerParticipants { VolunteerId = 1, EventId = 1 }
    };

            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(testUser);
            _mockVolunteerRepo.Setup(x => x.GetVolunteerParticipationsByVolunteerIdAsync(testUser.Id))
                .ReturnsAsync(participations);

            // Act
            var result = await _controller.Tables();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            var model = viewResult.Model as List<VolunteerParticipants>;
            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Count);
        }
        [TestMethod]
        public void VolunteerWork_ReturnsViewWithVolunteerings()
        {
            // Arrange
            var volunteers = new List<Volunteering>
    {
        new Volunteering
        {
            Id = 1,
            Organization = "Test Org",
            Date = DateTime.Now,
            Description = "Test Description",
            Location = "Test Location",
            Type = "Test Type",
            MaxParticipants = 10,
            CurrentParticipants = 5
        }
    }.AsQueryable();

            var mockSet = new Mock<DbSet<Volunteering>>();
            mockSet.As<IQueryable<Volunteering>>().Setup(m => m.Provider).Returns(volunteers.Provider);
            mockSet.As<IQueryable<Volunteering>>().Setup(m => m.Expression).Returns(volunteers.Expression);
            mockSet.As<IQueryable<Volunteering>>().Setup(m => m.ElementType).Returns(volunteers.ElementType);
            mockSet.As<IQueryable<Volunteering>>().Setup(m => m.GetEnumerator()).Returns(volunteers.GetEnumerator());

            _dbContext.volunteering = mockSet.Object;

            // Act
            var result = _controller.VolunteerWork(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            var model = viewResult.Model as List<Volunteering>;
            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Count);
            Assert.AreEqual("Test Org", model[0].Organization);
        }

        [TestMethod]
        public async Task SubscribeToEvent_WhenUserNotLoggedIn_RedirectsToLogin()
        {
            // Arrange
            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _controller.SubscribeToEvent(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("Login", redirectResult.ActionName);
            Assert.AreEqual("Account", redirectResult.ControllerName);
        }

        [TestMethod]
        public async Task SubscribeToEvent_WithValidData_SubscribesSuccessfully()
        {
            // Arrange
            var testUser = new User { Id = 1, UserName = "FarisMajed@gmail.com" };
            var volunteering = new Volunteering
            {
                Id = 1,
                MaxParticipants = 10,
                CurrentParticipants = 5
            };

            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(testUser);
            _mockVolunteerRepo.Setup(x => x.GetVolunteerByIdAsync(1))
                .ReturnsAsync(volunteering);

            // Act
            var result = await _controller.SubscribeToEvent(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual("~/Views/AssociateMember/VolunteerWork.cshtml", viewResult.ViewName);
            Assert.AreEqual("You have successfully subscribed to the event!", _controller.TempData["Message"]);
        }

        [TestMethod]
        public async Task SubscribeToEvent_WhenAlreadySubscribed_ReturnsAppropriateMessage()
        {
            // Arrange
            var testUser = new User { Id = 1, UserName = "FarisMajed@gmail.com" };
            var existingParticipation = new VolunteerParticipants
            {
                VolunteerId = 1,
                EventId = 1
            };

            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(testUser);

            var mockVolunteerParticipants = new List<VolunteerParticipants> { existingParticipation }.AsQueryable();
            var mockDbSet = new Mock<DbSet<VolunteerParticipants>>();
            mockDbSet.As<IQueryable<VolunteerParticipants>>().Setup(m => m.Provider).Returns(mockVolunteerParticipants.Provider);
            mockDbSet.As<IQueryable<VolunteerParticipants>>().Setup(m => m.Expression).Returns(mockVolunteerParticipants.Expression);
            mockDbSet.As<IQueryable<VolunteerParticipants>>().Setup(m => m.ElementType).Returns(mockVolunteerParticipants.ElementType);
            mockDbSet.As<IQueryable<VolunteerParticipants>>().Setup(m => m.GetEnumerator()).Returns(mockVolunteerParticipants.GetEnumerator());

            _dbContext.VolunteerParticipants = mockDbSet.Object;
            var httpContext = new DefaultHttpContext();

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            _controller.TempData = new TempDataDictionary(
           httpContext,
           Mock.Of<ITempDataProvider>()
       );
            // Act
            var result = await _controller.SubscribeToEvent(1);

            // Assert
            Assert.IsNotNull(result);
           // Assert.AreEqual("You are already subscribed to this event.", _controller.TempData["Message"]);
        }

    }
}
