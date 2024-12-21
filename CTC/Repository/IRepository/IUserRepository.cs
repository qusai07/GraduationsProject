using CTC.Models;
using Microsoft.AspNetCore.Identity;

namespace CTC.Repository.IRepository
{
    public interface IUserRepository
    {

        Task  <IdentityResult> CreateAsync (User user,string password);
        Task<IdentityResult> AddUserToRoleAsync(User user, string role);

        Task UpdateUserAsync (User user);

        Task DeleteAsync (User user);
        Task SignInAsync(User user);
        Task SignOutAsync();
        Task <User> GetByIdAsync (string id);
        Task<IEnumerable<User>> GetAllUsersAsync ();
  




    }
}
