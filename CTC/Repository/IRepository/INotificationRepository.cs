using CTC.Models;

namespace CTC.Repository.IRepository
{
    public interface INotificationRepository
    {
        Task AddNotification(Notification notification);
        Task<List<Notification>> GetUnreadNotificationsAsync();
        Task<int> GetNotificationCountAsync();

        //Task<int> GetNotificationFacultyMembersCountAsync();
        //Task<int> GetNotificationMaterialSummariesCountAsync();
        //Task<List<Facultymembers>> GetUnreadNotificationFacultyMembersAsync();
        //Task<List<MaterialSummary>> GetUnreadNotificationMaterialSummariesAsync();



    }
}
