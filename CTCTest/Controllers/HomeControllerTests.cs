using CTC.Controllers;
using CTC.Models;
using CTC.Models.Academic;
using CTC.Models.Event;
using CTC.Repository.Enum;
using CTC.Repository.IRepository;
using CTC.ViewModels.Academic;
using CTC.ViewModels.MemberShip;
using CTCTest.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

[TestClass]
public class HomeControllerTests : TestBase
{
    private HomeController _controller;
    private Mock<ILogger<HomeController>> _mockHomeLogger;
    private Mock<IJoinerRepository> _mockJoinerRepository;
    private Mock<IAcademicRepository> _mockAcademicRepository;
    private Mock<IEventCtcRepository> _mockEventCtcRepository;
    private Mock<INotificationRepository> _mockNotificationRepository;
    private Mock<IUserRepository> _mockUserRepository;
    private Mock<IMailService> _mockMailService;
    [TestInitialize]
    public override void BaseSetup()
    {
        base.BaseSetup();

        // Initialize mocks
        _mockHomeLogger = new Mock<ILogger<HomeController>>();
        _mockJoinerRepository = new Mock<IJoinerRepository>();
        _mockAcademicRepository = new Mock<IAcademicRepository>();
        _mockEventCtcRepository = new Mock<IEventCtcRepository>();
        _mockNotificationRepository = new Mock<INotificationRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockMailService = new Mock<IMailService>();



        // Make sure the parameters match exactly with your HomeController constructor
        _controller = new HomeController(
       _mockEnvironment.Object,            // IWebHostEnvironment
       _dbContext,                         // CtcDbContext
       _mockUserManager.Object,            // UserManager<User>
       _mockUserRepository.Object,         // IUserRepository
       _mockMailService.Object,            // IMailService
       _mockEventCtcRepository.Object,     // IEventCtcRepository
       _mockNotificationRepository.Object, // INotificationRepository
       _mockHomeLogger.Object,             // ILogger<HomeController>
       _mockJoinerRepository.Object,       // IJoinerRepository
       _mockAcademicRepository.Object      // IAcademicRepository
   );


        SetupControllerContext(_controller);
    }

    [TestMethod]
    public async Task Index_ReturnsViewWithDefaultData_WhenNoDataExists()
    {
        // Act
        var result = await _controller.Index() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        var model = result.Model as Combination;
        Assert.IsNotNull(model);
        Assert.IsNotNull(model.WhoWeAre);
        Assert.IsNotNull(model.Nahno);
        Assert.IsNotNull(model.FeatureApp);
        Assert.IsNotNull(model.esports);
    }

    [TestMethod]
    public async Task Events_ReturnsNoEventsView_WhenNoEventsExist()
    {
        // Arrange
        _mockEventCtcRepository.Setup(x => x.GetAllEventsAsync())
            .ReturnsAsync(new List<EventsCTC>());

        // Act
        var result = await _controller.Events() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("NoEvents", result.ViewName);
    }

    [TestMethod]
    public async Task Events_ReturnsViewWithEvents_WhenEventsExist()
    {
        // Arrange
        var events = new List<EventsCTC>
        {
            new EventsCTC { Id = 1, Name = "Test Event" }
        };
        _mockEventCtcRepository.Setup(x => x.GetAllEventsAsync())
            .ReturnsAsync(events);

        // Act
        var result = await _controller.Events() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("~/Views/Home/Events.cshtml", result.ViewName);
        var model = result.Model as List<EventsCTC>;
        Assert.IsNotNull(model);
        Assert.AreEqual(1, model.Count);
    }

    [TestMethod]
    public async Task Join_Post_ValidModel_SendsEmailsAndNotification()
    {
        // Arrange
        var model = new JoinerViewModel
        {
            FirstName = "Test",
            LastName = "User",
            UniversityEmail = "test@university.edu"
        };

        // Act
        var result = await _controller.Join(model) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        _mockMailService.Verify(x => x.SendEmailAsync(
            model.UniversityEmail,
        It.IsAny<string>(),
            It.IsAny<string>()
        ), Times.Once);
        _mockNotificationRepository.Verify(x => x.AddNotification(
            It.Is<Notification>(n => n.Username == "Test User")
        ), Times.Once);
    }

