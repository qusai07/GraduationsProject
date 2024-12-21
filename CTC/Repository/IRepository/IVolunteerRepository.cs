using CTC.Models.Volunteer;

namespace CTC.Repository.IRepository
{
    public interface IVolunteerRepository
    {
        Task<Volunteering> GetVolunteerByIdAsync(int volunteerId);
        Task<IEnumerable<VolunteerParticipants>> GetVolunteerParticipationsByVolunteerIdAsync(int volunteerId);

        Task <IEnumerable<VolunteerParticipants>> GetAllVolunteerParticipationsAsync();

        Task  UpdateVolunteer(Volunteering UpdateVolunteer);

    }
}
