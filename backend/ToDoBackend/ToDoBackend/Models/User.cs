namespace ToDoBackend.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? VerificationCode { get; set; } // VerificationCode nullable olmalı
        public bool IsVerified { get; set; } = false;

        public DateTime LastVerificationAttempt { get; set; } = DateTime.Now;
        public bool IsEmailNotificationEnabled { get; set; } = false; // Varsayılan değer

        public string? ResetCode { get; set; }
        public DateTime? ResetCodeExpiration { get; set; }

        // İlişkiler
        public ICollection<UserCommunity> UserCommunities { get; set; } = new List<UserCommunity>();
    }
}
