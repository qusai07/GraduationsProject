using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CTC.Models;
using System.Reflection.Emit;
using CTC.Models.MediaModels;
using CTC.Models.Academic;
using CTC.Models.Volunteer;
using CTC.Models.Admin;
using CTC.Models.Leader;
using CTC.Models.Event;


namespace CTC.Data
{
    public class CtcDbContext : IdentityDbContext<User, IdentityRole<int>,int>
    {
        public CtcDbContext(DbContextOptions<CtcDbContext> options):base(options) 
        {

        
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Appointment>()
           .HasOne(a => a.Joiner)
           .WithMany(j => j.Appointment)
           .HasForeignKey(a => a.UserId)
           .OnDelete(DeleteBehavior.Cascade); // or your desired behavior



            builder.Entity<VolunteerParticipants>()
                  .HasOne(vp => vp.Volunteering)          // Navigation property
                  .WithMany(v => v.VolunteerParticipants) // One Volunteering has many VolunteerParticipants
                  .HasForeignKey(vp => vp.EventId)        // Foreign key is EventId in VolunteerParticipants
                  .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(builder);
        }
        public  DbSet<User> Users { get; set; }
        public DbSet<Joiner> Joiners { get; set; }
        public DbSet<EventsCTC> Events { get; set; }
        public DbSet<MaterialSummary> materialSummaries { get; set; }
        public DbSet <Facultymembers> facultymembers { get; set; }

        public DbSet <ContactMessage> contactMessages { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<Notification> Notification { get; set; }
        public DbSet<Volunteering> volunteering { get; set; }
        public DbSet<AttendanceAtEveryEvent> attendanceAtEveryEvents { get; set; }
        public DbSet<QRCodeModel> QRCode { get; set; }

        public DbSet<Appointment> Appointment { get; set; }
        public DbSet<Duty> Duties { get; set; }
        public DbSet<VolunteerParticipants> VolunteerParticipants { get; set; }

        public DbSet<FeaturesApp> featuresApp { get; set; }
        public DbSet<Nahno> nahno { get; set; }
        public DbSet<Videohome> videohome { get; set; }
        public DbSet<WhoWeAre> whoWeAre { get; set; }
        public DbSet<Founders> founders { get; set; }
        public DbSet<Esports> esports { get; set; }
        public DbSet<Sponser> sponsers { get; set; }

        public DbSet<BachelorPrograms> bachelorPrograms { get; set; }
        public DbSet<CtcData> ctcData { get; set; }





    }
}
