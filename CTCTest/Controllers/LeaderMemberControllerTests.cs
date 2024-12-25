using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using CTC.Controllers;
using CTC.Data;
using CTC.Models.Leader;
using CTC.Repository.IRepository;
using Microsoft.EntityFrameworkCore.Diagnostics;

[TestClass]
public class LeaderMemberControllerTests
{
    private LeaderMemberController _controller;
    private Mock<IMailService> _mockMailService;
    private Mock<IUserRepository> _mockUserRepository;
    private CtcDbContext _dbContext;
    public class TestCtcDbContext : CtcDbContext
    {
        public TestCtcDbContext(DbContextOptions<CtcDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Make all properties of Joiner optional for testing
            modelBuilder.Entity<Joiner>(entity =>
            {
                entity.Property(e => e.UniversityID).IsRequired(false);
                entity.Property(e => e.Phone).IsRequired(false);
                entity.Property(e => e.Address).IsRequired(false);
                entity.Property(e => e.Facebook).IsRequired(false);
                entity.Property(e => e.LinkedIn).IsRequired(false);
                entity.Property(e => e.YourMessage).IsRequired(false);
            });
        }
    }
    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<CtcDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _dbContext = new TestCtcDbContext(options);
        _mockMailService = new Mock<IMailService>();
        _mockUserRepository = new Mock<IUserRepository>();

