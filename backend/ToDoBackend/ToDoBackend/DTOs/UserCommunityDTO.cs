namespace ToDoBackend.DTOs
{
    public class UserCommunityDTO
    {
        public int UserId { get; set; }
        public int CommunityId { get; set; }
        public string Role { get; set; } = string.Empty;
        public string CommunityName { get; set; } = string.Empty;
    }
}
