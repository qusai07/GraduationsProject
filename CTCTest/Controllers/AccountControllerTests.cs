using CTC.Models;
using CTCTest.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using System.Security.Principal;

namespace CTC.Controllers.Tests
{
    [TestClass()]
    public class AccountControllerTests : TestBase
    {
        private AccountController _controller;
        [TestInitialize]
        public void Setup()
        {
            base.BaseSetup();
            _controller = new AccountController(
                _mockEnvironment.Object,
                _mockUserManager.Object,
                _mockSignInManager.Object,
                _mockRoleManager.Object,
                _mockDbContext.Object
            );
            SetupControllerContext(_controller);
        }

        [TestMethod]
        public async Task Login_NotAuthenticated_ReturnsViewResult()
        {
            // Arrange
            var mockUser = new ClaimsPrincipal(new ClaimsIdentity());
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = mockUser }
            };

            // Act
            var result = await _controller.Login();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public async Task Login_Authenticated_AdminRole_RedirectsToAdminDashboard()
        {
            // Arrange
            var mockUser = new ClaimsPrincipal(new ClaimsIdentity("TestAuthentication"));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = mockUser }
            };

            var user = new User { UserName = "FarisMajed" };
            _mockUserManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });

            // Act
            var result = await _controller.Login();

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Dash", redirectResult.ActionName);
            Assert.AreEqual("Admin", redirectResult.ControllerName);
        }

        [TestMethod]
        public async Task SignOut_ValidCall_RedirectsToLogin()
        {
            // Act
            var result = await _controller.signout();

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Login", redirectResult.ActionName);
            Assert.AreEqual("Account", redirectResult.ControllerName);
        }

        [TestMethod]
        public async Task Profile_ValidUser_ReturnsViewWithUser()
        {
            // Arrange
            var user = new User { UserName = "FarisMajed" };
            _mockUserManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);

            // Mock the User.Identity.Name to return a valid username.
            var mockIdentity = new Mock<IIdentity>();
            mockIdentity.Setup(i => i.Name).Returns("FarisMajed");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(mockIdentity.Object) }
            };

            // Act
            var result = await _controller.Profile();

            // Assert
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(user, viewResult.Model);
        }


        [TestMethod]
        public async Task EditDataMember_ValidInput_UpdatesUserProfile()
        {
            // Arrange
            var user = new User { UserName = "FarisMajed" };
            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024); // Simulate a file of 1KB
            mockFile.Setup(f => f.FileName).Returns("testImage.jpg");

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
    {
        { "FullName", "Faris Majed" },
        { "UserName", "FarisMajed" },
        { "Email", "FarisMajed@example.com" },
        { "PhoneNumber", "+962799842558" }
    });

            // Mock IWebHostEnvironment
            _mockEnvironment.Setup(e => e.WebRootPath).Returns("C:\\TestWebRoot");

            // Set up TempData
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            _controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                httpContext,
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>()
            );

            // Act
            var result = await _controller.EditDataMember(mockFile.Object);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Profile", redirectResult.ActionName);
        }


        [TestMethod]
        public async Task ChangePassword_ValidPasswordChange_RedirectsToLogin()
        {
            // Arrange
            var user = new User { UserName = "FarisMajed" };
            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _mockUserManager.Setup(x => x.ChangePasswordAsync(user, It.IsAny<string>(), It.IsAny<string>()))
                            .ReturnsAsync(IdentityResult.Success);

            // Mock the Request.Form property
            var httpContext = new DefaultHttpContext();
            var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
             { "CurrentPassword", "Pa$$w0rd" },
             { "NewPassword", "Pa$$w0rd1" },
             { "ConfirmPassword", "Pa$$w0rd1" }
              });
            httpContext.Request.Form = formCollection;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            _controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(httpContext,
             Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());
            // Act
            var result = await _controller.ChangePassword();

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Login", redirectResult.ActionName);
        }


    }
}
