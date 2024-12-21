
using CTC.Data;
using CTC.Models;
using CTC.Models.Event;
using CTC.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using QRCoder;

namespace CTC.Repository.Repository
{
    public class EventCtcRepository : IEventCtcRepository
    {
        private readonly CtcDbContext _ctcDbContext;

        public EventCtcRepository(CtcDbContext ctcDbContext )
        {
            _ctcDbContext = ctcDbContext;
        }
        public async Task AddEventAsync(EventsCTC eventsCTC)
        {
            _ctcDbContext.Events.Add(eventsCTC);
            await _ctcDbContext.SaveChangesAsync();
        }

        public async Task DeleteEventAsync(int id)
        {
            var ev = await _ctcDbContext.Events.FindAsync(id);
            if (ev != null)
            {
                _ctcDbContext.Events.Remove(ev);
                await _ctcDbContext.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<EventsCTC>> GetAllEventsAsync()
        {
            return await _ctcDbContext.Events
                .Select(e => new EventsCTC
                {
                    Name = e.Name,
                    EventDate = e.EventDate,
                    Description = e.Description,
                    EventType = e.EventType,
                    Location = e.Location,
                    ImageUrl = e.ImageUrl,

                })
                .ToListAsync();
        }
        public async Task<EventsCTC> GetEventByIdAsync(int eventID)
        {
            var eventEntity = await _ctcDbContext.Events
                .Where(e => e.Id == eventID)
                .Select(e => new EventsCTC
                {
                    Id = e.Id,
                    QRCodeText = e.QRCodeText
                })
                .FirstOrDefaultAsync();

            return eventEntity;
        }
        public async Task<EventsCTC> GetEventByQrCodeAsync(string qrCode)
        {
            if (string.IsNullOrEmpty(qrCode))
            {
                return null;
            }

            // Find the event by QRCodeText
            var eventEntity = await _ctcDbContext.Events
                .FirstOrDefaultAsync(e => e.QRCodeText == qrCode);

            return eventEntity;
        }

        public async Task UpdateEventAsync(EventsCTC eventsCTC)
        {
            _ctcDbContext.Events.Update(eventsCTC);
            await _ctcDbContext.SaveChangesAsync();

        }

        public async Task<int> GetEventCountAsync()
        {
            return await _ctcDbContext.Events.CountAsync();
        }
        
    }
}
