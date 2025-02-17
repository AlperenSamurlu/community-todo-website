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
    public class CommunitiesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        public CommunitiesController(AppDbContext context ,EmailService emailService)
        {
           // _context = context;
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        // 1. Toplulukları Listele
        [HttpGet]
        public IActionResult GetCommunities()
        {
            var communities = _context.Communities.ToList();
            return Ok(communities);
        }



        // task type daki toplulukları doldurmaya yarar
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
                .Include(uc => uc.Community) // Community tablosunu ilişkilendir
                .Select(uc => new
                {
                    uc.CommunityId,
                    CommunityName = uc.Community != null ? uc.Community.CommunityName : "Unknown",
                    uc.Role
                })
                .ToListAsync();

            return Ok(userCommunities);
        }




        // 2. Yeni Topluluk Ekle
        [HttpPost]
        public IActionResult CreateCommunity([FromBody] Community community)
        {
            if (community == null || string.IsNullOrWhiteSpace(community.CommunityName))
            {
                return BadRequest("Community information is missing.");
            }

            // Token'da NameIdentifier claim varsa UserId'yi buradan alıyoruz
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized("User ID information could not be retrieved from the token.");
            }

           
            // token doğrulanırsa 'CreatedBy' değerini buradan ayarlayabilirsiniz.
            community.CreatedBy = userId;

            // Topluluk ekle
            _context.Communities.Add(community);
            _context.SaveChanges();

            // Topluluğu oluşturan kullanıcıyı otomatik olarak admin olarak ekle
            var userCommunity = new UserCommunity
            {
                UserId = community.CreatedBy,
                CommunityId = community.CommunityId,
                Role = "Admin",
                JoinedAt = DateTime.Now
            };

            _context.UserCommunities.Add(userCommunity);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetCommunities), new { id = community.CommunityId }, community);
        }






        // 3. Topluluğa Katıl
        [HttpPost("{communityId}/join")]
        public IActionResult JoinCommunity(int communityId, [FromBody] JoinCommunityRequest request)
        {
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized("User ID information could not be retrieved from the token.");
            }

            // Topluluğu veritabanında bul
            var community = _context.Communities.FirstOrDefault(c => c.CommunityId == communityId);
            if (community == null)
            {
                return NotFound(new { message = "Community not found." });
            }

            // Şifre kontrolü
            if (!string.Equals(community.Password, request.Password, StringComparison.Ordinal))
            {
                return BadRequest(new { message = "Incorrect password." });
            }

            // Kullanıcının zaten topluluk üyesi olup olmadığını kontrol et
            var existingMembership = _context.UserCommunities
                .FirstOrDefault(uc => uc.UserId == userId && uc.CommunityId == communityId);

            if (existingMembership != null)
            {
                return BadRequest(new { message = "User is already a member of this community." });
            }

            // Kullanıcı ve topluluk ilişkisi oluştur
            var userCommunity = new UserCommunity
            {
                UserId = userId,
                CommunityId = communityId,
                Role = "Member",
                JoinedAt = DateTime.Now
            };

            _context.UserCommunities.Add(userCommunity);
            _context.SaveChanges();

            return Ok(new { message = "Joined community successfully." });
        }

        // 6. Id ye göre topluluk bilgileri getir
        
        [HttpGet("community/{id}")]
        public IActionResult GetCommunityById(int id)
        {
            // Token'dan kullanıcı ID'sini al
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized("User ID information could not be retrieved from the token.");
            }

            // İlgili topluluğu bul
            var community = _context.Communities.FirstOrDefault(c => c.CommunityId == id);
            if (community == null)
            {
                return NotFound("Community not found.");
            }

            
             var membership = _context.UserCommunities.FirstOrDefault(uc => uc.UserId == userId && uc.CommunityId == id);
             if (membership == null)
             {
                 return Forbid("You do not have access to this community.");
             }

            return Ok(community);
        }
        


        // 8. Search
        [HttpGet("search")]
        public IActionResult SearchCommunities(string query)
        {
            // Token'dan kullanıcı ID'sini al
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
            {
                return Unauthorized("User ID information could not be retrieved from the token.");
            }

            // Parametrenin boş olup olmadığını kontrol et
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new { message = "Query parameter is required." });
            }

            // Veritabanında arama işlemi (büyük/küçük harf duyarsız hale getirildi)
            var communities = _context.Communities
                .Where(c => EF.Functions.Like(c.CommunityName, $"%{query}%"))
                .Select(c => new
                {
                    c.CommunityId,
                    c.CommunityName,
                    c.Description
                })
                .ToList();

            // Eğer topluluk bulunamazsa
            if (communities == null || !communities.Any())
            {
                return NotFound(new { message = "No communities found." });
            }

            return Ok(communities);
        }
    }
}
