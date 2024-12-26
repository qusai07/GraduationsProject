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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        protected static Mock<UserManager<User>> MockUserManager()
        {
            var store = new Mock<IUserStore<User>>();
            var options = new Mock<IOptions<IdentityOptions>>();
            var idOptions = new IdentityOptions();
            options.Setup(o => o.Value).Returns(idOptions);
            var userValidators = new List<IUserValidator<User>>();
            var validator = new Mock<IUserValidator<User>>();
            userValidators.Add(validator.Object);
            var pwdValidators = new List<PasswordValidator<User>>();
            pwdValidators.Add(new PasswordValidator<User>());
            var userManager = new Mock<UserManager<User>>(
                store.Object,
                options.Object,
                new PasswordHasher<User>(),
                userValidators,
                pwdValidators,
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                null,
                new Mock<ILogger<UserManager<User>>>().Object);

            return userManager;
        }
        protected virtual Mock<SignInManager<User>> MockSignInManager()
        {
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
            return new Mock<SignInManager<User>>(_mockUserManager.Object, contextAccessor.Object, claimsFactory.Object, null, null, null, null);
        }

        protected virtual Mock<RoleManager<IdentityRole<int>>> MockRoleManager()
        {
            var store = new Mock<IRoleStore<IdentityRole<int>>>();
            var roles = new List<IRoleValidator<IdentityRole<int>>>();
            return new Mock<RoleManager<IdentityRole<int>>>(store.Object, roles, new UpperInvariantLookupNormalizer(), new IdentityErrorDescriber(), null);
        }

    }
}