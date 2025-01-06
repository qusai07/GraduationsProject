using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CTC.Models;

namespace CTC.Data
{
    public static class SeedData
    {
        public static async Task InitializeUser(UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            if (await userManager.Users.AnyAsync())
                return;
            var roles = new List<IdentityRole<int>>()
            {
/*----------------------------------------------------------------------------------------------------------------------------*/
/*-----Done-----*/              new IdentityRole<int>{Name="Admin"},     // 1                                                       
/*-----Done-----*/              new IdentityRole<int>{Name="LeaderMember"},// 2                               
/*-----Done-----*/              new IdentityRole<int>{Name="AcademicManager"}, // 3                                                 
/*-----DonX-----*/              new IdentityRole<int>{Name="VolunteerManager"},// 4     
/*----------------------------------------------------------------------------------------------------------------------------*/
/*-----XXXX-----*/              new IdentityRole<int>{Name="MediaManager"}, // 5                         
/*----------------------------------------------------------------------------------------------------------------------------*/
/*-----Done-----*/              new IdentityRole<int>{Name="AssociateMemberShip"},   // 6                                          
/*-----Done-----*/              new IdentityRole<int>{Name="AcademicMemberShip"},    // 7                                          
/*----------------------------------------------------------------------------------------------------------------------------*/ 
              };
            foreach (var role in roles)
            {
                await roleManager.CreateAsync(role);
            }
            var Admin = new User()
            {
                UserName = "QusaiTahat",
                Email = "qusaitahat4@gmail.com",
                PhoneNumber = "+790150089",
                FullName = "Qusai Nayel Tahat"
            };
            await userManager.CreateAsync(Admin, "Pa$$w0rd");
            await userManager.AddToRoleAsync(Admin, "Admin");
        }
    }
}
 

//Delete any thing after 30 days

//Edit FeatureInfo Check

//ctcData footer page






