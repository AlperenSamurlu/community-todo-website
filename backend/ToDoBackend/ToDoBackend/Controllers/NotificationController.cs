using Microsoft.AspNetCore.Mvc;
using ToDoBackend.Data;
using ToDoBackend.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;


namespace ToDoBackend.Controllers
{
    [EnableCors("AllowSpecificOrigin")]
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationsController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Kullanıcının Bildirimlerini Getir
        [HttpGet]
        public IActionResult GetNotifications()
        {
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized("User ID information could not be retrieved from the token.");
            }

            var notifications = _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            return Ok(notifications);
        }

        // Bildirim oluşturma işlemi



        public async System.Threading.Tasks.Task CreateNotificationsForCommunity(int communityId, int createdByUserId, string message)
        {
            try
            {
                // Kullanıcı adı almak için kullanıcıyı sorgula
                var createdByUser = _context.Users.FirstOrDefault(u => u.UserId == createdByUserId);
                var createdByUserName = createdByUser?.UserName ?? "Unknown User";

                // Mesajı kullanıcı adı ile güncelle
                message = $"{message} by {createdByUserName}";

                // Topluluktaki tüm kullanıcıları getir
                var communityUsers = _context.UserCommunities
                    .Where(uc => uc.CommunityId == communityId)
                    .Select(uc => uc.UserId)
                    .ToList();

                // Bildirimleri topluluk kullanıcılarına oluştur
                var notifications = communityUsers.Select(userId => new Notification
                {
                    CommunityId = communityId,
                    UserId = userId,
                    Message = message,
                    CreatedAt = DateTime.UtcNow
                });

                _context.Notifications.AddRange(notifications);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating notifications: " + ex.Message);
            }

        }
    }
}
