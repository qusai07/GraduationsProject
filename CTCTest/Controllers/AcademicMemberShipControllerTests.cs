using CTC.Controllers;
using CTC.Models;
using CTC.Models.Academic;
using CTC.Repository.Enum;
using CTC.Repository.IRepository;
using CTC.ViewModels.Academic;
using CTCTest.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace CTC.Tests
{
    [TestClass]
    public class AcademicMemberShipControllerTests : TestBase
    {
        private AcademicMemberShipController _controller;
        private Mock<IAcademicRepository> _mockAcademicRepo;
        private Mock<IUserRepository> _mockUserRepo;

        [TestInitialize]
        public void Setup()
        {
            base.BaseSetup();

            _mockAcademicRepo = new Mock<IAcademicRepository>();
            _mockUserRepo = new Mock<IUserRepository>();

            _controller = new AcademicMemberShipController(
                _mockEnvironment.Object,
                _mockUserManager.Object,
                _mockAcademicRepo.Object,
                _dbContext,
                _mockUserRepo.Object
            );

            SetupControllerContext(_controller);
        }

        [TestMethod]
        public void MemberAcademic_ReturnsCorrectView()
        {
            // Act
            var result = _controller.MemberAcademic();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual("~/Views/MemberShip/AcademicMemberShip/MemberAcademic.cshtml", viewResult.ViewName);
        }

        [TestMethod]
        public async Task AddSummaryMaterial_UserNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            SetupUnauthenticatedUser();

            // Act
            var result = await _controller.AddSummaryMaterial();

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
        }

        [TestMethod]
        public async Task AddSummaryMaterial_AuthenticatedUser_ReturnsViewWithModel()
        {
            // Arrange
            var testUser = new User { Id = 1, UserName = "testuser" };
            SetupAuthenticatedUser(testUser);
            _mockUserRepo.Setup(x => x.GetByIdAsync(It.IsAny<string>()))
                        .ReturnsAsync(testUser);

            // Act
            var result = await _controller.AddSummaryMaterial();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.IsInstanceOfType(viewResult.Model, typeof(MaterialSummaryViewModel));
            Assert.AreEqual("~/Views/MemberShip/AcademicMemberShip/AddSummaryMaterial.cshtml", viewResult.ViewName);
        }

        [TestMethod]
        public async Task AddSummaryMaterial_Post_InvalidFile_ReturnsView()
        {
            // Arrange
            var testUser = new User { Id = 1, UserName = "FarisMajed" };
            SetupAuthenticatedUser(testUser);

            var model = new MaterialSummaryViewModel
            {
                MaterialName = "Test Material",
                MaterialDescription = "Test Description",
                MemberName = "Test Member",
                materialsDepartment = Department.ComputerScience,
                pdfFile = null // Invalid file
            };
            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                    .ReturnsAsync(testUser);

            // Add ModelState error to simulate invalid model
            _controller.ModelState.AddModelError("pdfFile", "File is required");

            // Act
            var result = await _controller.AddSummaryMaterial(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.AreEqual("~/Views/MemberShip/Academic/AddSummaryMaterial.cshtml",
                ((ViewResult)result).ViewName);
        }
        [TestMethod]
        public async Task AddSummaryMaterial_Post_InvalidModel_ReturnsView()
        {
            // Arrange
            var testUser = new User { Id = 1, UserName = "testuser" };
            SetupAuthenticatedUser(testUser);

            var model = new MaterialSummaryViewModel(); // Invalid model (missing required fields)
            _controller.ModelState.AddModelError("", "Test error");

            // Act
            var result = await _controller.AddSummaryMaterial(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }
        [TestMethod]
        public async Task FacultyMembers_ReturnsViewWithFacultyList()
        {
            // Arrange
            var testUser = new User { Id = 1, UserName = "testuser" };
            SetupAuthenticatedUser(testUser);

            var facultyList = new List<Facultymembers>
            {
                new Facultymembers { Id = 1, NameDoctor = "Dr. Test" }
            };

            _mockAcademicRepo.Setup(x => x.GetFacultyForUserAsync(It.IsAny<int>()))
                            .ReturnsAsync(facultyList);

            // Act
            var result = await _controller.FacultyMembers(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.IsInstanceOfType(viewResult.Model, typeof(List<FacultymembersViewModel>));
        }

        [TestMethod]
        public async Task TableSummaryMaterial_ReturnsFilteredMaterials()
        {
            // Arrange
            var testUser = new User { Id = 1, UserName = "testuser" };
            SetupAuthenticatedUser(testUser);

            var materials = new List<MaterialSummary>
            {
                new MaterialSummary { Id = 1, MaterialName = "Test Material" }
            };

            _mockAcademicRepo.Setup(x => x.GetMaterialsForUserAsync(It.IsAny<int>()))
                            .ReturnsAsync(materials);

            // Act
            var result = await _controller.TableSummaryMaterial("CS");

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.IsInstanceOfType(viewResult.Model, typeof(List<MaterialSummaryViewModel>));
        }

        #region Helper Methods
        private void SetupUnauthenticatedUser()
        {
            var mockUser = new ClaimsPrincipal(new ClaimsIdentity());
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = mockUser }
            };
        }

        private void SetupAuthenticatedUser(User user)
        {
            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                           .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
                           .Returns(user.Id.ToString());

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }
        #endregion
        [TestMethod]
        public async Task AddSummaryMaterial_Post_ValidModel_RedirectsToAction()
        {
            // Arrange
            var testUser = new User { Id = 1, UserName = "testuser" };
            SetupAuthenticatedUser(testUser);

            // Create a mock file
            var fileMock = new Mock<IFormFile>();
            var content = "Hello World from a Fake File";
            var fileName = "test.pdf";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;

            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);

            // Setup environment mock for file handling
            _mockEnvironment.Setup(e => e.WebRootPath).Returns("wwwroot");

            var model = new MaterialSummaryViewModel
            {
                MaterialName = "Test Material",
                MaterialDescription = "Test Description",
                MemberName = "Test Member",
                materialsDepartment = Department.ComputerScience,
                pdfFile = fileMock.Object // Use the mock file object directly
            };

            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                            .ReturnsAsync(testUser);

            // Act
            var result = await _controller.AddSummaryMaterial(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("AddSummaryMaterial", redirectResult.ActionName);

            // Cleanup
            ms.Dispose();
        }

        // Update the CreateMockFormFile helper method
        private IFormFile CreateMockFormFile()
        {
            var content = "Hello World from a Fake File";
            var fileName = "test.pdf";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);

            return fileMock.Object;
        }



    }
}