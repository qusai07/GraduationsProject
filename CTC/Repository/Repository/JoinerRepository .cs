using CTC.Data;
using CTC.Models.Leader;
using CTC.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CTC.Repository.Repository
{
    public class JoinerRepository : IJoinerRepository
    {
        private readonly CtcDbContext _context;
        private readonly List<Joiner> _joiners = new List<Joiner>();


        public JoinerRepository(CtcDbContext context)
        {
            _context = context;
            _joiners = new List<Joiner>();
        }



        public async Task<List<Joiner>> GetAllRequestsJoinerAsync()
        {
            return await _context.Joiners.ToListAsync();
        }

        public async Task<Joiner> GetUserById(int id)
        {
            var user = await _context.Joiners.FindAsync(id);
            return user;
            
        }

        public async Task<int> GetUserCountAsync()
        {
            return await _context.Joiners.CountAsync();
        }

        public async Task JoinMemberAsync(Joiner joiners)
        {
            _context.Joiners.Add(joiners);
            await _context.SaveChangesAsync();
            _joiners.Add(joiners);

        }
    }
}