        _controller = new LeaderMemberController(
            _mockMailService.Object,
            _dbContext,
            _mockUserRepository.Object
        );
    }
    [TestMethod]
    public async Task TableAppointment_ReturnsViewWithCorrectAppointments()
    {
        // Arrange
        var testAppointments = new List<Appointment>
    {
        new Appointment
        {
            AppointmentId = 1,
            Status = "Waiting",
            CreatedAt = DateTime.Now.AddDays(-1),
            Name = "Test Appointment 1",
            AppointmentDate = DateTime.Now,
            LinkMeeting = "https://test1.com",
            Joiner = new Joiner
            {
                FirstName = "Test1",
                LastName = "User1",
                UniversityEmail = "test1@university.edu",
                UniversityID = "ID001",
                Phone = "1234567890",
                Address = "Test Address 1",
                Facebook = "fb.com/test1",
                LinkedIn = "linkedin.com/test1",
                YourMessage = "Test message 1"
            }
        },
        // Add similar data for other appointments...
    };

        await _dbContext.Appointment.AddRangeAsync(testAppointments);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _controller.TableAppointment() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("~/Views/LeaderDepartment/LeaderMember/TableAppointment.cshtml", result.ViewName);

        var model = result.Model as Appointment;
        Assert.IsNotNull(model);
        Assert.AreEqual(1, model.Waiting.Count);
    }
    [TestMethod]
    public async Task BookAppointment_ValidAppointment_RedirectsToTableAppointment()
    {
        // Arrange
        var joiner = new Joiner
        {
            FirstName = "Test User",
            LastName = "Test Last",
            UniversityEmail = "test@university.edu",
            UniversityID = "ID001",
            Phone = "1234567890",
            Address = "Test Address",
            Facebook = "facebook.com/test",
            LinkedIn = "linkedin.com/test",
            YourMessage = "Test message",
            Status = "Pending"
        };

        var appointment = new Appointment
        {
            AppointmentId = 1,
            Status = "Waiting",
            Name = "Test Appointment",
            AppointmentDate = DateTime.Now,  // Initial date
            LinkMeeting = "initial-link",    // Initial link
            CreatedAt = DateTime.Now,
            Joiner = joiner
        };

        // Add Joiner first
        await _dbContext.Joiners.AddAsync(joiner);
        await _dbContext.SaveChangesAsync();

        // Then add Appointment
        await _dbContext.Appointment.AddAsync(appointment);
        await _dbContext.SaveChangesAsync();

        var newAppointmentDate = DateTime.Now.AddDays(1);
        var newLinkMeeting = "https://meet.test.com";

        // Act
        var result = await _controller.BookAppointment(1, newAppointmentDate, newLinkMeeting) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("TableAppointment", result.ActionName);

        // Verify email was sent
        _mockMailService.Verify(x => x.SendEmailAsync(
            joiner.UniversityEmail,
            "CTC Team",
            It.IsAny<string>()
        ), Times.Once);

        // Verify appointment was updated
        var updatedAppointment = await _dbContext.Appointment
            .Include(a => a.Joiner)
            .FirstOrDefaultAsync(a => a.AppointmentId == 1);

        Assert.IsNotNull(updatedAppointment);
        Assert.AreEqual("Pending", updatedAppointment.Status);
        Assert.AreEqual(newAppointmentDate, updatedAppointment.AppointmentDate);
        Assert.AreEqual(newLinkMeeting, updatedAppointment.LinkMeeting);
    }

    [TestMethod]
    public async Task BookAppointment_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = 999;

        // Act
        var result = await _controller.BookAppointment(invalidId, DateTime.Now, "test-link");

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
    }
    [TestMethod]
    public async Task MarkAsBooked_AcceptAction_UpdatesStatusAndRedirects()
    {
        // Arrange
        var joiner = new Joiner
        {
            Status = "Pending",
            FirstName = "Test User",
            LastName = "Test Last",
            UniversityEmail = "test@university.edu",
            UniversityID = "ID001",
            Phone = "1234567890",
            Address = "Test Address",
            Facebook = "facebook.com/test",
            LinkedIn = "linkedin.com/test",
            YourMessage = "Test message"
        };

        var appointment = new Appointment
        {
            AppointmentId = 1,
            Status = "Pending",
            Name = "Test Appointment",
            AppointmentDate = DateTime.Now,
            LinkMeeting = "https://meet.test.com",
            CreatedAt = DateTime.Now,
            Joiner = joiner
        };

        // Add Joiner first
        await _dbContext.Joiners.AddAsync(joiner);
        await _dbContext.SaveChangesAsync();

        // Then add Appointment
        await _dbContext.Appointment.AddAsync(appointment);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _controller.MarkAsBooked(1, "Accept") as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("TableAppointment", result.ActionName);

        var updatedAppointment = await _dbContext.Appointment
            .Include(a => a.Joiner)
            .FirstOrDefaultAsync(a => a.AppointmentId == 1);

        Assert.IsNotNull(updatedAppointment, "Updated appointment should not be null");
        Assert.AreEqual("Accepted", updatedAppointment.Status);
        Assert.IsNotNull(updatedAppointment.Joiner, "Updated appointment's joiner should not be null");
        Assert.AreEqual("Accepted", updatedAppointment.Joiner.Status);
    }

    [TestMethod]
    public async Task MarkAsBooked_RejectAction_UpdatesStatusAndRedirects()
    {
        // Arrange
        var joiner = new Joiner
        {
            FirstName = "Test User",
            LastName = "Test Last",
            UniversityEmail = "test@university.edu",
            Status = "Pending",
            // Optional properties
            UniversityID = "ID001",
            Phone = "1234567890",
            Address = "Test Address",
            Facebook = "facebook.com/test",
            LinkedIn = "linkedin.com/test",
            YourMessage = "Test message"
        };

        // Add and save joiner first
        _dbContext.Joiners.Add(joiner);
        await _dbContext.SaveChangesAsync();

        var appointment = new Appointment
        {
            AppointmentId = 1,
            Status = "Pending",
            Name = "Test Appointment",
            AppointmentDate = DateTime.Now,
            LinkMeeting = "https://meet.test.com",
            CreatedAt = DateTime.Now,
            Joiner = joiner
        };

        // Add and save appointment
        _dbContext.Appointment.Add(appointment);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _controller.MarkAsBooked(1, "Reject") as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("TableAppointment", result.ActionName);

        // Refresh context to ensure we're getting fresh data
        var updatedAppointment = await _dbContext.Appointment
            .Include(a => a.Joiner)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AppointmentId == 1);

        Assert.IsNotNull(updatedAppointment, "Updated appointment should not be null");
        Assert.AreEqual("Rejected", updatedAppointment.Status);
    }
    [TestMethod]
    public async Task MarkAsBooked_InvalidId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.MarkAsBooked(999, "Accept");

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
    }

    [TestMethod]
    public async Task MarkAsBooked_NonPendingStatus_ReturnsBadRequest()
    {
        // Arrange
        var joiner = new Joiner
        {
            Status = "Accepted",
            FirstName = "Test User",
            LastName = "Test Last",
            UniversityEmail = "test@university.edu",
            UniversityID = "ID001",
            Phone = "1234567890",
            Address = "Test Address",
            Facebook = "facebook.com/test",
            LinkedIn = "linkedin.com/test",
            YourMessage = "Test message"
        };

        var appointment = new Appointment
        {
            AppointmentId = 1,
            Status = "Accepted",
            Name = "Test Appointment",
            AppointmentDate = DateTime.Now,
            LinkMeeting = "https://meet.test.com",
            CreatedAt = DateTime.Now,
            Joiner = joiner
        };

        await _dbContext.Joiners.AddAsync(joiner);
        await _dbContext.SaveChangesAsync();
        await _dbContext.Appointment.AddAsync(appointment);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _controller.MarkAsBooked(1, "Accept") as BadRequestObjectResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("The appointment or joiner is not in a pending status.", result.Value);
    }

    [TestMethod]
    public async Task HomeLeader_ReturnsViewWithCorrectCounts()
    {
        // Arrange
        var joiner = new Joiner
        {
            FirstName = "Test User",
            LastName = "Test Last",
            UniversityEmail = "test@university.edu",
            Status = "Pending",
            UniversityID = "ID001",
            Phone = "1234567890",
            Address = "Test Address",
            Facebook = "facebook.com/test",
            LinkedIn = "linkedin.com/test",
            YourMessage = "Test message"
        };

        await _dbContext.Joiners.AddAsync(joiner);
        await _dbContext.SaveChangesAsync();

        var appointments = new List<Appointment>
    {
        new Appointment
        {
            Status = "Waiting",
            Name = "Appointment 1",
            AppointmentDate = DateTime.Now,
            LinkMeeting = "https://meet1.test.com",
            CreatedAt = DateTime.Now,
            Joiner = joiner
        },
        new Appointment
        {
            Status = "Pending",
            Name = "Appointment 2",
            AppointmentDate = DateTime.Now.AddDays(1),
            LinkMeeting = "https://meet2.test.com",
            CreatedAt = DateTime.Now.AddHours(-1),
            Joiner = joiner
        },
        new Appointment
        {
            Status = "Scheduled",
            Name = "Appointment 3",
            AppointmentDate = DateTime.Now.AddDays(2),
            LinkMeeting = "https://meet3.test.com",
            CreatedAt = DateTime.Now.AddHours(-2),
            Joiner = joiner
        },
        new Appointment
        {
            Status = "Waiting",
            Name = "Appointment 4",
            AppointmentDate = DateTime.Now.AddDays(3),
            LinkMeeting = "https://meet4.test.com",
            CreatedAt = DateTime.Now.AddHours(-3),
            Joiner = joiner
        }
    };

        await _dbContext.Appointment.AddRangeAsync(appointments);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _controller.HomeLeader() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("~/Views/LeaderDepartment/LeaderMember/HomeLeader.cshtml", result.ViewName);

        // Access counts through ViewData
        Assert.AreEqual(4, result.ViewData["TotalAppointments"]);
        Assert.AreEqual(2, result.ViewData["WaitingAppointments"]);
        Assert.AreEqual(1, result.ViewData["PendingAppointments"]);
        Assert.AreEqual(1, result.ViewData["ScheduledAppointments"]);

        // Check RecentAppointments
        var recentAppointments = result.ViewData["RecentAppointments"] as List<Appointment>;
        Assert.IsNotNull(recentAppointments);
        Assert.IsTrue(recentAppointments.Count <= 5); // Should return at most 5 recent appointments

        // Verify appointments are ordered by CreatedAt
        var isOrdered = recentAppointments
            .Select((appointment, index) =>
                index == 0 ||
                recentAppointments[index - 1].CreatedAt >= appointment.CreatedAt)
            .All(x => x);
        Assert.IsTrue(isOrdered, "Recent appointments should be ordered by CreatedAt descending");
    }
    [TestMethod]
    public async Task TableAllAppointment_ReturnsViewWithAcceptedAppointments()
    {
        // Arrange
        var joiners = new List<Joiner>
    {
        new Joiner
        {
            FirstName = "Test User 1",
            LastName = "Test Last 1",
            UniversityEmail = "test1@university.edu",
            Status = "Accepted",
            UniversityID = "ID001",
            Phone = "1234567890",
            Address = "Test Address 1",
            Facebook = "facebook.com/test1",
            LinkedIn = "linkedin.com/test1",
            YourMessage = "Test message 1"
        },
        new Joiner
        {
            FirstName = "Test User 2",
            LastName = "Test Last 2",
            UniversityEmail = "test2@university.edu",
            Status = "Accepted",
            UniversityID = "ID002",
            Phone = "0987654321",
            Address = "Test Address 2",
            Facebook = "facebook.com/test2",
            LinkedIn = "linkedin.com/test2",
            YourMessage = "Test message 2"
        },
        new Joiner
        {
            FirstName = "Test User 3",
            LastName = "Test Last 3",
            UniversityEmail = "test3@university.edu",
            Status = "Accepted",
            UniversityID = "ID003",
            Phone = "1122334455",
            Address = "Test Address 3",
            Facebook = "facebook.com/test3",
            LinkedIn = "linkedin.com/test3",
            YourMessage = "Test message 3"
        }
    };

        // Add joiners first
        await _dbContext.Joiners.AddRangeAsync(joiners);
        await _dbContext.SaveChangesAsync();

        var appointments = new List<Appointment>
    {
        new Appointment
        {
            Status = "Accepted",
            Name = "Appointment 1",
            AppointmentDate = DateTime.Now.AddDays(1),
            LinkMeeting = "https://meet1.test.com",
            CreatedAt = DateTime.Now,
            Joiner = joiners[0]
        },
        new Appointment
        {
            Status = "Pending",
            Name = "Appointment 2",
            AppointmentDate = DateTime.Now.AddDays(2),
            LinkMeeting = "https://meet2.test.com",
            CreatedAt = DateTime.Now.AddHours(-1),
            Joiner = joiners[1]
        },
        new Appointment
        {
            Status = "Accepted",
            Name = "Appointment 3",
            AppointmentDate = DateTime.Now.AddDays(3),
            LinkMeeting = "https://meet3.test.com",
            CreatedAt = DateTime.Now.AddHours(-2),
            Joiner = joiners[2]
        }
    };

        await _dbContext.Appointment.AddRangeAsync(appointments);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _controller.TableAllAppointment() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("~/Views/LeaderDepartment/LeaderMember/TableAllAppointment.cshtml", result.ViewName);

        var model = result.Model as List<Appointment>;
        Assert.IsNotNull(model);
        Assert.AreEqual(2, model.Count, "Should only return Accepted appointments");
        Assert.IsTrue(model.All(a => a.Status == "Accepted"), "All appointments should have Accepted status");

        // Additional assertions to verify appointment details
        foreach (var appointment in model)
        {
            Assert.IsNotNull(appointment.Joiner, "Each appointment should have a joiner");
            Assert.IsNotNull(appointment.Name, "Each appointment should have a name");
            Assert.IsNotNull(appointment.LinkMeeting, "Each appointment should have a meeting link");
            Assert.IsTrue(appointment.AppointmentDate > DateTime.Now, "Appointment date should be in the future");
        }
    }
    [TestCleanup]
    public void Cleanup()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}