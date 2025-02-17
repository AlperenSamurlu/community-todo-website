namespace ToDoBackend.Models
{
    // mail doğrulaması
    public class VerificationRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
