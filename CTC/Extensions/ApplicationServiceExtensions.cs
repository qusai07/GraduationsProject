using CTC.Data;
using CTC.Models;
using CTC.Repository.IRepository;
using CTC.Repository.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CTC.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddServiceExtensions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContextPool<CtcDbContext>(options =>

            {
                options.UseSqlServer(configuration.GetConnectionString("ConnectionString"));
            });
            services.AddCors();
            services.AddScoped<IUserRepository,UserRepository>();
            services.AddScoped<IJoinerRepository,JoinerRepository>();
            services.AddScoped<IEventCtcRepository,EventCtcRepository>();
            services.AddTransient<IMailService, MailService>();           
            services.AddScoped<IAcademicRepository, AcademicRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IVolunteerRepository, VolunteerRepository>();

            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            return services;

        }
    }
}
