using CTC.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CTC.Models;
using CTC.Data;
using CTC.Extensions;
using CTC.Repository.IRepository;
using CTC.ViewModels.MemberShip;
using CTC.ViewModels.Account;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;


namespace CTC.Controllers
{
    public class AccountController : BaseController
    {
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _configuration;

        public AccountController(
            IWebHostEnvironment environment,
            CtcDbContext ctcDbContext,
            UserManager<User> userManager,
            IMailService mailService,
            ILogger<AccountController> logger,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole<int>> roleManager,
            IDistributedCache cache,
            IConfiguration configuration)
            : base(environment, ctcDbContext, userManager, mailService: mailService, logger: logger)
        {
            _signInManager = signInManager;
            _roleManager = roleManager;
            _cache = cache;
            _configuration = configuration;
        }
        public async Task<IActionResult> Login()
        {

            if (!User.Identity.IsAuthenticated)
            {
                return View();
            }
            else
            {
                var userLogin = await _usermanger.FindByNameAsync(User.Identity.Name);
                var UserRoles = await _usermanger.GetRolesAsync(userLogin);
                if ((UserRoles.Contains("Admin")))
                {
                    return RedirectToAction("Dash", "Admin");
                }
                else if ((UserRoles.Contains("AcademicMemberShip")))
                {
                    return RedirectToAction("profile", "Account");
                }
                else if ((UserRoles.Contains("VolunteerManager")))
                {
                    return RedirectToAction("TableVolunteerWork", "VolunteerManager");
                }
                else if ((UserRoles.Contains("AcademicManager")))
                {
                    return RedirectToAction("HomeAdmin", "Academic");
                }
                else if ((UserRoles.Contains("LeaderMember")))
                {
                    return RedirectToAction("HomeLeader", "LeaderMember");
                }
                else if ((UserRoles.Contains("MediaManager")))
                {
                    return RedirectToAction("Index", "Media");
                }
                else
                {
                    return View("Login");
                }
            }

        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _usermanger.FindByNameAsync(model.UserName);
                if (user != null)
                {
                    var role = await _usermanger.GetRolesAsync(user);
                    if (role.Contains("Admin"))
                    {
                        var resultAdmin = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);
                        if (resultAdmin.Succeeded)
                        {
                            return RedirectToAction("Dash", "Admin");
                        }
                    }
                    else if (role.Contains("AcademicMemberShip"))
                    {
                        var resultUser = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);
                        if (resultUser.Succeeded)
                        {
                            return RedirectToAction("Profile", "Account");
                        }

                    }
                    else if (role.Contains("VolunteerManager"))
                    {
                        var resultUser = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);
                        if (resultUser.Succeeded)
                        {
                            return RedirectToAction("AddVolunteerwork", "Volunteer");
                        }

                    }
                    else if (role.Contains("AssociateMemberShip"))
                    {
                        var resultUser = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);
                        if (resultUser.Succeeded)
                        {
                            return RedirectToAction("Home", "AssociateMember");
                        }

                    }
                    else if (role.Contains("AcademicManager"))
                    {
                        var resultUser = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);
                        if (resultUser.Succeeded)
                        {
                            return RedirectToAction("HomeAdmin", "Academic");
                        }

                    }
                    else if (role.Contains("LeaderMember"))
                    {
                        var resultUser = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);
                        if (resultUser.Succeeded)
                        {
                            return RedirectToAction("TableAppointment", "LeaderMember");
                        }

                    }
                    else if (role.Contains("LeaderMember"))
                    {
                        var resultUser = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);
                        if (resultUser.Succeeded)
                        {
                            return RedirectToAction("TableAppointment", "LeaderMember");
                        }

                    }
                    else if (role.Contains("MediaManager"))
                    {
                        var resultUser = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);
                        if (resultUser.Succeeded)
                        {
                            return RedirectToAction("Index", "Media");
                        }
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View(model);
        }
        [Authorize]
        public async Task<IActionResult> signout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
        public async Task<IActionResult> Profile()  
        {
            if (User.Identity?.Name == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _usermanger.FindByNameAsync(User.Identity.Name);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
        [HttpPost]
        public async Task<IActionResult> EditDataMember(IFormFile imageFile)
        {
            var user = await _usermanger.GetUserAsync(User);

            var FullName = Request.Form["FullName"];
            var UserName = Request.Form["UserName"];
            var Email = Request.Form["Email"];
            var PhoneNumber = Request.Form["PhoneNumber"];
            var FaceBookLink = Request.Form["Facebook"];   
            var LinkedInLink = Request.Form["LinkedIn"];

            if (imageFile != null && imageFile.Length > 0)
            {
                string uniqueFileName = FileExtensions.ConvertImageToString(imageFile, _webHostEnvironment);
                user.ImageUrl = "/Pic/" + uniqueFileName;

            }

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            user.FullName = FullName;
            user.UserName = UserName;
            user.Email = Email;
            user.PhoneNumber = PhoneNumber;
            user.Facebook = FaceBookLink;
            user.LinkedIn = LinkedInLink;   

            var result = await _usermanger.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profile updated successfully";
                return RedirectToAction("Profile");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View("EditProfile", user); // You can replace "EditProfile" with your actual edit view name
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _usermanger.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var currentPassword = Request.Form["CurrentPassword"];
            var newPassword = Request.Form["NewPassword"];
            var confirmPassword = Request.Form["ConfirmPassword"];

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "The new password and confirmation password do not match.");
                return View(user);
            }
            var result = await _usermanger.ChangePasswordAsync(user, currentPassword, newPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Password changed successfully.";
                await _signInManager.SignOutAsync();
                return RedirectToAction("Login", "Account");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(user);
        }

        private async Task SendPasswordResetEmail(JoinerViewModel model)
        {
            var subject = "Your Join Request Received";
            var message = $@"Dear {model.FirstName},

            ""Reset Password"",
           $""Please reset your password by clicking here: <a href='{{resetLink}}'>Reset Password</a>"");

            CTC Team";
            await _mailService.SendEmailAsync(model.UniversityEmail, subject, message);
        }
        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View("~/Views/Account/ForgetPassword.cshtml");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgetPassword(ForgotPasswordViewModel model)
        {   
            if (!ModelState.IsValid)
            { 
                return View(model);
            }
            try
            {
            

                // Rate limiting check
                if (!await CheckRateLimit(model.Email))
                {
                    ModelState.AddModelError(string.Empty, "Too many attempts. Please try again later.");
                    return View(model);
                }

                var user = await _usermanger.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Log the attempt but don't reveal user existence
                    _logger.LogWarning($"Password reset attempted for non-existent email: {model.Email}");
                    ModelState.AddModelError(string.Empty, "your email not exists in our system, make sure your email is true");
                    return View(model);
                }

                // Check if the account is locked
                if (await _usermanger.IsLockedOutAsync(user))
                {
                    ModelState.AddModelError(string.Empty, "This account is temporarily locked. Please try again later.");
                    return View(model);
                }

                // Generate password reset token
                var token = await _usermanger.GeneratePasswordResetTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));


                var callbackUrl = Url.Action(
                 "ResetPassword",
                 "Account",
                 new { email = model.Email, token = encodedToken },
                 protocol: HttpContext.Request.Scheme);

                // Send email
                await SendPasswordResetEmail(user.Email, callbackUrl);

                // Log success
                _logger.LogInformation($"Password reset token generated for user: {user.Email}");
                TempData["SuccessMessage"] = "Password reset instructions have been sent to your email.";
                return RedirectToAction("ForgotPasswordConfirmation");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ForgotPassword: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while processing your request.");
                return View(model);
            }
        }
        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
        private async Task<bool> CheckRateLimit(string email)
        {
            try
            {
                var cacheKey = $"PasswordReset_{email}";
                string? attemptsString = await _cache.GetStringAsync(cacheKey);
                int attempts = 0;

                if (!string.IsNullOrEmpty(attemptsString))
                {
                    attempts = int.Parse(attemptsString);
                }

                if (attempts >= 3) // Maximum 3 attempts per hour
                {
                    return false;
                }

                attempts++;
                await _cache.SetStringAsync(
                    cacheKey,
                    attempts.ToString(),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                    });

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking rate limit: {ex.Message}");
                return true; // In case of cache error, allow the operation
            }
        }
        private async Task SendPasswordResetEmail(string email, string resetUrl)
        {
            var ClickHere = $@"<a href='{resetUrl}'>Click here</a>";
            var subject = "Password Reset Request";
            var message =
            $@"
                    Hello Mr.{email}
                    You have requested to reset your password. Please click the link below to reset your password:
                    {ClickHere} Reset Password
                    This link will expire in 10 Min.
                    If you did not request this reset, please ignore this email or contact support if you have concerns.
                    Best regards,CTC Team";
            await _mailService.SendEmailAsync(email, subject, message);
        }
        private async Task SendPasswordResetConfirmationEmail(string email)
        {

            var subject = "Password Reset Successful";
            var message =
                $@"
            <html>
                <body>
                    <p>Hello,</p>
                    <p>Your password has been successfully reset.</p>
                    <p>If you did not make this change, please contact our support team immediately.</p>
                    <p>Best regards,<br>Your Application Team</p>
                </body>
            </html>";

            await _mailService.SendEmailAsync(email, subject, message);
        }


        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Error");
            }

            var model = new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _usermanger.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation");
            }

            try
            {
                // Decode the token before using it
                var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
                var result = await _usermanger.ResetPasswordAsync(user, decodedToken, model.Password);

                if (result.Succeeded)
                {
                    return RedirectToAction("ResetPasswordConfirmation");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                // Log the error
                ModelState.AddModelError(string.Empty, "Error resetting password. Please try again.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}