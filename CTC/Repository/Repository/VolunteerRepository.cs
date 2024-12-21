using CTC.Data;
using CTC.Models.Volunteer;
using CTC.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace CTC.Repository.Repository
{
    public class VolunteerRepository :IVolunteerRepository
    {
        private readonly CtcDbContext _ctcDbContext;
        public VolunteerRepository(CtcDbContext ctcDbContext)
        {
            _ctcDbContext = ctcDbContext;
        }

        public async Task<IEnumerable<VolunteerParticipants>> GetAllVolunteerParticipationsAsync()
        {
            return await _ctcDbContext.VolunteerParticipants.Include(vp => vp.Volunteering).ToListAsync();
        
        }

        public async Task<Volunteering> GetVolunteerByIdAsync(int volunteerId)
        {
            var volunteer =await _ctcDbContext.volunteering
                                  .FirstOrDefaultAsync(v => v.Id == volunteerId);
            return volunteer;
        }

        public async Task<IEnumerable<VolunteerParticipants>> GetVolunteerParticipationsByVolunteerIdAsync(int volunteerId)
        {
            return await _ctcDbContext.VolunteerParticipants
                .Where(vp => vp.VolunteerId == volunteerId)
                .Include(vp => vp.Volunteering)  // Load related event details
                .ToListAsync();  // Using ToListAsync to asynchronously fetch the data
        }

        public async Task UpdateVolunteer(Volunteering UpdateVolunteer)
        {
            var existingVolunteer = await _ctcDbContext.volunteering.FindAsync(UpdateVolunteer.Id);
            if (existingVolunteer != null)
            {
                existingVolunteer.Organization=UpdateVolunteer.Organization;
                existingVolunteer.Description=UpdateVolunteer.Description;
                existingVolunteer.Date=UpdateVolunteer.Date;
                existingVolunteer.ImageFile=UpdateVolunteer.ImageFile;
                existingVolunteer.MaxParticipants=UpdateVolunteer.MaxParticipants;
                existingVolunteer.Type=UpdateVolunteer.Type;

                await _ctcDbContext.SaveChangesAsync();
              
            }
        }
    }
}
