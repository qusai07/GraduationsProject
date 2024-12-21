﻿using Microsoft.AspNetCore.Identity;
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
/*-----Done-----*/              new IdentityRole<int>{Name="Admin"},                                                            
/*-----Done-----*/              new IdentityRole<int>{Name="LeaderMember"},//Administrative body                                
/*-----Done-----*/              new IdentityRole<int>{Name="AcademicManager"},                                                  
/*-----DonX-----*/              new IdentityRole<int>{Name="VolunteerManager"},//Done 4 else Delete and upload a new image      
/*----------------------------------------------------------------------------------------------------------------------------*/
/*-----XXXX-----*/              new IdentityRole<int>{Name="ActivitiesManager"}, //  Same of the Events 5                       
/*-----XXXX-----*/              new IdentityRole<int>{Name="MediaManager"}, //?? 6   Manager view layout                        
/*----------------------------------------------------------------------------------------------------------------------------*/
/*-----Done-----*/              new IdentityRole<int>{Name="AssociateMemberShip"},   //7                                           
/*-----Done-----*/              new IdentityRole<int>{Name="AcademicMemberShip"},    //8                                           
/*----------------------------------------------------------------------------------------------------------------------------*/ 
              };
            foreach (var role in roles)
            {
                await roleManager.CreateAsync(role);
            }
            var Admin = new User()
            {
                UserName = "FarisMajed",
                Email = "FarisMajed@gmail.com",
                PhoneNumber = "+962799842558",
                FullName = "Faris Majed Masa'deh",
            };
            await userManager.CreateAsync(Admin, "Pa$$w0rd");
            await userManager.AddToRoleAsync(Admin, "Admin");
        }
    }
}

//Delete any thing after 30 days
//Image Home with ctcData
//ctcData footer page
//we should write it for CTC App
//[Route("api/[controller]")]






