using Microsoft.AspNetCore.Identity;
using CTC.Models.Leader;

namespace CTC.Repository.IRepository
{
    public interface IJoinerRepository
    {
        Task JoinMemberAsync(Joiner joiners);
        Task<List<Joiner>> GetAllRequestsJoinerAsync();
        Task<Joiner> GetUserById(int id);
        Task<int> GetUserCountAsync();

     

    }
}
