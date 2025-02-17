using System;
using System.Text.Json.Serialization;

namespace ToDoBackend.Models
{
    public class UserCommunity
    {
        public int UserId { get; set; }
        public int CommunityId { get; set; }

        // Varsayılan rol "Member" olarak ayarlanabilir.
        public string Role { get; set; } = "Member"; // Member, Admin

        public DateTime JoinedAt { get; set; } = DateTime.Now;

        // Navigasyon özelliklerini nullable yapıyoruz
        [JsonIgnore]
        public User? User { get; set; }

        [JsonIgnore]
        public Community? Community { get; set; }
    }
}
