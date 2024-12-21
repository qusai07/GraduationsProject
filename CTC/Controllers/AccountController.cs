using CTC.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CTC.Models;
using CTC.Data;
using CTC.Repository.IRepository;
using CTC.ViewModels.MemberShip;
using CTC.Repository.Repository;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;
using CTC.Extensions;


namespace CTC.Controllers
{
    public class AccountController : Controller
    {

        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly CtcDbContext _CtcDbContext;
        private readonly IWebHostEnvironment _environment;

        public AccountController(IWebHostEnvironment environment, UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole<int>> roleManager, CtcDbContext ctcDbContext )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _CtcDbContext = ctcDbContext;
            _environment = environment;
        }
        public async Task<IActionResult> Login()
        {

            if (!User.Identity.IsAuthenticated)
            {
                return View();
            }
            else
            {
                var userLogin = await _userManager.FindByNameAsync(User.Identity.Name);
                var UserRoles = await _userManager.GetRolesAsync(userLogin);
                if ((UserRoles.Contains("Admin"))){
                    return RedirectToAction("Dash", "Admin");
                }
                else if((UserRoles.Contains("AcademicMemberShip"))) 
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
                var user = await _userManager.FindByNameAsync(model.UserName);
                if (user != null)
                {
                    var role = await _userManager.GetRolesAsync(user);
                    if (role.Contains("Admin"))
                    {
                        var resultAdmin = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);
                        if (resultAdmin.Succeeded)
                        {
                            return RedirectToAction("Dash", "Admin");
                        }
                    }
                    else if(role.Contains("AcademicMemberShip"))
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

            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
        [HttpPost]
        public async Task<IActionResult> EditDataMember(IFormFile imageFile)
        {
            var user = await _userManager.GetUserAsync(User);

            var FullName = Request.Form["FullName"];
            var UserName = Request.Form["UserName"];
            var Email = Request.Form["Email"];
            var PhoneNumber = Request.Form["PhoneNumber"];
           // var Image = Request.Form["ImageFile"];

            if (imageFile != null && imageFile.Length > 0)
            {
                string uniqueFileName = FileExtensions.ConvertImageToString(imageFile, _environment);
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

            var result = await _userManager.UpdateAsync(user);
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
            var user = await _userManager.GetUserAsync(User);
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
            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
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
       





    }
}