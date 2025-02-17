using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ToDoBackend.Models
{
    public class Community
    {
        public int CommunityId { get; set; }

        [Required]
        public string CommunityName { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public int CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Password { get; set; } = string.Empty;
        // UserCommunities alanını varsayılan olarak boş bir liste ile başlatın
        public ICollection<UserCommunity> UserCommunities { get; set; } = new List<UserCommunity>();

    }
}
