using CTC.Controllers;
using CTC.Models.Academic;
using CTC.Models;
using CTC.Repository.Enum;
using CTC.Repository.IRepository;
using CTC.ViewModels.Academic;
using CTCTest.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

[TestClass]
public class AcademicControllerTests : TestBase
{
    private AcademicController _controller;
    private Mock<IAcademicRepository> _mockAcademicRepository;
    private Mock<IUserRepository> _mockUserRepository;

    [TestInitialize]
    public  void Setup()
    {
        base.BaseSetup();

        _mockAcademicRepository = new Mock<IAcademicRepository>();
        _mockUserRepository = new Mock<IUserRepository>();

        _controller = new AcademicController(
            _mockEnvironment.Object,
            _mockUserManager.Object,
            _mockAcademicRepository.Object,
            _dbContext,
            _mockUserRepository.Object
        );

        SetupControllerContext(_controller);
    }
    [TestMethod]
    public async Task HomeAdmin_ReturnsViewWithCorrectCounts()
    {
        // Arrange
        var facultyCount = 2;
        var materialsCount = 2;
        var membersCount = 15;

        _mockAcademicRepository.Setup(x => x.GetAcademicMemberShipCount("AcademicMemberShip"))
            .ReturnsAsync(membersCount);

        await AddTestDataToContext();

        // Act
        var result = await _controller.HomeAdmin() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("~/Views/LeaderDepartment/Academic/HomeAdmin.cshtml", result.ViewName);

        // Add debug information
        Console.WriteLine($"Expected faculty count: {facultyCount}");
        Console.WriteLine($"Actual faculty count: {_controller.ViewBag.facultymembersCount}");
        Console.WriteLine($"Expected materials count: {materialsCount}");
        Console.WriteLine($"Actual materials count: {_controller.ViewBag.materialsummariescount}");

        Assert.AreEqual(facultyCount, _controller.ViewBag.facultymembersCount);
        Assert.AreEqual(materialsCount, _controller.ViewBag.materialsummariescount);
        Assert.AreEqual(membersCount, _controller.ViewBag.members);
    }
    [TestMethod]
    public async Task FacultyMembers_ReturnsViewWithFacultyList()
    {
        // Arrange
        var facultyMembers = new List<Facultymembers>
    {
        new Facultymembers
        {
            Id = 1,
            NameDoctor = "Dr. Test",
            Email = "test@test.com",
            department = Department.ComputerScience,
            prefx = "Dr.",
            MemberName = "TestMember", // Added required property
            UserId = "user123",        // Added required property
            Approved = true            // Optional but good to set
        }
    };

        await _dbContext.facultymembers.AddRangeAsync(facultyMembers);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _controller.FacultyMembers(1) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("~/Views/LeaderDepartment/Academic/FacultyMembers.cshtml", result.ViewName);
        var model = result.Model as List<FacultymembersViewModel>;
        Assert.IsNotNull(model);
        Assert.AreEqual(1, model.Count);

        // Additional assertions to verify the model data
        var firstFaculty = model.First();
        Assert.AreEqual("Dr. Test", firstFaculty.Name);
        Assert.AreEqual("test@test.com", firstFaculty.Email);
        Assert.AreEqual(Department.ComputerScience, firstFaculty.department);
        Assert.AreEqual("Dr.", firstFaculty.prefx);
    }
    [TestMethod]
    public async Task TableSummaryMaterial_ReturnsFilteredMaterials()
    {
        // Arrange
        var materials = new List<MaterialSummary>
        {
            new MaterialSummary
            {
                Id = 1,
                MaterialName = "Test Material",
                materialsDepartment = Department.ComputerScience
            }
        };
        _mockAcademicRepository.Setup(x => x.GetAllMaterialsAsync())
            .ReturnsAsync(materials);

        // Act
        var result = await _controller.TableSummaryMaterial("ComputerScience") as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        var model = result.Model as List<MaterialSummaryViewModel>;
        Assert.IsNotNull(model);
        Assert.AreEqual(1, model.Count);
        Assert.AreEqual("ComputerScience", _controller.ViewBag.SelectedDepartment);
    }

    [TestMethod]
    public async Task Delete_ValidId_RedirectsToTableSummaryMaterial()
    {
        // Arrange
        var material = new MaterialSummary
        {
            Id = 1,
            MaterialName = "Test Material",
            MaterialDescription = "Test Description",
            materialsDepartment = Department.ComputerScience,
            PdfUrl = "/test.pdf",
            UploadDate = DateTime.Now,
            username = "testuser",
            Approved = true
        };

        // Setup mock environment
        _mockEnvironment.Setup(x => x.WebRootPath)
            .Returns(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));

        // Create test file
        var webRootPath = _mockEnvironment.Object.WebRootPath;
        Directory.CreateDirectory(webRootPath); // Ensure directory exists
        var filePath = Path.Combine(webRootPath, "test.pdf");
        File.WriteAllText(filePath, "test content");

        _mockAcademicRepository.Setup(x => x.GetMaterialByIDAsync(1))
            .ReturnsAsync(material);

        _mockAcademicRepository.Setup(x => x.DeleteMaterialAsync(1))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(1) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("TableSummaryMaterial", result.ActionName);

