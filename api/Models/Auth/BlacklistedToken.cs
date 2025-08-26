using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    public class BlacklistedToken
    {
        [Key]
        public int Id { get; set; }
				[Required]
        public required string Token { get; set; }
				[Required]
        public required DateTime BlacklistedAt { get; set; }
    }
}