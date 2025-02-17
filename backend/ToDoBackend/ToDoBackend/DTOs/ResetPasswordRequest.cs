namespace ToDoBackend.DTOs
{
    public class ResetPasswordRequest
    {
        public string ResetCode { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
