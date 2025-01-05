using CTC.Data;
using CTC.Models.Admin;
using CTC.Models.Volunteer;
using Microsoft.EntityFrameworkCore;

public class TestCtcDbContext : CtcDbContext
{
    public TestCtcDbContext(DbContextOptions<CtcDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Make Volunteering properties optional
        modelBuilder.Entity<Volunteering>(entity =>
        {
            entity.Property(e => e.Organization).IsRequired(false);
            entity.Property(e => e.Description).IsRequired(false);
            entity.Property(e => e.Location).IsRequired(false);
            entity.Property(e => e.Type).IsRequired(false);
        });

        // Make VolunteerParticipants properties optional
        modelBuilder.Entity<VolunteerParticipants>(entity =>
        {
            entity.Property(e => e.ParticipateName).IsRequired(false);
            entity.Property(e => e.Status).IsRequired(false);
        });
        modelBuilder.Entity<CtcData>(entity =>
        {
            entity.Property(e => e.Country).IsRequired(false);
            entity.Property(e => e.FaceBook).IsRequired(false);
            entity.Property(e => e.Instagram).IsRequired(false);
            entity.Property(e => e.LinedIn).IsRequired(false);
            entity.Property(e => e.Nahno).IsRequired(false);
            entity.Property(e => e.CaptionHome).IsRequired(false);
        });
    }
}