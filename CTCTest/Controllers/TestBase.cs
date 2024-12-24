using CTC.Data;
using CTC.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
namespace CTCTest.Controllers
{
    public abstract class TestBase
    {
        protected Mock<UserManager<User>> _mockUserManager;
        protected Mock<SignInManager<User>> _mockSignInManager;
        protected Mock<RoleManager<IdentityRole<int>>> _mockRoleManager;
        protected Mock<IWebHostEnvironment> _mockEnvironment;
        protected CtcDbContext _dbContext;
        protected Mock<CtcDbContext> _mockDbContext;

        [TestInitialize]
        public virtual void BaseSetup()
        {
            // Setup UserManager
            var userStoreMock = new Mock<IUserStore<User>>();
            _mockUserManager = MockUserManager();

            // Setup SignInManager
            _mockSignInManager = new Mock<SignInManager<User>>(
                _mockUserManager.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<User>>(),
                null, null, null, null
            );

            // Setup RoleManager
            _mockRoleManager = new Mock<RoleManager<IdentityRole<int>>>(
                Mock.Of<IRoleStore<IdentityRole<int>>>(), null, null, null, null
            );

            // Setup Environment
            _mockEnvironment = new Mock<IWebHostEnvironment>();

            // Setup DbContext
            var options = new DbContextOptionsBuilder<CtcDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _dbContext = new CtcDbContext(options);
            _mockDbContext = new Mock<CtcDbContext>(new DbContextOptions<CtcDbContext>());
        }

        protected Mock<UserManager<User>> MockUserManager()
        {
            var store = new Mock<IUserStore<User>>();
            var mgr = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
            mgr.Object.UserValidators.Add(new UserValidator<User>());
            mgr.Object.PasswordValidators.Add(new PasswordValidator<User>());
            return mgr;
        }

        protected void SetupControllerContext(Controller controller)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "FarisMajed@gmail.com"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
            }));

            var httpContext = new DefaultHttpContext { User = user };
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            controller.TempData = new TempDataDictionary(
                httpContext,
                Mock.Of<ITempDataProvider>()
            );
        }
    
}
}
