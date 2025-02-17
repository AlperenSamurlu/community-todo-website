using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ToDoBackend.Data;
using ToDoBackend.DTOs;
using ToDoBackend.Models;
using ToDoBackend.Services;


namespace ToDoBackend.Controllers
{
    [EnableCors("AllowSpecificOrigin")]
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        private readonly IConfiguration _configuration;
        public UsersController(AppDbContext context, EmailService emailService, IConfiguration configuration)
    {
        _context = context;
        _emailService = emailService;
        _configuration = configuration;
    }


        
        private string GenerateJwtToken(User user)
        {
            try
            {
                var key = _configuration["Jwt:Key"];
                if (string.IsNullOrEmpty(key) || key.Length < 16)
                {
                    throw new InvalidOperationException("JWT key must be at least 16 characters long");
                }

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                };

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddHours(10),
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token generation error: {ex.Message}");
                throw;
            }
        }

        // 1. Kullanıcı Listesi Getir
        [HttpGet]
        [Authorize]
        public IActionResult GetUsers()
        {
            var users = _context.Users.ToList();
            return Ok(users);
        }
        // 2. Yeni Kullanıcı Ekle
        [HttpPost("register")]
        [AllowAnonymous]
        public IActionResult Register([FromBody] User user)
        {
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                return Conflict("User with this email already exists.");
            }

            // Rastgele 6 basamaklı doğrulama kodu oluştur
            var verificationCode = new Random().Next(100000, 999999).ToString();
            //user.VerificationCode = verificationCode;
            user.VerificationCode = BCrypt.Net.BCrypt.HashPassword(verificationCode);
            user.IsVerified = false;
            user.CreatedAt = DateTime.Now;
            user.LastVerificationAttempt = DateTime.Now; // Yeni alanı doldur

            // Şifreyi hashleyerek sakla
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            _context.Users.Add(user);
            _context.SaveChanges();

            // Doğrulama kodunu e-posta ile gönder
            _emailService.SendVerificationCode(user.Email, verificationCode);

            return Ok("User registered successfully. Verification code sent.");
        }

        // 2 dk den sonra kod silinir
        [HttpDelete("cleanup-unverified")]
        [Authorize]
        public IActionResult CleanupUnverifiedUsers()
        {
            var threshold = DateTime.Now.AddMinutes(-2); // 2 dakikalık süreyi kontrol et
            var unverifiedUsers = _context.Users
                .Where(u => !u.IsVerified && u.LastVerificationAttempt <= threshold)
                .ToList();

            _context.Users.RemoveRange(unverifiedUsers);
            _context.SaveChanges();

            return Ok("Unverified users cleaned up.");
        }


        // şifre değiştirme
        [HttpPut("change-password")]
        [Authorize]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var tokenUserIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(tokenUserIdString, out int tokenUserId))
            {
                return Unauthorized("User ID information could not be retrieved from the token.");
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId == tokenUserId);
            if (user == null) return NotFound("User not found.");

            // Eski şifre kontrolü
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                return BadRequest("Current password is incorrect.");
            }

            // Yeni şifreyi hashleyip kaydet
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            _context.SaveChanges();

            return Ok("Password updated successfully.");
        }

        // Login Metodu
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] DTOs.LoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Invalid login request.");
            }

            // Kullanıcıyı email ile asenkron olarak bul
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            // Şifre kontrolü (hash kullanarak)
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            var token = GenerateJwtToken(user);

            // Başarılı yanıt
            return Ok(new
            {
                message = "Login successful!",
                token = token
            });
        }

        [HttpGet("verify")]
        [AllowAnonymous]
        public IActionResult VerifyToken()
        {
             return Ok(new { valid = true });
        }
        
      



        // 6. E-posta Doğrulama
        [HttpPost("verify")]
        [AllowAnonymous]
        public IActionResult VerifyEmail([FromBody] VerificationRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // if (user.VerificationCode == request.Code)
            if (BCrypt.Net.BCrypt.Verify(request.Code, user.VerificationCode))
            {
                user.IsVerified = true;
                user.VerificationCode = null; // Kullanım sonrası doğrulama kodu sıfırlanır
                _context.SaveChanges();
                return Ok(new { message = "Email verified successfully." });
            }

            return BadRequest(new { message = "Invalid verification code." });
        }


        // Kullanıcı adını alma
        [HttpGet("getUserName")]
        [Authorize]
        public async Task<IActionResult> GetUserName()
        {
            var tokenUserIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(tokenUserIdString, out int tokenUserId))
            {
                return Unauthorized("User ID information could not be retrieved from the token.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == tokenUserId);
            if (user == null)
                return NotFound("User not found.");

            return Ok(new { userName = user.UserName });
        }



        // E-mail bilglilendirmesi istiyor mu???
        [HttpPut("notifications/enable")]
        [Authorize]
        public IActionResult EnableEmailNotifications()
        {
            var tokenUserIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(tokenUserIdString, out int tokenUserId))
            {
                return Unauthorized("User ID information could not be retrieved from the token.");
            }
            
            var user = _context.Users.FirstOrDefault(u => u.UserId == tokenUserId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.IsEmailNotificationEnabled = true;
            _context.SaveChanges();

            return Ok("E-mail notifications enabled.");
        }

        [HttpPut("notifications/disable")]
        [Authorize]
        public IActionResult DisableEmailNotifications()
        {
            var tokenUserIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(tokenUserIdString, out int tokenUserId))
            {
                return Unauthorized("User ID information could not be retrieved from the token.");
            }
            
            var user = _context.Users.FirstOrDefault(u => u.UserId == tokenUserId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.IsEmailNotificationEnabled = false;
            _context.SaveChanges();

            return Ok("E-mail notifications disabled.");
        }


        // Şifre sıfırlama
        [HttpPost("send-code")]
        [AllowAnonymous]
        public IActionResult SendResetCode([FromBody] ResetRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null) return NotFound(new { message = "Email not found" });

            var resetCode = new Random().Next(100000, 999999).ToString(); // 6 haneli kod
            //user.ResetCode = resetCode;
            user.ResetCode = BCrypt.Net.BCrypt.HashPassword(resetCode);
            user.ResetCodeExpiration = DateTime.UtcNow.AddMinutes(10);

            _context.SaveChanges();

            // Email gönderim işlemi
            _emailService.SendResetCode(user.Email, resetCode);

            return Ok(new { message = "Reset code sent successfully" });
        }
        [HttpPost("verify-code")]
        [AllowAnonymous]
        public IActionResult VerifyResetCode([FromBody] VerifyCodeRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.ResetCode != null);
            if (user == null || user.ResetCodeExpiration < DateTime.UtcNow || !BCrypt.Net.BCrypt.Verify(request.ResetCode, user.ResetCode))
            {
                return BadRequest(new { message = "Invalid or expired reset code" });
            }
            return Ok(new { message = "Reset code verified successfully" });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public IActionResult ResetPassword([FromBody] DTOs.ResetPasswordRequest request)
        {
            
            var user = _context.Users.FirstOrDefault(u => u.ResetCode != null);


            // Kod geçersiz veya süresi dolmuşsa hata döndür
            if (user == null || user.ResetCodeExpiration < DateTime.UtcNow || !BCrypt.Net.BCrypt.Verify(request.ResetCode, user.ResetCode))
            {
                return BadRequest(new { message = "Invalid or expired reset code" });
            }

           
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.ResetCode = string.Empty; // Kodları sıfırla
            user.ResetCodeExpiration = null;

            _context.SaveChanges();
            return Ok(new { message = "Password reset successfully" });
        }


    }
}
