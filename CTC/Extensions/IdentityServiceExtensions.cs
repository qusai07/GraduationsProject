using CTC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using CTC.Data;
using System.Security.Cryptography;
using System.Text;

namespace CTC.Extensions
{
    public static class IdentityServiceExtensions
    {
        private static readonly char[] passwordChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()_+".ToCharArray();
        public static IServiceCollection AddIdentityServiceExtensions(this IServiceCollection services, IConfiguration options)
        {
            services.AddIdentity<User, IdentityRole<int>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.User.RequireUniqueEmail = true;
            })
                .AddEntityFrameworkStores<CtcDbContext>()
                .AddDefaultTokenProviders();

            return services;


        }
        public static string GenerateRandomPassword(int length)
        {
            const string uppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowercaseChars = "abcdefghijklmnopqrstuvwxyz";
            const string digitChars = "0123456789";
            const string nonAlphanumericChars = "!@#$%^&*()-_=+[]{}|;:,.<>?";

            // This includes all the characters that can be used to generate the password
            string allChars = uppercaseChars + lowercaseChars + digitChars + nonAlphanumericChars;

            // This is to make sure the password has at least one of each required character type
            var password = new List<char>();
            Random random = new Random();

            // Ensure that at least one character from each required set is included
            password.Add(uppercaseChars[random.Next(uppercaseChars.Length)]);
            password.Add(lowercaseChars[random.Next(lowercaseChars.Length)]);
            password.Add(digitChars[random.Next(digitChars.Length)]);
            password.Add(nonAlphanumericChars[random.Next(nonAlphanumericChars.Length)]);

            // Fill the rest of the password with random characters from all possible characters
            for (int i = password.Count; i < length; i++)
            {
                password.Add(allChars[random.Next(allChars.Length)]);
            }

            // Shuffle the characters so the required ones aren't always in the same positions
            return new string(password.OrderBy(_ => random.Next()).ToArray());
        }
        public static string GenerateRandomPasswordHash()
        {
            string password = GenerateRandomPassword(15); // Generate a 15-character random password
            Console.WriteLine("Generated Password: " + password); // To see the generated password
            return password; // Return the raw password (without hashing) to satisfy IdentityManager
        }






    }











}