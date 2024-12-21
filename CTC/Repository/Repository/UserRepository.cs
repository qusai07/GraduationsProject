using CTC.Data;
using CTC.Models;
using CTC.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace CTC.Repository.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _UserManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly CtcDbContext _ctcDbContext;

        public UserRepository(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole<int>> roleManager, CtcDbContext ctcDbContext)
        {
            _UserManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _ctcDbContext = ctcDbContext;
        }

        public async Task<IdentityResult> AddUserToRoleAsync(User user, string role)
        {
            return await _UserManager.AddToRoleAsync(user, role);
        }

        public async Task<IdentityResult> CreateAsync(User user, string password)
        {
            return await _UserManager.CreateAsync(user, password);
        }

        public async Task DeleteAsync(User user)
        {
            var result = await _UserManager.DeleteAsync(user);
        }

       
        public  async Task<User> GetByIdAsync(string id)
        {
            return await _UserManager.FindByIdAsync(id);
        }


        public async Task SignInAsync(User user)
        {
            await _signInManager.SignInAsync(user, false);
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            var result = await _UserManager.UpdateAsync(user);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _ctcDbContext.Users.ToListAsync();
        }
    }
}