        // Verify that DeleteMaterialAsync was called
        _mockAcademicRepository.Verify(x => x.DeleteMaterialAsync(1), Times.Once);

        // Cleanup
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        if (Directory.Exists(webRootPath))
        {
            Directory.Delete(webRootPath, true);
        }
    }

    // Add test for when material doesn't exist
    [TestMethod]
    public async Task Delete_InvalidId_ReturnsNotFound()
    {
        // Arrange
        _mockAcademicRepository.Setup(x => x.GetMaterialByIDAsync(1))
            .ReturnsAsync((MaterialSummary)null);

        // Act
        var result = await _controller.Delete(1) as NotFoundResult;

        // Assert
        Assert.IsNotNull(result);
    }

    // Add test for when file doesn't exist
    [TestMethod]
    public async Task Delete_FileDoesNotExist_StillDeletesRecord()
    {
        // Arrange
        var material = new MaterialSummary
        {
            Id = 1,
            MaterialName = "Test Material",
            MaterialDescription = "Test Description",
            materialsDepartment = Department.ComputerScience,
            PdfUrl = "/nonexistent.pdf",
            UploadDate = DateTime.Now,
            username = "testuser",
            Approved = true
        };

        _mockEnvironment.Setup(x => x.WebRootPath)
            .Returns(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));

        _mockAcademicRepository.Setup(x => x.GetMaterialByIDAsync(1))
            .ReturnsAsync(material);

        _mockAcademicRepository.Setup(x => x.DeleteMaterialAsync(1))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(1) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("TableSummaryMaterial", result.ActionName);
        _mockAcademicRepository.Verify(x => x.DeleteMaterialAsync(1), Times.Once);
    }
    [TestMethod]
    public async Task ReviewFacultyRequests_ReturnsViewWithPendingRequests()
    {
        // Arrange
        var pendingRequests = new List<Facultymembers>
        {
            new Facultymembers
            {
                Id = 1,
                NameDoctor = "Dr. Test",
                Approved = false
            }
        };
        _mockAcademicRepository.Setup(x => x.GetAllFactualMemberAsync())
            .ReturnsAsync(pendingRequests);

        // Act
        var result = await _controller.ReviewFacultyRequests() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        var model = result.Model as List<FacultymembersViewModel>;
        Assert.IsNotNull(model);
        Assert.AreEqual(1, model.Count);
    }

    [TestMethod]
    public async Task AssignDuties_Get_ReturnsViewWithModel()
    {
        // Arrange
        var members = new List<User>
        {
            new User { Id = 1, UserName = "TestUser" }
        };
        _mockAcademicRepository.Setup(x => x.GetAcademicMemberShipAsync())
            .ReturnsAsync(members);

        // Act
        var result = await _controller.AssignDuties() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        var model = result.Model as AssignDutiesViewModel;
        Assert.IsNotNull(model);
        Assert.AreEqual(1, model.Users.Count);
    }

    [TestMethod]
    public async Task AssignDuties_Post_ValidModel_RedirectsToAssignDuties()
    {
        // Arrange
        var model = new AssignDutiesViewModel
        {
            SelectedUsers = new List<string> { "1" },
            DutyDescription = "Test Duty"
        };

        var user = new User { Id = 1 };
        _mockUserManager.Setup(x => x.FindByIdAsync("1"))
            .ReturnsAsync(user);
        _mockUserManager.Setup(x => x.IsInRoleAsync(user, "AcademicMemberShip"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.AssignDuties(model) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("AssignDuties", result.ActionName);
    }
    [TestCleanup]
    public void Cleanup()
    {
        _dbContext?.Database.EnsureDeleted();
        _dbContext?.Dispose();
    }
    private async Task AddTestDataToContext()
    {
        var facultyMembers = new List<Facultymembers>
    {
        CreateTestFacultyMember(1, "Test1", Department.ComputerScience),
        CreateTestFacultyMember(2, "Test2", Department.DataScience)
    };

        var materials = new List<MaterialSummary>
    {
        CreateTestMaterial(1, "Material1", Department.ComputerScience, "user1"),
        CreateTestMaterial(2, "Material2", Department.DataScience, "user2")
    };

        await _dbContext.facultymembers.AddRangeAsync(facultyMembers);
        await _dbContext.materialSummaries.AddRangeAsync(materials);
        await _dbContext.SaveChangesAsync();
    }
    private MaterialSummary CreateTestMaterial(int id, string name, Department department, string userId)
    {
        return new MaterialSummary
        {
            Id = id,
            MaterialName = name,
            MaterialDescription = $"Description for {name}",
            materialsDepartment = department,
            UploadDate = DateTime.Now,
            username = $"user{id}",
            UserId = userId,
            PdfUrl = $"/materials/test{id}.pdf",
            Approved = true
        };
    }

    private Facultymembers CreateTestFacultyMember(int id, string name, Department department)
    {
        return new Facultymembers
        {
            Id = id,
            NameDoctor = name,
            Email = $"test{id}@example.com",
            MemberName = $"Member{id}",
            UserId = $"user{id}",
            prefx = "Dr.",
            department = department,
            Approved = true
        };
    }
}