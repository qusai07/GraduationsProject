using CTC.Controllers;
using CTC.Models.Volunteer;
using CTC.Repository.IRepository;
using CTCTest.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

[TestClass()]
public class VolunteerManagerControllerTests : TestBase
{
    private VolunteerManagerController _controller;
    private Mock<IVolunteerRepository> _mockVolunteerRepository;
    private Mock<IWebHostEnvironment> _mockEnvironment;
    private Mock<IMailService> _mockMailService;

    [TestInitialize]
    public  void Setup()
    {
        base.BaseSetup();

        // Setup mocks
        _mockVolunteerRepository = new Mock<IVolunteerRepository>();
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockMailService = new Mock<IMailService>();
        _mockEnvironment.Setup(x => x.WebRootPath).Returns("wwwroot");

        _controller = new VolunteerManagerController(
            _mockMailService.Object,
     _mockEnvironment.Object,         
     _dbContext,                     
     _mockUserManager.Object,         
     _mockVolunteerRepository.Object 
 );

        // Setup controller context
        SetupControllerContext(_controller);
    }

    [TestMethod]
    public async Task HomeAdmin_ReturnsViewWithCorrectCounts()
    {
        try
        {
            // Arrange
            var volunteering = new List<Volunteering>
            {
                new Volunteering
                {
                    Id = 1,
                    Organization = "Test Org 1",
                    Description = "Description 1",
                    Location = "Location 1",
                    Type = "Type 1",
                    Date = DateTime.Now,
                    MaxParticipants = 10,
                    ImageUrl = "test1.jpg"
                },
                new Volunteering
                {
                    Id = 2,
                    Organization = "Test Org 2",
                    Description = "Description 2",
                    Location = "Location 2",
                    Type = "Type 2",
                    Date = DateTime.Now,
                    MaxParticipants = 10,
                    ImageUrl = "test2.jpg"
                }
            };

            var participants = new List<VolunteerParticipants>
            {
                new VolunteerParticipants
                {
                    Id = 1,
                    ParticipateName = "Participant 1",
                    Status = "Pending",
                    VolunteerId = 1
                },
                new VolunteerParticipants
                {
                    Id = 2,
                    ParticipateName = "Participant 2",
                    Status = "Pending",
                    VolunteerId = 1
                },
                new VolunteerParticipants
                {
                    Id = 3,
                    ParticipateName = "Participant 3",
                    Status = "Pending",
                    VolunteerId = 2
                }
            };

            await _dbContext.volunteering.AddRangeAsync(volunteering);
            await _dbContext.VolunteerParticipants.AddRangeAsync(participants);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _controller.HomeAdmin() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("~/Views/LeaderDepartment/Volunteer/HomeAdmin.cshtml", result.ViewName);
            Assert.AreEqual(2, _controller.ViewBag.VolunteeCount);
            Assert.AreEqual(3, _controller.ViewBag.VolunterParticipationCount);
        }
        catch (Exception ex)
        {
            Assert.Fail($"Test failed with exception: {ex.Message}\nStack trace: {ex.StackTrace}");
        }
    }

    [TestMethod]
    public void TableVolunteerWork_ReturnsViewWithVolunteerList()
    {
        try
        {
            // Arrange
            var volunteers = new List<Volunteering>
            {
                new Volunteering
                {
                    Id = 1,
                    Organization = "Org1",
                    Description = "Description 1",
                    Location = "Location 1",
                    Type = "Type 1",
                    Date = DateTime.Now,
                    MaxParticipants = 10,
                    ImageUrl = "test1.jpg"
                },
                new Volunteering
                {
                    Id = 2,
                    Organization = "Org2",
                    Description = "Description 2",
                    Location = "Location 2",
                    Type = "Type 2",
                    Date = DateTime.Now,
                    MaxParticipants = 10,
                    ImageUrl = "test2.jpg"
                }
            };
            _dbContext.volunteering.AddRange(volunteers);
            _dbContext.SaveChanges();

            // Act
            var result = _controller.TableVolunteerWork(0) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("~/Views/LeaderDepartment/Volunteer/TableVolunteerWork.cshtml", result.ViewName);
            var model = result.Model as List<Volunteering>;
            Assert.IsNotNull(model);
            Assert.AreEqual(2, model.Count);
        }
        catch (Exception ex)
        {
            Assert.Fail($"Test failed with exception: {ex.Message}\nStack trace: {ex.StackTrace}");
        }
    }
    [TestMethod]
    public async Task AddVolunteerwork_POST_ValidModel_CreatesNewVolunteering()
    {
        // Arrange
        var model = new Volunteering
        {
            Organization = "Test Org",
            Date = DateTime.Now,
            Description = "Test Description",
            Location = "Test Location",
            Type = "Test Type",
            MaxParticipants = 10,
            ImageFile = CreateMockImageFile().Object
        };

        // Act
        var result = await _controller.AddVolunteerwork(model) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("~/Views/LeaderDepartment/Volunteer/AddVolunteerwork.cshtml", result.ViewName);

        var savedVolunteering = await _dbContext.volunteering.FirstOrDefaultAsync();
        Assert.IsNotNull(savedVolunteering);
        Assert.AreEqual("Test Org", savedVolunteering.Organization);
        Assert.IsTrue(savedVolunteering.ImageUrl.StartsWith("/Pic/"));
    }

    [TestMethod]
    public async Task TableParticipation_ReturnsViewWithParticipations()
    {
        // Arrange
        var participations = new List<VolunteerParticipants>
        {
            new VolunteerParticipants { Id = 1 },
            new VolunteerParticipants { Id = 2 }
        };
        _mockVolunteerRepository.Setup(x => x.GetAllVolunteerParticipationsAsync())
            .ReturnsAsync(participations);

        // Act
        var result = await _controller.TableParticipation() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("~/Views/LeaderDepartment/Volunteer/TableParticipation.cshtml", result.ViewName);
        var model = result.Model as List<VolunteerParticipants>;
        Assert.IsNotNull(model);
        Assert.AreEqual(2, model.Count);
    }

    [TestMethod]
    public async Task EditVolunteer_GET_ValidId_ReturnsViewWithVolunteer()
    {
        // Arrange
        var volunteer = new Volunteering { Id = 1, Organization = "Test Org" };
        _mockVolunteerRepository.Setup(x => x.GetVolunteerByIdAsync(1))
            .ReturnsAsync(volunteer);

        // Act
        var result = await _controller.EditVolunteer(1) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("~/Views/LeaderDepartment/Volunteer/EditVolunteer.cshtml", result.ViewName);
        var model = result.Model as Volunteering;
        Assert.IsNotNull(model);
        Assert.AreEqual("Test Org", model.Organization);
    }

    [TestMethod]
    public async Task EditVolunteer_POST_ValidModel_UpdatesVolunteer()
    {
        // Arrange
        var updateModel = new Volunteering
        {
            Id = 1,
            Organization = "Updated Org",
            ImageFile = CreateMockImageFile().Object
        };

        _mockVolunteerRepository.Setup(x => x.UpdateVolunteer(It.IsAny<Volunteering>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.EditVolunteer(updateModel) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("EditVolunteer", result.ActionName);
        _mockVolunteerRepository.Verify(x => x.UpdateVolunteer(It.IsAny<Volunteering>()), Times.Once);
    }

    private Mock<IFormFile> CreateMockImageFile()
    {
        var fileMock = new Mock<IFormFile>();
        var content = new byte[] { 0x42, 0x43 };
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
        if (_dbContext != null)
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}