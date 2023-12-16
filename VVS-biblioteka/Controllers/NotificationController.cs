using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VVS_biblioteka.Models;

namespace VVS_biblioteka.Controllers
{
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly LibDbContext _context;

        public NotificationController(LibDbContext context)
        {
            _context = context;
        }

        [HttpPost("sendNotification")]
        public async Task<IActionResult> SendNotification(int userId, string message)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Id == userId);

            var notification = new Notification
            {
                Message = message,
                CreatedAt = DateTime.Now,
                IsRead = false,
                UserId = userId
            };

            _context.Notification.Add(notification);
            await _context.SaveChangesAsync();

            return Ok("Notification sent successfully");
        }

        [HttpGet("getUnread")]
        public List<Notification> GetUnreadNotifications(int userId)
        {
            return _context.Notification
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToList();        
        }

        [HttpPost("markAsRead")]
        public async Task<IActionResult> MarkNotificationAsRead(int userId, int notificationId)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Id == userId);

            var notification = await _context.Notification.FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new { Success = true });
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteNotification(int userId, int notificationId)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Id == userId);

            var notification = await _context.Notification.FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            _context.Notification.Remove(notification);
            await _context.SaveChangesAsync();

            return Ok(new { Success = true });
        }
    }
}
