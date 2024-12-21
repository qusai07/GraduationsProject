using CTC.Data;
using CTC.Models;
using CTC.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;

namespace CTC.Repository.Repository
{
    public class NotificationRepository: INotificationRepository
    {
        private readonly CtcDbContext _context;

        public NotificationRepository(CtcDbContext context)
        {
            _context = context;
        }
        public async Task<List<Notification>> GetUnreadNotificationsAsync()
        {
            return await _context.Notification.Where(n => !n.IsRead).ToListAsync();
        }

        public async Task<int> GetNotificationCountAsync()
        {
            return await _context.Notification.CountAsync(n => !n.IsRead);
        }

        public async Task AddNotification(Notification notification)
        {
            await _context.Notification.AddAsync(notification);
            await _context.SaveChangesAsync();
        }



        //public async Task<int> GetNotificationFacultyMembersCountAsync()
        //{
        //    return await _context.facultymembers.CountAsync(f => !f.Approved);
        //}
        //public async Task<List<Facultymembers>> GetUnreadNotificationFacultyMembersAsync()
        //{
        //    return await _context.facultymembers.Where(n => !n.Approved).ToListAsync();
        //}
        //public async Task<int> GetNotificationMaterialSummariesCountAsync()
        //{
        //    return await _context.materialSummaries.CountAsync(f => !f.Approved);
        //}
        //public async Task<List<MaterialSummary>> GetUnreadNotificationMaterialSummariesAsync()
        //{
        //    return await _context.materialSummaries.Where(n => !n.Approved).ToListAsync();
        //}
    }
}
