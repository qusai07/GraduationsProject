
using CTC.Data;
using CTC.Models;
using CTC.Models.Academic;
using CTC.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CTC.Repository.Repository
{
    public class AcademicRepository : IAcademicRepository
    {
        private readonly CtcDbContext _ctcDbContext;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly UserManager<User> _userManager;


        public AcademicRepository(CtcDbContext ctcDbContext, RoleManager<IdentityRole<int>> roleManager, UserManager<User> userManager)
        {
            _ctcDbContext = ctcDbContext;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task AddMaterialAsync(MaterialSummary materialSummary)
        {
            _ctcDbContext.materialSummaries.Add(materialSummary);
            await _ctcDbContext.SaveChangesAsync();
        }
        public async Task DeleteMaterialAsync(int id)
        {
            var  matrial=await _ctcDbContext.materialSummaries.FindAsync(id);
            if(matrial != null)
            {
                _ctcDbContext.materialSummaries.Remove(matrial);
                await _ctcDbContext.SaveChangesAsync();
            }
        }
        public async Task<IEnumerable<MaterialSummary>> GetAllMaterialsAsync()
        {
            return await _ctcDbContext.materialSummaries.ToListAsync();

        }
        public async Task<MaterialSummary> GetMaterialByIDAsync(int id)
        {
            return await _ctcDbContext.materialSummaries.FindAsync(id);
        }
        public async Task<IEnumerable<MaterialSummary>> GetMaterialsByUserIdAsync(string userId)
        {
            return await _ctcDbContext.materialSummaries
                   .Where(m => m.UserId == userId) 
                   .ToListAsync();
        }
        public async Task UpdateMaterialAsync(MaterialSummary materialSummary)
        {
            _ctcDbContext.materialSummaries.Update(materialSummary);
            await _ctcDbContext.SaveChangesAsync();
        }
        public async Task<MaterialSummary> GetPendingMaterialRequestById(int requestId)
        {
            return await _ctcDbContext.materialSummaries.FirstOrDefaultAsync(m => m.Id == requestId);
        }
        public async Task<List<MaterialSummary>> GetMaterialsForUserAsync(int userId)
        {
            return await _ctcDbContext.materialSummaries
                                      .Where(m => m.UserId == userId.ToString()) // Ensure materials are filtered by user
                                      .ToListAsync();
        }



        public async Task<IEnumerable<Facultymembers>> GetAllFactualMemberAsync()
        {
            return await _ctcDbContext.facultymembers.ToListAsync();
        }
        public async Task DeletePendingFacultyRequest(int requestId)
        {
            var request = await _ctcDbContext.facultymembers.FindAsync(requestId);
            if (request != null)
            {
                _ctcDbContext.facultymembers.Remove(request);
                await _ctcDbContext.SaveChangesAsync();
            }
        }
        public async Task AddFacultymembers(Facultymembers member)
        {
            _ctcDbContext.facultymembers.Add(member);
            await _ctcDbContext.SaveChangesAsync();
        }
        public async Task<Facultymembers> GetPendingFacultyRequestById(int requestId)
        {
            return await _ctcDbContext.facultymembers.FirstOrDefaultAsync(r => r.Id == requestId);

        }
        public async Task UpdateFacultyMemberAsync(Facultymembers member)
        {
            _ctcDbContext.facultymembers.Update(member);
            await _ctcDbContext.SaveChangesAsync();

        }
        public async Task<Facultymembers> GetFacultymembersByIDAsync(int id)
        {
            return await _ctcDbContext.facultymembers.FindAsync(id);
        
        }
        public async Task DeleteFacultyMemberAsync(int id)
        {
            var member = await _ctcDbContext.facultymembers.FindAsync(id);
            if(member !=null)
            {
                _ctcDbContext.facultymembers.Remove(member);
                await _ctcDbContext.SaveChangesAsync();

            }
        }
        public async Task<List<Facultymembers>> GetFacultyForUserAsync(int userId)
        {
            return await _ctcDbContext.facultymembers
                                      .Where(m => m.UserId == userId.ToString()) // Ensure materials are filtered by user
                                      .ToListAsync();
        }



        public async Task<int> GetAcademicMemberShipCount(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                return 0;
            }
            var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
            return usersInRole.Count;
        }
        public async Task<List<User>> GetAcademicMemberShipAsync()
        {
            var role = await _roleManager.FindByNameAsync("AcademicMemberShip");
            if (role == null)
            {
                return new List<User>();
            }
            var usersInLeaderRole = await _userManager.GetUsersInRoleAsync("AcademicMemberShip");
            return usersInLeaderRole.ToList();
        }
        public async Task AssignDutyToMemberAsync(Duty duty)
        {
            _ctcDbContext.Duties.Add(duty);
            await _ctcDbContext.SaveChangesAsync();
        }
        public async Task<List<Duty>> GetDutiesForMemberAsync(int memberId)
        {
            return await _ctcDbContext.Duties
                .Where(d => d.MemberId == memberId)
                .Include(d=> d.Member)
                .ToListAsync();
        }

       
    }
}
