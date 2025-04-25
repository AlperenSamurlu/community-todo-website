using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ToDoBackend.Data;
using ToDoBackend.Models;
using ToDoBackend.Services;
using System.Threading.Tasks; // Task.Run için gereken
using ToDoBackend.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;

namespace ToDoBackend.Controllers
{
    [EnableCors("AllowSpecificOrigin")]
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        private readonly IServiceProvider _serviceProvider;
        public TasksController(AppDbContext context, EmailService emailService , IServiceProvider serviceProvider)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _serviceProvider = serviceProvider;
        }


        // 1. Görev Listeleme
        [HttpGet("user/tasks")]
        public async Task<IActionResult> GetUserTasks()
        {
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized("User ID information could not be retrieved from the token.");
            }

            var userCommunities = await _context.UserCommunities
                .Where(uc => uc.UserId == userId)
                .Select(uc => uc.CommunityId)
                .ToListAsync();

            var tasks = await _context.Tasks
    .Where(t =>
        (t.IsIndividual && t.CreatedBy == userId) || // Sadece kendine ait bireysel görevler
        (!t.IsIndividual && t.CommunityId != null && userCommunities.Contains(t.CommunityId.Value)) // Topluluk görevleri
    )
    .ToListAsync();


            return Ok(new { values = tasks });
        }





        // 2. Yeni Görev Ekleme
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] ToDoBackend.Models.Task task)
        {
            // Token'dan kullanıcı ID'sini al ve doğrula
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized("User ID information could not be retrieved from the token.");
            }

            // Oluşturucuyu, token'dan alınan kullanıcı ID'sine eşitleyin
            task.CreatedBy = userId; 

            try
            {
                if (task == null)
                {
                    return BadRequest(new { message = "Task object is null." });
                }

                // Görevi veritabanına ekle
                _context.Tasks.Add(task);
                await _context.SaveChangesAsync();

                // E-posta gönderimini ayrı bir metoda bırak
                if (!task.IsIndividual && task.CommunityId != null)
                {
                    // Bildirim oluşturmak için NotificationsController'ı çağır
                    var notificationsController = new NotificationsController(_context);
                    await notificationsController.CreateNotificationsForCommunity(
                        communityId: task.CommunityId.Value,
                        createdByUserId: task.CreatedBy,
                        message: $"A new task '{task.TaskTitle}' has been added."
                    );

                    // E-posta gönder
                     System.Threading.Tasks.Task.Run(() => SendEmailsForTask(task));
                    
                }

                return Ok(task);
            }
            catch (NullReferenceException nullEx)
            {
                Console.WriteLine($"NullReferenceException: {nullEx.Message}");
                return StatusCode(500, new { message = $"Internal server error: {nullEx.Message}" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }





        // üsttekinin yardımcı fonksiyonu
        private void SendEmailsForTask(ToDoBackend.Models.Task task)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    if (!task.IsIndividual && task.CommunityId != null)
                    {
                        // Topluluk bilgilerini alın
                        var community = context.Communities.FirstOrDefault(c => c.CommunityId == task.CommunityId);
                        var communityName = community?.CommunityName ?? "Unknown Community";

                        // Görevi oluşturan kullanıcıyı alın
                        var creator = context.Users.FirstOrDefault(u => u.UserId == task.CreatedBy);
                        var taskCreatorName = creator?.UserName ?? "Unknown User";

                        // Topluluk üyelerini alın
                        var communityMembers = context.UserCommunities
                            .Where(uc => uc.CommunityId == task.CommunityId)
                            .Select(uc => uc.UserId)
                            .ToList();

                        // Her topluluk üyesi için e-posta gönder
                        foreach (var userId in communityMembers)
                        {
                            var user = context.Users.FirstOrDefault(u => u.UserId == userId);

                            // Kullanıcının e-posta bildirimlerinin etkin olup olmadığını kontrol edin
                            if (user != null && user.IsEmailNotificationEnabled)
                            {
                                try
                                {
                                    Console.WriteLine($"Sending email to: {user.Email}");
                                    _emailService.SendTaskNotification(user.Email, communityName, task.TaskTitle, taskCreatorName);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"E-posta gönderimi sırasında hata: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendEmailsForTask metodu hatası: {ex.Message}");
            }
        }










        // 3. Görev Güncelleme
        [HttpPut("{taskId}")]
        public async Task<IActionResult> UpdateTask(int taskId, [FromBody] TaskUpdateRequest updatedTask)
        {
            // Token'dan kullanıcı ID'sini al
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized("User ID information could not be retrieved from the token.");
            }

            var task = _context.Tasks.FirstOrDefault(t => t.TaskId == taskId);
            if (task == null)
            {
                return NotFound("Task not found.");
            }

            // Eğer topluluk görevi ise, o topluluğa üye misiniz kontrol et
            if (!task.IsIndividual && task.CommunityId.HasValue)
            {
                var membership = await _context.UserCommunities
                    .FirstOrDefaultAsync(uc => uc.CommunityId == task.CommunityId.Value && uc.UserId == userId);

                if (membership == null)
                {
                    return Forbid("You are not a member of this community and cannot update the task.");
                }
            }

            // Task alanlarını güncelle
            task.TaskTitle = updatedTask.TaskTitle;
            task.TaskDescription = updatedTask.TaskDescription;
            task.StartDate = updatedTask.StartDate;
            task.EndDate = updatedTask.EndDate;
            task.IsCompleted = updatedTask.IsCompleted;

            _context.SaveChanges();

            // Topluluk görevi ise bildirimleri oluştur
            if (!task.IsIndividual && task.CommunityId.HasValue)
            {
                var notificationsController = new NotificationsController(_context);
                await notificationsController.CreateNotificationsForCommunity(
                    communityId: task.CommunityId.Value,
                    createdByUserId: userId,
                    message: $"The task '{task.TaskTitle}' has been updated."
                );
            }

            return Ok(task);
        }




        // 4. Görev Silme
        [HttpDelete("{taskId}")]
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            // Token'dan kullanıcı ID'sini al
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized("User ID information could not be retrieved from the token.");
            }

            var task = _context.Tasks.FirstOrDefault(t => t.TaskId == taskId);
            if (task == null)
            {
                return NotFound("Task not found.");
            }

            // Eğer topluluk görevi ise silme işlemi yapacak kullanıcının topluluğa üye olup olmadığını kontrol et
            if (!task.IsIndividual && task.CommunityId.HasValue)
            {
                var membership = await _context.UserCommunities
                    .FirstOrDefaultAsync(uc => uc.CommunityId == task.CommunityId.Value && uc.UserId == userId);

                if (membership == null)
                {
                    return Forbid("You are not a member of this community and cannot delete this task.");
                }

                // Bildirim gönder
                var notificationsController = new NotificationsController(_context);
                await notificationsController.CreateNotificationsForCommunity(
                    communityId: task.CommunityId.Value,
                    createdByUserId: userId,
                    message: $"The task '{task.TaskTitle}' has been deleted"
                );
            }

            // Görevi veritabanından sil
            _context.Tasks.Remove(task);
            _context.SaveChanges();

            return Ok("Task deleted successfully.");
        }



        // 5. Görev tamamlandı mı?
        [HttpPut("{id}/complete")]
        public IActionResult UpdateTaskCompletionStatus(int id, [FromBody] JsonElement data)
        {
            // Token'dan kullanıcı ID'sini al
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized("User ID information could not be retrieved from the token.");
            }

            var task = _context.Tasks.FirstOrDefault(t => t.TaskId == id);
            if (task == null)
            {
                return NotFound("Task not found.");
            }

            // Eğer topluluk görevi ise, kullanıcının topluluğa üye olup olmadığını kontrol et
            if (!task.IsIndividual && task.CommunityId.HasValue)
            {
                var membership = _context.UserCommunities
                    .FirstOrDefault(uc => uc.CommunityId == task.CommunityId.Value && uc.UserId == userId);

                if (membership == null)
                {
                    return Forbid("You are not a member of this community and cannot update the task.");
                }
            }

            // Görev tamamlanma durumu
            if (data.TryGetProperty("isCompleted", out JsonElement isCompletedElement))
            {
                task.IsCompleted = isCompletedElement.GetBoolean();
                _context.SaveChanges();

                return Ok(new { message = "Task completion status updated.", isCompleted = task.IsCompleted });
            }

            return BadRequest("Invalid request data.");
        }





        // görev getirme 
        [HttpGet("{taskId}")]
        public IActionResult GetTaskById(int taskId)
        {
            // Token'dan kullanıcı ID'sini al
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized(new { message = "User ID information could not be retrieved from the token." });
            }

            var task = _context.Tasks.FirstOrDefault(t => t.TaskId == taskId);
            if (task == null)
            {
                return NotFound(new { message = "Task not found" });
            }
	if (task.IsIndividual)
    {
        // Eğer görev bireysel ise, sadece oluşturan kişi görebilir
        if (task.CreatedBy != userId)
        {
            return Forbid("You are not authorized to view this individual task.");
        }
    }
            // Eğer görev topluluğa aitse, ilgili toplulukta üye misiniz kontrolü
            if (!task.IsIndividual && task.CommunityId.HasValue)
            {
                var membership = _context.UserCommunities
                    .FirstOrDefault(uc => uc.CommunityId == task.CommunityId.Value && uc.UserId == userId);

                if (membership == null)
                {
                    return Forbid("You are not a member of this community and cannot view this task.");
                }
            }
            // Bireysel bir görevse, isterseniz görevi oluşturanın userId ile eşleşip eşleşmediğini burada kontrol edebilirsiniz.

            return Ok(task);
        }

    }
}
