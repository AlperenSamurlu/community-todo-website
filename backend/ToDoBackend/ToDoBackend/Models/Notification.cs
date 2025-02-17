namespace ToDoBackend.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public int CommunityId { get; set; } // Bildirimin ait olduğu topluluk
        public int UserId { get; set; } // Bildirimi oluşturan kullanıcı
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

}
