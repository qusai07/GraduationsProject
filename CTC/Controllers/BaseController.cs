using CTC.Data;
using CTC.Models;
using CTC.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CTC.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly IWebHostEnvironment _webHostEnvironment;
        protected readonly CtcDbContext _ctcDbContext;
        protected readonly UserManager<User> _usermanger;
        protected readonly IUserRepository _userRepository;
        protected readonly IMailService _mailService;
        protected readonly IEventCtcRepository _eventCtcRepository;
        protected readonly INotificationRepository _notificationRepository;
        protected readonly ILogger _logger;

        protected BaseController(
            IWebHostEnvironment environment,
            CtcDbContext ctcDbContext,
            UserManager<User> userManager,
            IUserRepository userRepository = null,
            IMailService mailService = null,
            IEventCtcRepository eventCtcRepository = null,
            INotificationRepository notificationRepository = null,
            ILogger logger = null)
        {
            _webHostEnvironment = environment;
            _ctcDbContext = ctcDbContext;
            _usermanger = userManager;
            _userRepository = userRepository;
            _mailService = mailService;
            _eventCtcRepository = eventCtcRepository;
            _notificationRepository = notificationRepository;
            _logger = logger;
        }
    }
}
