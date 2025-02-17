using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoBackend.Data;
using ToDoBackend.DTOs;
using ToDoBackend.Models;
using ToDoBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;

namespace ToDoBackend.Controllers
{
    [EnableCors("AllowSpecificOrigin")]
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    
    public class UserCommunitiesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        public UserCommunitiesController(AppDbContext context ,EmailService emailService) 
        {
            //_context = context;
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        // 1. Kullanıcının Topluluklarını Listeleme
        [HttpGet("user/communities")]
        public async Task<IActionResult> GetUserCommunities()
        {
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized("User ID information could not be retrieved from the token.");
            }

            var userCommunities = await _context.UserCommunities
                .Where(uc => uc.UserId == userId)
                .Include(uc => uc.Community) // Topluluk verilerini dahil eder
                .Select(uc => new UserCommunityDTO
                {
                    UserId = uc.UserId,
                    CommunityId = uc.CommunityId,
                    Role = uc.Role,
                    CommunityName = uc.Community != null ? uc.Community.CommunityName : "Unknown"
                })
                .ToListAsync();

            if (userCommunities == null || !userCommunities.Any())
                return NotFound("User's communities not found.");

            return Ok(userCommunities);
        }


        // Topluluktaki tüm üyeleri getirme
        [HttpGet("{communityId}/members")]
        public IActionResult GetCommunityMembers(int communityId)
        {
            // Token'dan çağrı yapan kullanıcı ID'sini al
            var tokenUserIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(tokenUserIdString, out int tokenUserId))
            {
                return Unauthorized("User ID information could not be retrieved from the token.");
            }

            // Topluluktaki kullanıcı kaydını bul
            var memberRecord = _context.UserCommunities
                .FirstOrDefault(uc => uc.CommunityId == communityId && uc.UserId == tokenUserId);

            // Kullanıcı topluluğa üye değilse ya da kayıt yoksa
            if (memberRecord == null)
            {
                return Forbid("You are not a member of this community.");
            }

            // Kullanıcı admin değilse topluluk üyelerini göremesin
            if (memberRecord.Role != "Admin")
            {
                return Forbid("Only admins can view the community members.");
            }

            // Kullanıcı admin olduğuna göre tüm üyeleri getir
            var members = _context.UserCommunities
                .Where(uc => uc.CommunityId == communityId)
                .Select(uc => new
                {
                    UserId = uc.UserId,
                    UserName = uc.User != null ? uc.User.UserName : "Unknown",
                    Role = uc.Role
                })
                .ToList();

            return Ok(members);
        }


       [HttpDelete("{communityId}/{userId?}")]
        public async Task<IActionResult> RemoveOrLeaveCommunity(int communityId, int? userId = null)
        {
            // Token'dan çağrı yapan kullanıcı ID'sini al
            var tokenUserIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(tokenUserIdString, out int tokenUserId))
            {
                return Unauthorized("User ID information could not be retrieved from the token.");
            }

            // Eğer userId null ise, çağrıyı yapan kullanıcı kendi isteğiyle çıkmak istiyor
            if (userId == null)
            {
                userId = tokenUserId;
            }

            // İlgili topluluk üyeliğini getir
            var userCommunity = await _context.UserCommunities
                .FirstOrDefaultAsync(uc => uc.CommunityId == communityId && uc.UserId == userId);

            if (userCommunity == null)
            {
                return NotFound("User not found in the community.");
            }

            // Eğer çağrı yapan kullanıcı, çıkmak istediği kullanıcıysa (kendi isteğiyle ayrılma)
            if (tokenUserId == userId)
            {
                _context.UserCommunities.Remove(userCommunity);
                await _context.SaveChangesAsync();
                return Ok("User left the community.");
            }
            else
            {
                // Eğer farklı bir kullanıcı ise, çağrıyı yapan kullanıcının admin yetkisini kontrol et
                var adminMembership = await _context.UserCommunities
                    .FirstOrDefaultAsync(uc => uc.CommunityId == communityId && uc.UserId == tokenUserId);

                if (adminMembership == null || adminMembership.Role != "Admin")
                {
                    return Forbid("Only admins can remove other users from the community.");
                }

                _context.UserCommunities.Remove(userCommunity);
                await _context.SaveChangesAsync();
                return Ok("User removed from the community.");
            }
        }

        [HttpPut("{communityId}/promote/{userId}")]
        public IActionResult PromoteUserToAdmin(int communityId, int userId)
        {
            // Token'dan çağrı yapan kullanıcı ID'sini al ve doğrula
            var tokenUserIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(tokenUserIdString, out int tokenUserId))
            {
                return Unauthorized("User ID information could not be retrieved from the token.");
            }

            
            var callerMembership = _context.UserCommunities
                .FirstOrDefault(uc => uc.CommunityId == communityId && uc.UserId == tokenUserId);
            if (callerMembership == null || callerMembership.Role != "Admin")
            {
                return Forbid("Only admins can promote users to Admin.");
            }

            var userCommunity = _context.UserCommunities
                .FirstOrDefault(uc => uc.CommunityId == communityId && uc.UserId == userId);

            if (userCommunity == null)
            {
                return NotFound("User not found in the community.");
            }

            if (userCommunity.Role == "Admin")
            {
                return BadRequest("User is already an Admin.");
            }

            userCommunity.Role = "Admin";
            _context.SaveChanges();

            return Ok("User promoted to Admin.");
        }
    }
}
