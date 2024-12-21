using CTC.Models;
using CTC.Models.Academic;

namespace CTC.Repository.IRepository
{
    public interface IAcademicRepository
    {
        Task <IEnumerable<MaterialSummary>> GetAllMaterialsAsync();
        Task <MaterialSummary> GetMaterialByIDAsync(int id);
        Task AddMaterialAsync(MaterialSummary  materialSummary);
        Task DeleteMaterialAsync(int id);
        Task UpdateMaterialAsync(MaterialSummary materialSummary );  
        Task<IEnumerable<MaterialSummary>> GetMaterialsByUserIdAsync(string userId);
        Task<MaterialSummary> GetPendingMaterialRequestById(int requestId);

        Task<List<MaterialSummary>> GetMaterialsForUserAsync(int userId);
        Task<Facultymembers> GetFacultymembersByIDAsync(int id);
        Task<IEnumerable<Facultymembers>> GetAllFactualMemberAsync();
        Task DeletePendingFacultyRequest(int requestId);
        Task AddFacultymembers(Facultymembers member);
        Task<Facultymembers> GetPendingFacultyRequestById(int requestId);
        Task UpdateFacultyMemberAsync(Facultymembers member);
        Task DeleteFacultyMemberAsync(int id);
        Task<List<Facultymembers>> GetFacultyForUserAsync(int requestId);




        Task<int> GetAcademicMemberShipCount(string roleName);
        Task<List<User>> GetAcademicMemberShipAsync();
        Task AssignDutyToMemberAsync(Duty duty);
        Task<List<Duty>> GetDutiesForMemberAsync(int memberId);




    }
}