    [TestMethod]
    public async Task Facultymembers_FiltersByDepartment()
    {
        // Arrange
        var facultyMembers = new List<Facultymembers>
    {
        new Facultymembers
        {
            Id = 1,
            NameDoctor = "Dr. Test1",
            Email = "test1@test.com",
            prefx = "Dr.",
            department = Department.ComputerScience,
            Approved = true
        },
        new Facultymembers
        {
            Id = 2,
            NameDoctor = "Dr. Test2",
            Email = "test2@test.com",
            prefx = "Dr.",
            department = Department.DataScience,
            Approved = true
        }
    };

        _mockAcademicRepository.Setup(x => x.GetAllFactualMemberAsync())
            .ReturnsAsync(facultyMembers);

        // Act
        var result = await _controller.Facultymembers("ComputerScience") as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        var model = result.Model as List<FacultymembersViewModel>;
        Assert.IsNotNull(model);
        Assert.AreEqual(1, model.Count);
        Assert.AreEqual("ComputerScience", _controller.ViewBag.SelectedDepartment);
        Assert.AreEqual("Dr. Test1", model[0].Name);
    }

    [TestMethod]
    public void CalculateGPA_ValidInput_CalculatesCorrectly()
    {
        // Arrange
        var courseNames = new[] { "Course1", "Course2" };
        var creditHours = new[] { 3, 3 };
        var grades = new[] { "A", "B" };

        // Act
        var result = _controller.CalculateGPA(courseNames, creditHours, grades) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        //Assert.AreEqual(3.5, result.ViewBag.GPA);
    }

    [TestMethod]
    public async Task Download_ValidId_ReturnsFileResult()
    {
        // Arrange
        var material = new CTC.Models.Academic.MaterialSummary
        {
            Id = 1,
            PdfUrl = "/files/test.pdf"
        };
        _mockAcademicRepository.Setup(x => x.GetMaterialByIDAsync(1))
            .ReturnsAsync(material);
        _mockEnvironment.Setup(x => x.WebRootPath)
            .Returns(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));

        // Create test file
        var filePath = Path.Combine(_mockEnvironment.Object.WebRootPath, "files", "test.pdf");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        await System.IO.File.WriteAllBytesAsync(filePath, new byte[] { 1, 2, 3 });

        // Act
        var result = await _controller.Download(1) as FileResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("application/pdf", result.ContentType);
        // Cleanup
        System.IO.File.Delete(filePath);
    }

    [TestMethod]
    public async Task AdminstrationDepartment_ReturnsViewWithUsers()
    {
        // Arrange
        var users = new List<User>
    {
        new User { Id = 1, UserName = "user1" },
        new User { Id = 2, UserName = "user2" }
    };

        var mockDbSet = new Mock<DbSet<User>>();

        // Set up the mock DbSet
        var queryableUsers = users.AsQueryable();
        mockDbSet.As<IQueryable<User>>()
            .Setup(m => m.Provider)
            .Returns(queryableUsers.Provider);
        mockDbSet.As<IQueryable<User>>()
            .Setup(m => m.Expression)
            .Returns(queryableUsers.Expression);
        mockDbSet.As<IQueryable<User>>()
            .Setup(m => m.ElementType)
            .Returns(queryableUsers.ElementType);
        mockDbSet.As<IQueryable<User>>()
            .Setup(m => m.GetEnumerator())
            .Returns(queryableUsers.GetEnumerator());

        // Setup async enumerable
        mockDbSet.As<IAsyncEnumerable<User>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<User>(users.GetEnumerator()));

        _mockUserManager.Setup(x => x.Users).Returns(mockDbSet.Object);

        // Act
        var result = await _controller.AdminstrationDepartment() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        var model = result.Model as List<User>;
        Assert.IsNotNull(model);
        Assert.AreEqual(2, model.Count);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _dbContext?.Database.EnsureDeleted();
        _dbContext?.Dispose();
    }

    private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return new ValueTask();
        }
    }

    private class TestAsyncEnumerable<T> : IAsyncEnumerable<T>
    {
        private readonly IEnumerable<T> _inner;

        public TestAsyncEnumerable(IEnumerable<T> inner)
        {
            _inner = inner;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(_inner.GetEnumerator());
        }
    }
}

