using CTC.Models;
using CTC.Models.Event;

namespace CTC.Repository.IRepository
{
    public interface IEventCtcRepository
    {
        Task<IEnumerable<EventsCTC>> GetAllEventsAsync();
        Task AddEventAsync(EventsCTC eventsCTC);
        Task DeleteEventAsync(int id);
        Task UpdateEventAsync(EventsCTC eventsCTC);
        Task<EventsCTC> GetEventByIdAsync(int eventID);
        Task<int> GetEventCountAsync();
        Task<EventsCTC> GetEventByQrCodeAsync(string qrCode);



    }
}
